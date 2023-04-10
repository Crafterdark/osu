﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
    public class CatchModMagnetAttractSkill : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield, IApplicableToBeatmap
    {
        public override string Name => "Magnet Attract Skill";
        public override string Acronym => "MA";
        public override LocalisableString Description => "The fruits will be attracted by the catcher.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Fun;

        public static List<PalpableCatchHitObject> ListPalpableCatchableObject = new List<PalpableCatchHitObject>();

        public static bool Trigger = false!;

        public static int Index = 0;

        [SettingSource("Magnetic Power", "The power that the catcher will use to attract fruits.")]
        public Bindable<int> MagneticPower { get; } = new BindableInt(5)
        {
            MinValue = 1,
            MaxValue = 10
        };

        public override Type[] IncompatibleMods => new[] { typeof(CatchModCatchTheMania), typeof(CatchModRelax), typeof(CatchModTeleportSkill) };

        public override string SettingDescription
        {
            get
            {
                string magneticPower_string = MagneticPower.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    magneticPower_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.Catcher.CanAttract = true;
        }

        public void Update(Playfield playfield)
        {
            //TODO: MAKE THIS WORK

            if (Index < ListPalpableCatchableObject.Count)
            {
                PalpableCatchHitObject CurrentObject = ListPalpableCatchableObject[Index];
                CurrentObject.OriginalX = ((CatchPlayfield)playfield).Catcher.X;
                //if ([something]) Index++;
            }
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            ListPalpableCatchableObject.Clear();

            Index = 0;

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
