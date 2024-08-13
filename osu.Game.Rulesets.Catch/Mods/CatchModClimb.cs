// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClimb : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Climb";
        public override string Acronym => "CB";
        public override LocalisableString Description => "Catch the flying fruits!";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.HandRock;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModFloatingFruits) };

        [SettingSource("Wall to climb", "Choose the wall that the catcher will climb.")]
        public Bindable<WallType> WallToClimb { get; } = new Bindable<WallType>();

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            drawableRuleset.PlayfieldAdjustmentContainer.Origin = Anchor.Centre;

            switch (WallToClimb.Value)
            {
                case WallType.Left:
                    drawableRuleset.PlayfieldAdjustmentContainer.Rotation = 90;
                    break;
                case WallType.Right:
                    drawableRuleset.PlayfieldAdjustmentContainer.Rotation = -90;
                    break;
            }

            // Required to prevent the playfield to be offscreen. Might artificially increase difficulty.
            drawableRuleset.PlayfieldAdjustmentContainer.Scale = new Vector2(0.75f, 0.75f);
        }

        public enum WallType
        {
            Left,
            Right
        }
    }
}
