using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class SharpIcicle : NatureProjectile {
    protected override void SafeSetDefaults() {
        int width = 14, height = 18;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;

        Projectile.tileCollide = true;
        Projectile.friendly = true;

        Projectile.aiStyle = -1;

        Projectile.timeLeft = 200;
        Projectile.penetrate = 1;
    }

    protected override void SafeOnSpawn(IEntitySource source) => Projectile.ai[2] = Projectile.velocity.Length();

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 4;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity);

        if (Projectile.ai[1] == 1f) {
            //if (Projectile.ai[0] % 4f == 0f) {
            //    Projectile.damage++;
            //}
            Projectile.velocity.Y += Projectile.ai[2] * 0.1f;
            Projectile.velocity.Y = Math.Min(15f, Projectile.velocity.Y);
            Projectile.velocity.X *= 0.95f;
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 8, 8, 176, 0, 0, 0, new Color(Color.Azure.ToVector3()), 1.2f);
            Main.dust[dust].velocity *= 0.5f;
            Main.dust[dust].scale *= 0.6f;
            Main.dust[dust].noLight = true;
        }

        if (player.whoAmI != Main.myPlayer) {
            return;
        }

        Vector2 pointPosition = player.GetViableMousePosition();
        Projectile.ai[0]++;
        bool flag = Projectile.ai[0] > 35f;
        if ((pointPosition.X >= player.position.X && Projectile.position.X >= pointPosition.X) || (pointPosition.X <= player.position.X && Projectile.position.X <= pointPosition.X) || flag) {
            Projectile.ai[1] = 1f;
        }
        Projectile.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 5; i++) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 8, 8, 176, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 0, new Color(Color.Azure.ToVector3()), 1.3f);
            Main.dust[dust].velocity *= 0.5f;
            Main.dust[dust].scale *= 0.6f;
            Main.dust[dust].noLight = true;
        }
        SoundEngine.PlaySound(SoundID.Item27, new Vector2(Projectile.position.X, Projectile.position.Y));
    }
}