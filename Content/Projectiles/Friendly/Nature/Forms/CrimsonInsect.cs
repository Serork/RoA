using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

class CrimsonInsect : CorruptionInsect {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Crimson Swarmer");
        Main.projFrames[Projectile.type] = 4;
    }

    protected override void SafeSetDefaults() {
        base.SafeSetDefaults();
    }

    public override void AI() {
        base.AI();
    }

    public override void OnKill(int timeLeft) {
        for (int count = 0; count < 9; count++) {
            int dust = Dust.NewDust(Projectile.position, 10, 10, DustID.Ichor, 0, 0, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 0.6f;
            Main.dust[dust].velocity *= 0.4f;
        }
    }
}