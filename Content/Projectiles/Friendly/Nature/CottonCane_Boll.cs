using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.IO;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class CottonBoll : InteractableProjectile_Nature {
    private static Asset<Texture2D> _hoverTexture = null!;

    private static ushort TIMELEFT => MathUtils.SecondsToFrames(15);

    private bool _nextFiberDirectedLeft;

    protected override Asset<Texture2D> HoverTexture => _hoverTexture;

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.Write(_nextFiberDirectedLeft);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _nextFiberDirectedLeft = reader.ReadBoolean();
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(4);

        if (!Main.dedServ) {
            _hoverTexture = ModContent.Request<Texture2D>(Texture + "_Hover");
        }
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(48, 64);
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void SafeAI() {
        void pushOthers() {
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CottonBoll>(checkProjectile => checkProjectile.SameAs(Projectile))) {
                if (Projectile.Distance(projectile.Center) > Projectile.width * 1.25f) {
                    continue;
                }
                projectile.velocity += projectile.DirectionFrom(Projectile.Center) * TimeSystem.LogicDeltaTime * 2f;
            }
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CottonBollSmall>()) {
                if (Projectile.Distance(projectile.Center) > Projectile.width * 1.25f) {
                    continue;
                }
                projectile.velocity += projectile.DirectionFrom(Projectile.Center) * TimeSystem.LogicDeltaTime * 2f;
            }
        }

        pushOthers();

        Projectile.Animate(10);

        if (Projectile.ai[2] > 1f) {
            Projectile.timeLeft = 21;

            Projectile.ai[2]--;
        }
        else if (Projectile.ai[2] == 1f) {
            Projectile.timeLeft = 21;

            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.1f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
        else {
            if (Projectile.timeLeft < 20) {
                Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.1f);
                if (Projectile.Opacity <= 0f) {
                    Projectile.Kill();
                }
            }
            else {
                Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.1f);
            }
        }

        bool flag = false;
        if (Projectile.ai[2] > 0f) {
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.X * 0.1f, 0.1f);
            flag = true;
        }

        if (Main.rand.NextBool(50)) {
            for (int i = 0; i < 1; i++) {
                Vector2 position = Projectile.Center - Vector2.UnitY * Projectile.height / 3 + Main.rand.NextVector2CircularEdge(10f, 10f);
                Vector2 velocity = -Vector2.UnitY * Main.rand.NextFloat(1f, 2f) + Vector2.UnitX * Main.rand.NextFloat(-1f, 1f);
                velocity.Y *= 0.5f;
                Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<Dusts.CottonDust>(), velocity, Alpha: 25);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.scale *= 1f;
                dust.alpha = Projectile.alpha;
            }
        }

        if (Projectile.ai[1]-- > 0f) {
            Projectile.timeLeft = 21;

            pushOthers();
            pushOthers();

            Projectile.localAI[2]++;
            if (Projectile.localAI[2] > 2f) {
                Projectile.localAI[2] = 0f;

                Vector2 velocity = Vector2.UnitX * 5f * _nextFiberDirectedLeft.ToDirectionInt();

                if (Main.rand.NextBool(3)) {
                    int num693 = 1;
                    for (int num694 = 0; num694 < num693; num694++) {
                        int num695 = Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<CottonDust>(), 0f, 0f, 0);
                        Dust dust2 = Main.dust[num695];
                        Vector2 position = Projectile.Center - Vector2.UnitY * Projectile.height / 4 + Main.rand.RandomPointInArea(4f);
                        dust2.position = position;
                        dust2.velocity *= 1.6f;
                        dust2 = Main.dust[num695];
                        dust2.velocity += -Projectile.velocity * (Main.rand.NextFloat() * 2f - 1f) * 2f;
                        Main.dust[num695].scale = 1f;
                        Main.dust[num695].fadeIn = 1.5f;
                        Main.dust[num695].noGravity = true;
                        dust2 = Main.dust[num695];
                        dust2.velocity *= 0.7f;
                        dust2 = Main.dust[num695];
                        dust2.position += Main.dust[num695].velocity * 5f;
                        dust2.velocity += velocity;
                    }
                }

                if (Projectile.IsOwnerLocal()) {
                    _nextFiberDirectedLeft = !_nextFiberDirectedLeft;

                    Vector2 position = Projectile.Center - Vector2.UnitY * Projectile.height / 3 + Main.rand.RandomPointInArea(10f);
                    int damage = Projectile.damage;
                    float knockBack = Projectile.knockBack;
                    ProjectileUtils.SpawnPlayerOwnedProjectile<CottonFiber>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromThis()) {
                        Position = position,
                        Velocity = velocity,
                        Damage = damage,
                        KnockBack = knockBack
                    });

                    Projectile.netUpdate = true;
                }
            }

            Projectile.velocity.Y -= 0.4f;
            if (Projectile.velocity.Y > 16f) {
                Projectile.velocity.Y = 16f;
            }
            if (Projectile.IsOwnerLocal()) {
                Player.BlockInteractionWithProjectiles = 30;
            }
            if (Projectile.ai[1] == 1f) {
                Projectile.ai[2] = 30f;
            }

            Projectile.velocity += Projectile.Center.DirectionTo(Projectile.GetOwnerAsPlayer().GetPlayerCorePoint()) * 0.1f * new Vector2(1f, 1f);

            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.X * 0.1f, 0.1f);
            flag = true;
        }
        else {
            Projectile.velocity *= Projectile.ai[2] > 0f ? 0.9f : 0.97f;
        }

        if (!flag) {
            float offsetY = 0.1f;
            Projectile.localAI[0] = Helper.Wave(-offsetY, offsetY, 2.5f, Projectile.identity);
            Projectile.velocity.Y += Projectile.localAI[0] * 0.15f;
            Projectile.rotation = Projectile.localAI[0] * 1f + Projectile.velocity.X * 0.1f;
        }
    }

    protected override void OnInteraction(Player player) {
        player.GetCommon().CollideWithCottonBall(this);

        Projectile.ai[1] = 30f;
        Projectile.netUpdate = true;
    }

    protected override void OnHover(Player player) {
        if (Player.BlockInteractionWithProjectiles > 0 || Projectile.Opacity < 1f) {
            return;
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.CottonBollSmall>();
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }

    protected override void DrawHoverMask(SpriteBatch spriteBatch, Color selectionGlowColor) {
        if (Projectile.IsOwnerLocal() && Player.BlockInteractionWithProjectiles > 0) {
            return;
        }
        if (Projectile.Opacity < 1f) {
            return;
        }

        Projectile.QuickDrawAnimated(selectionGlowColor, texture: _hoverTexture.Value);
    }
}
