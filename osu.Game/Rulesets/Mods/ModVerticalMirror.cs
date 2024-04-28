// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public class ModVerticalMirror : ModMirror
    {
        public override LocalisableString Description => "Flip objects on the vertical axis.";
        public override Bindable<MirrorType> Reflection { get; } = null!;
    }
}
