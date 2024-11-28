using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;

using Terraria.ModLoader;

namespace RoA.Core;

static class ResourceManager {
    public const string RESOURCESPATH = "Resources";
    public const string TEXTURESPATH = RESOURCESPATH + "/Textures";
    public const string SOUNDSPATH = RESOURCESPATH + "/Sounds";

    public static string Textures => RoA.ModName + $"/{TEXTURESPATH}/";
    public static string EmptyTexture => Textures + "Empty";
    public static string PixelTexture => Textures + "Pixel";

    public static Texture2D Pixel => ModContent.Request<Texture2D>(PixelTexture, AssetRequestMode.ImmediateLoad).Value;

    public static string GUITextures => Textures + "GUI/";

    public static string BackgroundTextures => Textures + "Backgrounds/";

    public static string ItemsTextures => Textures + "Items/";
    public static string ItemsWeaponsTextures => ItemsTextures + "Weapons/";
    public static string ItemsWeaponsMeleeTextures => ItemsWeaponsTextures + "Melee/";
    public static string ItemsWeaponsMagicTextures => ItemsWeaponsTextures + "Magic/";

    public static string ProjectileTextures => Textures + "Projectiles/";
    public static string EnemyProjectileTextures => ProjectileTextures + "Enemies/";
    public static string FriendlyProjectileTextures => ProjectileTextures + "Friendly/";

    public static string DustTextures => Textures + "Dusts/";

    public static string TilesTextures => Textures + "Tiles/";
    public static string GlowTilesTextures => TilesTextures + "Glow/";

    public static string BiomesTextures => Textures + "Biomes/";
    public static string BackwoodsTextures => BiomesTextures + "Backwoods/";

    public static string Sounds => RoA.ModName + $"/{SOUNDSPATH}/";
    public static string AmbientSounds => Sounds + "Ambient/";
    public static string Music => Sounds + "Music/";
    public static string ItemSounds => Sounds + "Items/";

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
