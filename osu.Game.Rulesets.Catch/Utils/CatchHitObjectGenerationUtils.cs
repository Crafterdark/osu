// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Utils
{
    public static partial class CatchHitObjectGenerationUtils
    {
        /// <summary>
        /// Reflects the position of the <see cref="CatchHitObject"/> in the playfield horizontally.
        /// </summary>
        /// <param name="catchHitObject">The object to reflect.</param>
        public static void ReflectHorizontallyAlongPlayfield(CatchHitObject catchHitObject)
        {
            switch (catchHitObject)
            {
                case Fruit fruit:
                    mirrorEffectiveX(fruit);
                    break;

                case JuiceStream juiceStream:
                    mirrorEffectiveX(juiceStream);
                    mirrorJuiceStreamPath(juiceStream);
                    break;

                case BananaShower bananaShower:
                    mirrorBananaShower(bananaShower);
                    break;
            }
        }

        /// <summary>
        /// Mirrors the effective X position of <paramref name="catchObject"/> and its nested hit objects.
        /// </summary>
        private static void mirrorEffectiveX(CatchHitObject catchObject)
        {
            catchObject.OriginalX = CatchPlayfield.WIDTH - catchObject.OriginalX;
            catchObject.XOffset = -catchObject.XOffset;

            foreach (var nested in catchObject.NestedHitObjects.Cast<CatchHitObject>())
            {
                nested.OriginalX = CatchPlayfield.WIDTH - nested.OriginalX;
                nested.XOffset = -nested.XOffset;
            }
        }

        /// <summary>
        /// Mirrors the path of the <paramref name="juiceStream"/>.
        /// </summary>
        private static void mirrorJuiceStreamPath(JuiceStream juiceStream)
        {
            var controlPoints = juiceStream.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                point.Position = new Vector2(-point.Position.X, point.Position.Y);

            juiceStream.Path = new SliderPath(controlPoints, juiceStream.Path.ExpectedDistance.Value);
        }

        /// <summary>
        /// Mirrors X positions of all bananas in the <paramref name="bananaShower"/>.
        /// </summary>
        private static void mirrorBananaShower(BananaShower bananaShower)
        {
            foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                banana.XOffset = CatchPlayfield.WIDTH - banana.XOffset;
        }
    }
}
