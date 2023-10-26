// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModAutopilot : Mod, IApplicableFailOverride, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield

    {
        public override string Name => "Autopilot";

        public override string Acronym => "AP";

        public override LocalisableString Description => @"Automatic catcher movement - directional dash with two keys.";

        public override double ScoreMultiplier => 0.1;

        public override ModType Type => ModType.Automation;

        public override IconUsage? Icon => OsuIcon.ModAutopilot;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModDirectionalDash), typeof(CatchModRelax), typeof(CatchModAutoplay), typeof(CatchModCinema) };

        public bool PerformFail() => false;

        public bool RestartOnFail => false;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchRuleset = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchRuleset.CatcherArea.DisableMainDash = true;
            catchRuleset.CatcherArea.DisableMainMovement = true;
            catchRuleset.CatcherArea.IsDirectionalDash = true;
            catchRuleset.CatcherArea.IsAutopilot = true;
        }

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;
            var catcherArea = catchPlayfield.CatcherArea;
            var catcher = catchPlayfield.Catcher;
            double currentTime = catchPlayfield.Time.Current;
            double currentElapsed = catchPlayfield.Time.Elapsed;
            double exactTime = currentTime - currentElapsed;

            CatchHitObject? incomingObject = FindIncomingCatchHitObject(catchPlayfield, catchPlayfield.Catcher, exactTime);

            //WIP: This system does not handle many patterns and it's just a placeholder
            if (incomingObject != null)
            {
                if (incomingObject.EffectiveX > catcher.X)
                {
                    catcherArea.CurrentAutopilotDirection = 1;
                }

                else if (incomingObject.EffectiveX < catcher.X)
                {
                    catcherArea.CurrentAutopilotDirection = -1;
                }

                else
                    catcherArea.CurrentAutopilotDirection = 0;
            }

            else
                catcherArea.CurrentAutopilotDirection = 0;
        }

        public CatchHitObject? FindIncomingCatchHitObject(CatchPlayfield catchPlayfield, Catcher catcher, double exactTime)
        {
            foreach (DrawableHitObject drawableHitObject in catchPlayfield.AllHitObjects)
            {
                if ((DrawableCatchHitObject)drawableHitObject is DrawableBananaShower)
                {
                    foreach (var banana in drawableHitObject.NestedHitObjects)
                    {
                        if (banana.HitObject.StartTime > exactTime)
                            return (CatchHitObject)banana.HitObject;
                    }
                }

                else if ((DrawableCatchHitObject)drawableHitObject is DrawableJuiceStream)
                {
                    foreach (var nestedFruit in drawableHitObject.NestedHitObjects)
                    {
                        if (nestedFruit.HitObject.StartTime > exactTime)
                            return (CatchHitObject)nestedFruit.HitObject;
                    }
                }

                else
                {
                    if (drawableHitObject.HitObject.StartTime > exactTime)
                        return (CatchHitObject)drawableHitObject.HitObject;
                }
            }
            //Logger.Log("Current count of bananashower: " + countBananaShower);
            //Logger.Log("Current count of juicestream: " + countJuiceStream);
            return null;
        }
    }
}
