using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class NewMoneyBite : ModProjectile {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);

        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;

        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool? CanDamage() => true;
    public override bool? CanCutTiles() => false;

    public override bool? CanHitNPC(NPC target) => target.whoAmI == (int)Projectile.ai[0];

    public override void AI() {
        Projectile.localAI[0]++;
        if (Projectile.localAI[0] >= 8 && Projectile.frameCounter++ > 4) {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame > Projectile.GetFrameCount()) {
                Projectile.Kill();
                if (Projectile.IsOwnerLocal()) {
                    ProjectileUtils.SpawnPlayerOwnedProjectile<NewMoneyBat>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                        Position = Projectile.Center
                    });
                }
            }
        }
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor);
        if (_glowTexture?.IsLoaded == true) {
            Projectile.QuickDrawAnimated(Color.White * 0.9f, texture: _glowTexture.Value);
        }

        return false;
    }
} 
