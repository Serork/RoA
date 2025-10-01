using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using SteelSeries.GameSense;

using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;

using Terraria.ModLoader;

namespace RoA.Core;

static class ResourceManager {
    private static Asset<Texture2D> _noiseRGB;

    public const string RESOURCESPATH = "Resources";
    public const string TEXTURESPATH = RESOURCESPATH + "/Textures";
    public const string SOUNDSPATH = RESOURCESPATH + "/Sounds";
    public const string EFFECTSPATH = RESOURCESPATH + "/Effects";

    public const string VANILLAPATH = "Terraria/Images/";

    public static string Textures => RoA.ModName + $"/{TEXTURESPATH}/";
    public static string EmptyTexture => Textures + "Empty";
    public static string PixelTexture => Textures + "Pixel";

    public static string AchievementsTextures => UITextures + "Achievements/";

    public static string Effects => RoA.ModName + $"/{EFFECTSPATH}/";

    public static Texture2D Pixel => ModContent.Request<Texture2D>(PixelTexture, AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D DefaultSparkle => ModContent.Request<Texture2D>("RoA/Resources/Textures/VisualEffects/DefaultSparkle", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle => ModContent.Request<Texture2D>(Textures + "Circle", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle2 => ModContent.Request<Texture2D>(Textures + "Circle2", AssetRequestMode.ImmediateLoad).Value;

    public static string UITextures => Textures + "UI/";

    public static string BackgroundTextures => Textures + "Backgrounds/";

    public static string BestiaryTextures => Textures + "Bestiary/";

    public static string ItemTextures => Textures + "Items/";
    public static string WeaponTextures => ItemTextures + "Weapons/";
    public static string MeleeWeaponTextures => WeaponTextures + "Melee/";
    public static string MagicWeaponTextures => WeaponTextures + "Magic/";

    public static string ProjectileTextures => Textures + "Projectiles/";
    public static string EnemyProjectileTextures => ProjectileTextures + "Enemies/";
    public static string FriendlyProjectileTextures => ProjectileTextures + "Friendly/";
    public static string NatureProjectileTextures => FriendlyProjectileTextures + "Nature/";
    public static string FormProjectileTextures => NatureProjectileTextures + "Forms/";
    public static string MagicProjectileTextures => FriendlyProjectileTextures + "Magic/";
    public static string MeleeProjectileTextures => FriendlyProjectileTextures + "Melee/";
    public static string SummonProjectileTextures => FriendlyProjectileTextures + "Summon/";

    public static string DustTextures => Textures + "Dusts/";

    public static string BuffTextures => Textures + "Buffs/";

    public static string TileTextures => Textures + "Tiles/";
    public static string WaterTextures => TileTextures + "Waters/";
    public static string GlowMaskTileTextures => TileTextures + "Glow/";
    public static string GlowMaskItemTextures => ItemTextures + "Glow/";
    public static string DecorationTileTextures => TileTextures + "Decorations/";

    public static string BiomeTextures => Textures + "Biomes/";
    public static string BackwoodsTextures => BiomeTextures + "Backwoods/";

    public static string Sounds => RoA.ModName + $"/{SOUNDSPATH}/";
    public static string AmbientSounds => Sounds + "Ambient/";
    public static string Music => Sounds + "Music/";
    public static string ItemSounds => Sounds + "Items/";
    public static string NPCSounds => Sounds + "NPCs/";
    public static string MiscSounds => Sounds + "Other/";

    public static Asset<Texture2D> NoiseRGB {
        get {
            _noiseRGB ??= ModContent.Request<Texture2D>(Textures + "Noise3");
            return _noiseRGB;
        }
    }

    public static IEnumerable<Asset<Texture2D>> GetAllTexturesInPath(string path, string? searchPattern = null) {
        string texturesPath = Path.Combine(RoA.ModSourcePath, RoA.ModName + path);
        string format = ".png";
        searchPattern ??= string.Empty;
        IEnumerable<string> allSprites = Directory.EnumerateFiles(texturesPath, '*' + format)
            .Where(x => string.Equals(Path.GetExtension(x), format, StringComparison.InvariantCultureIgnoreCase))
            .Where(x => x.Contains(searchPattern))
            .Select(x => Path.ChangeExtension(x.Substring(texturesPath.Length + 1), null));
        foreach (string file in allSprites) {
            yield return RoA.Instance.Assets.Request<Texture2D>(string.Concat(path.AsSpan(1), $"/{file}"), AssetRequestMode.ImmediateLoad);
        }
    }
}
