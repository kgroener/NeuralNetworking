using System.Numerics;

namespace Genometry.Game
{
    public static class World
    {
        private static Vector2[] _targets = new[]
        {
            new Vector2(0, 500),
            new Vector2(500, 500),
            new Vector2(0, 0),
            new Vector2(500, 0),
        };

        public static Vector2[] Targets => _targets;

    }
}
