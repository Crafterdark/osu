// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
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
        public Catcher Twin
        {
            get => twin;
            set => twinContainer.Child = twin = value;
        }

        public bool TwinCatchersApplies { get; set; } = false!;

        private readonly Container<Catcher> catcherContainer;

        private readonly Container<Catcher> twinContainer;

        private readonly CatchComboDisplay comboDisplay;

        private readonly CatchComboDisplay? comboDisplayTwin;

        private readonly CatcherTrailDisplay catcherTrails;

        private readonly CatcherTrailDisplay? twinTrails;

        private Catcher catcher = null!;

        private Catcher twin = null!;

        /// <summary>
        /// <c>-1</c> when only left button is pressed.
        /// <c>1</c> when only right button is pressed.
        /// <c>0</c> when none or both left and right buttons are pressed.
        /// </summary>
        private int currentDirection;

        private int currentDirectionTwin;

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
                twinContainer = new Container<Catcher>{RelativeSizeAxes = Axes.Both },
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
            if (TwinCatchersApplies)
            {
                Children.Append(twinTrails = new CatcherTrailDisplay());
                Children.Append(comboDisplayTwin = new CatchComboDisplay
                {
                    RelativeSizeAxes = Axes.None,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Bottom = 350f },
                    X = CatchPlayfield.CENTER_X
                });
            }
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
                (float)(Catcher.X + Catcher.Speed * currentDirection * Clock.ElapsedFrameTime), Catcher);

            if (TwinCatchersApplies) SetCatcherPosition(
                replayState?.CatcherX ??
                (float)(Twin.X + Twin.Speed * currentDirectionTwin * Clock.ElapsedFrameTime), Twin);

        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            comboDisplay.X = Catcher.X;

            if (Time.Elapsed <= 0)
            {
                // This is probably a wrong value, but currently the true value is not recorded.
                // Setting `true` will prevent generation of false-positive after-images (with more false-negatives).
                lastHyperDashState = true;
                return;
            }

            if (!lastHyperDashState && Catcher.HyperDashing)
                displayCatcherTrail(CatcherTrailAnimation.HyperDashAfterImage, Catcher, catcherTrails);

            if (Catcher.Dashing || Catcher.HyperDashing)
            {
                double generationInterval = Catcher.HyperDashing ? 25 : 50;

                if (Time.Current - catcherTrails.LastDashTrailTime >= generationInterval)
                    displayCatcherTrail(Catcher.HyperDashing ? CatcherTrailAnimation.HyperDashing : CatcherTrailAnimation.Dashing, Catcher, catcherTrails);
            }

            lastHyperDashState = Catcher.HyperDashing;
        }

        public void SetCatcherPosition(float x, Catcher currCatcher)
        {
            float lastPosition = currCatcher.X;
            float newPosition = Math.Clamp(x, 0, CatchPlayfield.WIDTH);
            if (TwinCatchersApplies)
            {
                //Replaces newPosition limits, when the Twin Catchers mod is applied
                if (currCatcher == catcher) newPosition = Math.Clamp(x, 0, (CatchPlayfield.WIDTH / 2) - (catcher.CatchWidth / 2));
                else if (currCatcher == twin) newPosition = Math.Clamp(x, (CatchPlayfield.WIDTH / 2) + (twin.CatchWidth / 2), CatchPlayfield.WIDTH);
            }
            currCatcher.X = newPosition;

            if (lastPosition < newPosition)
                currCatcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                currCatcher.VisualDirection = Direction.Left;
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
            }

            if (TwinCatchersApplies)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeftTwin:
                        currentDirectionTwin--;
                        return true;

                    case CatchAction.MoveRightTwin:
                        currentDirectionTwin++;
                        return true;

                    case CatchAction.DashTwin:
                        Twin.Dashing = true;
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
                    currentDirection++;
                    break;

                case CatchAction.MoveRight:
                    currentDirection--;
                    break;

                case CatchAction.Dash:
                    Catcher.Dashing = false;
                    break;
            }
            if (TwinCatchersApplies)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeftTwin:
                        currentDirectionTwin++;
                        break;

                    case CatchAction.MoveRightTwin:
                        currentDirectionTwin--;
                        break;

                    case CatchAction.DashTwin:
                        Twin.Dashing = false;
                        break;
                }
            }
        }

        //Replacement of CheckIfWeCanCatch from CatchPlayfield, to be more generic
        public bool CheckIfWeCanCatch(CatchHitObject obj)
        {
            if (TwinCatchersApplies) return Catcher.CanCatch(obj) || Twin.CanCatch(obj);
            return Catcher.CanCatch(obj);

        }

        private void displayCatcherTrail(CatcherTrailAnimation animation, Catcher currCatcher, CatcherTrailDisplay trails) => trails.Add(new CatcherTrailEntry(Time.Current, currCatcher.CurrentState, currCatcher.X, currCatcher.BodyScale, animation));
    }
}
