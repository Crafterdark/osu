// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMirror : Mod
    {
        public override string Name => "Mirror";
        public override string Acronym => "MR";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;
        public override LocalisableString Description => "Flip objects on the chosen axes.";
        public abstract Bindable<MirrorType> Reflection { get; }
    }

    public enum MirrorType
    {
        Horizontal,
        Vertical,
        Both
    }
}
