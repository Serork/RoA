using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Fly : NatureProjectile_NoTextureLoad {
    private static ushort MAXTIMELEFT => 1000;

    private Color _color;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(12);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
    }

    public override void AI() {
        Projectile parent = Main.projectile[(int)Projectile.ai[0]];

        if (!parent.active || parent.type != ModContent.ProjectileType<Rafflesia>()) {
            Projectile.Kill();
            return;
        }

        Vector2 destination = parent.Center - Vector2.UnitY.RotatedBy(parent.rotation) * 10f;
        float distanceToDestination = Vector2.Distance(Projectile.position, destination);
        float minDistance = 100f;
        float inertiaValue = 30, extraInertiaValue = inertiaValue * 5;
        float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
        float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
        Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, inertia: inertia);
        float length = Projectile.velocity.Length();
        float minLength = 1f;
        if (length < minLength) {
            Projectile.velocity = Projectile.velocity.SafeNormalize() * minLength;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            _color = new Color((float)(0.7000000059604645 + Main.rand.NextDouble() * 0.20000000298023224), (float)(0.7000000059604645 + Main.rand.NextDouble() * 0.20000000298023224), 0f);
        }
    }

    public override void OnKill(int timeLeft) {
 
    }

    protected override void Draw(ref Color lightColor) {
        Texture2D texture = ResourceManager.Pixel;
         Main.spriteBatch.Draw(texture, Projectile.Center, DrawInfo.Default with {
            Clip = texture.Bounds,
            Color = _color,
            Scale = Vector2.One * 4f
        });
    }
}
