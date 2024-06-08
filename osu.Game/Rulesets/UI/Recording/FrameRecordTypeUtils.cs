// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.UI
{
    public class FrameRecordTypeUtils
    {
        public static bool IsFrameRecordTypeValidForJudgement(FrameRecordType frameRecordType)
        {
            switch (frameRecordType)
            {
                default:
                    return false;
                case FrameRecordType.Judgement:
                case FrameRecordType.LegacyUpdateOrJudgement:
                case FrameRecordType.LegacyInputOrJudgement:
                    return true;
            }
        }

        public static bool IsFrameRecordTypeValidForInput(FrameRecordType frameRecordType)
        {
            switch (frameRecordType)
            {
                default:
                    return false;
                case FrameRecordType.Input:
                case FrameRecordType.LegacyInputOrJudgement:
                    return true;
            }
        }
    }
}
