using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Cloudberry : NatureProjectile_NoTextureLoad {
    private static short TIMELEFT => 360;

    private static Asset<Texture2D>? _cloudberryTexture;

    public override void Load() {
        LoadCloudberryTexture();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSize(16);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 7;
    }

    public override void AI() {

    }

    protected override void Draw(ref Color lightColor) {
        if (_cloudberryTexture?.IsLoaded != true) {
            return;
        }


    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    private void LoadCloudberryTexture() {
        if (Main.dedServ) {
            return;
        }

        _cloudberryTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "Cloudberry");
    }
}
