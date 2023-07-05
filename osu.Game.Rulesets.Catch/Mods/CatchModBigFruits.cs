// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModBigFruits : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmap
    {
        public override string Acronym => "BF";

        public override string Name => "Big Fruits";

        public override LocalisableString Description => "Every fruit is slightly bigger and easier to catch.";
        //Note: This does not affect Circle Size, but it affects player catching ranges!
        public override IconUsage? Icon => null;

        public override ModType Type => ModType.DifficultyReduction;

        public override double ScoreMultiplier => 0.5;

        [SettingSource("New Fruit Size", "The new size for fruits")]
        public BindableDouble NewFruitsSize { get; } = new BindableDouble(1.25d)
        {
            MinValue = 1.01d,
            MaxValue = 1.50d,
            Precision = 0.01d
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            foreach (var hitObject in beatmap.HitObjects)
                applyToHitObject(hitObject);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchBiggerFruits = true;
            catchPlayfield.Catcher.CatchBiggerFruitsSize = NewFruitsSize.Value;
        }


        private void applyToHitObject(HitObject hitObject)
        {
            var catchObject = (CatchHitObject)hitObject;

            switch (catchObject)
            {
                case Fruit fruit:
                    fruit.Scale *= (float)(NewFruitsSize.Value + 0.1);
                    break;

                case Droplet droplet:
                    droplet.Scale *= (float)(NewFruitsSize.Value + 0.050);
                    break;

                case JuiceStream juiceStream:
                    foreach (var nested in juiceStream.NestedHitObjects.Cast<CatchHitObject>())
                    {
                        if (nested is Fruit) nested.Scale *= (float)(NewFruitsSize.Value + 0.1);
                        if (nested is Droplet) nested.Scale *= (float)(NewFruitsSize.Value + 0.050);
                        if (nested is TinyDroplet) nested.Scale *= (float)(NewFruitsSize.Value + 0.025);
                    }
                    break;
            }

        }

    }
}
