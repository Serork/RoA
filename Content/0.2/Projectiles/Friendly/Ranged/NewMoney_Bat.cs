using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class NewMoneyBat : ModProjectile {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);

        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;

        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        Projectile.Animate(NewMoney.BAT_ANIMATIONTIME);

        Projectile.rotation = Projectile.velocity.X * 0.025f;

        Player player = Projectile.GetOwnerAsPlayer();
        Projectile.SlightlyMoveTo(player.Center, speed: 7.5f);
        if (player.Distance(Projectile.Center) < 40f) {
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] > 20f) {
                Projectile.Kill();
            }
        }
    }

    public override bool ShouldUpdatePosition() => true;

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor);
        if (_glowTexture?.IsLoaded == true) {
            Projectile.QuickDrawAnimated(Color.White * 0.9f, texture: _glowTexture.Value);
        }

        return false;
    }
}
