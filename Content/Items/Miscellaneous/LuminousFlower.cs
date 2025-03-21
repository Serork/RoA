using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.GlowMasks;
using RoA.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class LuminousFlower : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
		Item.SetSize(34, 38);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 30, 0);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        LightUp(Item, spriteBatch, Texture, rotation);
    }

    public static void LightUp(Item item, SpriteBatch spriteBatch, string texture, float rotation) {
        Vector2 pos = item.getRect().Center();
        int i = (int)(pos.X / 16f);
        int j = (int)(pos.Y / 16f);
        Tiles.Miscellaneous.LuminousFlower.LuminiousFlowerLightUp(i, j, out float progress);

        Texture2D glowMaskTexture = ModContent.Request<Texture2D>(texture + "_Glow").Value;
        Vector2 origin = glowMaskTexture.Size() / 2f;
        Color color = Color.White * progress;
        spriteBatch.Draw(glowMaskTexture, item.Center - Main.screenPosition, null, color, rotation, origin, 1f, SpriteEffects.None, 0f);
    }
}