// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Utils
{
    public class LimitedCatchPlayfieldContainer(double value)
    {
        public float MinWidth = (float)(CatchPlayfield.WIDTH - value) / 2;
        public float MaxWidth = (float)value + (float)(CatchPlayfield.WIDTH - value) / 2;
        public float ConversionFactor = (float)value / CatchPlayfield.WIDTH;

        public void Convert(CatchHitObject catchHitObject)
        {
            catchHitObject.SetConversionFactorLimitation(true, ConversionFactor, MinWidth);
        }

        public void Unconvert(CatchHitObject catchHitObject)
        {
            catchHitObject.SetConversionFactorLimitation(false, ConversionFactor, MinWidth);
        }

        public void ConvertPair(CatchHitObject currObject, CatchHitObject nextObject)
        {
            Convert(currObject);
            Convert(nextObject);
        }

        public void UnconvertPair(CatchHitObject currObject, CatchHitObject nextObject)
        {
            Unconvert(currObject);
            Unconvert(nextObject);
        }
    }
}
