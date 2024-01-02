// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
            get => mainCatcher;
            set => mainCatcherContainer.Child = mainCatcher = value;
        }

        public List<Catcher> CatcherList = new List<Catcher>();

        public bool IsPlayfieldUnique = true;

        public List<float> CatcherNewRanges = new List<float>();

        public List<float> TwinNewRanges = new List<float>();

        private readonly Container<Catcher> mainCatcherContainer;

        private readonly CatchComboDisplay comboDisplay;

        private readonly CatcherTrailDisplay catcherTrails;

        private Catcher mainCatcher = null!;

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
                mainCatcherContainer = new Container<Catcher> { RelativeSizeAxes = Axes.Both },
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
            foreach (Catcher catcher in CatcherList)
            {
                catcher.OnNewResult(hitObject, result);
                comboDisplay.OnNewResult(hitObject, result);
            }
        }

        public void OnRevertResult(JudgementResult result)
        {
            foreach (Catcher catcher in CatcherList)
            {
                comboDisplay.OnRevertResult(result);
                catcher.OnRevertResult(result);
            }
        }

        protected override void Update()
        {
            base.Update();

            var replayState = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            foreach (Catcher catcher in CatcherList)
            {
                SetCatcherPosition(
                replayState?.CatcherX ??
                (float)(catcher.X + catcher.Speed * currentDirection * Clock.ElapsedFrameTime), catcher);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            foreach (Catcher catcher in CatcherList)
            {
                comboDisplay.X = catcher.X;

                if ((Clock as IGameplayClock)?.IsRewinding == true)
                {
                    // This is probably a wrong value, but currently the true value is not recorded.
                    // Setting `true` will prevent generation of false-positive after-images (with more false-negatives).
                    lastHyperDashState = true;
                    return;
                }

                if (!lastHyperDashState && catcher.HyperDashing)
                    displayCatcherTrail(CatcherTrailAnimation.HyperDashAfterImage, catcher);

                if (catcher.Dashing || catcher.HyperDashing)
                {
                    double generationInterval = catcher.HyperDashing ? 25 : 50;

                    if (Time.Current - catcherTrails.LastDashTrailTime >= generationInterval)
                        displayCatcherTrail(catcher.HyperDashing ? CatcherTrailAnimation.HyperDashing : CatcherTrailAnimation.Dashing, catcher);
                }

                lastHyperDashState = catcher.HyperDashing;
            }
        }

        public void SetCatcherPosition(float x, Catcher catcher)
        {
            float lastPosition = catcher.X;

            float newPosition;

            if (IsPlayfieldUnique)
                newPosition = Math.Clamp(x, 0, CatchPlayfield.WIDTH);
            else
            {
                newPosition = Math.Clamp(x, catcher.IsTwin ? TwinNewRanges.ElementAt(0) : CatcherNewRanges.ElementAt(0), catcher.IsTwin ? TwinNewRanges.ElementAt(1) : CatcherNewRanges.ElementAt(1));
            }

            catcher.X = newPosition;

            if (lastPosition < newPosition)
                catcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                catcher.VisualDirection = Direction.Left;
        }

        public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
        {
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

                case CatchAction.MoveLeftTwin:
                    currentDirection--;
                    return true;

                case CatchAction.MoveRightTwin:
                    currentDirection++;
                    return true;

                case CatchAction.DashTwin:
                    Catcher.Dashing = true;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
        {
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

                case CatchAction.MoveLeftTwin:
                    currentDirection++;
                    break;

                case CatchAction.MoveRightTwin:
                    currentDirection--;
                    break;

                case CatchAction.DashTwin:
                    Catcher.Dashing = false;
                    break;
            }
        }

        private void displayCatcherTrail(CatcherTrailAnimation animation, Catcher catcher) => catcherTrails.Add(new CatcherTrailEntry(Time.Current, catcher.CurrentState, catcher.X, catcher.BodyScale, animation));
    }
}
