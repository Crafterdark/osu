// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public abstract partial class LegacyCatchHitObjectPiece : PoolableDrawable
    {
        protected readonly Bindable<int> IndexInBeatmap = new Bindable<int>();

        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();
        private readonly Bindable<bool> hyperDash = new Bindable<bool>();

        private readonly Sprite colouredSprite;
        private readonly Sprite overlaySprite;
        private readonly Sprite hyperSprite;

        [Resolved]
        protected ISkinSource Skin { get; private set; } = null!;

        [Resolved]
        protected IHasCatchObjectState ObjectState { get; private set; } = null!;

        /// <summary>
        /// Animation time obtained from comparison with osu!stable.
        /// </summary>
        private const double legacy_hyperdash_animation_time = 500;

        protected LegacyCatchHitObjectPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                colouredSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                overlaySprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                hyperSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Depth = 1,
                    Alpha = 0,
                    Scale = new Vector2(1.2f),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            accentColour.BindTo(ObjectState.AccentColour);
            hyperDash.BindTo(ObjectState.HyperDash);
            IndexInBeatmap.BindTo(ObjectState.IndexInBeatmap);

            hyperSprite.Colour = Skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashFruit)?.Value ??
                                 Skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                                 Catcher.DEFAULT_HYPER_DASH_COLOUR;

            accentColour.BindValueChanged(colour =>
            {
                colouredSprite.Colour = LegacyColourCompatibility.DisallowZeroAlpha(colour.NewValue);
            }, true);

            hyperDash.BindValueChanged(hyper =>
            {
                hyperSprite.Alpha = hyper.NewValue ? 1f : 0;
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            if (!ObjectState.HyperDash.Value)
                return;

            double animationCurrentTime = Time.Current;

            double animationStartTime = ObjectState.HitObject.StartTime - ObjectState.HitObject.TimePreempt; // ObjectState.DisplayStartTime is intentionally not used.
            double animationEndTime = animationStartTime + legacy_hyperdash_animation_time;

            //Cycle back the animation when needed.
            while (animationCurrentTime > animationEndTime)
                animationCurrentTime -= legacy_hyperdash_animation_time;

            hyperSprite.Alpha = Interpolation.ValueAt(
                    animationCurrentTime, 1f, 0.5f,
                    animationStartTime,
                    animationEndTime,
                    Easing.None
            );

            hyperSprite.Scale = Interpolation.ValueAt(
                    animationCurrentTime, new Vector2(1.2f), new Vector2(1.3f),
                    animationStartTime,
                    animationEndTime,
                    Easing.None
            );
        }

        protected void SetTexture(Texture? texture, Texture? overlayTexture)
        {
            colouredSprite.Texture = texture;
            overlaySprite.Texture = overlayTexture;
            hyperSprite.Texture = texture;
        }
    }
}
