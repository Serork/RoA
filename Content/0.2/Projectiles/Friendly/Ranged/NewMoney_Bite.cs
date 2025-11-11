using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class NewMoneyBite : ModProjectile {
    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);
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

        Projectile.ArmorPenetration = 10;
    }

    public override bool? CanDamage() => true;
    public override bool? CanCutTiles() => false;

    public override bool? CanHitNPC(NPC target) => Projectile.ai[2] == 0f && target.whoAmI == (int)Projectile.ai[0];
    public override bool CanHitPlayer(Player target) => Projectile.ai[2] != 0f && target.whoAmI == (int)Projectile.ai[0];

    public override void AI() {
        Lighting.AddLight(Projectile.Center, NewMoneyBullet.BulletColor.ToVector3() * 0.5f);

        if (Projectile.ai[2] != 0f) {
            Projectile.friendly = false;
            Projectile.hostile = true;
        }

        if (Projectile.localAI[0]++ > NewMoney.BITE_FIRSTFRAMETIME && Projectile.frameCounter++ > NewMoney.BITE_ANIMATIONTIME) {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame == Projectile.GetFrameCount() - 1) {
                if (Projectile.IsOwnerLocal()) {
                    ProjectileUtils.SpawnPlayerOwnedProjectile<NewMoneyBat>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                        Position = Projectile.Center
                    });
                }
            }
            if (Projectile.frame >= Projectile.GetFrameCount()) {
                Projectile.Kill();
            }
        }
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        Color color = NewMoneyBullet.BulletColor;
        color.A /= 2;

        Projectile projectile = Projectile;
        Texture2D mainTex = projectile.GetTexture();

        int frameSize = mainTex.Height / Main.projFrames[projectile.type];
        float progress = Projectile.localAI[0] / (NewMoney.BITE_FIRSTFRAMETIME * 1.5f);
        int frame = (int)(progress * Main.projFrames[projectile.type]);
        frame = Math.Min(frame, Main.projFrames[projectile.type]);
        Rectangle frameBox = new(0, frameSize * (Main.projFrames[projectile.type] - frame), mainTex.Width, frameSize);
        SpriteEffects effects = projectile.spriteDirection.ToSpriteEffects();
        Vector2 origin = frameBox.Size() / 2;
        float colorModifier = Utils.GetLerpValue(1.5f, 1.25f, progress, true);
        Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, frameBox, color * 0.5f * colorModifier, projectile.rotation,
                      origin, projectile.scale * 2f * progress, effects, 0);


        if (Projectile.localAI[0] >= NewMoney.BITE_FIRSTFRAMETIME) {
            float scaleModifier = 1f + 1f - colorModifier;
            float scale = Projectile.scale + scaleModifier * 0.1f;

            frameSize = mainTex.Height / Main.projFrames[projectile.type];
            frameBox = new(0, frameSize * projectile.frame, mainTex.Width, frameSize);
            effects = projectile.spriteDirection.ToSpriteEffects();
            origin = frameBox.Size() / 2;
            Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, frameBox, color, projectile.rotation,
                                  origin, scale, effects, 0);
        }

        //Projectile.QuickDrawAnimated(color);
        //if (_glowTexture?.IsLoaded == true) {
        //    Projectile.QuickDrawAnimated(color * 0.9f, texture: _glowTexture.Value);
        //}

        return false;
    }
} 
