using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Accessories;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class BoneLeg : ModProjectile {
    public override void SetDefaults() {
        int width = 12; int height = 18;
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
        ushort _type = (ushort)ModContent.ProjectileType<BoneLeg_Back>();
        Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center.X, Projectile.Center.Y, 0, 0, _type, Projectile.damage, 1f, Projectile.owner);
    }
}

sealed class BoneLeg_Back : SkeletonBodyPart {
    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<BoneLeg>()).Texture;
}