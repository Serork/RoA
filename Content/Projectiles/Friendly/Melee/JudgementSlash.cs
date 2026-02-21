using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class JudgementSlash : ModProjectile {
    private static Asset<Texture2D> _slashTexture = null!,
                                    _slashShadowTexture = null!;

    private Vector2 _startCenter;
    private Color _baseColor, _slashColor;

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _slashTexture = ModContent.Request<Texture2D>(ResourceManager.MeleeProjectileTextures + "JudgementSlash");
        _slashShadowTexture = ModContent.Request<Texture2D>(ResourceManager.MeleeProjectileTextures + "JudgementSlashShadow");
    }

    public override void OnSpawn(IEntitySource source) {
        if (Projectile.owner == Main.myPlayer) {
            _startCenter = Projectile.Center;
            switch (Main.rand.Next(3)) {
                case 0:
                    _baseColor = new(28, 39, 59);
                    break;
                case 1:
                    _baseColor = new(15, 31, 110);
                    break;
                case 2:
                    _baseColor = new(62, 85, 120);
                    break;
            }
            switch (Main.rand.Next(3)) {
                case 0:
                    _slashColor = new(23, 80, 166);
                    break;
                case 1:
                    _slashColor = new(30, 87, 171);
                    break;
                case 2:
                    _slashColor = new(42, 90, 127);
                    break;
            }
            Projectile.netUpdate = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_startCenter);
        writer.WriteRGBA(_slashColor);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _startCenter = reader.ReadVector2();
        _slashColor = reader.ReadRGBA();
    }

    public override void SetDefaults() {
        Projectile.localNPCHitCooldown = 500;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.width = Projectile.height = 30;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = 60;
        Projectile.timeLeft = 120;
        Projectile.extraUpdates = 2;

        Projectile.noEnchantments = true;
    }

    public override bool? CanDamage() => Projectile.velocity.Length() > 2f && Projectile.timeLeft > 15 && Projectile.timeLeft < 110;

    public override bool? CanCutTiles() => Projectile.velocity.Length() > 2f && Projectile.timeLeft > 15 && Projectile.timeLeft < 110;

    public override void AI() {
        Projectile.velocity *= 0.95f;
        if (Main.rand.NextBool(9) && Projectile.timeLeft > 45 && Projectile.timeLeft < 105) {
            Vector2 vel = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.01f, 0.01f)) * Main.rand.NextFloat(3f, 6f);
            int type = ModContent.DustType<DaikatanaDust>();
            Dust dust = Dust.NewDustDirect(Projectile.position - Vector2.Normalize(Projectile.velocity) * Main.rand.NextFloat(0f, (120 - Projectile.timeLeft) * 3f), Projectile.width, Projectile.height, type, 0, 0, 0, default, Main.rand.NextFloat(0.45f, 0.7f) * 0.95f * Main.rand.NextFloat(1.25f, 1.75f));
            dust.velocity = vel * 0.5f;
            dust.noGravity = true;

            Rectangle rectangle = Utils.CenteredRectangle(Projectile.position - Vector2.Normalize(Projectile.velocity) * Main.rand.NextFloat(0f, (120 - Projectile.timeLeft) * 3f), new Vector2(Projectile.width, Projectile.height));
            Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Main.spriteBatch.DrawWithSnapshot(() => {
            Vector2 hitCenter = _startCenter + Vector2.Normalize(Projectile.velocity) * 120f;
            Color lightColor = Lighting.GetColor((int)(hitCenter.X / 16f), (int)(hitCenter.Y / 16f));
            float value0 = (120 - Projectile.timeLeft) / 120f;
            float value1 = (float)Math.Pow(value0, 0.5f);
            float width = (float)(1f - Math.Cos(value1 * 2f * Math.PI)) * 9f;
            Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 normalize = normalizedVelocity.RotatedBy(Math.PI / 2f) * width;
            Color color = _baseColor.MultiplyRGB(lightColor);
            color *= width / 9.5f;
            normalize *= width * width / 375f;
            List<Vertex2D> bars = [new(_startCenter + normalize - Main.screenPosition, color, new Vector3(0f, 0f, 0f)),
                                   new(_startCenter - normalize - Main.screenPosition, color, new Vector3(0f, 1f, 0f)),
                                   new(Projectile.Center + normalize - Main.screenPosition, color, new Vector3(1f, 0f, 0f)),
                                   new(Projectile.Center - normalize - Main.screenPosition, color, new Vector3(1f, 1f, 0f))];
            //Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Main.graphics.GraphicsDevice.Textures[0] = _slashShadowTexture.Value;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);

            color = _slashColor.MultiplyRGB(lightColor);
            color *= width / 9.5f;
            normalize *= width * width / 385f;
            bars = [new(_startCenter + normalize - Main.screenPosition, color, new Vector3(0f, 0f, 0f)),
                    new(_startCenter - normalize - Main.screenPosition, color, new Vector3(0f, 1f, 0f)),
                    new(Projectile.Center + normalize - Main.screenPosition, color, new Vector3(1f, 0f, 0f)),
                    new(Projectile.Center - normalize - Main.screenPosition, color, new Vector3(1f, 1f, 0f))];
            Main.graphics.GraphicsDevice.Textures[0] = _slashTexture.Value;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
        });
        return false;
    }
}
