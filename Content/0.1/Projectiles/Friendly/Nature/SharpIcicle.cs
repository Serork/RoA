using Microsoft.Xna.Framework;

using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SharpIcicle : NatureProjectile {
    protected override void SafeSetDefaults() {
        int width = 14, height = 18;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;

        Projectile.tileCollide = true;
        Projectile.friendly = true;

        Projectile.aiStyle = -1;

        Projectile.timeLeft = 200;
        Projectile.penetrate = 2;

        Projectile.Opacity = 0f;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Projectile.ai[2] = Projectile.velocity.Length();
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 4;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Projectile.ai[1] == 1f) {
            Player player = Main.player[Projectile.owner];
            int penalty = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(player.HeldItem.damage) * 2;
            modifiers.FinalDamage *= penalty / (float)player.HeldItem.damage;
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        if (Projectile.ai[1] == 1f) {
            Player player = Main.player[Projectile.owner];
            int penalty = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(player.HeldItem.damage) * 2;
            modifiers.FinalDamage *= penalty / (float)player.HeldItem.damage;
        }
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
        }

        if (Projectile.ai[1] == 1f) {
            if (Projectile.Opacity < 1f) {
                Projectile.Opacity += 0.1f;
            }
            Projectile.velocity.Y += Projectile.ai[2] * 0.1f;
            Projectile.velocity.Y = Math.Min(15f, Projectile.velocity.Y);
            Projectile.velocity.X *= 0.95f;
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 8, 8, 176, 0, 0, 0, new Color(Color.Azure.ToVector3()), 1.2f);
            Main.dust[dust].velocity *= 0.5f;
            Main.dust[dust].scale *= 0.6f;
            Main.dust[dust].noLight = true;
        }
        else {
            if (Projectile.Opacity < 0.8f) {
                Projectile.Opacity += 0.2f;
            }
        }

        if (player.whoAmI != Main.myPlayer) {
            return;
        }

        Vector2 pointPosition = player.GetWorldMousePosition();
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