using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Ranged.Hardmode;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class TaprootArrow : ModProjectile {
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        foreach (Taproot.TaprootNPCHitInfo taprootNPCHitInfo in Taproot.NPCsThatTakeIncreasedDamage) {
            if (taprootNPCHitInfo.NPCTypeToApplyIncreasedDamage != target.type) {
                continue;
            }
            modifiers.FlatBonusDamage += taprootNPCHitInfo.FlatBonusDamage;
            modifiers.FinalDamage *= taprootNPCHitInfo.FinalDamageModifier;
        }
    }

    public override void SetDefaults() {
        Projectile.width = 10; // The width of projectile hitbox
        Projectile.height = 10; // The height of projectile hitbox

        Projectile.arrow = true;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1200;

        Projectile.penetrate = 5;
    }

    public override void AI() {
        // The code below was adapted from the ProjAIStyleID.Arrow behavior. Rather than copy an existing aiStyle using Projectile.aiStyle and AIType,
        // like some examples do, this example has custom AI code that is better suited for modifying directly.
        // See https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#what-is-ai for more information on custom projectile AI.

        // Apply gravity after a quarter of a second
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] >= 15f) {
            Projectile.ai[0] = 15f;
            Projectile.velocity.Y += 0.1f;
        }

        // The projectile is rotated to face the direction of travel
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        // Cap downward velocity
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }

        if (Main.rand.Next(10) == 0) {
            int dustType = Main.rand.NextBool() ? ModContent.DustType<Taproot1>() : ModContent.DustType<Taproot2>();
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default(Color), 1f + Main.rand.NextFloat(0.2f));
            Main.dust[dust].velocity *= 0.5f;
        }
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position); // Plays the basic sound most projectiles make when hitting blocks.
        for (int num642 = 0; num642 < 10; num642++) {
            int dustType = Main.rand.NextBool() ? ModContent.DustType<Taproot1>() : ModContent.DustType<Taproot2>();
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, dustType, 0f, 0f, 0, default(Color), 1f + Main.rand.NextFloat(0.2f));
            Main.dust[dust].velocity *= 0.5f;
        }
    }
}