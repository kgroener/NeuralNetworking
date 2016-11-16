using System.Numerics;

namespace Simulation.Game
{
    public static class World
    {
        private static Vector2[] _targets = new[]
        {
            new Vector2(500, 0),
            new Vector2(625, 400),
            new Vector2(1000, 400),
            new Vector2(690, 625),
            new Vector2(800, 1000),
            new Vector2(500, 775),
            new Vector2(200, 1000),
            new Vector2(310, 625),
            new Vector2(0, 400),
            new Vector2(375, 400),
            new Vector2(500, 0),
        };

        public static Vector2[] Targets => _targets;

    }
}
