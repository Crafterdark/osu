// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModTwinCatchers : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Twin Catchers";
        public override string Acronym => "TC";
        public override LocalisableString Description => "Two catchers, two fields.";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Moon; //Placeholder
        public override ModType Type => ModType.Conversion;

        //The edge of the Catcher field, near the the middle of the screen
        public float LeftEdgeFromMiddle;

        //The edge of the Twin catcher field, near the the middle of the screen
        public float RightEdgeFromMiddle;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.CatcherArea.TwinCatchersApplies = true;

            catchPlayfield.CatcherArea.Twin = catchPlayfield.Twin;

            //The edge of the Catcher field, near the the middle of the screen
            LeftEdgeFromMiddle = (CatchPlayfield.WIDTH / 2) - (catchPlayfield.Catcher.CatchWidth / 2);

            //The edge of the Twin catcher field, near the the middle of the screen
            RightEdgeFromMiddle = (CatchPlayfield.WIDTH / 2) + (catchPlayfield.Twin.CatchWidth / 2);

            catchPlayfield.CatcherArea.Catcher.X = LeftEdgeFromMiddle / 2;
            catchPlayfield.CatcherArea.Catcher.VisualDirection = Direction.Right;
            catchPlayfield.CatcherArea.Twin.X = CatchPlayfield.WIDTH - ((CatchPlayfield.WIDTH - RightEdgeFromMiddle) / 2);
            catchPlayfield.CatcherArea.Twin.VisualDirection = Direction.Left;

        }

    }
}
