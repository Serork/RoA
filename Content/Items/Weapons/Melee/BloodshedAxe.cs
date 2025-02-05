using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

sealed class BloodshedAxe : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bloodshed Axe");
        // Tooltip.SetDefault("'There will be blood'");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 54; int height = 50;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.useTurn = false;

        Item.autoReuse = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 24;
        Item.knockBack = 4f;

        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.useTime = Item.useAnimation = 50; // DO NOT CHANGE AT ANY COST

        Item.value = Item.sellPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item1;
    }

    public override bool CanUseItem(Player player) {
        int type = ModContent.ProjectileType<Projectiles.Friendly.Melee.BloodshedAxe>();
        if (player.ownedProjectileCounts[type] != 0) {
            return false;
        }
        player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
        Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, type, Item.damage, Item.knockBack, player.whoAmI).rotation = player.direction == 1 ? 0f : MathHelper.Pi;
        return true;
    }
}