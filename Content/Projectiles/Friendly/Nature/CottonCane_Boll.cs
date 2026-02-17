using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Projectiles;
using RoA.Content.Items;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CottonBoll : InteractableProjectile_Nature {
    private static Asset<Texture2D> _hoverTexture = null!;

    private static ushort TIMELEFT => MathUtils.SecondsToFrames(15);

    protected override Asset<Texture2D> HoverTexture => _hoverTexture;

    public override void SetStaticDefaults() {
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
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor);

        return false;
    }

    public override void SafeAI() {
        if (Projectile.ai[1]-- > 0f) {
            Projectile.velocity.Y -= 0.4f;
            if (Projectile.velocity.Y > 16f) {
                Projectile.velocity.Y = 16f;
            }
            if (Projectile.IsOwnerLocal()) {
                Player.BlockInteractionWithProjectiles = 30;
            }
        }
        else {
            Projectile.velocity *= 0.9f;
        }
    }

    protected override void OnInteraction(Player player) {
        player.GetCommon().CollideWithCottonBall(this);

        Projectile.ai[1] = 30f;
        Projectile.netUpdate = true;
    }

    protected override void OnHover(Player player) {
        if (Player.BlockInteractionWithProjectiles > 0) {
            return;
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<CottonBollSmall>();
    }

    protected override void DrawHoverMask(SpriteBatch spriteBatch, Color selectionGlowColor) {
        if (Projectile.IsOwnerLocal() && Player.BlockInteractionWithProjectiles > 0) {
            return;
        }

        Projectile.QuickDraw(selectionGlowColor, texture: _hoverTexture.Value);
    }
}
