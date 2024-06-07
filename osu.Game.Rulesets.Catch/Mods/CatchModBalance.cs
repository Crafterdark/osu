// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModBalance : Mod, IApplicableToBeatmap
    {
        public override string Name => @"Balance";
        public override string Acronym => @"BE";
        public override LocalisableString Description => @"Remove difficult edge dashes.";
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax) };
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => FontAwesome.Solid.BalanceScale;

        /// <summary>
        /// Necessary to avoid certain patterns to become too difficult during the removal of edge patterns.
        /// </summary>
        /// <param name="catcherPos"></param>
        /// <param name="halfCatcherWidth"></param>
        /// <param name="firstStackNotePos"></param>
        /// <param name="secondStackNotePos"></param>
        /// <returns>Whether this pattern is stackable</returns>
        private bool isStackablePatternInRange(float catcherPos, double halfCatcherWidth, float firstStackNotePos, float secondStackNotePos)
        {
            bool isFirstStackNoteCatchable = catcherPos - halfCatcherWidth <= firstStackNotePos && firstStackNotePos <= catcherPos + halfCatcherWidth;
            bool isSecondStackNoteCatchable = catcherPos - halfCatcherWidth <= secondStackNotePos && secondStackNotePos <= catcherPos + halfCatcherWidth;

            return isFirstStackNoteCatchable && isSecondStackNoteCatchable;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var palpableObjects = CatchBeatmap.GetPalpableObjects(beatmap.HitObjects)
                                              .Where(h => h is Fruit || (h is Droplet && h is not TinyDroplet))
                                              .ToArray();

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
            // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
            // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
            halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            for (int i = 0; i < palpableObjects.Length - 1; i++)
            {
                var prevObject = i - 1 >= 0 ? palpableObjects[i - 1] : null;
                var currentObject = palpableObjects[i];
                var nextObject = palpableObjects[i + 1];

                // Reset variables in-case values have changed (e.g. after applying HR)
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = 0;

                int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;

                // Int truncation added to match osu!stable.
                double timeToNext = (int)nextObject.StartTime - (int)currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNext = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                float distanceToHyper = (float)(timeToNext * Catcher.BASE_DASH_SPEED - distanceToNext);

                bool forceHyperDash = false;

                // Force the generation of hyperdashes when the center of the next note cannot be reached from the previous center.
                double timeToNextCenter = nextObject.StartTime - currentObject.StartTime;
                double distanceToNextCenter = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX);
                double distanceToEdge = timeToNextCenter * Catcher.BASE_DASH_SPEED - distanceToNextCenter;

                bool isStackablePattern = false;

                if (prevObject != null)
                {
                    // Note: This is a good approximation of what could be considered a stackable pattern.
                    double stackableCenter = (currentObject.EffectiveX + nextObject.EffectiveX) / 2;

                    // Calculate if the stackable pattern can be caught in time. (It's still possible that the catcher cannot reach this position)
                    double timeToStackablePattern = currentObject.StartTime - prevObject.StartTime;
                    double distanceToNextStackablePattern = Math.Abs(stackableCenter - prevObject.EffectiveX);
                    double distanceToStackable = timeToStackablePattern * Catcher.BASE_DASH_SPEED - distanceToNextStackablePattern;

                    isStackablePattern = distanceToStackable >= 0 && isStackablePatternInRange((currentObject.EffectiveX + nextObject.EffectiveX) / 2, halfCatcherWidth, currentObject.EffectiveX, nextObject.EffectiveX);
                }

                if (distanceToEdge < 0 && !isStackablePattern)
                    forceHyperDash = true;

                if (distanceToHyper < 0 || forceHyperDash)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    lastExcess = Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
