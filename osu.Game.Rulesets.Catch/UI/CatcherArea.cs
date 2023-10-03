﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Mods.Skills;
using osu.Game.Rulesets.Catch.Objects;
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
        public Catcher Twin
        {
            get => twin;
            set => twinContainer.Child = twin = value;
        }

        public bool TwinCatchersApplies { get; set; } = false!;
        public bool TwinCatchersInvertApplies { get; set; } = false!;
        public bool AlwaysDashApplies { get; set; } = false!;
        public bool NoDashingApplies { get; set; } = false!;
        public bool TeleportApplies { get; set; } = false!;
        public bool GrowthApplies { get; set; } = false!;

        private readonly Container<Catcher> catcherContainer;

        private readonly Container<Catcher> twinContainer;

        private readonly CatchComboDisplay comboDisplay;

        private readonly CatchComboDisplay comboDisplayTwin;

        private readonly CatcherTrailDisplay catcherTrails;

        private readonly CatcherTrailDisplay twinTrails;

        private Catcher catcher = null!;

        private Catcher twin = null!;

        public bool HasTeleported = false!;

        public bool HasGrowth = false!;

        public double TeleportTimer = -1;

        public double TeleportCooldownTimer = -1;
        public double TeleportTimerParam { get; set; }
        public double TeleportCooldownParam { get; set; }

        public double GrowthTimer = -1;

        public double GrowthCooldownTimer = -1;
        public double GrowthTimerParam { get; set; }
        public double GrowthCooldownParam { get; set; }

        public float GrowthMultiplier { get; set; }
        /// <summary>
        /// <c>-1</c> when only left button is pressed.
        /// <c>1</c> when only right button is pressed.
        /// <c>0</c> when none or both left and right buttons are pressed.
        /// </summary>
        private int currentDirection, currentDirectionTwin;

        // TODO: support replay rewind
        private bool lastHyperDashState, lastHyperDashStateTwin;

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
                comboDisplay = SetNewCatchComboDisplay(),

                twinContainer = new Container<Catcher> { RelativeSizeAxes = Axes.Both },
                twinTrails = new CatcherTrailDisplay(),
                comboDisplayTwin = SetNewCatchComboDisplay(),
            };

        }

        public void OnNewResult(DrawableCatchHitObject hitObject, JudgementResult result)
        {

            Catcher.OnNewResult(hitObject, result);
            GetCatchComboDisplay(Catcher).OnNewResult(hitObject, result);
            Catcher.CanCatchObj = false;

            if (TwinCatchersApplies)
            {
                Twin.OnNewResult(hitObject, result);
                GetCatchComboDisplay(Twin).OnNewResult(hitObject, result);
                Twin.CanCatchObj = false;
            }

        }

        public void OnRevertResult(JudgementResult result)
        {

            GetCatchComboDisplay(Catcher).OnRevertResult(result);
            Catcher.OnRevertResult(result);
            if (TwinCatchersApplies)
            {
                GetCatchComboDisplay(Twin).OnRevertResult(result);
                Twin.OnRevertResult(result);
            }
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

            if (HasTeleported)
            {

                if (Catcher.X > CatchModSkillTeleport.CatchableObjectEffectiveX) Catcher.VisualDirection = Direction.Left;
                else if (Catcher.X < CatchModSkillTeleport.CatchableObjectEffectiveX) Catcher.VisualDirection = Direction.Right;

                Catcher.X = CatchModSkillTeleport.CatchableObjectEffectiveX;

                TeleportTimer -= Clock.ElapsedFrameTime;
                if (TeleportTimer <= 0) HasTeleported = false;

            }
            else if (TeleportApplies && !HasTeleported && TeleportCooldownTimer > 0) TeleportCooldownTimer -= Clock.ElapsedFrameTime;

            if (HasGrowth)
            {
                GrowthTimer -= Clock.ElapsedFrameTime;

                if (GrowthTimer <= 0)
                {
                    Catcher.Scale = new Vector2(1.0f - 0.7f * (CatchModSkillGrowth.MapOriginalCS - 5) / 5);
                    Catcher.CatchWidth = Catcher.CalculateCatchWidth(Catcher.Scale);
                    CatchModSkillGrowth.Trigger = false;
                    HasGrowth = false;
                }

            }
            else if (GrowthApplies && !HasGrowth && GrowthCooldownTimer > 0) GrowthCooldownTimer -= Clock.ElapsedFrameTime;

        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();



            GetCatchComboDisplay(Catcher).X = Catcher.X;
            if (TwinCatchersApplies) GetCatchComboDisplay(Twin).X = Twin.X;

            if ((Clock as IGameplayClock)?.IsRewinding == true)
            {
                // This is probably a wrong value, but currently the true value is not recorded.
                // Setting `true` will prevent generation of false-positive after-images (with more false-negatives).
                lastHyperDashState = true;
                if (TwinCatchersApplies) lastHyperDashStateTwin = true;
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

            if (TwinCatchersApplies)
            {

                if (!lastHyperDashStateTwin && Twin.HyperDashing)
                    displayCatcherTrail(CatcherTrailAnimation.HyperDashAfterImage, Twin, twinTrails);

                if (Twin.Dashing || Twin.HyperDashing)
                {
                    double generationInterval = Twin.HyperDashing ? 25 : 50;

                    if (Time.Current - twinTrails.LastDashTrailTime >= generationInterval)
                        displayCatcherTrail(Twin.HyperDashing ? CatcherTrailAnimation.HyperDashing : CatcherTrailAnimation.Dashing, Twin, twinTrails);
                }

                lastHyperDashStateTwin = Twin.HyperDashing;

            }
        }

        public void SetCatcherPosition(float x, Catcher currCatcher)
        {
            float lastPosition = currCatcher.X;
            float newPosition = Math.Clamp(x, 0, CatchPlayfield.WIDTH);
            if (TwinCatchersApplies)
            {
                //Replaces newPosition limits, when the Twin Catchers mod is applied
                if (currCatcher == Catcher) newPosition = GetNewPositionTwins(x, Catcher);
                else if (currCatcher == Twin) newPosition = GetNewPositionTwins(x, Twin);
            }
            currCatcher.X = newPosition;

            if (lastPosition < newPosition)
                currCatcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                currCatcher.VisualDirection = Direction.Left;
        }

        public float GetNewPositionTwins(float x, Catcher currCatcher)
        {
            if (currCatcher == Catcher)
            {
                if (!TwinCatchersInvertApplies) return Math.Clamp(x, 0, (CatchPlayfield.WIDTH / 2) - (Catcher.CatchWidth / 2));
                return Math.Clamp(x, (CatchPlayfield.WIDTH / 2) + (Catcher.CatchWidth / 2), CatchPlayfield.WIDTH);
            }

            if (!TwinCatchersInvertApplies) return Math.Clamp(x, (CatchPlayfield.WIDTH / 2) + (Twin.CatchWidth / 2), CatchPlayfield.WIDTH);
            return Math.Clamp(x, 0, (CatchPlayfield.WIDTH / 2) - (Twin.CatchWidth / 2));
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
                    if (!NoDashingApplies) Catcher.Dashing = true;
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

                        if (!NoDashingApplies) Twin.Dashing = true;

                        return true;
                }
            }

            if (TeleportApplies)
            {
                switch (e.Action)
                {
                    case CatchAction.Teleport:
                        if (!HasTeleported && (TeleportCooldownTimer <= 0))
                        {
                            HasTeleported = true;
                            TeleportTimer = TeleportTimerParam * 1000d;
                            TeleportCooldownTimer = TeleportCooldownParam * 1000d;
                        }
                        return true;
                }
            }

            if (GrowthApplies)
            {
                switch (e.Action)
                {
                    case CatchAction.Growth:
                        if (!HasGrowth && (GrowthCooldownTimer <= 0))
                        {
                            HasGrowth = true;
                            CatchModSkillGrowth.Trigger = true;
                            GrowthTimer = GrowthTimerParam * 1000d;
                            GrowthCooldownTimer = GrowthCooldownParam * 1000d;
                            Catcher.Scale = new Vector2(1.0f - 0.7f * (CatchModSkillGrowth.MapOriginalCS * GrowthMultiplier - 5) / 5);
                            Catcher.CatchWidth = Catcher.CalculateCatchWidth(Catcher.Scale);
                        }
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
                    if (!AlwaysDashApplies) Catcher.Dashing = false;
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

                        if (!AlwaysDashApplies) Twin.Dashing = false;

                        break;
                }
            }
        }

        //Replacement of CheckIfWeCanCatch from CatchPlayfield, to be more generic
        public bool CheckIfWeCanCatch(CatchHitObject obj)
        {
            bool catcherCanCatchObj = Catcher.CanCatch(obj);
            Catcher.CanCatchObj = catcherCanCatchObj;

            if (TwinCatchersApplies)
            {
                bool twinCanCatchObj = Twin.CanCatch(obj);
                Twin.CanCatchObj = twinCanCatchObj;
                return catcherCanCatchObj || twinCanCatchObj;
            };
            return catcherCanCatchObj;

        }

        public CatchComboDisplay GetCatchComboDisplay(Catcher currCatcher)
        {
            if (TwinCatchersApplies)
            {
                if (currCatcher == Twin) return comboDisplayTwin;
            }
            return comboDisplay;
        }

        public CatchComboDisplay SetNewCatchComboDisplay()
        {
            return new CatchComboDisplay
            {
                RelativeSizeAxes = Axes.None,
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.Centre,
                Margin = new MarginPadding { Bottom = 350f },
                X = CatchPlayfield.CENTER_X
            };
        }

        private void displayCatcherTrail(CatcherTrailAnimation animation, Catcher currCatcher, CatcherTrailDisplay trails) => trails.Add(new CatcherTrailEntry(Time.Current, currCatcher.CurrentState, currCatcher.X, currCatcher.BodyScale, animation));
    }
}
