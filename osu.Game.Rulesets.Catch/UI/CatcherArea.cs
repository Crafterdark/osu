// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// The horizontal band at the bottom of the playfield the catcher is moving on.
    /// It holds a <see cref="Catcher"/> as a child and translates input to the catcher movement.
    /// It also holds a combo display that is above the catcher, and judgment results are translated to the catcher and the combo display.
    /// </summary>
    public partial class CatcherArea : Container, IKeyBindingHandler<CatchAction>
    {
        public Catcher Catcher
        {
            get => catcher;
            set => catcherContainer.Child = catcher = value;
        }

        public List<CatchAction> InvalidCatchActionList = new List<CatchAction>();

        public BindableBool HideComboDisplay { get; set; } = new BindableBool();

        private readonly Container<Catcher> catcherContainer;

        private readonly CatchComboDisplay comboDisplay;

        private readonly CatcherTrailDisplay catcherTrails;

        private Catcher catcher = null!;

        /// <summary>
        /// <c>-1</c> when only left button is pressed.
        /// <c>1</c> when only right button is pressed.
        /// <c>0</c> when none or both left and right buttons are pressed.
        /// </summary>
        private int currentDirection;

        // TODO: support replay rewind
        private bool lastHyperDashState;

        /// <remarks>
        /// <see cref="Catcher"/> must be set before loading.
        /// </remarks>
        public CatcherArea()
        {
            Size = new Vector2(CatchPlayfield.WIDTH, Catcher.BASE_SIZE);
            Children = new Drawable[]
            {
                catcherContainer = new Container<Catcher> { RelativeSizeAxes = Axes.Both },
                catcherTrails = new CatcherTrailDisplay(),
                comboDisplay = new CatchComboDisplay
                {
                    RelativeSizeAxes = Axes.None,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Bottom = 350f },
                    X = CatchPlayfield.CENTER_X
                }
            };
        }

        public void OnNewResult(DrawableCatchHitObject hitObject, JudgementResult result)
        {
            Catcher.OnNewResult(hitObject, result);
            comboDisplay.OnNewResult(hitObject, result);
        }

        public void OnRevertResult(JudgementResult result)
        {
            comboDisplay.OnRevertResult(result);
            Catcher.OnRevertResult(result);
        }

        protected override void Update()
        {
            base.Update();

            var replayState = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            SetCatcherPosition(
                replayState?.CatcherX ??
                (float)(Catcher.X + Catcher.Speed * currentDirection * Clock.ElapsedFrameTime));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            comboDisplay.X = Catcher.X;

            if (HideComboDisplay.Value)
                comboDisplay.Alpha = 0;
            else if (Catcher.IsGhost)
                comboDisplay.Alpha = Catcher.Alpha;

            if ((Clock as IGameplayClock)?.IsRewinding == true)
            {
                // This is probably a wrong value, but currently the true value is not recorded.
                // Setting `true` will prevent generation of false-positive after-images (with more false-negatives).
                lastHyperDashState = true;
                return;
            }

            if (!lastHyperDashState && Catcher.HyperDashing && Catcher.ShowHyperDashTrail && !Catcher.IsGhost)
                displayCatcherTrail(CatcherTrailAnimation.HyperDashAfterImage);

            if (Catcher.Dashing || Catcher.HyperDashing)
            {
                double generationInterval = Catcher.HyperDashing ? 25 : 50;

                if (Time.Current - catcherTrails.LastDashTrailTime >= generationInterval && !Catcher.IsGhost)
                    displayCatcherTrail(Catcher.HyperDashing && Catcher.ShowHyperDashTrail ? CatcherTrailAnimation.HyperDashing : CatcherTrailAnimation.Dashing);
            }

            lastHyperDashState = Catcher.HyperDashing;
        }

        public void SetCatcherPosition(float x)
        {
            float lastPosition = Catcher.X;
            float newPosition = Math.Clamp(x, Catcher.MinX + Catcher.MinOffsetX, Catcher.MaxX + Catcher.MaxOffsetX);

            Catcher.X = newPosition;

            if (lastPosition < newPosition)
                Catcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                Catcher.VisualDirection = Direction.Left;
        }

        public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
        {
            if (!IsValidCatchAction(e))
                return false;

            switch (e.Action)
            {
                case CatchAction.MoveLeft:
                    currentDirection--;
                    return true;

                case CatchAction.MoveRight:
                    currentDirection++;
                    return true;

                case CatchAction.Dash:
                    Catcher.Dashing = true;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
        {
            if (!IsValidCatchAction(e))
                return;

            switch (e.Action)
            {
                case CatchAction.MoveLeft:
                    currentDirection++;
                    break;

                case CatchAction.MoveRight:
                    currentDirection--;
                    break;

                case CatchAction.Dash:
                    Catcher.Dashing = false;
                    break;
            }
        }

        /// <summary>
        /// Check the validity of all the <see cref="CatchAction"/>.
        /// </summary>
        public bool IsValidCatchAction(KeyBindingEvent<CatchAction> keyEvent)
        {
            foreach (var invalidAction in InvalidCatchActionList)
            {
                if (invalidAction == keyEvent.Action)
                    return false;
            }

            return true;
        }

        private void displayCatcherTrail(CatcherTrailAnimation animation) => catcherTrails.Add(new CatcherTrailEntry(Time.Current, Catcher.CurrentState, Catcher.X, Catcher.BodyScale, animation));
    }
}
