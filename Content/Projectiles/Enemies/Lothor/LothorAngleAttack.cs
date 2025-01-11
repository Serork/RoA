using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using RoA.Core;
using RoA.Core.Utility;

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
        Projectile.penetrate = 2;
        Projectile.extraUpdates = 1;
        Projectile.ignoreWater = true;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.aiStyle = -1;
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

    public override void AI() {
        Projectile.tileCollide = true;

        if (Collision.SolidCollision(Projectile.position - Vector2.One * 2f, Projectile.width + 4, Projectile.height + 4)) {
            Projectile.Kill();
        }

        NPC npc = Main.npc[(int)Projectile.ai[0]];
        if (!npc.active) {
            return;
        }

        Vector2 startPos = new(npc.position.X + npc.width / 2 * npc.direction, npc.position.Y + npc.height / 4);
        float value = npc.direction == -1 ? npc.width / 2.5f + 4 : 0;

        int bossCurrentFrame = (int)UsedBossFrame;
        switch (bossCurrentFrame) {
            case 0:
                startPos += new Vector2(-26 * npc.direction + value, -32);
                //10
                break;
            case 1:
                startPos += new Vector2(-24 * npc.direction + value, -32);
                //10
                break;
            case 2:
                startPos += new Vector2(-10 * npc.direction + value, -26);
                _distY = 150f;
                //11
                break;
            case 3:
                startPos += new Vector2(0 * npc.direction + value, -15);
                _distY = 100f;
                //12
                break;
            case 4:
                startPos += new Vector2(5 * npc.direction + value, -6);
                _distY = 50f;
                //13
                break;
            case 5:
                startPos += new Vector2(8 * npc.direction + value, 2);
                _distY = 50f;
                //13
                break;
        }

        bool flag = npc.direction == 1;
        if (flag) {
            startPos.X += 20f;
        }
        else {
            startPos.X -= 4f;
        }

        Vector2 destination = new(Projectile.ai[1], Projectile.ai[2]);
        destination.X -= npc.direction == -1 ? value : 0;
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
            Projectile.localAI[2] = 1f;

            Vector2 offset = new Vector2(10f, 10f);
            for (int num58 = 0; num58 < 2; num58++) {
                int num59 = Dust.NewDust(Projectile.position + offset, 0, 0, DustID.PoisonStaff, 0f, 0f, 0, default(Color), 1f + Main.rand.NextFloatRange(0.1f));
                Main.dust[num59].velocity = Main.dust[num59].velocity.RotatedByRandom(MathHelper.PiOver4);
                Main.dust[num59].velocity *= 0.2f;
                //Main.dust[num59].velocity += Projectile.velocity / 5f;
                Main.dust[num59].noGravity = true;
                Main.dust[num59].fadeIn = 1.25f;
            }
        }

        for (int i = 0; i < 3; i++) {
            float x = Projectile.velocity.X / 3f * i;
            float y = Projectile.velocity.Y / 3f * i;
            int deviation = 14;
            int dust = Dust.NewDust(new Vector2(Projectile.position.X - Projectile.width / 2 + deviation, Projectile.position.Y - Projectile.width / 2 + deviation),
                Projectile.width - deviation * 2,
                Projectile.height - deviation * 2,
                DustID.PoisonStaff, 0f, 0f, 100, default, 1.1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.1f;
            Main.dust[dust].velocity += Projectile.velocity * 0.5f;
            Dust dust2 = Main.dust[dust];
            dust2.position.X -= x;
            Dust dust3 = Main.dust[dust];
            dust3.position.Y -= y;
        }
    }
}