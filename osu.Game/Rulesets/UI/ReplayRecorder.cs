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

        private readonly List<T> pressedActions = new List<T>();

        private InputManager inputManager;

        public int RecordFrameRate = 60;

        public bool HasJudgement;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        protected ReplayRecorder(Score target)
        {
            this.target = target;

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
            if (HasJudgement)
                recordFrame(true, TypeCaller.Update);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            recordFrame(false, TypeCaller.OnMouseMove);
            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<T> e)
        {
            pressedActions.Add(e.Action);
            recordFrame(true, TypeCaller.OnPressed);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<T> e)
        {
            pressedActions.Remove(e.Action);
            recordFrame(true, TypeCaller.OnReleased);
        }

        private void recordFrame(bool important, TypeCaller typeCaller)
        {
            var last = target.Replay.Frames.LastOrDefault();

            if (!important && last != null && Time.Current - last.Time < (1000d / RecordFrameRate))
                return;

            var position = ScreenSpaceToGamefield?.Invoke(inputManager.CurrentState.Mouse.Position) ?? inputManager.CurrentState.Mouse.Position;

            var frame = HandleFrame(position, pressedActions, last, typeCaller);

            if (frame != null)
            {
                target.Replay.Frames.Add(frame);

                spectatorClient?.HandleFrame(frame);
            }
        }

        protected abstract ReplayFrame HandleFrame(Vector2 mousePosition, List<T> actions, ReplayFrame previousFrame, TypeCaller typeCaller);
    }

    public abstract partial class ReplayRecorder : Component
    {
        public Func<Vector2, Vector2> ScreenSpaceToGamefield;
    }
    public enum TypeCaller
    {
        OnPressed,
        OnReleased,
        OnMouseMove,
        Update
    }
}
