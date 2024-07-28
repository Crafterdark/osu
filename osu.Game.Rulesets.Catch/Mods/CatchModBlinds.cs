// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModBlinds : ModBlinds, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(Blinds = new DrawableBlinds(drawableRuleset.Playfield, drawableRuleset.Beatmap));
        }
    }
}
