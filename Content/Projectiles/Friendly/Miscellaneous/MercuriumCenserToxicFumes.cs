using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Projectiles.Friendly.Melee;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class MercuriumCenserToxicFumes : NatureProjectile {
    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<MercuriumFumes>()).Texture;

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

    protected override void SafeSetDefaults() {
        int width = 24; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;

        Projectile.timeLeft = 150;
        Projectile.tileCollide = false;

        Projectile.friendly = true;

        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = 50;

        ShouldIncreaseWreathPoints = false;

        Projectile.aiStyle = -1;
    }

    public override bool? CanDamage() => Projectile.Opacity >= 0.3f;

    public override void AI() {
        if (Projectile.owner == Main.myPlayer) {
            if (Projectile.localAI[0] == 0f) {
                Projectile.ai[1] = Main.rand.NextFloat(0.75f, 1f);

                Projectile.localAI[0] = 1f;
                Projectile.netUpdate = true;
            }
        }

        if (Projectile.ai[1] != 0f) {
            Projectile.Opacity = Projectile.ai[1];
            Projectile.localAI[0] = Projectile.Opacity;
            Projectile.ai[1] = 0f;
        }

        if (++Projectile.frameCounter >= 6) {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }

        if (Projectile.owner == Main.myPlayer) {
            float distance = 30f;
            for (int findNPC = 0; findNPC < Main.npc.Length; findNPC++) {
                NPC npc = Main.npc[findNPC];
                if (npc.active && npc.life > 0 && !npc.friendly && Vector2.Distance(Projectile.Center, npc.Center) < distance) {
                    npc.AddBuff(ModContent.BuffType<ToxicFumes>(), Main.rand.Next(40, 120));
                }
            }
        }

        if (Projectile.Opacity > 0f) {
            Projectile.Opacity -= Projectile.localAI[0] * 0.025f * 0.2f;
        }
        else {
            Projectile.Kill();
        }
    }

    public override Color? GetAlpha(Color lightColor) => new Color(106, 140, 34, 100).MultiplyRGB(lightColor) * Projectile.Opacity * 0.75f;

    public override bool? CanCutTiles() => false;
}
