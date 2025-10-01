using RoA.Common.Projectiles;
using RoA.Content.NPCs.Enemies.Backwoods.Hardmode;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Projectiles.Enemies;

sealed class WardenPurification2 : ModProjectile_NoTextureLoad {
    private static ushort TIMELEFT => 40;

    public override void SetDefaults() {
        Projectile.SetSizeValues(80);

        Projectile.hostile = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.hide = true;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        if (Projectile.localAI[0]++ > 5f) {
            Projectile.localAI[0] = 0f;

            if (!Helper.SinglePlayerOrServer) {
                return;
            }

            ProjectileUtils.SpawnHostileProjectile<WardenPurification>(new ProjectileUtils.SpawnHostileProjectileArgs(Projectile, Projectile.GetSource_FromAI()) {
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack,
                Position = Projectile.Center + Main.rand.NextVector2Circular(WardenOfTheWoods.TARGETDISTANCE, WardenOfTheWoods.TARGETDISTANCE) / 2f,
                AI0 = Projectile.ai[0],
                //AI1 = 1f - (float)NPC.life / NPC.lifeMax,
                AI2 = Projectile.ai[2]
            });
        }
    }
}
