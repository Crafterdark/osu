// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Mods.DebugMods;
using osu.Game.Rulesets.Catch.Mods.DebugMods.Utility;
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

        public bool DisableMainDash { get; set; }
        public bool DisableMainMovement { get; set; }
        public bool ForceVisualDirectionChanges { get; set; }
        public bool IsAutopilot { get; set; }
        public bool IsFirstPerson { get; set; }
        public bool IsDirectionalDash { get; set; }
        public bool IsUnlockedDirection { get; set; }
        public bool SetPressedForLeft { get; set; }
        public bool SetPressedForRight { get; set; }

        public int SetPressedFirst = 0;
        public float FirstPersonPosition { get; set; } = CatchPlayfield.CENTER_X;

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

        /// <summary>
        /// <c>-1</c> when only left movement button is pressed.
        /// <c>1</c> when only right movement button is pressed.
        /// <c>0</c> when none or both left and right dash buttons are pressed.
        /// </summary>
        private int currentDashDirection;

        /// <summary>
        /// <c>-1</c> when only left autopilot button is pressed.
        /// <c>1</c> when only right autopilot button is pressed.
        /// <c>0</c> when none or both left and right dash autopilot buttons are pressed.
        /// </summary>
        public int CurrentAutopilotDirection { get; set; }

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

            //Visual direction potential changes
            if (ForceVisualDirectionChanges)
            {
                int localCurrentDirection = IsAutopilot ? CurrentAutopilotDirection : currentDirection;
                //Doesn't do anything if currentDirection is 0
                if (localCurrentDirection > 0)
                    Catcher.VisualDirection = Direction.Right;
                else if (localCurrentDirection < 0)
                    Catcher.VisualDirection = Direction.Left;

                if (IsFirstPerson)
                {
                    FirstPersonPosition += (float)Catcher.Speed * localCurrentDirection * (float)Clock.ElapsedFrameTime;
                    float minPlayfieldWidth = CatchUtilityForMods.GetMinPlayfieldWidth(2 * CatchModFirstPerson.COMPRESSION_LEVEL);
                    float maxPlayfieldWidth = CatchUtilityForMods.GetMaxPlayfieldWidth(2 * CatchModFirstPerson.COMPRESSION_LEVEL);
                    FirstPersonPosition = Math.Clamp(FirstPersonPosition, minPlayfieldWidth, maxPlayfieldWidth);
                }
            }

            //Current direction potential changes
            int localDirection = 0;

            if (IsAutopilot)
                localDirection = CurrentAutopilotDirection;
            else if (!DisableMainMovement)
                localDirection = currentDirection;

            if (!IsFirstPerson)
                SetCatcherPosition(
                     replayState?.CatcherX ??
                     (float)(Catcher.X + Catcher.Speed * localDirection * Clock.ElapsedFrameTime));

            //Current dashing potential changes
            if (IsDirectionalDash)
                Catcher.Dashing = (currentDashDirection != 0 && currentDashDirection == localDirection) ? true : false;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            comboDisplay.X = Catcher.X;

            if ((Clock as IGameplayClock)?.IsRewinding == true)
            {
                // This is probably a wrong value, but currently the true value is not recorded.
                // Setting `true` will prevent generation of false-positive after-images (with more false-negatives).
                lastHyperDashState = true;
                return;
            }

            if (!lastHyperDashState && Catcher.HyperDashing)
                displayCatcherTrail(CatcherTrailAnimation.HyperDashAfterImage);

            if (Catcher.Dashing || Catcher.HyperDashing)
            {
                double generationInterval = Catcher.HyperDashing ? 25 : 50;

                if (Time.Current - catcherTrails.LastDashTrailTime >= generationInterval)
                    displayCatcherTrail(Catcher.HyperDashing ? CatcherTrailAnimation.HyperDashing : CatcherTrailAnimation.Dashing);
            }

            lastHyperDashState = Catcher.HyperDashing;
        }

        public void SetCatcherPosition(float x)
        {
            float lastPosition = Catcher.X;
            float newPosition = Math.Clamp(x, 0, CatchPlayfield.WIDTH);

            Catcher.X = newPosition;

            if (lastPosition < newPosition)
                Catcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                Catcher.VisualDirection = Direction.Left;
        }

        public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
        {
            switch (e.Action)
            {
                case CatchAction.MoveLeft:
                    if (processUnlockedDirectionMod(true, CatchAction.MoveLeft))
                        return true;

                    currentDirection--;
                    return true;

                case CatchAction.MoveRight:
                    if (processUnlockedDirectionMod(true, CatchAction.MoveRight))
                        return true;

                    currentDirection++;
                    return true;

                case CatchAction.Dash:
                    if (DisableMainDash)
                        return true;

                    Catcher.Dashing = true;
                    return true;
            }

            if (IsDirectionalDash)
            {
                switch (e.Action)
                {
                    case CatchAction.DashLeft:
                        processDirectionalDashMod(true, currentDirection, CatchAction.DashLeft);

                        return true;

                    case CatchAction.DashRight:
                        processDirectionalDashMod(true, currentDirection, CatchAction.DashRight);

                        return true;
                }
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
        {
            switch (e.Action)
            {
                case CatchAction.MoveLeft:
                    if (processUnlockedDirectionMod(false, CatchAction.MoveLeft))
                        break;

                    currentDirection++;
                    break;

                case CatchAction.MoveRight:
                    if (processUnlockedDirectionMod(false, CatchAction.MoveRight))
                        break;

                    currentDirection--;
                    break;

                case CatchAction.Dash:
                    if (DisableMainDash)
                        break;

                    Catcher.Dashing = false;
                    break;
            }

            if (IsDirectionalDash)
            {
                switch (e.Action)
                {
                    case CatchAction.DashLeft:
                        processDirectionalDashMod(false, currentDirection, CatchAction.DashLeft);

                        break;

                    case CatchAction.DashRight:
                        processDirectionalDashMod(false, currentDirection, CatchAction.DashRight);

                        break;
                }
            }

        }

        private void displayCatcherTrail(CatcherTrailAnimation animation) => catcherTrails.Add(new CatcherTrailEntry(Time.Current, Catcher.CurrentState, Catcher.X, Catcher.BodyScale, animation));

        private bool processUnlockedDirectionMod(bool isPressed, CatchAction moveDirection)
        {
            if (!IsUnlockedDirection)
                return false;

            if (isPressed)
            {
                if (moveDirection == CatchAction.MoveLeft)
                {
                    SetPressedForLeft = true;

                    if (!SetPressedForRight)
                        currentDirection = -1;

                    return true;
                }

                if (moveDirection == CatchAction.MoveRight)
                {
                    SetPressedForRight = true;

                    if (!SetPressedForLeft)
                        currentDirection = 1;

                    return true;
                }
            }

            else
            {
                if (moveDirection == CatchAction.MoveLeft)
                {
                    SetPressedForLeft = false;
                    currentDirection = 0;

                    return true;
                }

                if (moveDirection == CatchAction.MoveRight)
                {
                    SetPressedForRight = false;
                    currentDirection = 0;

                    return true;
                }
            }

            return false;
        }

        private void processDirectionalDashMod(bool isPressed, int moveDirection, CatchAction dashDirection)
        {
            if (!IsDirectionalDash)
                return;

            if (isPressed)
            {
                if (dashDirection == CatchAction.DashLeft)
                {
                    currentDashDirection--;
                }

                if (dashDirection == CatchAction.DashRight)
                {
                    currentDashDirection++;
                }
            }

            else
            {
                if (dashDirection == CatchAction.DashLeft)
                {
                    currentDashDirection++;
                }

                if (dashDirection == CatchAction.DashRight)
                {
                    currentDashDirection--;
                }
            }
        }
    }
}
