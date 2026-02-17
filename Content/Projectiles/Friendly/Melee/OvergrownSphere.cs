using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;
using RoA.Content.Dusts;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class OvergrownSphere : ModProjectile {
    private bool _changeAlpha;
    private int _collisionRegistered;

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Overgrown Sphere");
        //ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        //ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults() {
        int width = 20; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Melee;

        Projectile.aiStyle = -1;

        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.penetrate = 1;
        Projectile.alpha = 255;

        Projectile.netImportant = true;
    }

    public override void OnSpawn(IEntitySource source) {
        int num = 10;
        if (num != Projectile.oldPos.Length) {
            Array.Resize(ref Projectile.oldPos, num);
            Array.Resize(ref Projectile.oldRot, num);
            Array.Resize(ref Projectile.oldSpriteDirection, num);
        }

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            Projectile.oldPos[i].X = 0f;
            Projectile.oldPos[i].Y = 0f;
            Projectile.oldRot[i] = 0f;
            Projectile.oldSpriteDirection[i] = 0;
        }
    }

    public override void PostAI() {
        for (int num29 = Projectile.oldPos.Length - 1; num29 > 0; num29--) {
            Projectile.oldPos[num29] = Projectile.oldPos[num29 - 1];
            Projectile.oldRot[num29] = Projectile.oldRot[num29 - 1];
            Projectile.oldSpriteDirection[num29] = Projectile.oldSpriteDirection[num29 - 1];
        }
        if (Projectile.oldPos[0] == Vector2.Zero) {
            Projectile.oldPos[0] = Projectile.position;
        }
        Projectile.oldPos[0] = Vector2.Lerp(Projectile.oldPos[0], Projectile.position, 0.5f);
        Projectile.oldRot[0] = Projectile.rotation;
        Projectile.oldSpriteDirection[0] = Projectile.spriteDirection;
        float amount = 0.65f;
        int num30 = 1;
        for (int num31 = 0; num31 < num30; num31++) {
            for (int num32 = Projectile.oldPos.Length - 1; num32 > 0; num32--) {
                if (!(Projectile.oldPos[num32] == Vector2.Zero)) {
                    if (Projectile.oldPos[num32].Distance(Projectile.oldPos[num32 - 1]) > 2f)
                        Projectile.oldPos[num32] = Vector2.Lerp(Projectile.oldPos[num32], Projectile.oldPos[num32 - 1], amount);

                    Projectile.oldRot[num32] = (Projectile.oldPos[num32 - 1] - Projectile.oldPos[num32]).SafeNormalize(Vector2.Zero).ToRotation();
                }
            }
        }
    }

    public override void AI() {
        float num99 = Projectile.scale * 0.325f;
        if (num99 > 1f)
            num99 = 1f;

        Lighting.AddLight((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), num99 * 0.1f, num99, num99 * 0.4f);

        Player player = Main.player[Projectile.owner];

        /*if (Projectile.alpha == 0 && Main.rand.NextBool(20)) {
			Dust dust5 = Dust.NewDustDirect(Projectile.position + Vector2.One * 2f, 20, 20, ModContent.DustType<OvergrownSpearDust>(), newColor: default, Scale: MathHelper.Lerp(0.45f, 0.8f, Main.rand.NextFloat()));
			dust5.velocity *= 1.25f;
			dust5.fadeIn = Main.rand.Next(0, 17) * 0.1f;
			dust5.noGravity = true;
			dust5.position += dust5.velocity * 0.75f;
			dust5.noLight = true;
			dust5.noLightEmittence = true;
		}*/

        Projectile.timeLeft = 2;

        double deg = (double)(Projectile.ai[1] + Projectile.ai[0] * 240) / 2;
        double rad = deg * (Math.PI / 180);
        double dist = 70;
        Projectile.position.X = player.GetPlayerCorePoint().X - (int)(Math.Cos(rad) * dist) - player.width / 2;
        Projectile.position.Y = player.GetPlayerCorePoint().Y - (int)(Math.Sin(rad) * dist) - player.height / 2 + player.height / 4f;
        Projectile.ai[1] += 2f;
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (_collisionRegistered != 0) _collisionRegistered--;

        ushort spearType = (ushort)ModContent.ProjectileType<OvergrownSpear>();
        ushort boltType = (ushort)ModContent.ProjectileType<OvergrownBolt>();
        for (var i = 0; i < 200; i++) {
            var proj = Main.projectile[i];
            if (proj.active && proj.type == spearType && proj.owner == player.whoAmI) {
                var rect = new Rectangle((int)proj.Center.X - 15, (int)proj.Center.Y - 15, 30, 30);
                var rec2 = Projectile.getRect();
                if (rect.Intersects(rec2) && _collisionRegistered == 0) {
                    SoundEngine.PlaySound(SoundID.NPCDeath55, Projectile.Center);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, proj.velocity, boltType, (int)player.GetTotalDamage(DamageClass.Melee).ApplyTo(player.GetSelectedItem().damage) * 2, player.GetTotalKnockback(DamageClass.Melee).ApplyTo(player.GetSelectedItem().knockBack) / 2, Projectile.owner);
                    float dustCount = 8f;
                    int dustCount2 = 0;
                    while (dustCount2 < dustCount) {
                        Vector2 vector = Vector2.UnitX * 0f;
                        vector += -Vector2.UnitY.RotatedBy(dustCount2 * (7f / dustCount), default) * new Vector2(1.5f, 1.5f);
                        vector = vector.RotatedBy(Projectile.velocity.ToRotation(), default);
                        int dust = Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<OvergrownSpearDust>(), 0f, 0f, 120, default(Color), Main.rand.NextFloat(0.8f, 1.6f));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].position = Projectile.Center + vector;
                        Main.dust[dust].velocity = Vector2.Normalize(Projectile.Center - Main.dust[dust].position) * 2f + Projectile.velocity * 2f;
                        int max = dustCount2;
                        dustCount2 = max + 1;
                    }
                    for (int num615 = 0; num615 < 6; num615++) {
                        int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height,
                             ModContent.DustType<OvergrownSpearDust>(), 0f, 0f, 120, default, Main.rand.NextFloat(0.8f, 1.6f));
                        Main.dust[num616].noGravity = true;
                        Dust dust2 = Main.dust[num616];
                        dust2.scale *= 1.25f;
                        dust2 = Main.dust[num616];
                        dust2.velocity *= 0.5f;
                    }
                    _collisionRegistered = 20;
                }
            }
        }
        if (Projectile.Opacity < 1f) {
            if (!_changeAlpha) Projectile.Opacity += 0.025f;
        }
        else _changeAlpha = true;

        int type = ModContent.ItemType<Items.Weapons.Melee.OvergrownSpear>();
        if (player.GetSelectedItem().type != type || !player.active || player.dead || player.GetFormHandler().IsInADruidicForm) {
            _changeAlpha = true;

            if (Projectile.Opacity > 0f) Projectile.Opacity -= 0.025f;
            else Projectile.Kill();
        }
        else if (Projectile.Opacity < 1f) {
            _changeAlpha = false;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Rectangle frameRect = new Rectangle(0, 0, texture.Width, texture.Height);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            if (Projectile.oldPos[k] == Vector2.Zero) {
                continue;
            }
            ulong seed = (ulong)k;
            float shakeX = Utils.RandomInt(ref seed, -10, 11);
            float shakeY = Utils.RandomInt(ref seed, -10, 11);
            float i = Helper.Wave(-1f, 1f, 2f, k);
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin;
            Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            spriteBatch.Draw(texture, drawPos + new Vector2(shakeX * i, shakeY * i), frameRect, color * Projectile.Opacity * (0.25f + 0.1f * Utils.RandomInt(ref seed, 0, 5)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        spriteBatch.Draw(texture, Projectile.position - Main.screenPosition + drawOrigin, frameRect, Projectile.GetAlpha(lightColor) * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White * 0.9f, 0.5f);

    public override bool? CanCutTiles() => false;

    public override bool? CanDamage() => false;
}