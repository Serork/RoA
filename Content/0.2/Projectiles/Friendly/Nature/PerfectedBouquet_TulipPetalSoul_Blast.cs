using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

using static tModPorter.ProgressUpdate;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class TulipBlast : NatureProjectile {
    private static byte FRAMECOUNT => 3;
    private static ushort MAXTIMELEFT => 80;

    public override Color? GetAlpha(Color lightColor) => TulipPetalSoul.SoulColor * Projectile.Opacity;

    public override void SetStaticDefaults() {
        //ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
        //ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode

        Projectile.SetFrameCount(FRAMECOUNT);
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.Opacity = 1f;
    }

    public override void AI() {
        float opacityFluff = MAXTIMELEFT * 0.2f;
        int npcWhoAmI = (int)Projectile.ai[2];
        NPC target = Main.npc[npcWhoAmI];
        //Projectile.Opacity = Utils.GetLerpValue(MAXTIMELEFT, MAXTIMELEFT - opacityFluff, Projectile.timeLeft, true);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.Center += Projectile.velocity.SafeNormalize() * 20f;
            Projectile.hide = true;
            return;
        }
        Projectile.localAI[0] = 2f;
        Projectile.hide = false;
        if (Projectile.velocity.Length() != 0f) {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        float maxSpeedModifier = 2.5f;
        ref float chargeUpTimer = ref Projectile.ai[0];
        chargeUpTimer += 1f;
        float progress = Projectile.ai[0] / opacityFluff;
        chargeUpTimer += maxSpeedModifier * MathUtils.Clamp01(1f - progress / maxSpeedModifier);
        Projectile.velocity = Projectile.velocity.SafeNormalize() * MathF.Pow(progress, maxSpeedModifier);
        Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(maxSpeedModifier * 10f);

        Projectile.frame = (int)Projectile.ai[1];
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        //// Redraw the projectile with the color not influenced by light
        //Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        //for (int k = 0; k < Projectile.oldPos.Length; k++) {
        //    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
        //    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
        //    Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        //}

        return Projectile.localAI[0] == 2f;
    }

    public override void OnKill(int timeLeft) {
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
    }
}
