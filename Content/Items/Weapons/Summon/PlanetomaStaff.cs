using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon;

sealed class PlanetomaStaff : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(0, 1, 50));
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 14f;
        Item.shoot = ModContent.ProjectileType<PlanetomaStaffProjectile>();
        Item.damage = 14;
        Item.width = 18;
        Item.height = 20;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.noMelee = true;
        Item.knockBack = 4.5f;
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

sealed class PlanetomaStaffProjectile : ModProjectile {
    private int _direction;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + "Summon/PlanetomaSentry";

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;

        ProjectileID.Sets.NeedsUUID[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 38;
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

    public override void PostAI() => ProjectileUtils.Animate(Projectile, 6);

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[1] = 1f;
            Projectile.localAI[0] = 1f;

            for (int i = 0; i < Projectile.ai.Length; i++) {
                Projectile.ai[i] = 20f * (i + 1);
            }

            int num430 = 80;
            for (int num433 = 0; num433 < num430; num433++) {
                int num434 = Dust.NewDust(Projectile.position + Vector2.UnitY * 10f, Projectile.width, Projectile.height - 10, DustID.CorruptGibs, 0f, 0f, 100);
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
            Projectile.ai[1] = 0f;
            Projectile.ai[2] = 0f;
            SoundEngine.PlaySound(SoundID.Item46, Projectile.position);
        }

        Projectile.direction = Projectile.spriteDirection = _direction;

        Projectile.velocity.X = 0f;
        Projectile.velocity.Y += 0.2f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;

        int type = ModContent.ProjectileType<PlanetomaStaffProjectile2>();
        bool activeSelfWith(int ai) {
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner != Main.myPlayer) {
                    continue;
                }
                if (projectile.type != type) {
                    continue;
                }
                if ((int)projectile.ai[2] == ai) {
                    return true;
                }
            }

            return false;
        }

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

        if (Collision.CanHit(Projectile, Main.player[Projectile.owner]) || flag22) {
            float[] attackRates = [20f, 40f, 60f];
            for (int i = 0; i < attackRates.Length; i++) {
                if (Projectile.ai[i] < attackRates[i]) {
                    if (!activeSelfWith(i + 1)) {
                        Projectile.ai[i]++;
                    }
                }
                else if (Projectile.owner == Main.myPlayer) {
                    int whoAmI = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack, Projectile.owner, whoAmI, 0f, i + 1);
                    Projectile.ai[i] = 0f;
                    Projectile.netUpdate = true;
                }
            }
        }
    }
}

sealed class PlanetomaStaffProjectile2 : ModProjectile {
    private int _direction;
    private Vector2 _previousCenter;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + "Summon/PlanetomaSentryProjectile";

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;

        ProjectileID.Sets.NeedsUUID[Type] = true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_direction);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _direction = reader.ReadInt32();
    }

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 36000;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.netImportant = true;

        Projectile.friendly = true;

        Projectile.tileCollide = false;

        Projectile.penetrate = 2;
    }

    //public override bool? CanDamage() => Projectile.Opacity >= 0.5f;

    public override bool? CanCutTiles() => false;

    public override void PostAI() => ProjectileUtils.Animate(Projectile, 4);

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }

    public override void AI() {
        if (_previousCenter != Projectile.Center) {
            _previousCenter = Projectile.Center;
        }

        if (Projectile.owner == Main.myPlayer) {
            if (_direction == 0) {
                _direction = (int)Main.rand.NextBool().ToDirectionInt();
                Projectile.netUpdate = true;
            }
        }

        if (Main.rand.NextBool(10)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs);
            dust.noGravity = true;
        }

        Projectile.velocity = Vector2.Zero;
        int meInQueue = (int)Projectile.ai[2];
        if (Projectile.ai[1] == 0f) {
            SoundEngine.PlaySound(SoundID.Item18 with { Pitch = -0.2f, Volume = 0.3f }, Projectile.Center);
        }
        Projectile.ai[1] += (1f + 0.1f * meInQueue) * _direction;
        float counter = Projectile.ai[1] / 180f;
        float angle = (float)Math.PI * 2f;
        float offset = 20f + 20f * meInQueue;
        int byUUID = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[0]);
        if (byUUID == -1) {
            Projectile.Kill();
            return;
        }
        if (Main.projectile.IndexInRange(byUUID)) {
            Projectile following = Main.projectile[byUUID];
            //if (!Collision.CanHit(following, Main.player[Projectile.owner])) {
            //    Projectile.Kill();
            //    return;
            //}
            Projectile.Center = following.Center + (counter * ((float)Math.PI * 2f) + angle * (float)meInQueue).ToRotationVector2() * offset;
            if (Projectile.Opacity > 0f) {
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, (Projectile.Center - _previousCenter).ToRotation(), 0.5f);
            }
        }

        Projectile.Opacity = Utils.GetLerpValue(36000, 36000 - 10, Projectile.timeLeft, true);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 6; i++) {
            Vector2 vector39 = Projectile.position;
            Point size = Projectile.Size.ToPoint();
            Dust obj2 = Main.dust[Dust.NewDust(vector39, size.X, size.Y, DustID.CorruptGibs, Projectile.oldVelocity.X, -2f, 0, default, 1.1f + 0.15f * Main.rand.NextFloat())];
            obj2.noGravity = true;
        }
    }
}