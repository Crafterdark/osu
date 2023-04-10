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

        public static PalpableCatchHitObject CatchableObject = null!;

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
            if (ListPalpableCatchableObject.Count > 0)
            {
                CatchableObject = ListPalpableCatchableObject[0];
            }

        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            ListPalpableCatchableObject.Clear();

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
