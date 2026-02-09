using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class CloudPlatform : ModProjectile_NoTextureLoad {
    private static Asset<Texture2D> _texture1 = null!,
                                    _texture2 = null!;

    private Vector2 _impactVelocity;

    public ref float SecondValue => ref Projectile.localAI[0];
    public ref float InitValue => ref Projectile.localAI[1];
    public ref float ImpactValue => ref Projectile.localAI[2];

    public bool Second {
        get => SecondValue != 0f;
        set => SecondValue = value.ToInt();
    }

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public bool Impacted {
        get => ImpactValue != 0f;
        set => ImpactValue = value.ToInt();
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);

        if (Main.dedServ) {
            return;
        }

        _texture1 = ModContent.Request<Texture2D>(ResourceManager.MiscellaneousProjectileTextures + "CloudPlatform1");
        _texture2 = ModContent.Request<Texture2D>(ResourceManager.MiscellaneousProjectileTextures + "CloudPlatform2");
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(32, 20);

        Projectile.friendly = true;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (!Init) {
            Init = true;

            Second = Main.rand.NextBool();
        }

        if (Projectile.ai[0] < 1f) {
            Projectile.ai[0] += TimeSystem.LogicDeltaTime * 2.5f;
        }
        else {
            Projectile.ai[0] = 1f;
        }

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, _impactVelocity, 0.5f);

        _impactVelocity *= 0.9f;
        if (_impactVelocity.Length() <= 1f) {
            Impacted = false;
        }

        Projectile.Animate(10);

        Player player = Projectile.GetOwnerAsPlayer();
        bool collided = Projectile.getRect().Intersects(player.getRect());
        if (player.velocity.Y > 0f && collided) {
            Impact(player.velocity.SafeNormalize() * 20f);
        }
    }

    public void Impact(Vector2 velocity) {
        if (Impacted) {
            return;
        }

        Impacted = true;

        _impactVelocity = velocity;
    }

    protected override void Draw(ref Color lightColor) {
        Vector2 position = Projectile.position;
        Projectile.position.Y += 2f;
        Projectile.position += Projectile.velocity;

        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, texture: Second ? _texture2.Value : _texture1.Value);

        Projectile.position = position;
    }
}
