// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Catch.UI
{
    public sealed class CatcherExtra
    {
        public static CatcherExtra INSTANCE = new CatcherExtra(false);

        public bool IsApplied;
        public bool CatchExtend { get; set; }
        private CatcherExtra(bool applies)
        {
            IsApplied = applies;
        }

        public void ResetInstance()
        {
            INSTANCE = new CatcherExtra(false);
        }
    }
}
