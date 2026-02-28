using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class BoneHarpyFeather : ModProjectile {
    private const int MAXTIMELEFT = 120;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;

        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults() {
        int width = 20; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;
        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Summon;

        Projectile.aiStyle = -1;
        Projectile.penetrate = 2;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;

        Projectile.tileCollide = false;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        if (Projectile.timeLeft > 100) return false;
        else return true;
    }

    public override void AI() {
        if (Projectile.timeLeft == 100) Projectile.tileCollide = true;

        Projectile.Opacity = Utils.GetLerpValue(MAXTIMELEFT, MAXTIMELEFT - 10f, Projectile.timeLeft, clamped: true) * Utils.GetLerpValue(0f, 10f, Projectile.timeLeft, clamped: true);
        Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + (float)Math.PI / 2f;
        Projectile.position += Projectile.velocity;
    }

    public override void OnKill(int timeLeft) {
        int ind4 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone, 0f, 0f, 0, default, 1f);
        Main.dust[ind4].velocity *= 0.95f;
        Main.dust[ind4].noGravity = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture2D = Projectile.GetTexture();
        Vector2 origin = new(texture2D.Width * 0.5f, Projectile.height * 0.5f);
        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            Vector2 position = Projectile.oldPos[i] - Main.screenPosition + origin + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor.MultiplyRGB(BoneHarpy.TrailColor)) * ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture2D, position, new Rectangle?(), color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}
