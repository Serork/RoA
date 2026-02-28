using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class Acorn : ModProjectile {
    public override void SetStaticDefaults() {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color2 = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            spriteBatch.Draw(texture, drawPos, null, color2, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.velocity.X > 0f).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new(0, 0, texture.Width, texture.Height);
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Vector2 origin = sourceRectangle.Size() / 2f;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        return false;
    }

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.aiStyle = 1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.extraUpdates = 1;

        Projectile.minion = true;
        Projectile.minionSlots = 0f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        NPC npc = Main.npc[(int)Projectile.ai[2]];
        if (!npc.active) {
            Projectile.Kill();
        }
        if (target.whoAmI == npc.whoAmI) {
            Projectile.Kill();
        }
        else {
            Projectile.damage = (int)((double)Projectile.damage * 0.95);
        }
    }

    public override void AI() {
        Projectile.tileCollide = Projectile.timeLeft <= 570;

        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity) + MathHelper.Pi;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void OnKill(int timeLeft) {
        for (int k = 0; k < 3; k++)
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 1, Projectile.oldVelocity.X * 0.2f, Projectile.oldVelocity.Y * 0.2f, newColor: Color.Lerp(new Color(129, 111, 67), new Color(107, 97, 55), Main.rand.NextFloat()));

        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig with { Pitch = Main.rand.NextFloat(0.8f, 1.2f) }, Projectile.Center);
    }
}
