using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.Items;
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
sealed class CloudPlatformAngry : NatureProjectile_NoTextureLoad, IDrawProjectileAbovePlayer {
    private static Asset<Texture2D> _texture1 = null!,
                                    _texture2 = null!,
                                    _crownTexture = null!;

    private Vector2 _impactVelocity;
    private float _attackCounter;

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

    public bool HasCrown {
        get => Projectile.ai[2] != 0f;
        set => Projectile.ai[2] = value.ToInt();
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;
    public override bool ShouldUpdatePosition() => false;

    // also see PlanterBoxes.cs
    public static bool On_Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(Player self) {
        bool result = false;
        if (!result && self.inventory[self.selectedItem].createTile >= 0 && TileID.Sets.Platforms[self.inventory[self.selectedItem].createTile]) {
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatformAngry>()) {
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
        Projectile.SetFrameCount(6);

        if (Main.dedServ) {
            return;
        }

        _texture1 = ModContent.Request<Texture2D>(ResourceManager.MiscellaneousProjectileTextures + "CloudPlatformAngry1");
        _texture2 = ModContent.Request<Texture2D>(ResourceManager.MiscellaneousProjectileTextures + "CloudPlatformAngry2");
        _crownTexture = ModContent.Request<Texture2D>(ResourceManager.MiscellaneousProjectileTextures + "CloudPlatformCrown");
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(32, 20);

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;

        Projectile.timeLeft = MathUtils.SecondsToFrames(15);
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

            if (Projectile.IsOwnerLocal()) {
                HasCrown = Main.rand.NextBool(10);
                Projectile.netUpdate = true;
            }
        }

        if (Projectile.Opacity >= 1f) {
            bool flag18 = true;
            int num352 = (int)Projectile.Center.X;
            int num353 = (int)(Projectile.position.Y + (float)Projectile.height);
            if (Collision.SolidTiles(new Vector2(num352, num353), 2, 20))
                flag18 = false;
            if (flag18) {
                _attackCounter += 1f;
                if (_attackCounter > 8f) {
                    _attackCounter = 0f;
                    if (Projectile.owner == Main.myPlayer) {
                        num352 += (int)(Main.rand.Next(-14, 15) * 0.75f);
                        int baseDamage = 50;
                        float baseKnockback = 0f;
                        int damage = (int)Projectile.GetOwnerAsPlayer().GetDamage(DruidClass.Nature).ApplyTo(baseDamage);
                        baseKnockback = Projectile.GetOwnerAsPlayer().GetKnockback(DruidClass.Nature).ApplyTo(baseKnockback);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), num352, num353, 0f, 5f, ModContent.ProjectileType<CloudPlatformAngryRain>(), damage, baseKnockback, Projectile.owner);
                    }
                }
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

        Projectile.Animate(6);

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

        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity * 0.9f;
        Projectile.QuickDrawAnimated(color, texture: Second ? _texture2.Value : _texture1.Value);

        Projectile.position = position;
    }

    void IDrawProjectileAbovePlayer.DrawAbovePlayer(Projectile projectile) {
        CloudPlatformAngry me = projectile.As<CloudPlatformAngry>();
        if (me.HasCrown) {
            Vector2 position = projectile.position;
            projectile.position.Y += 2f;
            projectile.position += projectile.velocity;

            Color color = Lighting.GetColor(projectile.Center.ToTileCoordinates()) * projectile.Opacity * 0.9f;
            projectile.position.Y -= 13f;
            projectile.QuickDraw(color, texture: _crownTexture.Value);

            projectile.position = position;
        }
    }
}
