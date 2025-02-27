﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.GlowMasks;
using RoA.Content.Items.Miscellaneous;

using System;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class LuminousFlowerHat : ModItem {
    private class LuminousFlowerHatLightValueHandler : ModPlayer {
        public float LightValue { get; private set; }

        public override void PostUpdateEquips() {
            int type = ModContent.ItemType<LuminousFlowerHat>();
            if (Player.armor[0].type == type || Player.armor[10].type == type) {
                float length = Player.velocity.Length();
                LightValue = MathHelper.Lerp(LightValue, 
                    MathHelper.Clamp(Utils.GetLerpValue(0f, Math.Abs(Player.maxRunSpeed), length, true), 
                    Tiles.Miscellaneous.LuminousFlower.MINLIGHTMULT, 1f), (float)Math.Round(length * 0.1f, 2));
            }
            else {
                LightValue = Tiles.Miscellaneous.LuminousFlower.MINLIGHTMULT;
            }
        }
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        LuminousFlower.LightUp(Item, spriteBatch, Texture, rotation);
    }

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Luminous Flower Hat");
        //Tooltip.SetDefault("Provides light when moving\n'It no more smells... but still brights the area around you'");
        ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 34; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 35);
    }

    private static float GetLerpValue(Player player) => player.GetModPlayer<LuminousFlowerHatLightValueHandler>().LightValue;

    public override void UpdateEquip(Player player) {
        void lightUp(float progress) {
            float r = 0.9f * progress;
            float g = 0.7f * progress;
            float b = 0.3f * progress;
            Lighting.AddLight((int)(player.Top.X + 2f * player.direction) / 16, (int)player.Top.Y / 16, r, g, b);
        }
        lightUp(GetLerpValue(player));
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * GetLerpValue(drawPlayer) * (1f - shadow);
    }
}
