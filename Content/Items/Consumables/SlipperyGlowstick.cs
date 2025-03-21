using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

public class SlipperyGlowstick : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Slippery Glowstick");
        //Tooltip.SetDefault("Works when wet\nSlips through solid tiles");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;

        ItemID.Sets.SingleUseInGamepad[Type] = true;
        ItemID.Sets.Torches[Type] = true;

        ItemID.Sets.Glowsticks[Type] = true;
    }

    public override void SetDefaults() {
        int width = 16; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.maxStack = Item.CommonMaxStack;

        Item.UseSound = SoundID.Item1;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 15;

        Item.noMelee = true;
        Item.consumable = true;
        Item.autoReuse = false;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.SlipperyGlowstick>();
        Item.shootSpeed = 6f;

        Item.holdStyle = 1;
        Item.color = new Color(255, 225, 255, 0);

        Item.value = Item.sellPrice(0, 0, 4, 5);
    }

    public override void HoldItem(Player player) {
        player.itemLocation.X -= 2 * player.direction;
        if (Main.rand.Next(player.itemAnimation > 0 ? 40 : 80) == 0)
            Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, 102);

        Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 8f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
        Lighting.AddLight(position, 0.95f, 0.75f, 0.4f);
    }

    public override void PostUpdate() {
        Lighting.AddLight((int)((Item.position.X + Item.width / 2) / 16f), (int)((Item.position.Y + Item.height / 2) / 16f), 0.45f, 0.35f, 0f);
    }
}
