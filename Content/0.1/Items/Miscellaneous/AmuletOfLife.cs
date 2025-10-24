using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Miscellaneous;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

[Autoload(false)]
sealed class AmuletOfLife : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Amulet Of Life");
        //Tooltip.SetDefault("Casts a vortex of healing wisps, that heal player upon impact");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.useAnimation = Item.useTime = 28;
        Item.useTurn = false;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item103;
        Item.mana = 30;
        Item.shoot = ModContent.ProjectileType<AmuletOfLifeWisps>();
        Item.shootSpeed = 10f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        for (int i = 0; i < 3; i++)
            Projectile.NewProjectile(source, player.Center - new Vector2(player.width / 2f, 0f) + new Vector2(4f, -4f), Vector2.Zero, type, 0, 0, player.whoAmI, 120 * i);

        return false;
    }

    public override bool CanUseItem(Player player)
        => player.ownedProjectileCounts[ModContent.ProjectileType<AmuletOfLifeWisps>()] == 0 ? true : false;
}
