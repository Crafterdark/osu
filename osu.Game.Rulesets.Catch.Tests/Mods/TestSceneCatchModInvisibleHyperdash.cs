// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public partial class TestSceneCatchModInvisibleHyperdash : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestNoHyperDashFromEveryType()
        {
            CreateModTest(new ModTestData
            {
                Mod = new CatchModInvisibleHyperdash(),
                Autoplay = true,
                Beatmap = CreateBeatmap(CreatePlayerRuleset().RulesetInfo),
                PassCondition = () => HyperDashHidden(Beatmap.Value)
            });
        }
        protected bool HyperDashHidden(IWorkingBeatmap workingBeatmap)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)Player.DrawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            foreach (DrawablePalpableCatchHitObject drawablePalpableCatchHitObject in catchPlayfield.AllHitObjects)
            {
                //if there's even one hitobject that is displaying the hyperdash status
                if (drawablePalpableCatchHitObject.HyperDash.Value == true)
                    return false;
            }

            //if the catcher trail hyperdash is shown
            if (catchPlayfield.CatcherArea.Catcher.ShowHyperDashTrail == true)
                return false;

            return true;
        }

        //Mostly copied from TestSceneHyperDash for easier comparison

        protected new IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo =
                {
                    Ruleset = ruleset,
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 3.6f,
                        SliderMultiplier = 1,
                    },
                }
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint());

            // Should produce a hyper-dash (edge case test)
            beatmap.HitObjects.Add(new Fruit { StartTime = 1816, X = 56, NewCombo = true });
            beatmap.HitObjects.Add(new Fruit { StartTime = 2008, X = 308, NewCombo = true });

            double startTime = 3000;

            const float left_x = 0.02f * CatchPlayfield.WIDTH;
            const float right_x = 0.98f * CatchPlayfield.WIDTH;

            createObjects(() => new Fruit { X = left_x });
            createObjects(() => new TestJuiceStream(right_x), 1);
            createObjects(() => new TestJuiceStream(left_x), 1);
            createObjects(() => new Fruit { X = right_x });
            createObjects(() => new Fruit { X = left_x });
            createObjects(() => new Fruit { X = right_x });
            createObjects(() => new TestJuiceStream(left_x), 1);

            beatmap.ControlPointInfo.Add(startTime, new TimingControlPoint
            {
                BeatLength = 50
            });

            createObjects(() => new TestJuiceStream(left_x)
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero),
                    new PathControlPoint(new Vector2(512, 0))
                })
            }, 1);

            createObjects(() => new Fruit { X = right_x }, count: 2, spacing: 0, spacingAfterGroup: 400);
            createObjects(() => new TestJuiceStream(left_x)
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero),
                    new PathControlPoint(new Vector2(0, 300))
                })
            }, count: 1, spacingAfterGroup: 150);
            createObjects(() => new Fruit { X = left_x }, count: 1, spacing: 0, spacingAfterGroup: 400);
            createObjects(() => new Fruit { X = right_x }, count: 2, spacing: 0);

            beatmap.ControlPointInfo.Add(startTime, new TimingControlPoint
            {
                BeatLength = 200
            });

            beatmap.HitObjects.Add(new BananaShower { StartTime = startTime + 140, X = CatchPlayfield.CENTER_X, Duration = 3000 });

            return beatmap;

            void createObjects(Func<CatchHitObject> createObject, int count = 3, float spacing = 140, float spacingAfterGroup = 700)
            {
                for (int i = 0; i < count; i++)
                {
                    var hitObject = createObject();
                    hitObject.StartTime = startTime + i * spacing;
                    beatmap.HitObjects.Add(hitObject);
                }

                startTime += spacingAfterGroup;
            }
        }
        private class TestJuiceStream : JuiceStream
        {
            public TestJuiceStream(float x)
            {
                X = x;

                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero),
                    new PathControlPoint(new Vector2(30, 0)),
                });
            }
        }
    }
}
