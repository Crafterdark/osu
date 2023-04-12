// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.Skills
{
    public class CatchModSkillGrowth : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield, IApplicableToBeatmap
    {
        public override string Name => "Growth Skill";
        public override string Acronym => "GR";
        public override LocalisableString Description => "The catcher and the fruits will increase in size for a limited amount of time.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Fun;

        public static List<PalpableCatchHitObject> ListPalpableCatchableObject = new List<PalpableCatchHitObject>();

        public static bool Trigger = false!;

        public static float MapOriginalCS;

        public static float OriginalScale;

        [SettingSource("Growth Time", "The time [seconds] for the effect to last.")]
        public Bindable<int> GrowthTime { get; } = new BindableInt(5)
        {
            MinValue = 5,
            MaxValue = 10
        };

        [SettingSource("Growth Cooldown", "The time [seconds] for using this skill again.")]
        public Bindable<int> GrowthCooldownTime { get; } = new BindableInt(10)
        {
            MinValue = 10,
            MaxValue = 30
        };

        [SettingSource("Growth Power", "The decreasing multiplier of the original circle size during the growth.")]
        public Bindable<float> GrowthPower { get; } = new BindableFloat((float)0.5)
        {
            Precision = (float)0.1,
            MinValue = (float)0.5,
            MaxValue = (float)0.9,
        };

        public override Type[] IncompatibleMods => new[] { typeof(CatchModCatchTheMania), typeof(CatchModRelax) };

        public override string SettingDescription
        {
            get
            {
                string growthTime_string = GrowthTime.IsDefault ? string.Empty : string.Empty;
                string growthCooldownTime_string = GrowthCooldownTime.IsDefault ? string.Empty : string.Empty;
                string growthPower_string = GrowthPower.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    growthTime_string,
                    growthCooldownTime_string,
                    growthPower_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.GrowthApplies = true;
            catchPlayfield.CatcherArea.Catcher.CanGrow = true;
            catchPlayfield.CatcherArea.GrowthTimerParam = GrowthTime.Value;
            catchPlayfield.CatcherArea.GrowthCooldownParam = GrowthCooldownTime.Value;
            catchPlayfield.CatcherArea.GrowthMultiplier = GrowthPower.Value;
        }

        public void Update(Playfield playfield)
        {
            if (Trigger && ListPalpableCatchableObject.Count > 0)
            {
                PalpableCatchHitObject currentObject = ListPalpableCatchableObject[0];
                ListPalpableCatchableObject[0].Scale = (1.0f - 0.7f * ((MapOriginalCS * GrowthPower.Value) - 5) / 5) / 2;
                double startTime = currentObject.StartTime;
                double timePreempt = currentObject.TimePreempt;
                for (int index = 1; index < ListPalpableCatchableObject.Count && ListPalpableCatchableObject[index].StartTime <= (startTime + timePreempt); index++)
                {
                    ListPalpableCatchableObject[index].Scale = (1.0f - 0.7f * ((MapOriginalCS * GrowthPower.Value) - 5) / 5) / 2;
                }
            }
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            ListPalpableCatchableObject.Clear();

            MapOriginalCS = beatmap.Difficulty.CircleSize;

            float cs_original = (1.0f - 0.7f * (MapOriginalCS - 5) / 5) / 2;

            Trigger = false;

            foreach (var currentObject in beatmap.HitObjects)
            {
                if (currentObject is Fruit fruitObject)
                {
                    fruitObject.Scale = cs_original;
                    ListPalpableCatchableObject.Add(fruitObject);
                }

                if (currentObject is JuiceStream)
                {
                    foreach (var juice in currentObject.NestedHitObjects)
                    {
                        if (juice is PalpableCatchHitObject palpableObject)
                        {
                            palpableObject.Scale = cs_original;
                            ListPalpableCatchableObject.Add(palpableObject);
                        }
                    }
                }
            }

            ListPalpableCatchableObject.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
        }
    }
}
