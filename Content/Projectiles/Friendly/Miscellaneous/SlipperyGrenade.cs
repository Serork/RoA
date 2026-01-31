using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class SlipperyGrenade : ModProjectile {
    private Vector2 memorizeVelocity;
    private int effectCounter;
    private int effectCounterMax = 1;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

        ProjectileID.Sets.Explosive[Type] = true;
    }

    public override void SetDefaults() {
        int width = 14; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.friendly = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.tileCollide = true;
        Projectile.timeLeft = 180;

        Projectile.DamageType = DamageClass.Ranged;

        DrawOriginOffsetY = 0;
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor * Projectile.Opacity);

        return false;
    }

    public override void PrepareBombToBlow() {
        Projectile.alpha = 255;
        Projectile.position = Projectile.Center;
        Projectile.Center = Projectile.position;
        Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the grenade explodes on slopes.
        Projectile.alpha = 255; // Make the grenade invisible.

        // Resize the hitbox of the projectile for the blast "radius".
        // Rocket I: 128, Rocket III: 200, Mini Nuke Rocket: 250
        // Measurements are in pixels, so 128 / 16 = 8 tiles.
        Projectile.Resize(128, 128);
        // Set the knockback of the blast.
        // Rocket I: 8f, Rocket III: 10f, Mini Nuke Rocket: 12f
        Projectile.knockBack = 8f;
    }

    public override void AI() {
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
            Projectile.PrepareBombToBlow();
        }

        if (!Projectile.tileCollide) {
            memorizeVelocity *= 0.97f;
            Projectile.velocity = memorizeVelocity;
            if (Projectile.ai[2] <= 0f) {
                if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
                    memorizeVelocity = Vector2.Zero;
                    Projectile.tileCollide = true;
                }
            }
            else {
                Projectile.ai[2]--;
            }
            effectCounter++;
            if (effectCounter == effectCounterMax && effectCounterMax < 20) {
                effectCounterMax += 3;
                effectCounter = 0;
                SoundEngine.PlaySound(SoundID.WormDig, Projectile.position);
            }
            if (effectCounter % 4 == 0 && effectCounterMax < 20) {
                int dustDig = Dust.NewDust(Projectile.Center - Vector2.One * 10, 20, 20, ModContent.DustType<Galipot2>(), 0f, 0f, 0, default(Color), 1f);
                Main.dust[dustDig].velocity *= 0.1f;
                Main.dust[dustDig].noGravity = true;
            }
        }

        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] >= 10f && Projectile.tileCollide)
            Projectile.velocity.Y = Projectile.velocity.Y + 0.2f; // 0.1f for arrow gravity, 0.4f for knife gravity
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;

        Projectile.rotation += Projectile.velocity.X * 0.1f;
        return;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[1] != 0) return true;
        if (memorizeVelocity == Vector2.Zero)
            memorizeVelocity = oldVelocity * 0.5f;
        Projectile.tileCollide = false;
        Projectile.ai[2] = 1f;
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.timeLeft > 4)
            Projectile.timeLeft = 4;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Main.expertMode) {
            if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) modifiers.FinalDamage /= 5;
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.FinalDamage /= 2;
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        for (int i = 0; i < 15; i++) {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X + (Projectile.width / 2), Projectile.position.Y + (Projectile.height / 2)), 4, 4, 31, 0f, 0f, 100, default(Color), 1.5f);
            Main.dust[dustIndex].velocity *= 1.2f;
        }

        for (int i = 0; i < 10; i++) {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X + (Projectile.width / 2), Projectile.position.Y + (Projectile.height / 2)), 4, 4, 6, 0f, 0f, 100, default(Color), 1f);
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].velocity *= 4f;
            dustIndex = Dust.NewDust(new Vector2(Projectile.position.X + (Projectile.width / 2), Projectile.position.Y + (Projectile.height / 2)), 4, 4, 6, 0f, 0f, 100, default(Color), 1.5f);
            Main.dust[dustIndex].velocity *= 2.5f;
        }

        if (!Main.dedServ) {
            for (int g = 0; g < 1; g++) {
                int goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2), Projectile.position.Y + (Projectile.height / 2)), default(Vector2), Main.rand.Next(61, 64), 1f);
                goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2), Projectile.position.Y + (Projectile.height / 2)), default(Vector2), Main.rand.Next(61, 64), 1f);
                goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2), Projectile.position.Y + (Projectile.height / 2)), default(Vector2), Main.rand.Next(61, 64), 1f);
            }
        }

        Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
        Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
        Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);
    }
}