using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon;

sealed class GastroIntestinalMallet : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(0, 1, 50));
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 14f;
        Item.shoot = ModContent.ProjectileType<GastroIntestinalMalletProjectile>();
        Item.damage = 24;
        Item.width = 18;
        Item.height = 20;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.noMelee = true;
        Item.knockBack = 7.5f;
        //Item.summon = true;
        Item.mana = 20;
        Item.sentry = true;

        Item.DamageType = DamageClass.Summon;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        bool num103 = false;
        int num104 = (int)((float)Main.mouseX + Main.screenPosition.X) / 16;
        int num105 = (int)((float)Main.mouseY + Main.screenPosition.Y) / 16;
        if (player.gravDir == -1f)
            num105 = (int)(Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY) / 16;

        if (!num103) {
            for (; num105 < Main.maxTilesY - 10 && Main.tile[num104, num105] != null && !WorldGen.SolidTile2(num104, num105) && Main.tile[num104 - 1, num105] != null && !WorldGen.SolidTile2(num104 - 1, num105) && Main.tile[num104 + 1, num105] != null && !WorldGen.SolidTile2(num104 + 1, num105); num105++) {
            }

            num105--;
        }

        Projectile.NewProjectile(player.GetSource_ItemUse(Item), (float)Main.mouseX + Main.screenPosition.X, num105 * 16 - 24, 0f, 15f, type, damage, knockback, player.whoAmI);

        return false;
    }
}

sealed class GastroIntestinalMalletProjectile : ModProjectile {
    public override string Texture => ResourceManager.ProjectileTextures + "GastroIntestinalMallet";

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 46;
        //Projectile.aiStyle = 53;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 36000;
        Projectile.ignoreWater = true;
        Projectile.sentry = true;
        Projectile.netImportant = true;

        Projectile.tileCollide = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override void AI() {
        if (Projectile.owner == Main.myPlayer) {
            for (int num825 = 0; num825 < 1000; num825++) {
                if (num825 != Projectile.whoAmI && Main.projectile[num825].active && Main.projectile[num825].owner == Projectile.owner && Main.projectile[num825].type == Projectile.type) {
                    if (Projectile.timeLeft >= Main.projectile[num825].timeLeft)
                        Main.projectile[num825].Kill();
                    else
                        Projectile.Kill();
                }
            }
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[1] = 1f;
            Projectile.localAI[0] = 1f;
            Projectile.ai[0] = 120f;
            int num430 = 80;
            SoundEngine.PlaySound(SoundID.Item46, Projectile.position);
        }

        Projectile.velocity.X = 0f;
        Projectile.velocity.Y += 0.2f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;
    }
}