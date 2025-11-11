using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.VisualEffects;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Content.VisualEffects;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class NewMoneyBullet : ModProjectile {
    public static Color BulletColor => Color.Lerp(new Color(244, 76, 78), new Color(255, 128, 129), Helper.Wave(0f, 1f, 15f, 0f));

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4; // The length of old position to be recorded
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
        //Projectile.light = 0.5f; // How much light emit around the projectile
        Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
        Projectile.tileCollide = true; // Can the projectile collide with tiles?
        Projectile.extraUpdates = 1; // Set to above 0 if you want the projectile to update multiple time in a frame

        AIType = ProjectileID.Bullet; // Act exactly like default Bullet

        Projectile.ArmorPenetration = 10;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, NewMoneyBullet.BulletColor.ToVector3() * 0.5f);

        if (Projectile.Distance(Projectile.GetOwnerAsPlayer().Center) > 25f && Main.rand.Next(3) == 0) {
            int num179 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y) - Vector2.One, Projectile.width, Projectile.height, ModContent.DustType<NewMoneyDust>(),
                Projectile.velocity.X, Projectile.velocity.Y, 50, default(Color), 0.8f + 0.2f * Main.rand.NextFloat());
            Main.dust[num179].noGravity = true;
            Main.dust[num179].velocity *= 0.375f;
            Main.dust[num179].velocity *= Main.rand.NextFloat(0.75f, 1f);
            Main.dust[num179].fadeIn = Main.rand.NextFloat(0.5f, 1f);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Color baseColor = BulletColor;

        // Redraw the projectile with the color not influenced by light
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length - 2; k++) {
            Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color = baseColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY), 
            null, baseColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }

    public override void OnKill(int timeLeft) {
        // This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

        // special hit effect
        int num697 = 6;
        for (int num698 = 0; num698 < num697; num698++) {
            int num699 = Dust.NewDust(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), 0, 0, ModContent.DustType<NewMoneyDust>(), 0f, 0f, 0);
            Dust dust2 = Main.dust[num699];
            dust2.fadeIn = Main.rand.NextFloat(0.5f, 1f);
            dust2.noGravity = true;
            Main.dust[num699].velocity += Vector2.One.RotatedByRandom(MathHelper.TwoPi);
            dust2 = Main.dust[num699];
            dust2.position -= Vector2.One * 4f;
            Main.dust[num699].position = Vector2.Lerp(Main.dust[num699].position, Projectile.Center, 0.5f);
            Main.dust[num699].velocity *= Main.rand.NextFloat(0.5f, 1f);
            Main.dust[num699].velocity += Projectile.oldVelocity * 0.15f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        //if (!Projectile.GetOwnerAsPlayer().HasProjectile<NewMoneyBat>()) 
        {
            target.GetCommon().NewMoneyEffectByPlayerWhoAmI = Projectile.owner;
            target.AddBuff<NewMoneyDebuff>(300);
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        //if (!Projectile.GetOwnerAsPlayer().HasProjectile<NewMoneyBat>()) 
        {
            target.GetCommon().NewMoneyEffectByPlayerWhoAmI = Projectile.owner;
            target.AddBuff<NewMoneyDebuff>(300);
        }
    }
}
