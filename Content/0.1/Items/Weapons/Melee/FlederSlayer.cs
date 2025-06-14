using Microsoft.Xna.Framework;

using RoA.Content.Items.Placeable.Decorations;

using Terraria;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

sealed class FlederSlayer : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("'It's too big to be called a sword'\nSends vulnerable enemies back with a crushing blow");
        Item.ResearchUnlockCount = 1;

        PrefixLegacy.ItemSets.SwordsHammersAxesPicks[Type] = true;

        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<FlederSlayerDecoration>();
    }

    public override void SetDefaults() {
        int width = 62; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 50;
        Item.noMelee = true;

        Item.channel = true;

        Item.noUseGraphic = true;
        Item.useTurn = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 35;
        Item.knockBack = 8f;

        Item.rare = ItemRarityID.LightRed;
        //Item.UseSound = SoundID.Item1;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Melee.FlederSlayer>();
        Item.shootSpeed = 4f;

        Item.value = Item.sellPrice(0, 3, 50, 0);
    }

    public override bool CanUseItem(Player player) {
        int type = Item.shoot;
        if (player.ownedProjectileCounts[type] != 0) {
            return false;
        }
        player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
        //Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, type, Item.damage, Item.knockBack, player.whoAmI)._rotation = player.direction == 1 ? 0f : MathHelper.Pi;
        return true;
    }
}