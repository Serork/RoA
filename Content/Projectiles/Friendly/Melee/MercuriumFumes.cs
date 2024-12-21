using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class MercuriumFumes : ModProjectile {
    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

    public override void SetDefaults() {
        int width = 24; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;

        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;

        Projectile.friendly = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.aiStyle = -1;
    }

    public override bool? CanDamage() => Projectile.Opacity >= 0.3f;

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
           Projectile.Opacity = Main.rand.NextFloat(0.75f, 1f);

           Projectile.localAI[0] = Projectile.Opacity;
        }

        if (++Projectile.frameCounter >= 6) {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }

        if (Projectile.owner == Main.myPlayer) {
            float distance = 30f;
            //for (int findPlayer = 0; findPlayer < byte.MaxValue; findPlayer++) {
            //    Player player = Main.player[findPlayer];
            //    if (player.active && !player.dead && Vector2.Distance(Projectile.Center, player.Center) < distance)
            //        player.AddBuff(ModContent.BuffType<ToxicFumes>(), 180, false);
            //}
            for (int findNPC = 0; findNPC < Main.npc.Length; findNPC++) {
                NPC npc = Main.npc[findNPC];
                if (npc.active && npc.life > 0 && Vector2.Distance(Projectile.Center, npc.Center) < distance)
                    npc.AddBuff(ModContent.BuffType<ToxicFumes>(), 180);
            }
        }

        if (Projectile.Opacity > 0f) {
            Projectile.Opacity -= Projectile.localAI[0] * 0.025f;
        }
        else {
            Projectile.Kill();
        }
    }

    public override Color? GetAlpha(Color lightColor) => new Color(106, 140, 34, 100) * Projectile.Opacity;

    public override bool? CanCutTiles() => false;
}