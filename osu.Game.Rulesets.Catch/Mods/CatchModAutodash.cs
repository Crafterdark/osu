// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public partial class CatchModAutodash : Mod, IApplicableMod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Autodash";
        public override string Acronym => "AD";
        public override LocalisableString Description => "Automatic catcher dashing.";
        public override double ScoreMultiplier => 0.5;
        public override IconUsage? Icon => FontAwesome.Solid.HandsHelping;
        public override ModType Type => ModType.Automation;

        private IFrameStableClock gameplayClock = null!;

        private Catcher catcher = null!;

        private Playfield playfield = null!;

        private PalpableCatchHitObject? incomingHitObject;

        private double halfCatchWidth;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            catchPlayfield.CatcherArea.InvalidCatchActionList.Add(CatchAction.Dash);

            gameplayClock = drawableRuleset.FrameStableClock;
            catcher = catchPlayfield.Catcher;
            playfield = catchDrawableRuleset.Playfield;

            halfCatchWidth = Catcher.CalculateCatchWidth(drawableRuleset.Beatmap.Difficulty) / 2;
        }

        public void Update(Playfield playfield)
        {
            incomingHitObject = GetIncomingObject(gameplayClock.CurrentTime);

            AdjustDashing(gameplayClock.CurrentTime);
        }

        public void AdjustDashing(double currentTime)
        {
            if (incomingHitObject != null && catcher.HyperDashing == false)
            {
                Logger.Log("here");
                int direction = catcher.X > incomingHitObject.EffectiveX ? -1 : (catcher.X == incomingHitObject.EffectiveX ? 0 : 1);
                float catcherBestX = (float)(catcher.X + direction * halfCatchWidth);
                float totalDistanceFromBestLocation = (float)((incomingHitObject.EffectiveX - catcherBestX) * direction);
                float totalDistanceFromTime = (float)((incomingHitObject.StartTime - currentTime) * Catcher.BASE_WALK_SPEED);
                if (totalDistanceFromBestLocation <= totalDistanceFromTime)
                    catcher.Dashing = false;
                else
                    catcher.Dashing = true;
            }
            else
                catcher.Dashing = true;
        }

        public PalpableCatchHitObject? GetIncomingObject(double currentTime)
        {
            if (playfield.AllHitObjects != null && playfield.AllHitObjects.Count() > 0)
            {
                foreach (var drawableHitObject in playfield.AllHitObjects)
                {
                    var hitObject = drawableHitObject.HitObject;
                    bool isContainer = hitObject is not PalpableCatchHitObject;

                    if (isContainer)
                    {
                        foreach (var nestedHitObject in hitObject.NestedHitObjects)
                        {
                            if (nestedHitObject.StartTime >= currentTime)
                                return (PalpableCatchHitObject?)nestedHitObject;
                        }
                    }
                    else if (hitObject.StartTime >= currentTime)
                    {
                        return (PalpableCatchHitObject?)hitObject;
                    }
                }
            }

            return null;
        }
    }

}
