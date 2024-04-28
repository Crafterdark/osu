// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModMirror : ModHorizontalMirror, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            var taikoPlayfield = (TaikoPlayfield)drawableRuleset.Playfield;

            taikoPlayfield.Anchor = Anchor.Centre;
            taikoPlayfield.Origin = Anchor.Centre;
            taikoPlayfield.Rotation = 180;
            taikoPlayfield.Scale = new osuTK.Vector2(1, -1);
        }
    }
}
