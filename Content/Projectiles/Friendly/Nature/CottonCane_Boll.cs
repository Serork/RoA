using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Items;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class CottonBoll : InteractableProjectile_Nature {
    private static Asset<Texture2D> _hoverTexture = null!;

    private static ushort TIMELEFT => MathUtils.SecondsToFrames(15);

    protected override Asset<Texture2D> HoverTexture => _hoverTexture;

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
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;
        }

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
            Projectile.ai[2]--;
        }
        else if (Projectile.ai[2] == 1f) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.1f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }

        bool flag = false;
        if (Projectile.ai[2] > 0f) {
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.X * 0.1f, 0.1f);
            flag = true;
        }

        if (Projectile.ai[1]-- > 0f) {
            pushOthers();
            pushOthers();

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

            if (!flag) {
                Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);
            }
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
