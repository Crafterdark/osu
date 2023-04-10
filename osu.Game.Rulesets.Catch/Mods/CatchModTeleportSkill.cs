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

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModTeleportSkill : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield, IApplicableToBeatmap
    {
        public override string Name => "Teleport Skill";
        public override string Acronym => "TS";
        public override LocalisableString Description => "The catcher will automatically teleport to catch fruits for a limited amount of time.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Fun;

        public static List<PalpableCatchHitObject> ListPalpableCatchableObject = new List<PalpableCatchHitObject>();

        public static float CatchableObjectEffectiveX;

        public static bool Trigger = false!;

        [SettingSource("Teleportation Time", "The length of time [seconds] that the catcher will teleport to catch fruits.")]
        public Bindable<int> TeleportationTime { get; } = new BindableInt(5)
        {
            MinValue = 5,
            MaxValue = 10
        };

        [SettingSource("Teleportation Cooldown", "The length of time [seconds] that the catcher has to wait for teleporting again.")]
        public Bindable<int> TeleportationCooldown { get; } = new BindableInt(15)
        {
            MinValue = 15,
            MaxValue = 30
        };

        public override Type[] IncompatibleMods => new[] { typeof(CatchModCatchTheMania), typeof(CatchModRelax) };

        public override string SettingDescription
        {
            get
            {
                string teleportationTime_string = TeleportationTime.IsDefault ? string.Empty : string.Empty;
                string teleportationCooldown_string = TeleportationCooldown.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    teleportationTime_string,
                    teleportationCooldown_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.TeleportApplies = true;
            catchPlayfield.Catcher.CanTeleport = true;
            catchPlayfield.CatcherArea.TeleportTimerParam = TeleportationTime.Value;
            catchPlayfield.CatcherArea.TeleportCooldownParam = TeleportationCooldown.Value;
        }

        public void Update(Playfield playfield)
        {
            if (!Trigger)
            {
                Trigger = true; //locks the function until one object is removed later
                if (ListPalpableCatchableObject.Count > 0)
                {
                    float tempObjectEffectiveX = ListPalpableCatchableObject[0].EffectiveX;

                    double startTimeFirstObject = ListPalpableCatchableObject[0].StartTime;

                    float halfCatcherWidth = ((CatchPlayfield)playfield).CatcherArea.Catcher.CatchWidth / 2;

                    //Find double+ notes catch position

                    if (ListPalpableCatchableObject.Count > 1) //multiple notes at the same time
                    {
                        int multipleNotesSameTime = 1;

                        int index = 1;

                        while (index < ListPalpableCatchableObject.Count && (startTimeFirstObject == ListPalpableCatchableObject[index].StartTime))
                        {
                            tempObjectEffectiveX += ListPalpableCatchableObject[index].EffectiveX;
                            multipleNotesSameTime++;
                            index++;
                        }
                        if (multipleNotesSameTime > 1) //previous cycle executed one time
                        {
                            tempObjectEffectiveX /= multipleNotesSameTime;
                            CatchableObjectEffectiveX = tempObjectEffectiveX;
                            return;
                        }
                    }

                    //Find no movement on a note OR two notes

                    if (tempObjectEffectiveX <= CatchableObjectEffectiveX + halfCatcherWidth && tempObjectEffectiveX >= CatchableObjectEffectiveX - halfCatcherWidth)
                    {
                        if (ListPalpableCatchableObject.Count > 1)
                        {
                            float testObjectEffectiveX = (tempObjectEffectiveX + ListPalpableCatchableObject[1].EffectiveX) / 2;
                            if (ListPalpableCatchableObject[0].EffectiveX <= testObjectEffectiveX + halfCatcherWidth && ListPalpableCatchableObject[0].EffectiveX >= testObjectEffectiveX - halfCatcherWidth)
                                if (ListPalpableCatchableObject[1].EffectiveX <= testObjectEffectiveX + halfCatcherWidth && ListPalpableCatchableObject[1].EffectiveX >= testObjectEffectiveX - halfCatcherWidth)
                                {
                                    CatchableObjectEffectiveX = testObjectEffectiveX;
                                    return;
                                }
                        }

                        return;
                    }

                    //Normal case: Move catcher in the middle of the next note

                    else
                    {

                        CatchableObjectEffectiveX = tempObjectEffectiveX;

                    }

                }
            }
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            ListPalpableCatchableObject.Clear();

            CatchableObjectEffectiveX = CatchPlayfield.WIDTH / 2;

            Trigger = false;

            foreach (var currentObject in beatmap.HitObjects)
            {
                if (currentObject is Fruit fruitObject)
                {
                    ListPalpableCatchableObject.Add(fruitObject);
                }

                if (currentObject is JuiceStream)
                {
                    foreach (var juice in currentObject.NestedHitObjects)
                    {
                        if (juice is PalpableCatchHitObject palpableObject)
                            ListPalpableCatchableObject.Add(palpableObject);
                    }
                }
            }

            ListPalpableCatchableObject.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
        }
    }
}
