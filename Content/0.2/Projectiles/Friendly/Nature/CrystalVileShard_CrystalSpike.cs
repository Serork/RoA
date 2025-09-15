using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CrystalSpikeTip : CrystalSpike { }

class CrystalSpike : Vilethorn {
    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    protected override int AppearAlphaValue => 100;
    protected override int DisappearAlphaValue => 4;
    protected override SoundStyle SpawnSoundID => SoundID.Item101;
    protected override bool CanSpawnBody => Type == ModContent.ProjectileType<CrystalSpike>();

    protected override void SafeSetDefaults2() {
        Projectile.ArmorPenetration = 10; // Added by TML.
    }

    public override void SafePostAI() {
        Projectile.hide = Projectile.Distance(Projectile.GetOwnerAsPlayer().Center) < 10f && Projectile.timeLeft > 190;

        float num = 0.2f;
        float num2 = 0.2f;
        float num3 = 0.2f;

        num2 *= 0.3f;

        Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), num, num2, num3);
    }

    protected override void SpawnBody() {
        int num73 = Type;
        ushort tipType = (ushort)ModContent.ProjectileType<CrystalSpikeTip>();
        if (Projectile.ai[1] >= (float)(7 + Main.rand.Next(2))) {
            num73 = tipType;
        }

        int num74 = Projectile.damage;
        float num75 = Projectile.knockBack;
        if (num73 == tipType) {
            num74 = (int)((double)Projectile.damage * 1.25);
            num75 = Projectile.knockBack * 1.25f;
        }

        int number = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + Projectile.velocity.X + (float)(Projectile.width / 2), Projectile.position.Y + Projectile.velocity.Y + (float)(Projectile.height / 2), Projectile.velocity.X, Projectile.velocity.Y, num73, num74, num75, Projectile.owner, 0f, Projectile.ai[1] + 1f);
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, number);
    }

    protected override void SpawnDusts() {
        for (int num80 = 0; num80 < 8; num80++) {
            int num81 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(68, 71), Projectile.velocity.X * 0.025f, Projectile.velocity.Y * 0.025f, 200, default, 1.3f);
            Main.dust[num81].noGravity = true;
            Dust dust2 = Main.dust[num81];
            dust2.velocity *= 0.5f;
        }
    }
}
