using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

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
        Item.damage = 25;
        Item.width = 18;
        Item.height = 20;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.noMelee = true;
        Item.knockBack = 2.5f;
        //Item.summon = true;
        Item.mana = 18;
        Item.sentry = true;

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 1, 50, 0);

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

        Projectile.NewProjectile(player.GetSource_ItemUse(Item), (float)Main.mouseX + Main.screenPosition.X, num105 * 16 - 7f, 0f, 15f, type, damage, knockback, player.whoAmI);

        player.UpdateMaxTurrets();

        return false;
    }
}

sealed class GastroIntestinalMalletProjectile : ModProjectile {
    private int _direction;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + "Summon/GastroIntestinalMallet";

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults() {
        Projectile.width = 46;
        Projectile.height = 40;
        //Projectile.aiStyle = 53;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 36000;
        Projectile.ignoreWater = true;
        Projectile.sentry = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.netImportant = true;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        fallThrough = false;
        return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_direction);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _direction = reader.ReadInt32();
    }

    public override void AI() {
        float attackRate = 120f;
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[1] = 1f;
            Projectile.localAI[0] = 1f;

            int num430 = 80;
            for (int num433 = 0; num433 < num430; num433++) {
                int num434 = Dust.NewDust(Projectile.position + Vector2.UnitY * 10f, Projectile.width, Projectile.height - 10, 5, 0f, 0f, 100);
                Main.dust[num434].scale = (float)Main.rand.Next(1, 10) * 0.25f;
                Main.dust[num434].noGravity = true;
                Main.dust[num434].fadeIn = 0.5f;
                Dust dust2 = Main.dust[num434];
                dust2.velocity *= 0.75f;
            }

            if (Projectile.owner == Main.myPlayer) {
                _direction = (int)Main.rand.NextBool().ToDirectionInt();
                Projectile.netUpdate = true;
            }

            Projectile.ai[0] = 0f;
            SoundEngine.PlaySound(SoundID.Item46, Projectile.position);
        }

        Projectile.direction = Projectile.spriteDirection = _direction;

        Projectile.velocity.X = 0f;
        Projectile.velocity.Y += 0.2f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;

        bool flag22 = false;
        float num439 = Projectile.Center.X;
        float num440 = Projectile.Center.Y;
        float num441 = 400f;
        int num442 = -1;
        NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
        if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(this)) {
            float num443 = ownerMinionAttackTargetNPC.position.X + (float)(ownerMinionAttackTargetNPC.width / 2);
            float num444 = ownerMinionAttackTargetNPC.position.Y + (float)(ownerMinionAttackTargetNPC.height / 2);
            float num445 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num443) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num444);
            if (num445 < num441 && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, ownerMinionAttackTargetNPC.position, ownerMinionAttackTargetNPC.width, ownerMinionAttackTargetNPC.height)) {
                num441 = num445;
                num439 = num443;
                num440 = num444;
                flag22 = true;
                num442 = ownerMinionAttackTargetNPC.whoAmI;
            }
        }

        if (!flag22) {
            for (int num446 = 0; num446 < 200; num446++) {
                if (Main.npc[num446].CanBeChasedBy(this)) {
                    float num447 = Main.npc[num446].position.X + (float)(Main.npc[num446].width / 2);
                    float num448 = Main.npc[num446].position.Y + (float)(Main.npc[num446].height / 2);
                    float num449 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num447) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num448);
                    if (num449 < num441 && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, Main.npc[num446].position, Main.npc[num446].width, Main.npc[num446].height)) {
                        num441 = num449;
                        num439 = num447;
                        num440 = num448;
                        flag22 = true;
                        num442 = Main.npc[num446].whoAmI;
                    }
                }
            }
        }

        if (flag22 || Projectile.ai[1] == 1f) {
            Projectile.ai[0] -= 1f;
            if (Projectile.ai[0] <= 0f) {
                Projectile.ai[0] = 0f;
                Projectile.ai[1] = 1f;
            }

            if (Projectile.ai[1] == 1f) {
                Projectile.ai[2] += 1f;
                int frame = (int)Projectile.ai[2] / 8;
                Projectile.frame = frame;
                if (frame > Main.projFrames[Type] - 1) {
                    if (Projectile.owner == Main.myPlayer) {
                        int count = Main.rand.Next(4, 7);
                        for (int i = -count / 2 - 1; i < count / 2 + 2; i++) {
                            Vector2 velocity = new Vector2(0f, 4f + 1f * Main.rand.NextFloat()).RotatedBy(MathHelper.Pi - MathHelper.PiOver4 / 2f / i + Main.rand.NextFloatRange(0.25f));
                            if (flag22) {
                                velocity.X += Projectile.DirectionTo(Main.npc[num442].Center).SafeNormalize(Vector2.Zero).X * Vector2.Distance(Main.npc[num442].Center, Projectile.Center) * 0.01f;
                            }
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                Projectile.Center - new Vector2(_direction == 1 ? 4f : -4f, 4f), velocity,
                                ModContent.ProjectileType<GastroIntestinalMalletProjectile2>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        }
                    }

                    Projectile.ai[2] = Projectile.ai[1] = 0f;
                    Projectile.ai[0] = attackRate;
                    Projectile.frame = 0;
                }
            }
        }
    }
}

sealed class GastroIntestinalMalletProjectile2 : ModProjectile {
    public override string Texture => ResourceManager.FriendlyProjectileTextures + "Summon/" + nameof(GastroIntestinalMalletProjectile2);

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 18;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.aiStyle = 1;
        Projectile.alpha = 255;
        Projectile.penetrate = 1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.hide = true;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(4, 4);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
            Projectile.localAI[0] = 1f;
            for (int num163 = 0; num163 < 4; num163++) {
                Dust obj13 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 5, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj13.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
                obj13.scale = 1.1f;
                obj13.fadeIn = 1.1f;
                obj13.position = Projectile.Center - Vector2.UnitY * 6f;
                obj13.velocity *= Main.rand.NextFloat(0.5f, 1f);
                obj13.velocity *= 0.75f;
            }
        }

        Projectile.alpha -= 20;
        if (Projectile.alpha < 0)
            Projectile.alpha = 0;

        for (int num164 = 0; num164 < 2; num164++) {
            if (Main.rand.Next(3) == 0) {
                Dust obj14 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 5, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj14.velocity = obj14.velocity / 4f + Projectile.velocity / 2f;
                obj14.scale = 1.2f;
                obj14.position = Projectile.Center + Main.rand.NextFloat() * Projectile.velocity * 2f;
            }
        }

        for (int num165 = 1; num165 < Projectile.oldPos.Length && !(Projectile.oldPos[num165] == Vector2.Zero); num165++) {
            if (Main.rand.Next(6) == 0) {
                Dust obj15 = Main.dust[Dust.NewDust(Projectile.oldPos[num165], Projectile.width, Projectile.height, 5, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj15.velocity = obj15.velocity / 4f + Projectile.velocity / 2f;
                obj15.scale = 1.2f * (1f - (float)num165 / Projectile.oldPos.Length * 0.25f);
                obj15.position = Projectile.oldPos[num165] + Projectile.Size / 2f + Main.rand.NextFloat() * Projectile.velocity * 2f;
            }
        }
    }

    public override void OnKill(int timeLeft) {
        for (int num237 = 0; num237 < 12; num237++) {
            Dust dust35 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 5, Alpha: 100)];
            dust35.scale = 1.15f + 0.2f * Main.rand.NextFloat();
            dust35.velocity *= 0.5f + 0.5f * Main.rand.NextFloat();
        }
    }
}