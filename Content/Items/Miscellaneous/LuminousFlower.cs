using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

[AutoloadGlowMask(shouldApplyItemAlpha: false)]
sealed class LuminousFlower : ModItem {
    public override void SetDefaults() {
		Item.SetSize(34, 38);
		Item.SetDefaultOthers(Item.sellPrice(gold: 3, silver: 50), ItemRarityID.Blue);
	}

    public override void PostUpdate() {
        Vector2 pos = Item.getRect().Center();
        int i = (int)(pos.X / 16f);
        int j = (int)(pos.Y / 16f);
        Tiles.Miscellaneous.LuminousFlower.LuminiousFlowerLightUp(i, j);
    }
}