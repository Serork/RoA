using Microsoft.Xna.Framework;

using RoA.Common.Projectiles;
using RoA.Content.Dusts;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

sealed class TarExplosion : ModProjectile_NoTextureLoad {
    public override void SetStaticDefaults() {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
        ProjectileID.Sets.Explosive[Type] = true;
    }


    public override void SetDefaults() {
        Projectile.width = 150;
        Projectile.height = 150;
        Projectile.friendly = true;
        Projectile.penetrate = -1;

        // 5 second fuse.
        Projectile.timeLeft = 300;

        // These help the projectile hitbox be centered on the projectile sprite.
        DrawOffsetX = -2;
        DrawOriginOffsetY = -5;
    }

    public override void AI() {
        if (Projectile.ai[2] == 1f) {
            Projectile.width = 225;
            Projectile.height = 225;

            Projectile.Center = Projectile.position + Projectile.Size;
        }
        else if (Projectile.ai[2] == 2f) {
            Projectile.width = 750;
            Projectile.height = 750;

            Projectile.Center = Projectile.position + Projectile.Size;
        }

        Projectile.timeLeft = 0;
        Projectile.PrepareBombToBlow();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        // Vanilla explosions do less damage to Eater of Worlds in expert mode, so we will too.
        if (Main.expertMode) {
            if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) {
                modifiers.FinalDamage /= 5;
            }
        }
    }

    public override void PrepareBombToBlow() {
        Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the bomb explodes on slopes.
        Projectile.alpha = 255; // Set to transparent. This projectile technically lives as transparent for about 3 frames

        // Change the hitbox size, centered about the original projectile center. This makes the projectile damage enemies during the explosion.

        Projectile.damage = 100; // Bomb: 100, Dynamite: 250
        Projectile.knockBack = 8f; // Bomb: 8f, Dynamite: 10f
    }

    public override void OnKill(int timeLeft) {
        int width = 75, height = 75;
        Vector2 position = Projectile.position + new Vector2(width, height) / 2;
        // Play explosion sound
        SoundEngine.PlaySound(SoundID.Item14, position);

        // Smoke Dust spawn
        for (int i = 0; i < 20; i++) {
            Dust dust = Dust.NewDustDirect(position, width, height, ModContent.DustType<Smoke>(), 0f, 0f, 100, default, 2f);
            dust.velocity *= 1.4f;
        }

        // Fire Dust spawn
        for (int i = 0; i < 40; i++) {
            Dust dust = Dust.NewDustDirect(position, width, height, ModContent.DustType<Torch>(), 0f, 0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(position, width, height, ModContent.DustType<Torch>(), 0f, 0f, 100, default, 2f);
            dust.velocity *= 3f;
        }

        // Large Smoke Gore spawn
        for (int g = 0; g < 1; g++) {
            var goreSpawnPosition = new Vector2(position.X + width / 2 - 24f, position.Y + height / 2 - 24f);
            Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, ModContent.Find<ModGore>(nameof(RoA) + $"/Smoke{1 + Main.rand.Next(3)}").Type, 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, ModContent.Find<ModGore>(nameof(RoA) + $"/Smoke{1 + Main.rand.Next(3)}").Type, 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, ModContent.Find<ModGore>(nameof(RoA) + $"/Smoke{1 + Main.rand.Next(3)}").Type, 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y -= 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, ModContent.Find<ModGore>(nameof(RoA) + $"/Smoke{1 + Main.rand.Next(3)}").Type, 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y -= 1.5f;
        }
        // reset size to normal width and height.
    }
}
