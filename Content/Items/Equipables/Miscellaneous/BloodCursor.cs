using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace RoA.Content.Items.Equipables.Miscellaneous;

sealed class BloodCursor : ModItem {
    public override void SetStaticDefaults() {
        //Tooltip.SetDefault("4% increased nature critical strike chance");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = 24;
        Item.height = 24;
        Item.accessory = true;
        Item.vanity = true;
        Item.rare = ItemRarityID.LightRed;
        Item.hasVanityEffects = true;

        Item.value = Item.sellPrice(0, 2, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (!hideVisual) {
            player.GetCommon().IsBloodCursorEffectActive = true;
        }
    }

    public override void UpdateVanity(Player player) => player.GetCommon().IsBloodCursorEffectActive = true;

    //public override void DrawArmorColor(Player drawPlayer, float shadow, ref DrawColor color, ref int glowMask, ref DrawColor glowMaskColor) {
    //	if (drawPlayer.active) {
    //		glowMask = RoAGlowMask.Get("DeerSkull_Head");
    //		glowMaskColor = DrawColor.White;
    //	}
    //}
}
