// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.UI
{
    public abstract partial class ReplayRecorder<T> : ReplayRecorder, IKeyBindingHandler<T>
        where T : struct
    {
        private readonly Score target;

        private readonly Playfield playfield;

        private readonly List<T> pressedActions = new List<T>();

        private InputManager inputManager;

        public int RecordFrameRate = 60;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        protected ReplayRecorder(Score target, Playfield playfield)
        {
            this.target = target;

            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;

            Depth = float.MinValue;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();

            if (playfield.HasJudgement)
                recordFrame(true, FrameRecordHandler.Judgement);

            recordFrame(false, FrameRecordHandler.Update);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            recordFrame(false, FrameRecordHandler.Mouse);
            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<T> e)
        {
            pressedActions.Add(e.Action);
            recordFrame(true, FrameRecordHandler.Input);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<T> e)
        {
            pressedActions.Remove(e.Action);
            recordFrame(true, FrameRecordHandler.Input);
        }

        private void recordFrame(bool important, FrameRecordHandler recordHandler)
        {
            var last = GetLastFrameRecordHandler(recordHandler, target.Replay.Frames);

            if (!important && last != null && Time.Current - last.Time < (1000d / RecordFrameRate))
                return;

            var position = ScreenSpaceToGamefield?.Invoke(inputManager.CurrentState.Mouse.Position) ?? inputManager.CurrentState.Mouse.Position;

            var frame = HandleFrame(position, pressedActions, last, recordHandler);

            //Reset judgement status for the next frame
            if (recordHandler == FrameRecordHandler.Judgement)
                playfield.HasJudgement = false;

            if (frame != null)
            {
                target.Replay.Frames.Add(frame);

                spectatorClient?.HandleFrame(frame);
            }
        }

        protected virtual ReplayFrame GetLastFrameRecordHandler(FrameRecordHandler recordHandler, List<ReplayFrame> replayFrames) => replayFrames.LastOrDefault();
        protected abstract ReplayFrame HandleFrame(Vector2 mousePosition, List<T> actions, ReplayFrame previousFrame, FrameRecordHandler recordHandler);
    }

    public abstract partial class ReplayRecorder : Component
    {
        public Func<Vector2, Vector2> ScreenSpaceToGamefield;
    }
}
