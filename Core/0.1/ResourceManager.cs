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
    public static string MetaballLayerTextures => Textures + "MetaballLayers/";

    public static string Effects => RoA.ModName + $"/{EFFECTSPATH}/";

    public static Texture2D Empty => ModContent.Request<Texture2D>(EmptyTexture, AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Pixel => ModContent.Request<Texture2D>(PixelTexture, AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D DefaultSparkle => ModContent.Request<Texture2D>(VisualEffectTextures + "DefaultSparkle", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D BasicCircle => ModContent.Request<Texture2D>(VisualEffectTextures + "BasicCircle", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle2 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle2", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle3 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle3", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle4 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle4", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle5 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle5", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle6 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle6", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle7 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle7", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle8 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle8", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Circle9 => ModContent.Request<Texture2D>(VisualEffectTextures + "Circle9", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Flash => ModContent.Request<Texture2D>(VisualEffectTextures + "Flash", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Laser0 => ModContent.Request<Texture2D>(VisualEffectTextures + "Laser0", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Laser1 => ModContent.Request<Texture2D>(VisualEffectTextures + "Laser1", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Star => ModContent.Request<Texture2D>(VisualEffectTextures + "Star", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Ray2 => ModContent.Request<Texture2D>(VisualEffectTextures + "Ray2", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Bloom => ModContent.Request<Texture2D>(VisualEffectTextures + "Bloom", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Bloom2 => ModContent.Request<Texture2D>(VisualEffectTextures + "Bloom2", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Light => ModContent.Request<Texture2D>(VisualEffectTextures + "Light", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Noise => ModContent.Request<Texture2D>(VisualEffectTextures + "Noise", AssetRequestMode.ImmediateLoad).Value;
    public static Texture2D Flare => ModContent.Request<Texture2D>(VisualEffectTextures + "Flare", AssetRequestMode.ImmediateLoad).Value;

    public static string VisualEffectTextures => Textures + "VisualEffects/";

    public static string UITextures => Textures + "UI/";

    public static string BackgroundTextures => Textures + "Backgrounds/";

    public static string BestiaryTextures => Textures + "Bestiary/";

    public static string ItemTextures => Textures + "Items/";
    public static string WeaponTextures => ItemTextures + "Weapons/";
    public static string MeleeWeaponTextures => WeaponTextures + "Melee/";
    public static string MagicWeaponTextures => WeaponTextures + "Magic/";

    public static string MiscellaneousItemTextures => ItemTextures + "Miscellaneous/";

    public static string EquipableTextures => ItemTextures + "Equipables/";
    public static string VanityEquipableTextures => EquipableTextures + "Vanity/";
    public static string DeveloperEquipableTextures => VanityEquipableTextures + "Developer/";

    public static string ArmorTextures => EquipableTextures + "Armor/";
    public static string MagicArmorTextures => ArmorTextures + "Magic/";

    public static string ProjectileTextures => Textures + "Projectiles/";
    public static string FriendlyMiscProjectiles => FriendlyProjectileTextures + "Miscellaneous/";
    public static string EnemyProjectileTextures => ProjectileTextures + "Enemies/";
    public static string FriendlyProjectileTextures => ProjectileTextures + "Friendly/";
    public static string NatureProjectileTextures => FriendlyProjectileTextures + "Nature/";
    public static string MiscellaneousProjectileTextures => FriendlyProjectileTextures + "Miscellaneous/";
    public static string RangedProjectileTextures => FriendlyProjectileTextures + "Ranged/";
    public static string FormProjectileTextures => NatureProjectileTextures + "Forms/";
    public static string MagicProjectileTextures => FriendlyProjectileTextures + "Magic/";
    public static string MeleeProjectileTextures => FriendlyProjectileTextures + "Melee/";
    public static string SummonProjectileTextures => FriendlyProjectileTextures + "Summon/";

    public static string NPCTextures => Textures + "NPCs/";
    public static string EnemyNPCTextures => NPCTextures + "Enemies/";
    public static string BackwoodsEnemyNPCTextures => EnemyNPCTextures + "Backwoods/";
    public static string TarEnemyNPCTextures => EnemyNPCTextures + "Tar/";

    public static string DustTextures => Textures + "Dusts/";
    public static string AdvancedDustTextures => Textures + "AdvancedDusts/";
    public static string BackwoodsAdvancedDustTextures => AdvancedDustTextures + "Backwoods/";

    public static string BuffTextures => Textures + "Buffs/";

    public static string AmbienceTextures => Textures + "Ambience/";

    public static string TileTextures => Textures + "Tiles/";
    public static string AmbientTileTextures => TileTextures + "Ambient/";
    public static string TreeTileTextures => TileTextures + "Trees/";
    public static string MiscTileTextures => TileTextures + "Miscellaneous/";
    public static string WaterTextures => TileTextures + "Waters/";
    public static string GlowMaskTileTextures => TileTextures + "Glow/";
    public static string DecorationTileTextures => TileTextures + "Decorations/";

    public static string BiomeTextures => Textures + "Biomes/";
    public static string BackwoodsBiomeTextures => BiomeTextures + "Backwoods/";
    public static string TarBiomeTextures => BiomeTextures + "Tar/";

    public static string Sounds => RoA.ModName + $"/{SOUNDSPATH}/";
    public static string AmbientSounds => Sounds + "Ambient/";
    //public static string Music => Sounds + "Music/";
    public static string Music => $"{SOUNDSPATH}/Music/";
    public static string ItemSounds => Sounds + "Items/";
    public static string NPCSounds => Sounds + "NPCs/";
    public static string MiscSounds => Sounds + "Other/";

    public static Asset<Texture2D> NoiseRGB {
        get {
            _noiseRGB ??= ModContent.Request<Texture2D>(VisualEffectTextures + "Noise3");
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
