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
        float r = 0.9f;
        float g = 0.7f;
        float b = 0.3f;
        Lighting.AddLight(Item.getRect().Center(), new Vector3(r, g, b) * 0.85f);
    }
}