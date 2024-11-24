using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class SapStream : NatureItem {
    protected override void SafeSetDefaults() {
        int width = 38; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 20;
        Item.autoReuse = true;

        Item.noMelee = true;
        Item.knockBack = 2f;

        Item.damage = 2;
        NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.5f);

        Item.value = Item.sellPrice(silver: 15);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item20;

        Item.shootSpeed = 1f;
        Item.shoot = ModContent.ProjectileType<GalipotStream>();

        Item.staff[Item.type] = true;
    }
    public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 velocity2 = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += velocity2 * 40f;
        position += new Vector2(-velocity2.Y, velocity2.X) * (5f * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            Vector2 vector2 = (velocity + Utils.RotatedByRandom(velocity, 0.25f) * Main.rand.NextFloat(0.8f, 1.4f) * Main.rand.NextFloat(0.75f, 1.35f)) * Main.rand.NextFloat(1.25f, 2f);
            Projectile.NewProjectileDirect(source, position + vector2, vector2, type, damage, knockback, player.whoAmI);
        }

        return false;
    }
}

sealed class GalipotDrop : VisualEffect<GalipotDrop> {
    internal Projectile projectile;

    private float _opacity;

    protected override void SetDefaults() {
        TimeLeft = MaxTimeLeft = 2;
        _opacity = 1f;
        //Color = Color.Lerp(new Color(201, 81, 0), new Color(126, 33, 0), 0.5f);
        DrawColor = new Color(255, 190, 44); 
    }

    public override void Update(ref ParticleRendererSettings settings) {
        if (!(projectile == null || !projectile.active)) {
            TimeLeft = projectile.timeLeft;
        }
        if ((Scale < 1f || projectile.timeLeft < 100) && _opacity > 0f) {
            _opacity -= 0.065f + Main.rand.NextFloatRange(0.025f);
        }
        if (--TimeLeft <= 0 || _opacity <= 0f) {
            RestInPool();
            return;
        }
        if (Collision.SolidCollision(Position, 0, 0)) {
            Velocity = Vector2.UnitY;
            Position.Y += 0.05f * Main.rand.NextFloat();
            if (Scale < 1f) {
                RestInPool();
            }
        }
        else {
            Velocity *= 0.98f;
            Velocity += new Vector2(Main.windSpeedCurrent * 0.1f, 0.21f * Scale * 0.1f);
            Position += Velocity;
            Scale *= 0.98f;
        }
        //if (_opacity < 1f) {
        //    _opacity += 0.1f;
        //}
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        DrawSelf(spritebatch);
    }

    private void DrawSelf(SpriteBatch spritebatch) {
        spritebatch.End();
        spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        float opacity = _opacity/* * 1.5f*/;
        Vector2 toCorner = new Vector2(0f, Scale).RotatedBy(Rotation);
        Color color2 = new Color(201, 85, 8) * opacity * 1f;
        Color color1 = new Color(126, 33, 0) * opacity * 1f;
        List<Vertex2D> bars = [
            new Vertex2D(Position - Main.screenPosition + Velocity + toCorner, Color.Lerp(color1, color2, 0f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(0f, 0f, 0f)),
            new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 0.5f), Color.Lerp(color1, color2, 0.33f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)),  new Vector3(0f, 1f, 0f)),
            new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 1.5f), Color.Lerp(color1, color2, 0.66f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 0f, 0f)),
            new Vertex2D(Position - Main.screenPosition - Velocity * AI0 + toCorner.RotatedBy(MathHelper.Pi), Color.Lerp(color1, color2, 1f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 1f, 0f))
        ];
        Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>(ResourceManager.Textures + "Lightball3").Value;
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
        color1 = new Color(255, 241, 44) * opacity * 0.225f;
        color2 = Color.White * opacity * 0.225f;
        bars = [
            new Vertex2D(Position - Main.screenPosition + Velocity + toCorner, Color.Lerp(color1, color2, 0f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(0f, 0f, 0f)),
            new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 0.5f), Color.Lerp(color1, color2, 0.4f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)),  new Vector3(0f, 1f, 0f)),
            new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 1.5f), Color.Lerp(color1, color2, 0.8f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 0f, 0f)),
            new Vertex2D(Position - Main.screenPosition - Velocity * AI0 + toCorner.RotatedBy(MathHelper.Pi), Color.Lerp(color1, color2, 1f).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 1f, 0f))
        ];
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        spritebatch.EndBlendState();
    }
}

sealed class GalipotStream : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    public bool IsActive => Projectile.ai[1] == 0f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailingMode[Type] = 0;
        ProjectileID.Sets.TrailCacheLength[Type] = 15;
    }

    protected override void SafeSetDefaults() {
        Projectile.width = Projectile.height = 10;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 500;
        Projectile.penetrate = 1;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity = Vector2.Zero;

        Projectile.ai[1] = 1f;

        return false;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Projectile.velocity *= Main.rand.NextFloat(1.25f, 1.75f) * Main.rand.NextFloat(0.75f, 1f);
        Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloatDirection() * MathHelper.PiOver2) * Projectile.velocity.Length() * 0.1f;
        Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
    }

    public override void AI() {
        Projectile.ai[2] *= 0.99f;
        void drop() {
            if (Main.rand.NextBool(2)) {
                GalipotDrop drop = VisualEffectSystem.New<GalipotDrop>(VisualEffectLayer.BEHINDTILESBEHINDNPCS).Setup(Projectile.Center - Vector2.UnitY * Projectile.ai[2],
                    Projectile.velocity);
                drop.projectile = Projectile;
                drop.Scale = Main.rand.NextFloat(8f, 10f);
                drop.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                drop.AI0 = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            }
        }
        if (IsActive) {
            drop();
            Vector2 pos = Projectile.Center - new Vector2(4f) + Projectile.velocity * Main.rand.NextFloat(1f);
            Dust dust = Dust.NewDustDirect(pos - Projectile.velocity.SafeNormalize(Vector2.Zero), 4, 4, ModContent.DustType<Galipot>(), 0, 0, 0, default, 0.575f + Main.rand.NextFloatRange(0.15f));
            dust.velocity *= 0.285f;
            dust.noGravity = true;
            Projectile.ai[2] = 0f;
            Projectile.velocity.Y += 0.15f;
            if (Collision.SolidCollision(Projectile.Center, 2, 2)) {
                Projectile.velocity = Vector2.Zero;

                Projectile.ai[1] = 1f;
            }
        }
        else {
            if (Main.rand.NextBool(3)) {
                drop();
            }
            float value = 0.05f * Main.rand.NextFloat();
            Projectile.ai[2] += value;
            Projectile.position.Y += value;
            if (!Collision.SolidCollision(Projectile.Center, 0, 0)) {
                Projectile.ai[1] = 0f;
            }
        }
    }
}