using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class FlakCannonBullet : WeaponWithCustomAmmoProjectile {
    protected override bool ShouldAttachWeapon => false;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
    }

    public override void SetDefaults() {
        Projectile.width = 8; // The width of projectile hitbox
        Projectile.height = 8; // The height of projectile hitbox
        Projectile.aiStyle = 1; // The ai style of the projectile, please reference the source code of Terraria
        Projectile.friendly = true; // Can the projectile deal damage to enemies?
        Projectile.hostile = false; // Can the projectile deal damage to the player?
        Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
        Projectile.penetrate = 1; // How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
        Projectile.timeLeft = 90; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
        Projectile.alpha = 255; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
        //Projectile.light = 0.5f; // How much light emit around the projectile
        Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
        Projectile.tileCollide = true; // Can the projectile collide with tiles?
        Projectile.extraUpdates = 2; // Set to above 0 if you want the projectile to update multiple time in a frame

        AIType = ProjectileID.Bullet; // Act exactly like default Bullet
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, new Color(143, 255, 133).ToVector3() * 0.5f);

        Projectile.Opacity = Utils.GetLerpValue(0, 20, Projectile.timeLeft, true);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Color baseColor = lightColor;
        baseColor = Color.Lerp(baseColor, Color.White, 0.375f);
        baseColor *= Projectile.Opacity;

        // Redraw the projectile with the color not influenced by light
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length - 2; k++) {
            Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color = baseColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY), 
            null, baseColor * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

        for (int k = 0; k < Projectile.oldPos.Length - 2; k++) {
            Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color = baseColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.Opacity > 0.05f) {
            // This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}
