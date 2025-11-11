using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class LuminousFlower : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(34, 38);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 30, 0);
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        LightUp(Item, spriteBatch, Texture, 0f, color: Color.White, position: position, scale: scale, shouldLightUp: false);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        LightUp(Item, spriteBatch, Texture, rotation);
    }

    public static void LightUp(Item item, SpriteBatch spriteBatch, string texture, float rotation, Vector2? position = null, float scale = 1f, Color? color = null, bool shouldLightUp = true) {
        Vector2 pos = item.getRect().Center();
        if (position != null) {
            pos = position.Value - new Point(1, 1).ToWorldCoordinates() + Main.screenPosition - TileHelper.ScreenOffset;
        }
        int i = (int)(pos.X / 16f);
        int j = (int)(pos.Y / 16f);
        Tiles.Miscellaneous.LuminousFlower.LuminiousFlowerLightUp(i, j, out float progress, shouldLightUp: shouldLightUp);

        Texture2D glowMaskTexture = ModContent.Request<Texture2D>(texture + "_Glow").Value;
        Vector2 origin = glowMaskTexture.Size() / 2f;
        color ??= Color.White;
        color *= progress;
        position ??= item.Center - Main.screenPosition;
        spriteBatch.Draw(glowMaskTexture, position.Value, null, color.Value * 0.9f, rotation, origin, scale, SpriteEffects.None, 0f);
    }
}