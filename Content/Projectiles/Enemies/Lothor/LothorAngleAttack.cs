using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorAngleAttack : ModProjectile {
    private float _distX = 0f;
    private float _distY = 250f;
    private float _timer;
    private NPC _owner;

    private Vector2 _startPosition;

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
        Projectile.timeLeft = 300;
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
        bool enraged = Projectile.localAI[0] == 1f;
        for (int value = 0; value < 11 + Main.rand.Next(0, 5); value++) {
            int dust = Dust.NewDust(Projectile.position, 2, 2, enraged ? ModContent.DustType<LothorPoison2>() : ModContent.DustType<LothorPoison>(), 0f, -0.5f, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width,
              Projectile.height, ModContent.DustType<LothorPoison>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default, 0.6f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = Main.dust[dust].noLightEmittence = true;
        }

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y + 1f, 0f, 0f, ModContent.ProjectileType<LothorAngleAttack2>(), Projectile.damage * 2, 0f, Projectile.owner, 0f, 0f, enraged.ToInt());
        }
    }

    public override void AI() {
        if (Projectile.timeLeft > 100) {
            Projectile.tileCollide = false;
        }
        else {
            Projectile.tileCollide = true;
        }
        if (_owner == null) {
            foreach (NPC checkNPC in Main.ActiveNPCs) {
                if (checkNPC.type == ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Lothor>() && Vector2.Distance(checkNPC.Center, Projectile.Center) < 10f) {
                    _owner = checkNPC;
                    break;
                }
            }
        }
        NPC npc = _owner;
        int bossCurrentFrame = (int)Projectile.ai[0];
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = npc.direction;

            int spitCount = 4 - bossCurrentFrame;
            SoundEngine.PlaySound(SoundID.Item111 with { Pitch = -0.2f + spitCount * 0.1f, PitchVariance = 0.1f }, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Splash") { Volume = 0.6f, Pitch = -0.5f + spitCount * 0.1f }, Projectile.Center);

            _startPosition = npc.position;
        }

        if (npc.active && npc.ModNPC != null && npc.As<NPCs.Enemies.Bosses.Lothor.Lothor>()._shouldEnrage) {
            Projectile.localAI[0] = 1f;
        }

        int npcDirection = (int)Projectile.localAI[1];

        if (Collision.SolidCollision(Projectile.position - Vector2.One * 2f, Projectile.width + 4, Projectile.height + 4)) {
            Projectile.Kill();
        }

        Vector2 startPos = new(_startPosition.X + npc.width / 2 * npcDirection, _startPosition.Y + npc.height / 4);
        float value = npcDirection == -1 ? npc.width / 2.5f + 4 : 0;

        switch (bossCurrentFrame) {
            case 0:
                startPos += new Vector2(-26 * npcDirection + value, -32);
                break;
            case 1:
                startPos += new Vector2(-24 * npcDirection + value, -32);
                break;
            case 2:
                startPos += new Vector2(-10 * npcDirection + value, -26);
                //_distY = 150f;
                break;
            case 3:
                startPos += new Vector2(0 * npcDirection + value, -15);
                //_distY = 100f;
                break;
            case 4:
                startPos += new Vector2(5 * npcDirection + value, -6);
                //_distY = 50f;
                break;
            case 5:
                startPos += new Vector2(8 * npcDirection + value, 2);
                //distY = 50f;
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

        float speed = _timer < 0.4f ? 0.015f : 0.01f;
        speed *= 0.8f + 0.2f * npc.As<NPCs.Enemies.Bosses.Lothor.Lothor>().LifeProgress;
        _timer += speed;

        bool enraged = Projectile.localAI[0] == 1f;

        float counting = 0.0f;
        if (counting < 1.0f) {
            counting += 1.0f * _timer;
            Vector2 start = Vector2.Lerp(startPos, dev, counting);
            Vector2 end = Vector2.Lerp(dev, destination, counting);
            Projectile.position = Vector2.Lerp(start, end, counting);
        }

        int dust = enraged ? ModContent.DustType<LothorPoison2>() : ModContent.DustType<LothorPoison>();

        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = npc.direction;

            Vector2 offset = new(8f, 10f);
            for (int num58 = 0; num58 < 2; num58++) {
                int num59 = Dust.NewDust(Projectile.position + offset, 0, 0, dust, 0f, 0f, 0, default(Color), 1f + Main.rand.NextFloatRange(0.1f));
                Main.dust[num59].velocity = Main.dust[num59].velocity.RotatedByRandom(MathHelper.PiOver4);
                Main.dust[num59].velocity *= 0.2f;
                //Main.dust[num59].velocity += Projectile.velocity / 5f;
                Main.dust[num59].noGravity = true;
                Main.dust[num59].fadeIn = 1.25f;
                Main.dust[num59].noLight = Main.dust[num59].noLightEmittence = true;
            }
        }

        if (flag) {
            Projectile.position.Y += 6f;
        }
        else {
            Projectile.position.Y += 3f;
        }

        if (Projectile.timeLeft > 300 - 1) {
            return;
        }

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Vector2 position = Projectile.position + Vector2.One * 4f;
            for (int i = 0; i < Main.rand.Next(1, 4); i++) {
                if (Main.rand.NextBool(5)) {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), position.X, position.Y,
                        velocity.X, velocity.Y, ModContent.ProjectileType<PoisonBubble_Large>(), Projectile.damage, 0f, Projectile.owner, enraged.ToInt());
                }
            }
            for (int i = 0; i < Main.rand.Next(1, 4); i++) {
                if (Main.rand.NextBool(5)) {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), position.X, position.Y,
                        velocity.X, velocity.Y, ModContent.ProjectileType<PoisonBubble_Small>(), Projectile.damage, 0f, Projectile.owner, enraged.ToInt());
                }
            }
        }

        float num3 = 0f;
        //float y = 0f;
        Vector2 vector6 = Projectile.position;
        Vector2 vector7 = Projectile.oldPosition;
        vector7.Y -= num3 / 2f;
        vector6.Y -= num3 / 2f;
        int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
        if (Vector2.Distance(vector6, vector7) % 3f != 0f)
            num5++;

        for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
            Dust obj = Main.dust[Dust.NewDust(Projectile.position, 0, 0, dust, Alpha: 100, Scale: 1.1f)];
            obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5) + new Vector2(Projectile.width, Projectile.height) / 2f + Main.rand.RandomPointInArea(2f);
            obj.noGravity = true;
            obj.velocity *= 0.1f;
            obj.velocity += Projectile.velocity * 0.5f;
            obj.noLight = obj.noLightEmittence = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_timer);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _timer = reader.ReadSingle();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        int time = 180;
        target.AddBuff(BuffID.Poisoned, time, true);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        int time = 180;
        target.AddBuff(BuffID.Poisoned, time, true);
    }
}