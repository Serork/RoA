using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Utilities;

namespace RoA.Core.Utility;

static class RandomExtensions {
    public static bool NextChance(this UnifiedRandom rand, double chance) => rand.NextDouble() <= chance;
    public static bool NextChance(this UnifiedRandom rand, float chance) => rand.NextFloat() <= chance;

    public static float NextFloatRange(this UnifiedRandom rand, float range) => Utils.NextFloat(rand, -range, range);

    public static Vector2 NextVector2(this UnifiedRandom random, float minX, float minY, float maxX, float maxY) => new Vector2(random.NextFloat(minX, maxX), random.NextFloat(minY, maxY));

    public static Vector2 RandomPointInArea(this UnifiedRandom random, float sizeX, float sizeY) => random.NextVector2(-sizeX, -sizeY, sizeX, sizeY);
    public static Vector2 RandomPointInArea(this UnifiedRandom random, Vector2 size) => random.RandomPointInArea(size.X, size.Y);
    public static Vector2 RandomPointInArea(this UnifiedRandom random, float size) => random.RandomPointInArea(size, size);

    public static Vector2 Random2(this UnifiedRandom rand, float minX, float maxX, float minY, float maxY) => new(rand.NextFloat(minX, maxX), rand.NextFloat(minY, maxY));
    public static Vector2 Random2(this UnifiedRandom rand, float max) => rand.Random2(-max, max, -max, max);
    public static Vector2 Random20(this UnifiedRandom rand, float max) => rand.Random2(0f, max, 0f, max);
    public static Vector2 Random2(this UnifiedRandom rand, Vector2 max) => rand.Random2(-max.X, max.X, -max.Y, max.Y);

    public static Vector2 RandomPointInArea(this UnifiedRandom rand, Vector2 a, Vector2 b) => new(rand.Next((int)a.X, (int)b.X) + 1, rand.Next((int)a.Y, (int)b.Y) + 1);
}
