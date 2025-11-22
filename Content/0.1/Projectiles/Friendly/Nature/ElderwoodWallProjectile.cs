using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;
using RoA.Content.Projectiles.Enemies;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class ElderwoodWallProjectile : NatureProjectile {
    private const int MAX_TIMELEFT = 180;
    private const float EXTRA = 2f;

    private int _direction;
    private float _offsetY;
    private float _currentLength;
    private bool _init;

    public override string Texture => ResourceManager.NatureProjectileTextures + "VileSpike";
    public static string TipTexture => ResourceManager.NatureProjectileTextures + "VileSpikeTip";
    public static string StartTexture => ResourceManager.NatureProjectileTextures + "VileSpikeStart";

    private int Length => (int)(Projectile.ai[0] * Projectile.scale);
    private bool Temporary => Projectile.ai[1] >= 1f;

    protected override void SafeSetDefaults() {
        int width = 30; int height = 32;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = AIType = -1;

        Projectile.penetrate = -1;
        Projectile.timeLeft = MAX_TIMELEFT;

        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 25;

        Projectile.hide = true;

        ShouldChargeWreathOnDamage = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 basePos = new Vector2(Projectile.position.X, Projectile.ai[2] + _currentLength - 35 * (Length - 1));
        return Collision.CheckAABBvAABBCollision(
            basePos,
            new Vector2(30, -(_currentLength - 35 * (Length - 1))),
            targetHitbox.Location.ToVector2(),
            targetHitbox.Size());
    }

    public override void AI() {
        if (!_init) {
            float scale = Projectile.GetOwnerAsPlayer().CappedMeleeOrDruidScale();
            Projectile.scale = scale;

            Offset();
            Projectile.ai[2] = Projectile.position.Y;
            if (Temporary) {
                Projectile.localAI[0] = Length * 10;
                Projectile.localAI[0] *= 1f + Projectile.ai[1] - 1f;
                Projectile.localAI[0] *= 2;
            }
            _currentLength = _offsetY = 22 * Length;
            Main.rand.NextDouble();
            Main.rand.NextBool();
            _direction = Main.rand.NextBool() ? -1 : 1;
            Projectile.localAI[1] = Projectile.timeLeft = (int)Projectile.localAI[0];

            _init = true;
        }

        bool flag = Projectile.owner == Main.myPlayer;
        if (flag) {
            if (Projectile.localAI[1] > Projectile.localAI[0] - 6f) {
                Point point = new Vector2(Projectile.Center.X - Projectile.width * 2f, Projectile.Center.Y - 20f).ToTileCoordinates();
                Point point2 = new Vector2(Projectile.Center.X + Projectile.width * 2f, Projectile.Center.Y + 20f).ToTileCoordinates();
                ElderwoodClaws.SpawnGroundDusts(point, point2, Projectile.ai[0] * 5f);
            }
        }

        float value = 1f + _offsetY / 10f;
        float min = MathHelper.Min(10f, Projectile.localAI[0] / 3f);
        if (Projectile.localAI[1] > (Projectile.localAI[0] - min)) {
            if (_currentLength > 0f) {
                _currentLength -= value;
            }
            Offset();
        }
        else if (Projectile.localAI[1] < min) {
            if (_currentLength < _offsetY) {
                _currentLength += value;
            }
        }
        Projectile.position.Y = Projectile.ai[2] + _currentLength;
        if (Projectile.localAI[1] > 0f) {
            Projectile.localAI[1]--;
        }

        if (!flag) {
            if (Projectile.localAI[1] > Projectile.localAI[0] - 9f) {
                Point point = new Vector2(Projectile.Center.X - Projectile.width * 2f, Projectile.Center.Y - 20f).ToTileCoordinates();
                Point point2 = new Vector2(Projectile.Center.X + Projectile.width * 2f, Projectile.Center.Y + 20f).ToTileCoordinates();
                ElderwoodClaws.SpawnGroundDusts(point, point2, Projectile.ai[0] * 5f);
            }
        }
    }

    private void Offset() {
        int width = 30;
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            int attempts = 10;
            while (projectile.owner == Projectile.owner && projectile.type == Type && projectile.identity != Projectile.identity &&
                    Projectile.position.X < projectile.position.X + width && Projectile.position.X > projectile.position.X - width) {
                Projectile.position.X -= (width + 2) * (Projectile.position - Projectile.GetOwnerAsPlayer().position).X.GetDirection();
                if (--attempts <= 0) {
                    break;
                }
            }
        }
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 start = Projectile.Center + Vector2.UnitY * texture.Height * EXTRA;
        int index = 0;
        int length = Length + (int)EXTRA;
        Texture2D startTexture = ModContent.Request<Texture2D>(StartTexture).Value;
        SpriteEffects effects = _direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        while (index < length) {
            if (index == 0) {
                texture = startTexture;
            }
            else {
                texture = ModContent.Request<Texture2D>(Texture).Value;
            }
            void next() {
                start.Y -= texture.Height;
                index++;
            }
            if (index == length - 1) {
                texture = ModContent.Request<Texture2D>(TipTexture).Value;
            }
            float value = Projectile.ai[2] + texture.Height;
            float value2 = Projectile.ai[2] + texture.Height * 2;
            bool flag = start.Y > value; // scissoring start
            bool flag2 = start.Y > value2;
            if (!flag2) {
                Vector2 pos = start - Vector2.UnitY * texture.Height * EXTRA / 2f + Vector2.UnitY * Projectile.height;
                float value3 = MathHelper.Clamp(start.Y - value, 0, texture.Height);
                int value4 = (int)MathHelper.Clamp(texture.Height - value3, 0, texture.Height);
                Rectangle rectangle = new(0, 0, texture.Width, flag ? value4 : texture.Height);
                Color color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16 - 1);
                Vector2 origin = new(texture.Width / 2f, texture.Height);
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, rectangle, color, Projectile.rotation, origin, 1f, effects);
                if (flag) {
                    texture = ModContent.Request<Texture2D>(StartTexture).Value;
                    pos.Y += value4;
                    color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);
                    origin = new(texture.Width / 2f, texture.Height);
                    Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, color, Projectile.rotation, origin, 1f, effects);
                }
            }
            next();
        }
        return false;
    }
}
