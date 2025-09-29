using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.GlowMasks;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

[AutoloadGlowMask]
sealed class OvergrownSpear : ModItem {
    public override void SetStaticDefaults() {
        //Tooltip.SetDefault("Hitting the orbiting spheres releases a magic spear");
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BeastBow>();
    }

    public override void SetDefaults() {
        int width = 50; int height = 58;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 30;
        Item.useAnimation = 20;

        Item.noUseGraphic = true;
        Item.autoReuse = false;
        Item.noMelee = true;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 18;
        Item.knockBack = 5f;

        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item1;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Melee.OvergrownSpear>();
        Item.shootSpeed = 8f;

        Item.value = Item.sellPrice(0, 1, 50, 0);
    }
}

sealed class OvergrownSpearPlayer : ModPlayer {
    private bool _overgrownSphereSpawned;

    public override void UpdateEquips() {
        ushort type = (ushort)ModContent.ProjectileType<Projectiles.Friendly.Melee.OvergrownSphere>();
        if (Player.whoAmI == Main.myPlayer) {
            if (Player.inventory[Player.selectedItem].type == ModContent.ItemType<OvergrownSpear>() && Player.active && !Player.dead && !Player.GetFormHandler().IsInADruidicForm) {
                bool flag = Player.ownedProjectileCounts[type] <= 1;
                if (!_overgrownSphereSpawned && flag) {
                    int _randCord = Main.rand.Next(-20, 20);
                    for (int i = 0; i < 3; i++) {
                        Projectile.NewProjectile(Player.GetSource_Misc("orbsspawned"), Player.MountedCenter.X + _randCord, Player.MountedCenter.Y + _randCord, 0f, 0f, type, 0, 0f, Player.whoAmI, 1 * i, 0);
                    }
                    _overgrownSphereSpawned = true;
                }
                if (Player.ownedProjectileCounts[type] < 1) {
                    _overgrownSphereSpawned = false;
                }
            }
        }
    }
}
