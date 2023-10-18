// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
//using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.Debug_Mods
{
    public class CatchModPile : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        //Idea from Mod Rework Group (Not from me)
        public override string Name => "Pile";

        public override string Acronym => "PL";

        public override LocalisableString Description => "No fruit will drop from the plate... unless you miss!";

        public override double ScoreMultiplier => 1.03;

        public override ModType Type => ModType.Fun;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden) };

        [SettingSource("Stacking Seed", "Use a custom seed for the stacking of the fruit pile", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Seed { get; } = new Bindable<int?>();
        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchRuleset = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchRuleset.Catcher.CatchFruitPile = true;
            if (Seed.Value != null)
                catchRuleset.Catcher.CatchFruitRandomPile = new Random((int)Seed.Value);
            else
                catchRuleset.Catcher.CatchFruitRandomPile = new Random(drawableCatchRuleset.Beatmap.BeatmapInfo.OnlineID);

            //Logger.Log("Beatmap Seed:" + Seed.Value);
            //Logger.Log("Beatmap Seed (online ID):" + drawableCatchRuleset.Beatmap.BeatmapInfo.OnlineID);
        }



    }
}
