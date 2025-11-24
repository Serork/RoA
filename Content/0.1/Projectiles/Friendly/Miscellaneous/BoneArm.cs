using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Equipables.Accessories;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class BoneArm : ModProjectile {
    public override void SetDefaults() {
        int width = 24; int height = 24;
        Projectile.Size = new Vector2(width, height);

        Projectile.CloneDefaults(ProjectileID.SpikyBall);
        AIType = ProjectileID.SpikyBall;
        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 900;

        Projectile.DamageType = DamageClass.Default;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (player.GetModPlayer<CoffinPlayer>().hurtCount == 2) Projectile.Kill();
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.NPCHit2, Projectile.position);
        ushort _type = (ushort)ModContent.ProjectileType<BoneArm_Back>();
        Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center.X, Projectile.Center.Y, 0, 0, _type, Projectile.damage, Projectile.knockBack * 0.5f, Projectile.owner);
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}

sealed class BoneArm_Back : SkeletonBodyPart {
    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<BoneArm>()).Texture;
}