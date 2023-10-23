// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModSecretHyper : Mod, IUpdatableByPlayfield
    {
        public override LocalisableString Description => @"Hyperdashing effect is no longer visible...";
        public override string Name => "Secret Hyper";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.01 : 1;
        public override string Acronym => "SH";
        public override ModType Type => ModType.DifficultyIncrease;

        public void Update(Playfield playfield)
        {
            CatchPlayfield cpf = (CatchPlayfield)playfield;

            foreach (DrawableHitObject hitObject in cpf.AllHitObjects)
            {
                if (!(hitObject is DrawableCatchHitObject))
                    return;

                if (hitObject.NestedHitObjects.Any())
                {
                    foreach (var nestedDrawable in hitObject.NestedHitObjects)
                    {
                        if (nestedDrawable is DrawableCatchHitObject nestedCatchDrawable)
                            HideHyper(nestedCatchDrawable);
                    }
                }

                else
                    HideHyper((DrawableCatchHitObject)hitObject);
            }
        }

        public void HideHyper(DrawableCatchHitObject drawable)
        {
            if (drawable is DrawableFruit df)
            {
                df.HyperDash.Value = false;
            }
            else if (drawable is DrawableDroplet dd)
            {
                dd.HyperDash.Value = false;
            }
            else if (drawable is DrawableTinyDroplet dtd)
            {
                dtd.HyperDash.Value = false;
            }
        }
    }
}
