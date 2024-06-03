// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Catch.Beatmaps.HyperDashGeneration
{
    public class HyperDashGenerator
    {
        /// <summary>
        /// Main generation mode for hyperdashes.
        /// </summary>
        public HyperDashGeneratorMode Mode;

        /// <summary>
        /// Extra generation options for hyperdashes.
        /// </summary>
        public List<HyperDashGeneratorOptions> Options;

        public HyperDashGenerator(HyperDashGeneratorMode mode, List<HyperDashGeneratorOptions> options)
        {
            Mode = mode;
            Options = options;
        }
    }
}
