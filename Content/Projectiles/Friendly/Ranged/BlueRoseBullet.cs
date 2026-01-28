using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class BlueRoseBullet : ModProjectile, ISpawnCopies {
    private float _scale, _trailOpacity, _copyCounter;
    private Dictionary<NPC, byte> _hitNPCs = null!;

    float ISpawnCopies.CopyDeathFrequency => 0.1f;

    public override void SetStaticDefaults() {
        Projectile.SetTrail(2, 7);
    }

    public override void SetDefaults() {
        Projectile.width = 20; // The width of projectile hitbox
        Projectile.height = 20; // The height of projectile hitbox
        Projectile.aiStyle = -1; // The ai style of the projectile, please reference the source code of Terraria
        Projectile.friendly = true; // Can the projectile deal damage to enemies?
        Projectile.hostile = false; // Can the projectile deal damage to the player?
        Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
        Projectile.penetrate = -1; // How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
        Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
        Projectile.alpha = 0; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
        //Projectile.light = 0.5f; // How much light emit around the projectile
        Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
        Projectile.tileCollide = true; // Can the projectile collide with tiles?
        Projectile.extraUpdates = 0; // Set to above 0 if you want the projectile to update multiple time in a frame

        AIType = ProjectileID.Bullet; // Act exactly like default Bullet

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.Opacity = 0f;
        _scale = 0f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.localAI[2]++;

        if (Projectile.ai[2] == 0f) {
            // This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);

            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = -0.5f }, Projectile.Center);
        }

        if (Projectile.localAI[2] >= 3f) {
            Projectile.ai[2] = 1f;
        }
        else {
            if ((double)Projectile.velocity.X != (double)oldVelocity.X)
                Projectile.velocity.X = (float)(-(double)oldVelocity.X * 1);
            if ((double)Projectile.velocity.Y != (double)oldVelocity.Y)
                Projectile.velocity.Y = (float)(-(double)oldVelocity.Y * 1);
        }

        return false;
    }

    public override bool ShouldUpdatePosition() => Projectile.ai[2] == 0f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {

    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (_hitNPCs.TryGetValue(target, out byte value)) {
            _hitNPCs[target] = ++value;
        }
        else {
            _hitNPCs.Add(target, 0);
        }

        modifiers.FinalDamage *= 1f + 0.5f * _hitNPCs[target];
    }

    public override void AI() {
        if (Main.rand.NextBool(4)) {
            int num4 = Dust.NewDust(Projectile.position - Projectile.velocity.SafeNormalize() * Projectile.width * 0.75f, Projectile.width, Projectile.height, ModContent.DustType<BlueRoseDust>(), Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 0, default, 1.2f);
            Main.dust[num4].noGravity = true;
            Main.dust[num4].velocity = -Projectile.velocity * Main.rand.NextFloat(0.25f, 0.5f);
            Main.dust[num4].customData = Main.rand.NextFloat(10f);
        }

        if (Projectile.ai[2] == 1f) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.2f);
            _scale = Helper.Approach(_scale, 1.5f, 0.1f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
        else {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.15f);
        }
        _scale = Helper.Approach(_scale, 1f, 0.0575f);
        if (Projectile.Opacity >= 1f) {
            _trailOpacity = Helper.Approach(_trailOpacity, 1f, 0.075f);
        }

        //if (_trailOpacity >= 1f && _copyCounter++ % 8 == 0) {
        //    CopyHandler.MakeCopy(Projectile);
        //}

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            _hitNPCs = [];

            float num114 = 4f;
            for (int num115 = 0; (float)num115 < num114; num115++) {
                Vector2 spinningpoint6 = Vector2.UnitX * 0f;
                spinningpoint6 += -Vector2.UnitY.RotatedBy((float)num115 * ((float)Math.PI * 2f / num114)) * new Vector2(1f, 4f);
                spinningpoint6 = spinningpoint6.RotatedBy(Projectile.velocity.ToRotation());
                int num116 = Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch);
                Main.dust[num116].scale = 1.7f;
                Main.dust[num116].noGravity = true;
                Main.dust[num116].position = Projectile.Center + spinningpoint6 + Projectile.velocity.SafeNormalize(Vector2.Zero) * 42f;
                Main.dust[num116].velocity = Main.dust[num116].velocity * 2f + spinningpoint6.SafeNormalize(Vector2.UnitY) * 0.3f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 3f;
                Main.dust[num116].velocity *= 0.7f;
                Main.dust[num116].position += Main.dust[num116].velocity * 5f;
            }

            CopyHandler.InitializeCopies(Projectile, 10);


            Projectile.velocity = Projectile.velocity.SafeNormalize() * 5f;
        }

        //Lighting.AddLight(Projectile.Center, new Color(143, 255, 133).ToVector3() * 0.5f);

        //Projectile.Opacity = Utils.GetLerpValue(0, 20, Projectile.timeLeft, true);
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        var handler = Projectile.GetGlobalProjectile<CopyHandler>();
        var copyData = handler.CopyData;
        int width = texture.Width,
            height = texture.Height;
        Color shadowColor = lightColor;
        shadowColor = shadowColor.MultiplyAlpha(Helper.Wave(0.75f, 1f, 20f, Projectile.whoAmI));
        float opacity = Projectile.Opacity;
        if (Projectile.ai[2] == 1f) {
            shadowColor = shadowColor.MultiplyAlpha(Projectile.Opacity);
            opacity = Utils.GetLerpValue(0f, 0.35f, Projectile.Opacity, true);
        }
        for (int i = 0; i < 10; i++) {
            CopyHandler.CopyInfo copyInfo = copyData![i];
            if (copyInfo.Opacity <= 0f) {
                continue;
            }
            if (MathUtils.Approximately(copyInfo.Position, Projectile.Center, 2f)) {
                continue;
            }
            batch.Draw(texture, copyInfo.Position, DrawInfo.Default with {
                Color = shadowColor * MathUtils.Clamp01(copyInfo.Opacity) * opacity * 0.5f * _trailOpacity,
                Rotation = copyInfo.Rotation,
                Scale = Vector2.One * MathF.Max(copyInfo.Scale, 1f) * _scale,
                Origin = new Vector2(width, height) / 2f,
                Clip = new Rectangle(0, copyInfo.UsedFrame * height, width, height)
            });
        }

        Projectile.QuickDrawShadowTrails(shadowColor * _trailOpacity * 0.375f, 0.5f, 1, 0f, scale: _scale);
        Projectile.QuickDrawAnimated(shadowColor * opacity, scale: Vector2.One * _scale);
        return false;
    }

    public override void OnKill(int timeLeft) {
    }
}
