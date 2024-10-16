// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class PerformanceBreakdownChart : Container
    {
        private readonly ScoreInfo score;
        private readonly IBeatmap playableBeatmap;

        private Drawable spinner = null!;
        private Drawable content = null!;
        private GridContainer chart = null!;
        private OsuSpriteText achievedPerformance = null!;
        private OsuSpriteText fcPerformance = null!;
        private OsuSpriteText maximumPerformance = null!;

        private readonly Color4 achieveColour = Color4Extensions.FromHex("#66FFCC");

        private readonly Color4 fcColour = Color4Extensions.FromHex("#609882");

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private readonly bool showFCPerformance;

        public PerformanceBreakdownChart(ScoreInfo score, IBeatmap playableBeatmap, bool showFCPerformance = true)
        {
            this.score = score;
            this.playableBeatmap = playableBeatmap;
            this.showFCPerformance = showFCPerformance;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var texts = new List<Drawable[]>
            {
                new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: StatisticItem.FONT_SIZE),
                        Text = "Achieved PP",
                        Colour = achieveColour
                    },
                    achievedPerformance = new OsuSpriteText
                    {
                        Origin = Anchor.CentreRight,
                        Anchor = Anchor.CentreRight,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: StatisticItem.FONT_SIZE),
                        Colour = achieveColour
                    }
                },
            };

            if (showFCPerformance)
            {
                texts.Add(
                    new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(weight: FontWeight.Regular, size: StatisticItem.FONT_SIZE),
                            Text = "Achievable PP on Full Combo",
                            Colour = fcColour
                        },
                        fcPerformance = new OsuSpriteText
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.CentreRight,
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: StatisticItem.FONT_SIZE),
                            Colour = fcColour
                        }
                    });
            }

            texts.Add(
                new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: StatisticItem.FONT_SIZE),
                        Text = "Achievable PP on Perfect Play",
                        Colour = OsuColour.Gray(0.7f)
                    },
                    maximumPerformance = new OsuSpriteText
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: StatisticItem.FONT_SIZE),
                        Colour = OsuColour.Gray(0.7f)
                    }
                });
            Children = new[]
            {
                spinner = new LoadingSpinner(true)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                },
                content = new FillFlowContainer
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.6f,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Spacing = new Vector2(15, 15),
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.8f,
                            AutoSizeAxes = Axes.Y,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = texts.ToArray(),
                        },
                        chart = new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize)
                            }
                        }
                    }
                }
            };

            spinner.Show();

            new PerformanceBreakdownCalculator(playableBeatmap, difficultyCache)
                .CalculateAsync(score, cancellationTokenSource.Token)
                .ContinueWith(t => Schedule(() => setPerformanceValue(t.GetResultSafely()!)));
        }

        private void setPerformanceValue(PerformanceBreakdown breakdown)
        {
            spinner.Hide();
            content.FadeIn(200);

            var displayAttributes = breakdown.Performance.GetAttributesForDisplay();
            var fcAttribute = breakdown.FCPerformance.GetAttributesForDisplay();
            var perfectDisplayAttributes = breakdown.PerfectPerformance.GetAttributesForDisplay();

            setTotalValues(
                displayAttributes.First(a => a.PropertyName == nameof(PerformanceAttributes.Total)),
                fcAttribute.First(a => a.PropertyName == nameof(PerformanceAttributes.Total)),
                perfectDisplayAttributes.First(a => a.PropertyName == nameof(PerformanceAttributes.Total))
            );

            var rowDimensions = new List<Dimension>();
            var rows = new List<Drawable[]>();

            foreach (PerformanceDisplayAttribute attr in displayAttributes)
            {
                if (attr.PropertyName == nameof(PerformanceAttributes.Total)) continue;

                var row = createAttributeRow(attr, fcAttribute.First(a => a.PropertyName == attr.PropertyName), perfectDisplayAttributes.First(a => a.PropertyName == attr.PropertyName));

                if (row != null)
                {
                    rows.Add(row);
                    rowDimensions.Add(new Dimension(GridSizeMode.AutoSize));
                }
            }

            chart.RowDimensions = rowDimensions.ToArray();
            chart.Content = rows.ToArray();
        }

        private void setTotalValues(PerformanceDisplayAttribute attribute, PerformanceDisplayAttribute fcAttribute, PerformanceDisplayAttribute perfectAttribute)
        {
            achievedPerformance.Text = Math.Round(attribute.Value, MidpointRounding.AwayFromZero).ToLocalisableString();
            if (showFCPerformance)
                fcPerformance.Text = Math.Round(fcAttribute.Value, MidpointRounding.AwayFromZero).ToLocalisableString();
            maximumPerformance.Text = Math.Round(perfectAttribute.Value, MidpointRounding.AwayFromZero).ToLocalisableString();
        }

        private Drawable[]? createAttributeRow(PerformanceDisplayAttribute attribute, PerformanceDisplayAttribute fcAttribute, PerformanceDisplayAttribute perfectAttribute)
        {
            // Don't display the attribute if its maximum is 0
            // For example, flashlight bonus would be zero if flashlight mod isn't on
            if (Precision.AlmostEquals(perfectAttribute.Value, 0f))
                return null;

            float percentage = (float)(attribute.Value / perfectAttribute.Value);
            float fcPercentage = showFCPerformance ? (float)(fcAttribute.Value / perfectAttribute.Value) : 0;

            float[] lengths = { fcPercentage, percentage };
            Color4[] colours = { fcColour, achieveColour };

            MultiValueBar bar = new MultiValueBar(2, lengths, colours)
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                CornerRadius = 2.5f,
                Masking = true,
                Height = 5,
                BackgroundColour = Color4.White.Opacity(0.5f),
            };

            return new Drawable[]
            {
                new OsuSpriteText
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Font = OsuFont.GetFont(weight: FontWeight.Regular, size: StatisticItem.FONT_SIZE),
                    Text = attribute.DisplayName,
                    Colour = Colour4.White
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 10, Right = 10 },
                    Child = bar
                },
                new OsuSpriteText
                {
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: StatisticItem.FONT_SIZE),
                    Text = percentage.ToLocalisableString("0%"),
                    Colour = Colour4.White
                }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
