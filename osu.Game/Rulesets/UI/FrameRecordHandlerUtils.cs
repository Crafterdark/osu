// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.UI
{
    public class FrameRecordHandlerUtils
    {
        public static bool IsRecordHandlerValidForJudgement(FrameRecordHandler recordHandler)
        {
            switch (recordHandler)
            {
                default:
                    return false;
                case FrameRecordHandler.Judgement:
                case FrameRecordHandler.LegacyUpdateJudgement:
                case FrameRecordHandler.LegacyInputJudgement:
                    return true;
            }
        }

        public static bool IsRecordHandlerValidForInput(FrameRecordHandler recordHandler)
        {
            switch (recordHandler)
            {
                default:
                    return false;
                case FrameRecordHandler.Input:
                case FrameRecordHandler.LegacyInputJudgement:
                    return true;
            }
        }
    }
}
