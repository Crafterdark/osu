// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public class ModClassicLegacy : ModClassic
    {
        public override string Name => "Classic (Legacy)";

        public override string Acronym => "CL*";

        public override LocalisableString Description => "Automatically applied to plays from legacy versions.";

        public override ModType Type => ModType.System;

        public override bool ValidForMultiplayer => false;

        public override bool ValidForMultiplayerAsFreeMod => false;
    }
}
