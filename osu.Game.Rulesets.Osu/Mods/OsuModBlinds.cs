﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModBlinds : ModBlinds, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override bool Ranked => BlindsFullOpaque.IsDefault;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(Blinds = new DrawableBlinds(drawableRuleset.Playfield, drawableRuleset.Beatmap));
        }
    }
}
