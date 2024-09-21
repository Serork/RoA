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
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

sealed class ShockLightning : ModProjectile {
    private static readonly Asset<Texture2D> segmentTexture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "LightningSegment");
    private static readonly Asset<Texture2D> endTexture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "LightningEnd");

    public override string Texture => ResourceManager.EmptyTexture;

    private float length = 1f;
    private static List<Line> _results = new List<Line>();

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
        if (Projectile.wet) {
            Projectile.hostile = true;
            Projectile.friendly = false;
        }
        length += Projectile.velocity.Length() * 1.35f;
        Projectile.localAI[0]--;
        Player player = Main.player[Projectile.owner];
        if (Projectile.ai[0] <= 4f && player.itemAnimation > 1) {
            if (Projectile.localAI[0] <= 0f) {
                Projectile.localAI[0] = 2f;
                Projectile.ai[0]++;
            }
        }
        else Projectile.Kill();

        Vector2 lineEnd = Projectile.position + Vector2.Normalize(Projectile.velocity) * length;
        if (Collision.SolidCollision(lineEnd, 4, 4)) {
            Projectile.Kill();
        }
    }

    private void UpdateSegments() {
        Player player = Main.player[Projectile.owner];
        Vector2 source = player.itemLocation + Utils.SafeNormalize(Projectile.velocity, Vector2.One) * (player.HeldItem.width + player.HeldItem.width / 4);
        float thickness = 4f;
        Vector2 dest = source + Vector2.Normalize(Projectile.velocity) * length;
        Vector2 dif = dest - source;
        Vector2 normal = Vector2.Normalize(new Vector2(dif.Y, -dif.X));
        float _length = dif.Length();
        int _positionCount = Math.Min(100, (int)(_length / 4));
        _results = new List<Line>();
        List<float> _positions = new List<float>(_positionCount + 50) {
                0
            };
        for (int i = 0; i < _positionCount; i++)
            _positions.Add(Main.rand.NextFloat(0, 1));
        _positions.Sort();
        const float _sway = 80;
        const float _jaggedness = 1 / _sway;
        Vector2 prevPoint = source;
        float prevDisplacement = 0;
        float _thickness = thickness;
        for (int i = 1; i < _positions.Count; i++) {
            float _position = _positions[i];
            float _scale = (_length * _jaggedness) * (_position - _positions[i - 1]);
            float _envelope = _position > 0.95f ? 20 * (1 - _position) : 1;
            float _displacement = Main.rand.NextFloat(-_sway, _sway);
            _displacement -= (_displacement - prevDisplacement) * (1 - _scale);
            _displacement *= _envelope;
            var _point = source + _position * dif + _displacement * normal;
            if (Main.rand.NextChance(0.25)) {
                _thickness -= _thickness / 10;
            }
            _results.Add(new Line(Projectile, segmentTexture.Value, endTexture.Value, prevPoint, _point, _thickness));
            prevPoint = _point;
            prevDisplacement = _displacement;
        }
        _results.Add(new Line(Projectile, segmentTexture.Value, endTexture.Value, prevPoint, dest, _thickness));
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        //bolt.Draw(Main.spriteBatch, Color.White);

        UpdateSegments();

        foreach (var segment in _results) {
            segment.Draw(Main.spriteBatch, Color.White);

            if (!Main.dedServ) {
                Lighting.AddLight(segment.Position, new Color(70, 224, 226).ToVector3());
            }
        }

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 lineEnd = Projectile.position + Vector2.Normalize(Projectile.velocity) * length * 1.15f;
        return Helper.DeathrayHitbox(Projectile.position, lineEnd, targetHitbox, 16f);
    }

    private struct Line {
        private readonly Projectile _projectile;
        private readonly Vector2 _a, _b;
        private readonly Texture2D _endTexture, _segmentTexture;
        private readonly float _thickness;

        public readonly Vector2 Position => _a + (_b - _a);
        public readonly Player Player => Main.player[_projectile.owner];

        public Line(Projectile projectile, Texture2D segmentTexture, Texture2D endTexture, Vector2 a, Vector2 b, float thickness = 1) {
            _projectile = projectile;
            _segmentTexture = segmentTexture;
            _endTexture = endTexture;
            _a = a;
            _b = b;
            _thickness = thickness;
        }

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
                dust.scale *= 0.325f;
                dust.fadeIn = dust.scale + 0.1f;
                dust.noGravity = true;
            }

            float rotation = (dest - source).ToRotation();
            float thicknessScale = _thickness / _segmentTexture.Height;
            Vector2 capOrigin = new(_endTexture.Width, _endTexture.Height / 2f);
            Vector2 middleOrigin = new(0, _segmentTexture.Height / 2f);
            Vector2 middleScale = new((dest - source).Length(), thicknessScale);
            spriteBatch.Draw(ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "LightningLight2").Value, source - Main.screenPosition, null, color, rotation, middleOrigin, middleScale * 0.01f, SpriteEffects.None, 0f);
            //spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(_segmentTexture, source - Main.screenPosition, null, color, rotation, middleOrigin, middleScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_endTexture, source - Main.screenPosition, null, color, rotation, capOrigin, thicknessScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_endTexture, dest - Main.screenPosition, null, color, rotation + MathHelper.Pi, capOrigin, thicknessScale, SpriteEffects.None, 0f);
            //spriteBatch.EndBlendState();
        }
    }
}