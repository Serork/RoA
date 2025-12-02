using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.BackwoodsSystems;
using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Core;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed class LightColorFix : ILoadable {
    private static Asset<Texture2D> _mapBGAsset = null!,
                                    _fogMapBGAsset = null!,
                                    _glowMapBGAsset = null!;

    void ILoadable.Load(Mod mod) {
        On_Main.DrawMapFullscreenBackground += On_Main_DrawMapFullscreenBackground;

        if (Main.dedServ) {
            return;
        }

        _mapBGAsset = ModContent.Request<Texture2D>(ResourceManager.BackwoodsTextures + "DruidBiomeMapBG");
        _glowMapBGAsset = ModContent.Request<Texture2D>(ResourceManager.BackwoodsTextures + "DruidBiomeMapBG_Fog");
        _mapBGAsset = ModContent.Request<Texture2D>(ResourceManager.BackwoodsTextures + "DruidBiomeMapBG_Glow");
    }

    private void On_Main_DrawMapFullscreenBackground(On_Main.orig_DrawMapFullscreenBackground orig, Vector2 screenPosition, int screenWidth, int screenHeight) {
        if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            Texture2D mapBGAsset = BackwoodsFogHandler.IsFogActive ? _fogMapBGAsset.Value : _mapBGAsset.Value;
            Color color = Main.ColorOfTheSkies * 3f;
            if ((double)screenPosition.Y > Main.worldSurface * 16.0) {
                color = Color.White;
            }
            //else {
            //    color = Main.ColorOfTheSkies;
            //    MapBGAsset = ((!Main.player[Main.myPlayer].ZoneDesert) ? (Texture2D)ModContent.Request<Texture2D>("TheConfectionRebirth/Biomes/ConfectionBiomeMapBackground") : ((Main.player[Main.myPlayer].ZoneSnow) ? (Texture2D)ModContent.Request<Texture2D>("TheConfectionRebirth/Biomes/ConfectionIceBiomeMapBackground") : (Texture2D)ModContent.Request<Texture2D>("TheConfectionRebirth/Biomes/ConfectionDesertBiomeMapBackground")));
            //}
            Main.spriteBatch.Draw(mapBGAsset, new Rectangle(0, 0, screenWidth, screenHeight), color);
            mapBGAsset = _glowMapBGAsset.Value;
            Main.spriteBatch.Draw(mapBGAsset, new Rectangle(0, 0, screenWidth, screenHeight), Color.White * 0.5f);
        }
        else {
            orig.Invoke(screenPosition, screenWidth, screenHeight);
        }
    }

    void ILoadable.Unload() { }
}

// only used for bestiary
sealed partial class BackwoodsBiomeFog : ModBiome {
    public static BackwoodsBiomeFog Instance => ModContent.GetInstance<BackwoodsBiomeFog>();

    public override string MapBackground => ResourceManager.BackwoodsTextures + "DruidBiomeMapBG_Fog";

    public override string BackgroundPath => MapBackground;


    public override string BestiaryIcon => ResourceManager.BackwoodsTextures + "Backwoods_Bestiary_Fog";
}

sealed partial class BackwoodsBiome : ModBiome {
    private class Shop_ForestFix : ILoadable {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ApplyPreference")]
        public extern static void BiomePreferenceListTrait_ApplyPreference(BiomePreferenceListTrait self, BiomePreferenceListTrait.BiomePreference preference, HelperInfo info, ShopHelper shopHelperInstance);


        public void Load(Mod mod) {
            On_BiomePreferenceListTrait.ModifyShopPrice += On_BiomePreferenceListTrait_ModifyShopPrice;
        }

        private void On_BiomePreferenceListTrait_ModifyShopPrice(On_BiomePreferenceListTrait.orig_ModifyShopPrice orig, BiomePreferenceListTrait self, HelperInfo info, Terraria.GameContent.ShopHelper shopHelperInstance) {
            BiomePreferenceListTrait.BiomePreference biomePreference = null;
            for (int i = 0; i < self.Preferences.Count; i++) {
                BiomePreferenceListTrait.BiomePreference biomePreference2 = self.Preferences[i];
                bool flag = biomePreference2.Biome.IsInBiome(info.player);
                if (biomePreference2.Biome is ForestBiome && info.player.InModBiome<BackwoodsBiome>()) {
                    flag = false;
                }
                if (flag && (biomePreference == null || biomePreference.Affection < biomePreference2.Affection)) {
                    biomePreference = biomePreference2;
                }
            }

            if (biomePreference != null) {
                BiomePreferenceListTrait_ApplyPreference(self, biomePreference, info, shopHelperInstance);
            }
        }

        public void Unload() { }
    }

    private class TownNPCsStayHomeWhenBackwoodsFogIsActive : GlobalNPC {
        private static bool _dayTime;

        public override bool PreAI(NPC npc) {
            _dayTime = Main.dayTime;
            if (npc.townNPC && BackwoodsFogHandler.IsFogActive && Main.player[npc.FindClosestPlayer()].InModBiome<BackwoodsBiome>()) {
                Main.dayTime = false;
            }

            return base.PreAI(npc);
        }

        public override void PostAI(NPC npc) {
            if (npc.townNPC && BackwoodsFogHandler.IsFogActive && Main.player[npc.FindClosestPlayer()].InModBiome<BackwoodsBiome>()) {
                Main.dayTime = _dayTime;
            }
        }
    }

    public static float TransitionSpeed => 0.05f;

    public static bool IsActiveForFogEffect => BiomeShouldBeActive;
    public static bool BiomeShouldBeActive => ModContent.GetInstance<TileCount>().BackwoodsTiles >= 140f;

    public static BackwoodsBiome Instance => ModContent.GetInstance<BackwoodsBiome>();

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override int BiomeTorchItemType => ModContent.ItemType<Items.Placeable.Crafting.ElderTorch>();
    public override int BiomeCampfireItemType => ModContent.ItemType<Items.Placeable.Crafting.BackwoodsCampfire>();

    public override void SpecialVisuals(Player player, bool isActive) => player.ManageSpecialBiomeVisuals(ShaderLoader.BackwoodsSky, player.InModBiome<BackwoodsBiome>(), player.Center);

    public override bool IsBiomeActive(Player player) {
        bool isInBiome = BiomeShouldBeActive;
        return isInBiome;
    }

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>(RoA.ModName + "/BackwoodsBackgroundSurface");

    public override int Music => BackwoodsFogHandler.IsFogActive ? MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "BackwoodsFog") : 
        !Main.IsItDay() ? MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "BackwoodsNight") : MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "BackwoodsDay")
        /*IsUndergroundBackwoods() ? MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "Backwoods") : */;

    public static bool IsUndergroundBackwoods() {
        bool result = false;

        Player player = Main.LocalPlayer;
        if (player.position.Y / 16 > BackwoodsVars.FirstTileYAtCenter + 35) {
            result = true;
        }

        return result;
    }

    public override string MapBackground => ResourceManager.BackwoodsTextures + "DruidBiomeMapBG";

    public override string BackgroundPath => MapBackground;

    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>(RoA.ModName + "/BackwoodsBackgroundUnderground");

    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

    public override string BestiaryIcon => ResourceManager.BackwoodsTextures + "Backwoods_Bestiary";

    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>(RoA.ModName + "/DruidBiomeWaterStyle");
}