using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class BrambleMaze : NatureItem {
    protected override void SafeSetDefaults() {
        int width = 42; int height = 48;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 20;
        Item.autoReuse = true;

        Item.noMelee = true;
        Item.knockBack = 2f;

        Item.damage = 6;
        NatureWeaponHandler.SetPotentialDamage(Item, 16);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.25f);

        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item20;

        Item.shootSpeed = 1f;
        Item.shoot = ModContent.ProjectileType<BrambleMazeRoot>();

        Item.value = Item.sellPrice(silver: 20);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        velocity = Vector2.Zero;
        position = player.GetPlayerCorePoint();
    }
}
