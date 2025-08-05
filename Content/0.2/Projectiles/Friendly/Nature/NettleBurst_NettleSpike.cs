using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class NettleSpikeTip : NettleSpikeLeft { }

class NettleSpikeRight : NettleSpikeLeft { }
class NettleSpikeLeft : Vilethorn {
    protected override int AppearAlphaValue => 75;
    protected override int DisappearAlphaValue => 3;
    protected override bool CanSpawnBody => Type == ModContent.ProjectileType<NettleSpikeLeft>() || Type == ModContent.ProjectileType<NettleSpikeRight>();

    protected override void SafeSetDefaults2() {
        Projectile.ArmorPenetration = 10; // Added by TML.
    }

    protected override void Init() { }

    protected override void SpawnBody() {
        int num76 = Type;
        ushort leftType = (ushort)ModContent.ProjectileType<NettleSpikeLeft>();
        ushort rightType = (ushort)ModContent.ProjectileType<NettleSpikeRight>();
        ushort tipType = (ushort)ModContent.ProjectileType<NettleSpikeTip>();
        if (Type == leftType)
            num76 = rightType;
        else if (Type == rightType)
            num76 = leftType;

        if (Projectile.ai[1] >= 10f && Type != tipType)
            num76 = tipType;

        int num77 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + Projectile.velocity.X + (float)(Projectile.width / 2), Projectile.position.Y + Projectile.velocity.Y + (float)(Projectile.height / 2), Projectile.velocity.X, Projectile.velocity.Y, num76, Projectile.damage, Projectile.knockBack, Projectile.owner);
        Main.projectile[num77].damage = Projectile.damage;
        Main.projectile[num77].ai[1] = Projectile.ai[1] + 1f;
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, num77);
    }

    protected override void SpawnDusts() {
        for (int num78 = 0; num78 < 8; num78++) {
            int num79 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 7, Projectile.velocity.X * 0.025f, Projectile.velocity.Y * 0.025f, 200, default, 1.3f);
            Main.dust[num79].noGravity = true;
            Dust dust2 = Main.dust[num79];
            dust2.velocity *= 0.5f;
        }
    }
}
