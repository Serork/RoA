using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

sealed class BloodshedAxe : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bloodshed Axe");
        // Tooltip.SetDefault("'There will be blood'");
        Item.ResearchUnlockCount = 1;
        PrefixLegacy.ItemSets.SwordsHammersAxesPicks[Type] = true;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ChemicalPrisoner>();
    }

    public override void SetDefaults() {
        int width = 54; int height = 50;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.useTurn = false;

        Item.autoReuse = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 34;
        Item.knockBack = 8f;

        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.useTime = Item.useAnimation = 50; // DO NOT CHANGE AT ANY COST

        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item1;

        Item.value = Item.sellPrice(0, 2, 25, 0);
    }

    public override bool CanUseItem(Player player) {
        int type = ModContent.ProjectileType<Projectiles.Friendly.Melee.BloodshedAxe>();
        if (player.ownedProjectileCounts[type] != 0) {
            return false;
        }
        player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
        int damage = (int)(Item.damage * 1.5f);
        Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, type,
            damage, Item.knockBack, player.whoAmI).rotation = player.direction == 1 ? 0f : MathHelper.Pi;
        return true;
    }
}