using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.VisualEffects;
using RoA.Content.AdvancedDusts;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged.Ammo;

sealed class MercuriumBulletProjectile : ModProjectile {
    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
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
        Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
        Projectile.alpha = 255; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
        Projectile.light = 0.5f; // How much light emit around the projectile
        Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
        Projectile.tileCollide = true; // Can the projectile collide with tiles?
        Projectile.extraUpdates = 0; // Set to above 0 if you want the projectile to update multiple time in a frame

        AIType = ProjectileID.Bullet; // Act exactly like default Bullet
    }

    public override void AI() {
        if (Main.rand.Next(6) == 0) {
            int num179 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 279
                , Projectile.velocity.X, Projectile.velocity.Y, 100, default(Color), 1.2f);
            Main.dust[num179].noLightEmittence = true;
            Main.dust[num179].noGravity = true;
            Main.dust[num179].velocity *= 0.2f;
        }

        if (Main.rand.NextBool(3) && Vector2.Distance(Projectile.position, Main.player[Projectile.owner].position) > 100f) {
            int num67 = Dust.NewDust(Projectile.position - Vector2.One * 18f - Projectile.velocity * 5f, 24, 24, ModContent.DustType<Dusts.ToxicFumes>(), 0f, 0f, Alpha: 50);
            Main.dust[num67].noGravity = true;
            Main.dust[num67].fadeIn = 1.5f;
            Main.dust[num67].velocity *= 0.25f;
            Main.dust[num67].velocity += Projectile.velocity * 0.25f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        // Redraw the projectile with the color not influenced by light
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }

    public override void OnKill(int timeLeft) {
        // This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

        Vector2 offset = new(0.2f);
        Vector2 velocity = 1.5f * offset;
        Vector2 position = Main.rand.NextVector2Circular(4f, 4f) * offset;
        Color color = new Color(242, 255, 214) * 0.75f;
        position = Projectile.Center;
        velocity = Vector2.Zero;
        int layer = AdvancedDustLayer.ABOVENPCS;
        float scale = Main.rand.NextFloat(0.7f, 1.3f);
        AdvancedDustSystem.New<MercuriumBulletParticle>(layer).
            Setup(position,
                  velocity,
                  color,
                  scale);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new AdvancedDustSpawnPacket(AdvancedDustSpawnPacket.VisualEffectPacketType.MercuriumBulletParticle, Main.player[Projectile.owner], layer, position, velocity, color,
                scale, 0f));
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 420);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 420);
    }
}