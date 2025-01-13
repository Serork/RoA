using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorAngleAttack : ModProjectile {
    private float _distX = 0f;
    private float _distY = 250f;
    private float _timer;

    public int UsedBossFrame;

    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        Projectile.Size = Vector2.One * 16f;
        Projectile.penetrate = -1;
        Projectile.extraUpdates = 1;
        Projectile.ignoreWater = true;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 180;
        Projectile.netImportant = true;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = 16;
        height = 16;

        return true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (targetHitbox.Width > 8 && targetHitbox.Height > 8) {
            targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
        }
        return projHitbox.Intersects(targetHitbox);
    }

    public override void OnKill(int timeLeft) {
        for (int value = 0; value < 11 + Main.rand.Next(0, 5); value++) {
            int dust = Dust.NewDust(Projectile.position, 2, 2, DustID.PoisonStaff, 0f, -0.5f, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width,
              Projectile.height, DustID.PoisonStaff, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default, 0.6f);
            Main.dust[dust].noGravity = true;
        }

        int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<LothorAngleAttack2>(), Projectile.damage * 2, 0f, Projectile.owner, 0f, 0f);
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
    }

    public override void AI() {
        Projectile.tileCollide = true;

        NPC npc = Main.npc[(int)Projectile.ai[0]];
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = npc.direction;

        }
        int npcDirection = (int)Projectile.localAI[1];

        if (Collision.SolidCollision(Projectile.position - Vector2.One * 2f, Projectile.width + 4, Projectile.height + 4)) {
            Projectile.Kill();
        }

        if (!npc.active) {
            return;
        }

        Vector2 startPos = new(npc.position.X + npc.width / 2 * npcDirection, npc.position.Y + npc.height / 4);
        float value = npcDirection == -1 ? npc.width / 2.5f + 4 : 0;

        int bossCurrentFrame = (int)UsedBossFrame;
        switch (bossCurrentFrame) {
            case 0:
                startPos += new Vector2(-26 * npcDirection + value, -32);
                break;
            case 1:
                startPos += new Vector2(-24 * npcDirection + value, -32);
                break;
            case 2:
                startPos += new Vector2(-10 * npcDirection + value, -26);
                _distY = 150f;
                break;
            case 3:
                startPos += new Vector2(0 * npcDirection + value, -15);
                _distY = 100f;
                break;
            case 4:
                startPos += new Vector2(5 * npcDirection + value, -6);
                _distY = 50f;
                break;
            case 5:
                startPos += new Vector2(8 * npcDirection + value, 2);
                _distY = 50f;
                break;
        }

        bool flag = npcDirection == 1;
        if (flag) {
            startPos.X += 20f;
        }
        else {
            startPos.X -= 4f;
        }

        Vector2 destination = new(Projectile.ai[1], Projectile.ai[2]);
        destination.X -= npcDirection == -1 ? value : 0;
        Vector2 mid = startPos + (destination - startPos) / 2;
        Vector2 dev = mid - new Vector2(0, _distY - _distX);

        float speed = 0.015f;
        _timer += speed;

        float counting = 0.0f;
        if (counting < 1.0f) {
            counting += 1.0f * _timer;
            Vector2 start = Vector2.Lerp(startPos, dev, counting);
            Vector2 end = Vector2.Lerp(dev, destination, counting);
            Projectile.position = Vector2.Lerp(start, end, counting);
        }

        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = npc.direction;

            Vector2 offset = new(8f, 10f);
            for (int num58 = 0; num58 < 2; num58++) {
                int num59 = Dust.NewDust(Projectile.position + offset, 0, 0, DustID.PoisonStaff, 0f, 0f, 0, default(Color), 1f + Main.rand.NextFloatRange(0.1f));
                Main.dust[num59].velocity = Main.dust[num59].velocity.RotatedByRandom(MathHelper.PiOver4);
                Main.dust[num59].velocity *= 0.2f;
                //Main.dust[num59].velocity += Projectile.velocity / 5f;
                Main.dust[num59].noGravity = true;
                Main.dust[num59].fadeIn = 1.25f;
            }
        }

        if (flag) {
            Projectile.position.Y += 6f;
        }
        else {
            Projectile.position.Y += 3f;
        }

        if (Projectile.timeLeft > 180 - 1) {
            return;
        }

        float num3 = 0f;
        float y = 0f;
        Vector2 vector6 = Projectile.position;
        Vector2 vector7 = Projectile.oldPosition;
        vector7.Y -= num3 / 2f;
        vector6.Y -= num3 / 2f;
        int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
        if (Vector2.Distance(vector6, vector7) % 3f != 0f)
            num5++;

        for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
            Dust obj = Main.dust[Dust.NewDust(Projectile.position, 0, 0, DustID.PoisonStaff, Alpha: 100, Scale: 1.1f)];
            obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5) + new Vector2(Projectile.width, Projectile.height) / 2f;
            obj.noGravity = true;
            obj.velocity *= 0.1f;
            obj.velocity += Projectile.velocity * 0.5f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        int time = 120;
        target.AddBuff(BuffID.Poisoned, time, true);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        int time = 120;
        target.AddBuff(BuffID.Poisoned, time, true);
    }
}