using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class CursedAcorn : ModProjectile {
    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.9f);

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.width = 10;
        Projectile.height = 18;
        Projectile.penetrate = 2;
        Projectile.alpha = 0;
        Projectile.scale = 1f;
        Projectile.friendly = false;
        Projectile.hostile = true;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox = GeometryUtils.CenteredSquare(Projectile.Center, 12);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color2 = Color.White * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            spriteBatch.Draw(texture, drawPos, null, color2, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.velocity.X > 0f).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new(0, 0, texture.Width, texture.Height);
        Color color = Color.White * 0.9f;
        Vector2 origin = sourceRectangle.Size() / 2f;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        return false;
    }

    public override void AI() {
        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity);

        Player player = Main.player[(int)Projectile.ai[2]];
        if (player.dead || !player.active) {
            return;
        }
        bool flag = player.position.Y > Projectile.position.Y + Projectile.height;
        if (flag) {
            return;
        }
        int vecX = (int)Projectile.position.X / 16;
        int vecY = (int)Projectile.position.Y / 16 + 1;
        Tile tile = Main.tile[vecX, vecY];
        if (tile != null && tile.HasTile) {
            if (TileID.Sets.Platforms[tile.TileType]) {
                Projectile.Kill();
            }
        }
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 0.3f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.8f, Pitch = 0.5f }, Projectile.Center);
        float lifeProgress = 1f;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.type == ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Lothor>()) {
                lifeProgress = npc.As<NPCs.Enemies.Bosses.Lothor.Lothor>().LifeProgress;
                break;
            }
        }
        int damage = (int)MathHelper.Lerp(NPCs.Enemies.Bosses.Lothor.Lothor.WREATH_DAMAGE, NPCs.Enemies.Bosses.Lothor.Lothor.WREATH_DAMAGE2, lifeProgress);
        damage /= 4;
        int knockBack = (int)MathHelper.Lerp(NPCs.Enemies.Bosses.Lothor.Lothor.WREATH_KNOCKBACK, NPCs.Enemies.Bosses.Lothor.Lothor.WREATH_KNOCKBACK2, lifeProgress);
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center + Vector2.UnitY * 20f, -Vector2.UnitY, ModContent.ProjectileType<LothorSpike>(),
                damage, knockBack, Main.myPlayer, ai2: 12.5f);
        }
    }
}
