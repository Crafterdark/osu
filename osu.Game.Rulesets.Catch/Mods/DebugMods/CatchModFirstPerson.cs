// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;
using System.Linq;
using osu.Game.Rulesets.Catch.Mods.DebugMods.Utility;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public partial class CatchModFirstPerson : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToPlayer, IUpdatableByPlayfield

    {
        public override string Name => "First-Person";

        public override string Acronym => "FP";

        public override LocalisableString Description => @"Catching fruits in first-person. [Warning: Motion sickness!]";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Fun;

        public override IconUsage? Icon => null;

        public bool IsCenteredByCatcher;

        public float LastTrackedCatcherPosition;

        private DrawableCatchRuleset drawableRuleset = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            this.drawableRuleset = drawableCatchRuleset;
        }

        public void ApplyToPlayer(Player player)
        {
            if (!drawableRuleset.HasReplayLoaded.Value && drawableRuleset.Mods.Any(m => m is CatchModRelax))
            {
                var catchPlayfield = (CatchPlayfield)drawableRuleset.Playfield;
                catchPlayfield.CatcherArea.Add(new MouseInputHelper(catchPlayfield.CatcherArea, catchPlayfield));
            }

            else
                IsCenteredByCatcher = true;
        }

        private partial class MouseInputHelper : Drawable, IKeyBindingHandler<CatchAction>, IRequireHighFrequencyMousePosition
        {
            private readonly CatcherArea catcherArea;

            private readonly CatchPlayfield catchPlayfield;

            private float lastTrackedMousePosition;

            public CatchHitObject? CurrentObject { get; set; }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public MouseInputHelper(CatcherArea catcherArea, CatchPlayfield catchPlayfield)
            {
                this.catcherArea = catcherArea;
                this.catchPlayfield = catchPlayfield;

                RelativeSizeAxes = Axes.Both;
            }

            // disable keyboard controls
            public bool OnPressed(KeyBindingPressEvent<CatchAction> e) => true;

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
            {
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                float mousePosition = Math.Clamp(e.MousePosition.X / DrawSize.X * CatchPlayfield.WIDTH, CatchUtilityForMods.GetMinPlayfieldWidth(catcherArea.ShrinkFactor), CatchUtilityForMods.GetMaxPlayfieldWidth(catcherArea.ShrinkFactor));

                UpdateCatcherVisualDirection(catcherArea.Catcher, mousePosition, lastTrackedMousePosition);

                lastTrackedMousePosition = mousePosition;

                UpdateHitObjects(catchPlayfield, mousePosition);

                return base.OnMouseMove(e);
            }
        }

        public static void UpdateCatcherVisualDirection(Catcher catcher, float currentTrackedPosition, float lastTrackedPosition)
        {
            if (lastTrackedPosition != currentTrackedPosition)
            {
                if (lastTrackedPosition > currentTrackedPosition)
                {
                    catcher.VisualDirection = UI.Direction.Left;
                }

                else
                {
                    catcher.VisualDirection = UI.Direction.Right;
                }
            }
        }

        public static void UpdateHitObjects(CatchPlayfield catchPlayfield, float currentTrackedPosition)
        {
            foreach (DrawableHitObject drawableHitObject in catchPlayfield.AllHitObjects)
            {
                if ((DrawableCatchHitObject)drawableHitObject is DrawableBananaShower)
                {
                    foreach (DrawableCatchHitObject banana in drawableHitObject.NestedHitObjects)
                    {
                        (banana.HitObject).XOffsetMod = (CatchPlayfield.WIDTH / 2) - currentTrackedPosition;
                        //(banana.HitObject).XOffset;
                    }
                }

                else if ((DrawableCatchHitObject)drawableHitObject is DrawableJuiceStream)
                {
                    foreach (DrawableCatchHitObject nestedFruit in drawableHitObject.NestedHitObjects)
                    {
                        (nestedFruit.HitObject).XOffsetMod = (CatchPlayfield.WIDTH / 2) - currentTrackedPosition;
                        //(nestedFruit.HitObject).XOffset = mousePosition - 256;
                    }
                }

                else
                {
                    ((DrawableCatchHitObject)drawableHitObject).HitObject.XOffsetMod = (CatchPlayfield.WIDTH / 2) - currentTrackedPosition;
                    //((DrawableCatchHitObject)drawableHitObject).HitObject.XOffset = mousePosition - 256;
                }
            }
            //Logger.Log("Current count of bananashower: " + countBananaShower);
            //Logger.Log("Current count of juicestream: " + countJuiceStream);
        }

        public void Update(Playfield playfield)
        {
            if (IsCenteredByCatcher)
            {
                var catchPlayfield = (CatchPlayfield)drawableRuleset.Playfield;

                LastTrackedCatcherPosition = catchPlayfield.CatcherArea.FirstPersonPosition;

                catchPlayfield.CatcherArea.DisableMainMovement = true;
                catchPlayfield.CatcherArea.ForceVisualDirectionChanges = true;
                catchPlayfield.CatcherArea.IsFirstPerson = true;

                //Logger.Log("Catcher Visual Direction: " + catcher.VisualDirection);
                //Logger.Log("Catcher Current Pos: " + catcher.X);

                UpdateHitObjects(catchPlayfield, LastTrackedCatcherPosition);
            }
        }
    }
}
