// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModWrench : Mod
    {
        public override string Name => "Wrench";
        public override string Acronym => "WH";
        public override LocalisableString Description => "Adjust gameplay features under specific conditions.";
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => FontAwesome.Solid.Wrench;
        public override double ScoreMultiplier => 1;

        [SettingSource("Exact beat length value", "Removes the lower and upper bound during beat length retrieval to support specific beatmaps. (mostly Aspire)")]
        public Bindable<bool> BeatmapTimingPointBeatLengthUnbounded { get; } = new BindableBool(true);
    }
}
