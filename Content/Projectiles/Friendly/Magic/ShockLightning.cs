using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class ShockLightning : ModProjectile {
    private const float SWAY = 80f;
    private const float JAGGEDNESS = 1f / SWAY;

    private static Asset<Texture2D> _segmentTexture, _endTexture, _endTexture2;

    public override string Texture => ResourceManager.EmptyTexture;

    private float _lightningLength = 1f;
    private static List<Line> _results = [];

    public override void Load() => _results = [];
    public override void Unload() {
        _results.Clear();
        _results = null;
    }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _segmentTexture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "LightningSegment");
        _endTexture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "LightningEnd");
        _endTexture2 = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "LightningLight2");
    }

    public override void SetDefaults() {
        int width = 12; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;

        Projectile.penetrate = -1;

        Projectile.timeLeft = 120;

        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;

        Projectile.netImportant = true;

        Projectile.localNPCHitCooldown = 500;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Vector2 pos = player.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (player.direction == 1 ? 49f : 56f);
            for (int num155 = 0; num155 < 8; num155++) {
                int num156 = ModContent.DustType<Electric>();
                Dust obj7 = Main.dust[Dust.NewDust(pos, 0, 0, num156, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj7.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f * Main.rand.NextFloat() + Projectile.velocity.SafeNormalize(Vector2.Zero) * (2f + Main.rand.NextFloat());
                obj7.noGravity = true;
                obj7.scale *= 0.85f;
                obj7.scale *= 0.4f + Main.rand.NextFloatRange(0.15f);
                obj7.fadeIn = obj7.scale + 0.1f;
            }
        }

        if (Projectile.wet) {
            Projectile.hostile = true;
            Projectile.friendly = false;
        }
        _lightningLength += Projectile.velocity.Length() * 1.35f;
        Projectile.localAI[0]--;
        if (Projectile.ai[0] <= 4f && player.itemAnimation > 1) {
            if (Projectile.localAI[0] <= 0f) {
                Projectile.localAI[0] = 2f;
                Projectile.ai[0]++;
            }
        }
        else Projectile.Kill();

        Vector2 lineEnd = Projectile.position + Vector2.Normalize(Projectile.velocity) * _lightningLength;
        if (Collision.SolidCollision(lineEnd, 4, 4)) {
            Projectile.Kill();
        }
    }

    private void UpdateSegments() {
        Player player = Main.player[Projectile.owner];
        Vector2 source = player.itemLocation + (new Vector2(0f, 3f) * player.direction).RotatedBy(Projectile.velocity.ToRotation()) + Utils.SafeNormalize(Projectile.velocity, Vector2.One) * (player.HeldItem.width + player.HeldItem.width / 4);
        float thickness = 3.25f;
        Vector2 dest = source + Vector2.Normalize(Projectile.velocity) * _lightningLength;
        Vector2 dif = dest - source;
        Vector2 normal = Vector2.Normalize(new Vector2(dif.Y, -dif.X));
        float length = dif.Length();
        int positionCount = Math.Min(100, (int)(length / 4));
        _results.Clear();
        List<float> positions = new(positionCount + 50) {
                0
            };
        for (int i = 0; i < positionCount; i++) {
            positions.Add(Main.rand.NextFloat(0, 1));
        }
        positions.Sort();
        Vector2 prevPoint = source;
        float prevDisplacement = 0;
        float tempThickness = thickness;
        for (int i = 1; i < positions.Count; i++) {
            float position = positions[i];
            float scale = (length * JAGGEDNESS) * (position - positions[i - 1]);
            float envelope = position > 0.95f ? 20 * (1 - position) : 1;
            float displacement = Main.rand.NextFloatRange(SWAY);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            displacement *= envelope;
            Vector2 to = source + position * dif + displacement * normal;
            if (tempThickness > 0.25f && Main.rand.NextChance(0.25)) {
                tempThickness -= tempThickness / 10;
            }
            _results.Add(new Line(Projectile, prevPoint, to, tempThickness));
            prevPoint = to;
            prevDisplacement = displacement;
        }
        _results.Add(new Line(Projectile, prevPoint, dest, tempThickness));
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 hitPoint = target.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2f;
        Vector2 normal = (-Projectile.velocity).SafeNormalize(Vector2.UnitX);
        Vector2 spinningpoint = Vector2.Reflect(Projectile.velocity, normal);
        for (int i = 0; i < 4; i++) {
            int num156 = ModContent.DustType<Electric>();
            Dust dust = Dust.NewDustPerfect(hitPoint, num156, spinningpoint.RotatedBy((float)Math.PI / 4f * Main.rand.NextFloatDirection()) * 0.6f * Main.rand.NextFloat(), 100, default, 0.5f + 0.3f * Main.rand.NextFloat());
            Dust dust2 = Dust.CloneDust(dust);
            dust2.color = Color.White;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        UpdateSegments();

        foreach (var segment in _results) {
            //Color color = Color.Lerp(Color.White, Lighting.GetColor((int)segment.Position.X / 16, (int)segment.Position.Y / 16), Lighting.Brightness((int)segment.Position.X / 16, (int)segment.Position.Y / 16));
            segment.Draw(Main.spriteBatch, Color.White);

            LightingCutTiles(segment.Position);

            if (!Main.dedServ) {
                Lighting.AddLight(segment.Position, new Color(99, 200, 204).ToVector3() * 0.625f);
            }
        }

        return false;
    }

    private void LightingCutTiles(Vector2 position) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }
        Vector2 boxPosition = position - Vector2.One * 2f;
        int boxWidth = 4;
        int boxHeight = 4;
        int num = (int)(boxPosition.X / 16f);
        int num2 = (int)((boxPosition.X + (float)boxWidth) / 16f) + 1;
        int num3 = (int)(boxPosition.Y / 16f);
        int num4 = (int)((boxPosition.Y + (float)boxHeight) / 16f) + 1;
        if (num < 0)
            num = 0;

        if (num2 > Main.maxTilesX)
            num2 = Main.maxTilesX;

        if (num3 < 0)
            num3 = 0;

        if (num4 > Main.maxTilesY)
            num4 = Main.maxTilesY;

        bool[] tileCutIgnorance = Main.player[Projectile.owner].GetTileCutIgnorance(allowRegrowth: false, Projectile.trap);
        for (int i = num; i < num2; i++) {
            for (int j = num3; j < num4; j++) {
                if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
                    WorldGen.KillTile(i, j);
                    if (Main.netMode != 0)
                        NetMessage.SendData(17, -1, -1, null, 0, i, j);
                }
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 lineEnd = Projectile.position + Vector2.Normalize(Projectile.velocity) * _lightningLength;
        return Helper.DeathrayHitbox(Projectile.position, lineEnd, targetHitbox, 26f);
    }

    public override bool? CanCutTiles() => false;

    private readonly struct Line(Projectile projectile, Vector2 a, Vector2 b, float thickness = 1) {
        private readonly Projectile _projectile = projectile;
        private readonly Vector2 _a = a, _b = b;
        private readonly float _thickness = thickness;

        public readonly Vector2 Position => _a + (_b - _a);
        public readonly Player Player => Main.player[_projectile.owner];

        public void Draw(SpriteBatch spriteBatch, Color color = default) {
            Player player = Main.player[_projectile.owner];
            Vector2 pos = player.itemLocation + Utils.SafeNormalize(_projectile.velocity, Vector2.One) * (player.HeldItem.width + player.HeldItem.width / 4);
            Vector2 offset = pos - _b;
            Vector2 dest = pos - offset;
            offset = pos - _a;
            Vector2 source = pos - offset;
            if (Main.rand.NextChance(0.05)) {
                Dust dust = Dust.NewDustPerfect(source, ModContent.DustType<Electric>(), Utils.SafeNormalize(source.DirectionTo(dest), Vector2.Zero) * 7.5f * Main.rand.NextFloat(0.25f, 1f), 0, Color.White);
                dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                dust.scale *= 0.4f + Main.rand.NextFloatRange(0.15f);
                dust.fadeIn = dust.scale + 0.1f;
                dust.noGravity = true;
            }
            float rotation = (dest - source).ToRotation();
            float thicknessScale = _thickness / _segmentTexture.Height();
            Vector2 capOrigin = new(_endTexture.Width(), _endTexture.Height() / 2f);
            Vector2 middleOrigin = new(0, _segmentTexture.Height() / 2f);
            Vector2 middleScale = new((dest - source).Length(), thicknessScale);
            spriteBatch.Draw(_endTexture2.Value, source - Main.screenPosition, null, color, rotation, middleOrigin, middleScale * 0.01f, SpriteEffects.None, 0f);
            spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(_segmentTexture.Value, source - Main.screenPosition, null, color * 0.75f, rotation, middleOrigin, middleScale * 0.95f, SpriteEffects.None, 0f);
            spriteBatch.Draw(ModContent.Request<Texture2D>(ResourceManager.Textures + "Light").Value, source - Main.screenPosition, null, color.MultiplyRGB(new Color(99, 200, 204)) * 0.75f, rotation, middleOrigin, new Vector2(middleScale.X, middleScale.Y * 2f) * 0.02f, SpriteEffects.None, 0f);
            spriteBatch.EndBlendState();
            spriteBatch.Draw(_segmentTexture.Value, source - Main.screenPosition, null, color, rotation, middleOrigin, middleScale * 0.85f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_endTexture.Value, source - Main.screenPosition, null, color, rotation, capOrigin, thicknessScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_endTexture.Value, dest - Main.screenPosition, null, color, rotation + MathHelper.Pi, capOrigin, thicknessScale, SpriteEffects.None, 0f);
        }
    }
}