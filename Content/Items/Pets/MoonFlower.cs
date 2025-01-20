using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Buffs;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Pets;

sealed class MoonFlower : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Moon Flower");
        // Tooltip.SetDefault("Summons a small moon to provide light");
    }

    public override void SetDefaults() {
        int width = 26; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.useTime = 22;
        Item.useAnimation = 22;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = false;
        Item.UseSound = SoundID.Item8;

        Item.value = Item.sellPrice(0, 0, 80, 60);
        Item.rare = ItemRarityID.Orange;

        Item.buffType = ModContent.BuffType<SmallMoon>();
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Pets.SmallMoon>();

        Item.shootSpeed = 3.5f;
        //Item.glowMask = RoAGlowMask.Get(nameof(MoonFlower));
    }

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0) player.AddBuff(Item.buffType, 3600, true);
        return null;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<SmallMoonPlayer>().smallMoon = true;
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Texture2D glowmask = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");
        Player player = Main.player[Main.myPlayer];
        ushort type = (ushort)ModContent.ItemType<MoonFlower>();
        Color color = player.GetModPlayer<SmallMoonPlayer>().smallMoonColor;
        color.A = 80;
        Vector2 drawPosition = position;
        float scale_ = Main.inventoryScale;
        spriteBatch.Draw(glowmask, position, null,
                        color, 0f, glowmask.Size() / 2f, new Vector2(scale_ *= 1f), SpriteEffects.None, 0);

        spriteBatch.Draw(glowmask, position,
                         new Rectangle(0, 0, glowmask.Width, glowmask.Height),
                         color,
                         0f,
                         glowmask.Size() * 0.5f,
                         scale,
                         SpriteEffects.None,
                         0f);
        //foreach (Item item in player.miscEquips) {
        //    if (item.stack > 0 && item.type == type) {
        //        float scale_ = Main.inventoryScale;
        //        spriteBatch.Draw(glowmask, position, null,
        //        color, 0f, glowmask.Size() / 2f, new Vector2(scale_ *= 1f), SpriteEffects.None, 0);
        //    }
        //}
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        Player player = Main.player[Main.myPlayer];
        Color color = player.GetModPlayer<SmallMoonPlayer>().smallMoonColor;
        color.A = 80;
        Lighting.AddLight(Item.position, player.GetModPlayer<SmallMoonPlayer>().smallMoonColor.ToVector3());
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Texture2D glowmask = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");
        spriteBatch.Draw(glowmask, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
            new Rectangle(0, 0, texture.Width, texture.Height),
            color,
            rotation,
            texture.Size() * 0.5f,
            scale,
            SpriteEffects.None,
            0f
        );
    }
}
sealed class MoonFlowerUseGlow : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].type == ModContent.ItemType<MoonFlower>();

    public override Position GetDefaultPosition()
        => new BeforeParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        Texture2D texture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<MoonFlower>()).Texture + "_Glow").Value;
        if (drawInfo.shadow != 0f || player.dead || player.frozen || player.itemAnimation <= 0)
            return;
        Color color = player.GetModPlayer<SmallMoonPlayer>().smallMoonColor;
        color.A = 80;
        Vector2 position = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y));
        Vector2 offset = new Vector2(player.direction == -1 ? texture.Width : 0, player.gravDir == -1f ? 0 : texture.Height);
        DrawData drawData = new DrawData(texture, position, null,
                                        color, player.itemRotation, offset,
                                        player.HeldItem.scale, SpriteEffects.None, 0);
        drawInfo.DrawDataCache.Add(drawData);
    }
}