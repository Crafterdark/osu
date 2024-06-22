// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.ReplayAnalysis;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Replays
{
    public partial class CatchReplayAnalysisSettings : ReplayAnalysisSettings
    {
        private List<ReplayFrame> allFrames = new List<ReplayFrame>();

        private readonly MasterGameplayClockContainer gameClock;

        private CatchReplayFrame? currFrame = null!;

        private CatchReplayFrame? nextFrame = null!;

        private List<TextLabel> textLabelList = new List<TextLabel>();

        private int lastIndexId = -1;

        private readonly Catcher catcher;

        public struct TextLabel
        {
            public IdSpriteText SpriteText;

            public TextLabel(IdSpriteText spriteText)
            {
                SpriteText = spriteText;
            }
        }

        public CatchReplayAnalysisSettings(MasterGameplayClockContainer clock, List<ReplayFrame> frames, CatchPlayfield catchPlayfield) : base(clock, frames)
        {
            allFrames.AddRange(frames);

            gameClock = clock;

            catcher = catchPlayfield.Catcher;
        }

        protected override void Update()
        {
            base.Update();

            if (TextFieldFillFlowContainer.Count == 0)
            {
                AddEntryToLabelList("currFrame", "title", "Current Frame", isTitle: true);
                AddEntryToLabelList("currFrame", "id", "ID", hasValue: true);
                AddEntryToLabelList("currFrame", "type", "Type", hasValue: true);
                AddEntryToLabelList("currFrame", "time", "Time", hasValue: true);
                AddEntryToLabelList("nextFrame", "title", "Next Frame", isTitle: true);
                AddEntryToLabelList("nextFrame", "id", "ID", hasValue: true);
                AddEntryToLabelList("nextFrame", "type", "Type", hasValue: true);
                AddEntryToLabelList("nextFrame", "time", "Time", hasValue: true);
                AddEntryToLabelList("catcher", "title", "Catcher", isTitle: true);
                AddEntryToLabelList("catcher", "startX", "Start X", hasValue: true);
                AddEntryToLabelList("catcher", "endX", "End X", hasValue: true);
                AddEntryToLabelList("catcher", "lerpX", "X (Lerp)", hasValue: true);
                AddEntryToLabelList("catcher", "dash", "Dash", hasValue: true);
                AddEntryToLabelList("catcher", "hyperDash", "HyperDash", hasValue: true);
                AddEntryToLabelList("catcher", "expectedSpeed", "Expected Speed", hasValue: true);
                AddEntryToLabelList("catcher", "errorOnSpeed", "Error Rate on Speed", hasValue: true);

                foreach (var textLabel in textLabelList)
                {
                    TextFieldFillFlowContainer.Add(textLabel.SpriteText);
                }
            }

            var currFrameUpdate = (CatchReplayFrame?)allFrames.Where(f => f.Time <= gameClock.CurrentTime).LastOrDefault();

            if (currFrameUpdate != null)
            {
                currFrame = currFrameUpdate;

                lastIndexId = allFrames.IndexOf(currFrame);

                CatchReplayFrame? nextFrameUpdate = null!;

                for (int i = lastIndexId + 1; lastIndexId < allFrames.Count - 1; i++)
                {
                    nextFrameUpdate = (CatchReplayFrame?)allFrames[i];

                    if (nextFrameUpdate == null)
                        break;
                    else if (currFrameUpdate.Time < nextFrameUpdate.Time && nextFrameUpdate.FrameRecordType == FrameRecordType.Update || nextFrameUpdate.FrameRecordType == FrameRecordType.Judgement)
                        break;
                }

                if (nextFrameUpdate != null)
                    nextFrame = nextFrameUpdate;
            }

            if (currFrame == null || nextFrame == null)
                return;

            string currId = $"{allFrames.IndexOf(currFrame)}";
            string currType = getFrameRecordTypeText(currFrame.FrameRecordType);
            string currTime = $"{currFrame.Time}";
            string nextId = $"{allFrames.IndexOf(nextFrame)}";
            string nextType = getFrameRecordTypeText(nextFrame.FrameRecordType);
            string nextTime = $"{nextFrame.Time}";

            string catcherStartX = $"{currFrame.Position}";
            string catcherEndX = $"{nextFrame.Position}";
            string catcherLerpX = $"{Interpolation.ValueAt(gameClock.CurrentTime, currFrame.Position, nextFrame.Position, currFrame.Time, nextFrame.Time)}";
            string catcherDash = $"{currFrame.Dashing}";
            string catcherHyperDash = $"{catcher.HyperDashing}";

            double catcherFrameExpectedSpeed = getExpectedSpeed(Math.Abs(currFrame.Position - nextFrame.Position), currFrame.Dashing, catcher.HyperDashing);
            double catcherFrameRealSpeed = Math.Abs(currFrame.Position - nextFrame.Position) / (nextFrame.Time - currFrame.Time);

            string catcherExpectedSpeed = $"{catcherFrameExpectedSpeed}";
            string catcherErrorOnSpeed = $"{catcherFrameRealSpeed - catcherFrameExpectedSpeed}";

            SetNewInfoToEntry("currFrame", "id", currId);
            SetNewInfoToEntry("currFrame", "type", currType);
            SetNewInfoToEntry("currFrame", "time", currTime);

            SetNewInfoToEntry("nextFrame", "id", nextId);
            SetNewInfoToEntry("nextFrame", "type", nextType);
            SetNewInfoToEntry("nextFrame", "time", nextTime);

            SetNewInfoToEntry("catcher", "startX", catcherStartX);
            SetNewInfoToEntry("catcher", "endX", catcherEndX);
            SetNewInfoToEntry("catcher", "lerpX", catcherLerpX);
            SetNewInfoToEntry("catcher", "dash", catcherDash);
            SetNewInfoToEntry("catcher", "hyperDash", catcherHyperDash);

            if (nextFrame.Time != currFrame.Time)
            {
                SetNewInfoToEntry("catcher", "expectedSpeed", catcherExpectedSpeed);
                SetNewInfoToEntry("catcher", "errorOnSpeed", catcherErrorOnSpeed);
            }
        }

        private double getExpectedSpeed(float position, bool dash, bool hyperDash)
        {
            if (position == 0)
                return 0; // No movement
            else if (!dash && !hyperDash)
                return Catcher.BASE_WALK_SPEED; // Walk
            else if (dash && !hyperDash)
                return Catcher.BASE_DASH_SPEED; // Dash
            else if (!dash && hyperDash)
                return catcher.GetHyperDashModifier() * Catcher.BASE_WALK_SPEED; // HyperWalk
            else if (dash && hyperDash)
                return catcher.GetHyperDashModifier(); // HyperDash

            return -1; // Unknown
        }

        public void AddEntryToLabelList(string label, string subLabel, string text, bool isTitle = false, bool hasValue = false)
        {
            string headerToAdd = text;

            if (isTitle)
                headerToAdd = "[" + headerToAdd + "]";
            else if (hasValue)
                headerToAdd += ": ";

            var newIdSpriteText = new IdSpriteText(label, subLabel)
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Font = OsuFont.GetFont(weight: FontWeight.Bold),
                Margin = new MarginPadding { Right = 20 },
            };

            newIdSpriteText.AddHeader(headerToAdd);

            textLabelList.Add(new TextLabel(newIdSpriteText));
        }

        public void SetNewInfoToEntry(string label, string subLabel, string newInfo)
        {
            foreach (IdSpriteText idSpriteText in TextFieldFillFlowContainer)
            {
                if (idSpriteText.Label == label && idSpriteText.SubLabel == subLabel)
                {
                    idSpriteText.AddInfo(newInfo);
                    break;
                }
            }
        }

        private string getFrameRecordTypeText(FrameRecordType type)
        {
            switch (type)
            {
                case FrameRecordType.Update:
                    return "Update";
                case FrameRecordType.Mouse:
                    return "Mouse";
                case FrameRecordType.Input:
                    return "Input";
                case FrameRecordType.Judgement:
                    return "Judgement";
            }

            return "Unknown";
        }
    }
}
public partial class IdSpriteText : SpriteText
{
    public string Label;

    public string SubLabel;

    public string CurrentHeader = "";

    public string CurrentInfo = "";

    public Bindable<string> CurrentText = new Bindable<string>();

    public IdSpriteText(string label, string subLabel)
    {
        Label = label;
        SubLabel = subLabel;
        Shadow = true;
        Font = OsuFont.Default;
    }

    public void AddHeader(string header)
    {
        CurrentHeader = header;
        UpdateText();
    }

    public void AddInfo(string info)
    {
        CurrentInfo = info;
        UpdateText();
    }

    public void UpdateText() => CurrentText.Value = CurrentHeader + CurrentInfo;

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Current.BindTo(CurrentText);
    }
}
