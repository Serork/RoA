using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class BloodshedAxesTarget : ModProjectile {
    private int NPC => (int)Projectile.ai[0];

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        int width = 14; int height = 14;
        Projectile.Size = new Vector2(width, height);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 300;
        Projectile.ignoreWater = true;
    }

    public override bool? CanDamage()
        => false;

    public override void AI() {
        NPC target = Main.npc[NPC];
        if (!target.active) {
            Projectile.Kill();
        }
        Player player = Main.player[Projectile.owner];
        if (player.HeldItem.type != ModContent.ItemType<Items.Weapons.Melee.BloodshedAxe>()) {
            Projectile.Kill();
        }
        Projectile.Center = target.Center;
        target.AddBuff(ModContent.BuffType<BloodshedAxesDebuff>(), 10);
        if (player.whoAmI == Main.myPlayer) {
            if (player.itemAnimation < player.itemAnimationMax / 2 - player.itemAnimationMax / 4 && Projectile.ai[1] == 0f) {
                Projectile.ai[1] = 1f;
                Projectile.netUpdate = true;
            }
            if (player.ItemAnimationJustStarted && Projectile.ai[1] == 2f) {
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }
        }
        if (Projectile.ai[1] != 1f || Projectile.ai[1] == 2f) {
            return;
        }

        Projectile.ai[1] = 2f;
        int type = ModContent.ProjectileType<BloodshedAxeEnergy>(); //spawn axes at target position
        int damage = Projectile.damage;
        float knockback = Projectile.knockBack;
        Dusts(target);

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new BloodshedAxeHitDustPacket(player, Projectile.identity, NPC));
        }

        float dir = Main.rand.NextBool() ? -1f : 1f;

        if (Projectile.owner == Main.myPlayer) {
            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI("Hit by Bloodshed Axe"), target.Center, Vector2.Zero, type, damage, knockback, Projectile.owner, target.whoAmI, dir);
        }
    }

    internal static void Dusts(int identity, int whoAmI) {
        NPC npc = Main.npc[whoAmI];
        Projectile prpjectile = Main.projectile[identity];
        for (int num251 = 0; num251 < 4; num251++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, npc.velocity.X, npc.velocity.Y, 150, default, 1.2f);
            Main.dust[num252].position = (Main.dust[num252].position + prpjectile.Center) / 2f;
            Main.dust[num252].noGravity = true;
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.5f;
        }
        for (int i = 0; i < 5; i++) {
            Color newColor = Main.hslToRgb((0.92f + Main.rand.NextFloat() * 0.02f) % 1f, 1f, 0.4f + Main.rand.NextFloat() * 0.25f);
            int num4 = Dust.NewDust(npc.Center - new Vector2(0f, npc.height / 4f), 0, 0, ModContent.DustType<VampParticle>(), 0f, 0f, 0, newColor);
            Main.dust[num4].velocity = Main.rand.NextVector2Circular(2f, 2f);
            Main.dust[num4].velocity -= -prpjectile.velocity * (0.5f + 0.5f * Main.rand.NextFloat()) * 1.4f;
            Main.dust[num4].noGravity = true;
            Main.dust[num4].scale = 1f;
            Main.dust[num4].position -= Main.rand.NextVector2Circular(16f, 16f);
            Main.dust[num4].velocity = prpjectile.velocity;
            if (num4 != 6000) {
                Dust dust = Dust.CloneDust(num4);
                dust.scale /= 2f;
                dust.fadeIn *= 0.75f;
                dust.color = new Color(255, 255, 255, 255);
            }
        }
        for (int num251 = 0; num251 < 3; num251++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, prpjectile.velocity.X, prpjectile.velocity.Y, 50, default, 1.2f);
            Main.dust[num252].position = (Main.dust[num252].position + prpjectile.Center) / 2f;
            Main.dust[num252].noGravity = true;
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.5f;
        }
        for (int num253 = 0; num253 < 2; num253++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, prpjectile.velocity.X, prpjectile.velocity.Y, 50, default, 0.4f);
            switch (num253) {
                case 0:
                    Main.dust[num252].position = (Main.dust[num252].position + prpjectile.Center * 5f) / 6f;
                    break;
                case 1:
                    Main.dust[num252].position = (Main.dust[num252].position + (prpjectile.Center + prpjectile.velocity / 2f) * 5f) / 6f;
                    break;
            }
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.1f;
            Main.dust[num252].noGravity = true;
            Main.dust[num252].fadeIn = 1f;
        }
    }

    private void Dusts(NPC npc) {
        for (int num251 = 0; num251 < 4; num251++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, npc.velocity.X, npc.velocity.Y, 150, default, 1.2f);
            Main.dust[num252].position = (Main.dust[num252].position + Projectile.Center) / 2f;
            Main.dust[num252].noGravity = true;
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.5f;
        }
        for (int i = 0; i < 5; i++) {
            Color newColor = Main.hslToRgb((0.92f + Main.rand.NextFloat() * 0.02f) % 1f, 1f, 0.4f + Main.rand.NextFloat() * 0.25f);
            int num4 = Dust.NewDust(npc.Center - new Vector2(0f, npc.height / 4f), 0, 0, ModContent.DustType<VampParticle>(), 0f, 0f, 0, newColor);
            Main.dust[num4].velocity = Main.rand.NextVector2Circular(2f, 2f);
            Main.dust[num4].velocity -= -Projectile.velocity * (0.5f + 0.5f * Main.rand.NextFloat()) * 1.4f;
            Main.dust[num4].noGravity = true;
            Main.dust[num4].scale = 1f;
            Main.dust[num4].position -= Main.rand.NextVector2Circular(16f, 16f);
            Main.dust[num4].velocity = Projectile.velocity;
            if (num4 != 6000) {
                Dust dust = Dust.CloneDust(num4);
                dust.scale /= 2f;
                dust.fadeIn *= 0.75f;
                dust.color = new Color(255, 255, 255, 255);
            }
        }
        for (int num251 = 0; num251 < 3; num251++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 1.2f);
            Main.dust[num252].position = (Main.dust[num252].position + Projectile.Center) / 2f;
            Main.dust[num252].noGravity = true;
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.5f;
        }
        for (int num253 = 0; num253 < 2; num253++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 0.4f);
            switch (num253) {
                case 0:
                    Main.dust[num252].position = (Main.dust[num252].position + Projectile.Center * 5f) / 6f;
                    break;
                case 1:
                    Main.dust[num252].position = (Main.dust[num252].position + (Projectile.Center + Projectile.velocity / 2f) * 5f) / 6f;
                    break;
            }
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.1f;
            Main.dust[num252].noGravity = true;
            Main.dust[num252].fadeIn = 1f;
        }
    }
}