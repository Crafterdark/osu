// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
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
        public Catcher Catcher { get; set; } = null!;
        public Catcher Twin { get; set; } = null!;

        public List<CatcherBundle> FinalCatcherBundleList = null!;

        public bool IsPlayfieldUnique = true;

        public List<float> CatcherNewRanges = new List<float>();

        public List<float> TwinNewRanges = new List<float>();

        /// <remarks>
        /// <see cref="Catcher"/> must be set before loading.
        /// </remarks>
        public CatcherArea(CatcherBundle mainCatcherBundle)
        {
            Catcher = mainCatcherBundle.Catcher;
            Size = new Vector2(CatchPlayfield.WIDTH, Catcher.BASE_SIZE);
            Add(Catcher);
            Add(mainCatcherBundle.CatcherTrailDisplay);
            Add(mainCatcherBundle.ComboDisplay);
            Add(mainCatcherBundle.DroppedObjectContainer);
        }

        public bool IsInPlayfield(float objectX, float minRange, float maxRange) => objectX >= minRange && objectX <= maxRange;

        public void OnNewResult(DrawableCatchHitObject hitObject, JudgementResult result)
        {
            foreach (CatcherBundle catcherBundle in FinalCatcherBundleList)
            {
                if (!IsPlayfieldUnique)
                {
                    bool shouldCheck = catcherBundle.Catcher.IsTwin ? IsInPlayfield(hitObject.HitObject.EffectiveX, TwinNewRanges.ElementAt(0), TwinNewRanges.ElementAt(1)) : IsInPlayfield(hitObject.HitObject.EffectiveX, CatcherNewRanges.ElementAt(0), CatcherNewRanges.ElementAt(1));
                    if (shouldCheck)
                        catcherBundle.Catcher.OnNewResult(hitObject, result);
                    if (catcherBundle.CanCatch)
                        catcherBundle.ComboDisplay.OnNewResult(hitObject, result);
                }
            }
        }

        public void OnRevertResult(JudgementResult result)
        {
            foreach (CatcherBundle catcherBundle in FinalCatcherBundleList)
            {
                catcherBundle.Catcher.OnRevertResult(result);
                catcherBundle.ComboDisplay.OnRevertResult(result);

            }
        }

        protected override void Update()
        {
            base.Update();

            var replayState = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            foreach (CatcherBundle catcherBundle in FinalCatcherBundleList)
            {
                SetCatcherPosition(
                replayState?.CatcherX ??
                (float)(catcherBundle.Catcher.X + catcherBundle.Catcher.Speed * catcherBundle.Catcher.CurrentDirection * Clock.ElapsedFrameTime), catcherBundle);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            foreach (CatcherBundle catcherBundle in FinalCatcherBundleList)
            {
                catcherBundle.ComboDisplay.X = catcherBundle.Catcher.X;

                if ((Clock as IGameplayClock)?.IsRewinding == true)
                {
                    // This is probably a wrong value, but currently the true value is not recorded.
                    // Setting `true` will prevent generation of false-positive after-images (with more false-negatives).
                    catcherBundle.Catcher.LastHyperDashState = true;
                    return;
                }

                if (!catcherBundle.Catcher.LastHyperDashState && catcherBundle.Catcher.HyperDashing)
                    displayCatcherTrail(CatcherTrailAnimation.HyperDashAfterImage, catcherBundle);

                if (catcherBundle.Catcher.Dashing || catcherBundle.Catcher.HyperDashing)
                {
                    double generationInterval = catcherBundle.Catcher.HyperDashing ? 25 : 50;

                    if (Time.Current - catcherBundle.CatcherTrailDisplay.LastDashTrailTime >= generationInterval)
                        displayCatcherTrail(catcherBundle.Catcher.HyperDashing ? CatcherTrailAnimation.HyperDashing : CatcherTrailAnimation.Dashing, catcherBundle);
                }

                catcherBundle.Catcher.LastHyperDashState = catcherBundle.Catcher.HyperDashing;
            }
        }

        public void SetCatcherPosition(float x, CatcherBundle catcherBundle)
        {
            float lastPosition = catcherBundle.Catcher.X;

            float newPosition;

            if (IsPlayfieldUnique)
                newPosition = Math.Clamp(x, 0, CatchPlayfield.WIDTH);
            else
            {
                newPosition = Math.Clamp(x, catcherBundle.Catcher.IsTwin ? TwinNewRanges.ElementAt(0) : CatcherNewRanges.ElementAt(0), catcherBundle.Catcher.IsTwin ? TwinNewRanges.ElementAt(1) : CatcherNewRanges.ElementAt(1));
            }

            catcherBundle.Catcher.X = newPosition;

            if (lastPosition < newPosition)
                catcherBundle.Catcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                catcherBundle.Catcher.VisualDirection = Direction.Left;
        }

        public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
        {
            switch (e.Action)
            {
                case CatchAction.MoveLeft:
                    Catcher.CurrentDirection--;
                    return true;

                case CatchAction.MoveRight:
                    Catcher.CurrentDirection++;
                    return true;

                case CatchAction.Dash:
                    Catcher.Dashing = true;
                    return true;
            }

            if (Twin != null)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeftTwin:
                        Twin.CurrentDirection--;
                        return true;

                    case CatchAction.MoveRightTwin:
                        Twin.CurrentDirection++;
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
                    Catcher.CurrentDirection++;
                    break;

                case CatchAction.MoveRight:
                    Catcher.CurrentDirection--;
                    break;

                case CatchAction.Dash:
                    Catcher.Dashing = false;
                    break;
            }

            if (Twin != null)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeftTwin:
                        Twin.CurrentDirection++;
                        break;

                    case CatchAction.MoveRightTwin:
                        Twin.CurrentDirection--;
                        break;

                    case CatchAction.DashTwin:
                        Twin.Dashing = false;
                        break;
                }
            }
        }

        private void displayCatcherTrail(CatcherTrailAnimation animation, CatcherBundle catcherBundle) => catcherBundle.CatcherTrailDisplay.Add(new CatcherTrailEntry(Time.Current, catcherBundle.Catcher.CurrentState, catcherBundle.Catcher.X, catcherBundle.Catcher.BodyScale, animation));
    }
}
