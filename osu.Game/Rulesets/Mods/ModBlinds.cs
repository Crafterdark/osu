﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public partial class ModBlinds : Mod
    {
        public override string Name => "Blinds";
        public override LocalisableString Description => "Play with blinds on your screen.";
        public override string Acronym => "BL";

        public override IconUsage? Icon => FontAwesome.Solid.Adjust;
        public override ModType Type => ModType.DifficultyIncrease;

        public override double ScoreMultiplier => BlindsFullOpaque.Value ? 1.12 : 1.06;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight) };
        public override bool Ranked => true;

        public DrawableBlinds Blinds = null!;

        [SettingSource("Start as full opaque", "Cannot see through blinds")]
        public BindableBool BlindsFullOpaque { get; } = new BindableBool(true);

        /// <summary>
        /// Element for the Blinds mod drawing 2 black boxes covering the whole screen which resize inside a restricted area with some leniency.
        /// </summary>
        public partial class DrawableBlinds : Container
        {
            /// <summary>
            /// Black background boxes behind blind panel textures.
            /// </summary>
            private Box blackBoxLeft = null!, blackBoxRight = null!;

            private Drawable panelLeft = null!;
            private Drawable panelRight = null!;
            private Drawable bgPanelLeft = null!;
            private Drawable bgPanelRight = null!;

            private readonly IBeatmap beatmap;

            /// <summary>
            /// Value between 0 and 1 setting a maximum "closedness" for the blinds.
            /// Useful for animating how far the blinds can be opened while keeping them at the original position if they are wider open than this.
            /// </summary>
            private const float target_clamp = 1;

            private readonly float targetBreakMultiplier;
            private readonly float easing;

            private readonly CompositeDrawable restrictTo;

            /// <summary>
            /// <para>
            /// Percentage of playfield to extend blinds over. Basically moves the origin points where the blinds start.
            /// </para>
            /// <para>
            /// -1 would mean the blinds always cover the whole screen no matter health.
            /// 0 would mean the blinds will only ever be on the edge of the playfield on 0% health.
            /// 1 would mean the blinds are fully outside the playfield on 50% health.
            /// Infinity would mean the blinds are always outside the playfield except on 100% health.
            /// </para>
            /// </summary>
            private const float leniency = 0.1f;

            public DrawableBlinds(CompositeDrawable restrictTo, IBeatmap beatmap)
            {
                this.restrictTo = restrictTo;
                this.beatmap = beatmap;

                targetBreakMultiplier = 0;
                easing = 1;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;

                Children = new[]
                {
                    blackBoxLeft = new Box
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Y,
                    },
                    blackBoxRight = new Box
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Y,
                    },
                    bgPanelLeft = new ModBlindsPanel
                    {
                        Origin = Anchor.TopRight,
                        Colour = Color4.Gray,
                    },
                    panelLeft = new ModBlindsPanel { Origin = Anchor.TopRight, },
                    bgPanelRight = new ModBlindsPanel { Colour = Color4.Gray },
                    panelRight = new ModBlindsPanel()
                };
            }

            private float calculateGap(float value) => Math.Clamp(value, 0, target_clamp) * targetBreakMultiplier;

            // lagrange polinominal for (0,0) (0.6,0.4) (1,1) should make a good curve
            private static float applyAdjustmentCurve(float value) => 0.6f * value * value + 0.4f * value;

            protected override void Update()
            {
                float start, end;

                if (Precision.AlmostEquals(restrictTo.Rotation, 0))
                {
                    start = Parent!.ToLocalSpace(restrictTo.ScreenSpaceDrawQuad.TopLeft).X;
                    end = Parent!.ToLocalSpace(restrictTo.ScreenSpaceDrawQuad.TopRight).X;
                }
                else
                {
                    float center = restrictTo.ToSpaceOfOtherDrawable(restrictTo.OriginPosition, Parent!).X;
                    float halfDiagonal = (restrictTo.DrawSize / 2).LengthFast;

                    start = center - halfDiagonal;
                    end = center + halfDiagonal;
                }

                float rawWidth = end - start;

                start -= rawWidth * leniency * 0.5f;
                end += rawWidth * leniency * 0.5f;

                float width = (end - start) * 0.5f * applyAdjustmentCurve(calculateGap(easing));

                // different values in case the playfield ever moves from center to somewhere else.
                blackBoxLeft.Width = start + width;
                blackBoxRight.Width = DrawWidth - end + width;

                panelLeft.X = start + width;
                panelRight.X = end - width;
                bgPanelLeft.X = start;
                bgPanelRight.X = end;
            }

            protected override void LoadComplete()
            {
                const float break_open_early = 500;
                const float break_close_late = 250;

                base.LoadComplete();

                var firstObj = beatmap.HitObjects[0];
                double startDelay = firstObj.StartTime - (firstObj is IHasTimePreempt firstObjWithTimePreempt ? firstObjWithTimePreempt.TimePreempt : 0);

                Logger.Log("Value is: " + (firstObj is IHasTimePreempt firstObjW ? firstObjW.TimePreempt : 0));

                using (BeginAbsoluteSequence(startDelay + break_close_late))
                    leaveBreak();

                foreach (var breakInfo in beatmap.Breaks)
                {
                    if (breakInfo.HasEffect)
                    {
                        using (BeginAbsoluteSequence(breakInfo.StartTime - break_open_early))
                        {
                            enterBreak();
                            using (BeginDelayedSequence(breakInfo.Duration + break_open_early + break_close_late))
                                leaveBreak();
                        }
                    }
                }
            }

            private void enterBreak() => this.TransformTo(nameof(targetBreakMultiplier), 0f, 1000, Easing.OutSine);

            private void leaveBreak() => this.TransformTo(nameof(targetBreakMultiplier), 1f, 2500, Easing.OutBounce);

            /// <summary>
            /// 0 is open, 1 is closed.
            /// </summary>
            public void AnimateClosedness(float value) => this.TransformTo(nameof(easing), value, 200, Easing.OutQuint);

            public partial class ModBlindsPanel : Sprite
            {
                [BackgroundDependencyLoader]
                private void load(TextureStore textures)
                {
                    Texture = textures.Get("Gameplay/osu/blinds-panel");
                }
            }
        }
    }
}
