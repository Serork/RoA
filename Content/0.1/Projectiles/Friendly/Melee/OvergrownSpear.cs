using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class OvergrownSpear : ModProjectile {
    public override void SetStaticDefaults() {
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults() {
        int width = 12; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Melee;

        Projectile.aiStyle = 19;
        AIType = 49;

        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;

        Projectile.friendly = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.tileCollide = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        bool flag = Collision.CheckAABBvAABBCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f - Vector2.One * 4f, Vector2.One * 8f);
        return flag;
    }

    public override void PostDraw(Color lightColor) {
        Projectile proj = Projectile;
        SpriteEffects dir = SpriteEffects.None;
        float num = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X) + 2.355f;
        Asset<Texture2D> asset = TextureAssets.Projectile[proj.type];
        Player player = Main.player[proj.owner];
        Microsoft.Xna.Framework.Rectangle value = asset.Frame();
        Microsoft.Xna.Framework.Rectangle rect = proj.getRect();
        Vector2 vector = Vector2.Zero;
        if (player.direction > 0) {
            dir = SpriteEffects.FlipHorizontally;
            vector.X = asset.Width();
            num -= (float)Math.PI / 2f;
        }

        if (player.gravDir == -1f) {
            if (proj.direction == 1) {
                dir = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                vector = new Vector2(asset.Width(), asset.Height());
                num -= (float)Math.PI / 2f;
            }
            else if (proj.direction == -1) {
                dir = SpriteEffects.FlipVertically;
                vector = new Vector2(0f, asset.Height());
                num += (float)Math.PI / 2f;
            }
        }

        Vector2.Lerp(vector, value.Center.ToVector2(), 0.25f);
        float num2 = 0f;
        Vector2 vector2 = proj.Center + new Vector2(0f, proj.gfxOffY);
        Color color = Color.Lerp(new Color(127, 127, 127), Lighting.GetColor((int)proj.Center.X / 16, (int)proj.Center.Y / 16), Lighting.Brightness((int)proj.Center.X / 16, (int)proj.Center.Y / 16));
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
    }
}
