using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
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

    // also see PlanterBoxes.cs
    public static bool On_Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(Player self) {
        bool result = false;
        if (!result && self.inventory[self.selectedItem].createTile >= 0 && TileID.Sets.Platforms[self.inventory[self.selectedItem].createTile]) {
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatform>()) {
                for (int k = Player.tileTargetX - 1; k <= Player.tileTargetX + 1; k++) {
                    for (int l = Player.tileTargetY - 1; l <= Player.tileTargetY + 1; l++) {
                        for (int i = 0; i < 3; i++) {
                            for (int j = 0; j < 3; j++) {
                                if (projectile.position.ToTileCoordinates() + new Point(i, j) == new Point(k, l)) {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        result = true;
        return result;
    }

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

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;

        Projectile.timeLeft = 900;
    }

    public override void AI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);

        if (!Init) {
            Init = true;

            Second = Main.rand.NextBool();

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());

            Projectile projectile = Projectile;
            int num22 = projectile.height;

            for (int num23 = 0; num23 < 10; num23++) {
                int num24 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ChainedCloudDust>(), (0f - projectile.velocity.X) * 0.5f, projectile.velocity.Y * 0.5f, 100, default(Color), 1.5f);
                Main.dust[num24].velocity.X = Main.dust[num24].velocity.X * 0.5f - projectile.velocity.X * 0.1f;
                Main.dust[num24].velocity.Y = Main.dust[num24].velocity.Y * 0.5f - projectile.velocity.Y * 0.3f;
            }
        }

        if (Projectile.ai[1]++ > 60f) {
            Projectile projectile = Projectile;
            int num22 = projectile.height;

            for (int num23 = 0; num23 < 1; num23++) {
                int num24 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ChainedCloudDust>(), (0f - projectile.velocity.X) * 0.5f, projectile.velocity.Y * 0.5f, 0, default(Color), 1.5f);
                Main.dust[num24].velocity.X = Main.dust[num24].velocity.X * 0.5f - projectile.velocity.X * 0.1f;
                Main.dust[num24].velocity.Y = Main.dust[num24].velocity.Y * 0.5f - projectile.velocity.Y * 0.3f;
            }
            Projectile.ai[1] = 0f;
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
        bool collided = player.position.Y < Projectile.position.Y && Projectile.getRect().Intersects(player.getRect());
        if (player.velocity.Y > 0f && collided) {
            Impact(player.velocity.SafeNormalize() * 20f);
        }
    }

    public override void OnKill(int timeLeft) {
        Projectile projectile = Projectile;
        int num22 = projectile.height;

        for (int num23 = 0; num23 < 10; num23++) {
            int num24 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ChainedCloudDust>(), 0f, 0f, 0, default(Color), 1.5f);
            Main.dust[num24].velocity *= 1f;
        }
    }

    public void Impact(Vector2 velocity) {
        if (Impacted) {
            return;
        }

        Impacted = true;

        _impactVelocity = velocity;

        Projectile.velocity = _impactVelocity * 0.5f;

        Projectile projectile = Projectile;
        int num22 = projectile.height;

        for (int num23 = 0; num23 < 10; num23++) {
            int num24 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ChainedCloudDust2>(), (0f - projectile.velocity.X) * 0.5f, projectile.velocity.Y * 0.5f, 100, default(Color), 1.5f);
            Main.dust[num24].velocity.X = Main.dust[num24].velocity.X * 0.5f - projectile.velocity.X * 0.1f;
            Main.dust[num24].velocity.Y = Main.dust[num24].velocity.Y * 0.5f - projectile.velocity.Y * 0.3f;
        }

        Projectile.velocity *= 0f;
    }

    protected override void Draw(ref Color lightColor) {
        Vector2 position = Projectile.position;
        Projectile.position.Y += 2f;
        Projectile.position += Projectile.velocity;

        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity * 0.9f, texture: Second ? _texture2.Value : _texture1.Value);

        Projectile.position = position;
    }
}
