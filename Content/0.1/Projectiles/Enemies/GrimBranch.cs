using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies;

sealed class GrimBranch : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    private int vibechecker;
    private int reducer;
    private int gobacktimer;
    private float rememberRotation;
    private bool initialize = false;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults() {
        int width = 8; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.CloneDefaults(ProjectileID.WoodenArrowHostile);

        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 100;

        Projectile.tileCollide = false;

        Projectile.alpha = byte.MaxValue;

        Projectile.friendly = false;
        Projectile.hostile = true;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (!initialize) {
            Projectile.timeLeft = (int)Projectile.ai[0];
            if (Projectile.ai[0] != 100) {
                reducer = 40;
                vibechecker = -1;
                rememberRotation = Projectile.ai[1];
                Projectile.velocity -= new Vector2(0, 5).RotatedBy(rememberRotation);
            }
            else {
                reducer = 0;
                rememberRotation = Projectile.rotation;
            }
            initialize = true;
        }
        for (int i = 0; i < 8; i++) {
            float centx = Projectile.Center.X - Projectile.velocity.X / 8f * i;
            float centy = Projectile.Center.Y - Projectile.velocity.Y / 8f * i;
            int dust = Dust.NewDust(Projectile.oldPos[i] + Projectile.Size / 2f - Vector2.One * 2, 4, 4, ModContent.DustType<GrimDruidDust>(), 0f, 0f, 255, Scale: 0.8f);
            Dust dust2 = Main.dust[dust];
            dust2.noGravity = true;
            dust2.position.X = centx;
            dust2.position.Y = centy;
            dust2.fadeIn = 1.05f;
        }
        if (Projectile.ai[0] != 100) {
            if (gobacktimer == 5 && Projectile.timeLeft > 20) {
                Projectile.velocity += new Vector2(0, 5).RotatedBy(rememberRotation);
                vibechecker = 0;
            }
            gobacktimer++;
        }
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            if (Main.rand.Next(reducer) == 0 && Projectile.timeLeft < 70 && Projectile.timeLeft > 20 && vibechecker == 0 || Projectile.ai[0] == 100 && Projectile.timeLeft == 80) {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, Projectile.velocity.X, Projectile.velocity.Y, Type, Projectile.damage, 0f, Projectile.owner, Projectile.timeLeft, rememberRotation);
                Projectile.velocity += new Vector2(0, 5).RotatedBy(rememberRotation);
                vibechecker = Projectile.timeLeft;
                reducer += 40;
                Projectile.netUpdate = true;
            }
        }
        if (vibechecker - Projectile.timeLeft >= 5 && Projectile.timeLeft > 20) {
            Projectile.velocity -= new Vector2(0, 5).RotatedBy(rememberRotation);
            vibechecker = 0;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(vibechecker);
        writer.Write(reducer);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        vibechecker = reader.ReadInt32();
        reducer = reader.ReadInt32();
    }

    public override void OnKill(int timeLeft) {
        //SoundEngine.PlaySound(SoundID.Grass, new Vector2(Projectile.position.X, Projectile.position.Y));
        for (int i = 0; i < 5; i++) {
            int dust = Dust.NewDust(Projectile.position - Projectile.velocity * 1f, 0, 0, ModContent.DustType<GrimDruidDust>(), 0, 0, 255, Scale: 0.8f);
            Main.dust[dust].velocity *= 0.1f;
            Main.dust[dust].velocity += Projectile.velocity;
            Main.dust[dust].velocity *= 0.5f;
            if (Main.rand.NextBool()) {
                Main.dust[dust].velocity *= 0.5f;
            }
            if (Main.rand.NextBool()) {
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }
}
