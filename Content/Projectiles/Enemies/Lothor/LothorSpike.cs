using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Utilities;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorSpike : ModProjectile {
    private const int LENGTH = 20;

    private struct PartInfo {
        public int Direction;
        public int Variant;
        public float Progress;
    }

    private PartInfo[] _partInfo;

    private int Length => Projectile.ai[2] != 0f ? (int)Projectile.ai[2] : LENGTH;

    public override string Texture => ResourceManager.EnemyProjectileTextures + "Lothor/LothorSpike";
    public static string TipTexture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<LothorSpike>()).Texture + "Tip";
    public static string StartTexture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<LothorSpike>()).Texture + "Start";

    public override void SetDefaults() {
        int width = 30; int height = 32;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = AIType = -1;

        Projectile.penetrate = -1;
        Projectile.timeLeft = 400;

        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float length = 0f;
        for (int i = 0; i < _partInfo.Length; i++) {
            length += _partInfo[i].Progress;
        }
        Vector2 lineEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * length * 11f;
        return Helper.DeathrayHitbox(Projectile.Center, lineEnd, targetHitbox, 30f);
    }

    public override void OnSpawn(IEntitySource source) {
        _partInfo = new PartInfo[LENGTH];
        for (int i = 0; i < _partInfo.Length; i++) {
            _partInfo[i].Direction = 0;
            _partInfo[i].Variant = (i % 2 == 0).ToInt();
        }

        float num = 20f;
        if (Projectile.velocity == -Vector2.UnitY) {
            num *= 2f;
        }
        Projectile.timeLeft = (int)(Length * num);
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
        }

        float length = 0f;
        for (int i = 0; i < _partInfo.Length; i++) {
            length += _partInfo[i].Progress;
        }
        if (Projectile.Opacity > 0f && length < (Length + 5) && Main.rand.NextBool(4)) {
            Vector2 position = Projectile.position + Projectile.velocity.SafeNormalize(Vector2.Zero) * length * 11f -
                Vector2.One * 2f;
            for (int k = 0; k < 3; k++) {
                Dust.NewDust(position, Projectile.width, Projectile.height, ModContent.DustType<RedLineDust>(), Projectile.velocity.X * 0.025f, Projectile.velocity.Y * 0.025f, Projectile.alpha, default, 1.2f);
            }
            Dust.NewDust(position, Projectile.width, Projectile.height, ModContent.DustType<RedLineDust>(), 0f, 0f, Projectile.alpha, default, 1.1f);
            if (length >= (Length + 4)) {
                for (int i = 0; i < 3; i++) {
                    int dust = Dust.NewDust(position, Projectile.width, Projectile.height, ModContent.DustType<RedLineDust>(), Projectile.velocity.X * 0.025f, Projectile.velocity.Y * 0.025f, Projectile.alpha, default, Scale: 1.2f);
                    Main.dust[dust].velocity += Projectile.velocity * Main.rand.NextFloat(0.6f, 0.8f);
                    Main.dust[dust].noGravity = true;
                }
            }
        }

        Projectile.Opacity = Utils.GetLerpValue(0, 40, Projectile.timeLeft, true);

        for (int i = 0; i < _partInfo.Length; i++) {
            if (_partInfo[i].Progress < 1f) {
                _partInfo[i].Progress += 0.325f;
                break;
            }
        }
    }

    public override void PostAI() {
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture;
        Vector2 start = Projectile.Center;
        int index = 0;
        int length = Length;
        Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.One);
        int width;
        int height;
        Vector2 origin;
        Rectangle sourceRectangle;
        SpriteEffects effects;
        while (index < length) {
            if (index == length - 1) {
                texture = ModContent.Request<Texture2D>(TipTexture).Value;
                width = texture.Width;
                height = texture.Height;
                sourceRectangle = new(0, 0, width, 0);
            }
            else if (index == 0) {
                texture = ModContent.Request<Texture2D>(StartTexture).Value;
                width = texture.Width;
                height = texture.Height;
                sourceRectangle = new(0, 0, width, 0);
            }
            else {
                texture = ModContent.Request<Texture2D>(Texture).Value;
                width = texture.Width;
                height = texture.Height / 2;
                sourceRectangle = new(0, _partInfo[index].Variant * height, width, 0);
            }
            sourceRectangle.Height = (int)(height * _partInfo[index].Progress);
            effects = (SpriteEffects)_partInfo[index].Direction;
            origin = new Vector2(width, height) / 2f;
            height -= 4;
            Vector2 drawPosition = start;
            Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, sourceRectangle, Color.White * Projectile.Opacity, velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi, origin, Projectile.scale, effects);
            start += height * velocity;
            index++;
        }
        return false;
    }
}
