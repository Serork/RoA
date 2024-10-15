using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class RootRing : NatureProjectile {
    private int _alpha = 255;

    public override Color? GetAlpha(Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 0);

    protected override void SafeSetDefaults() {
        int width = 296; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.friendly = true;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.aiStyle = -1;

        //Projectile.light = 0.25f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_alpha);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _alpha = reader.ReadInt32();
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        Player player = Main.player[Projectile.owner];

        float projOpacity = 1f - (float)_alpha / 255f;
        Lighting.AddLight(Projectile.Center, new Color(248, 119, 119).ToVector3() * projOpacity);

        Vector2 position = new((float)(player.Center.X - (Projectile.width / 2)), (float)(player.Center.Y - 90 + (float)(player.gfxOffY - 60.0)));
        Projectile.position = new Vector2((int)position.X, (int)position.Y);
        if (player.gravDir == -1.0) Projectile.position.Y += 120f;
        Projectile.rotation += 0.06f;

        if (Projectile.owner == Main.myPlayer) {
            var stats = player.GetModPlayer<WreathHandler>();
            Projectile.ai[0] = stats.PulseIntensity;
            Projectile.ai[2] = stats.ActualProgress2;

            bool flag = false;
            if (!player.GetModPlayer<DruidStats>().SoulOfTheWoods || !player.GetModPlayer<WreathHandler>().IsFull2 || player.dead) {
                flag = true;
                _alpha += 15;
                if (_alpha >= 255) {
                    _alpha = 255;
                    Projectile.Kill();
                    return;
                }
            }

            if (!flag) {
                if (_alpha >= 65)
                    Projectile.ai[1] += Projectile.ai[1] + 0.000000005f;
                FadeIn();
            }

            Projectile.netUpdate = true;
        }
    }

    private void FadeIn() {
        if (Projectile.ai[1] <= 50f) {
            _alpha -= 15;
            if (_alpha < 70)
                _alpha = 70;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        => target.immune[Projectile.owner] = 5;

    public override bool? CanHitNPC(NPC target)
        => target.life <= target.lifeMax / 2 && target.immune[Projectile.owner] <= 0 && Vector2.Distance(target.Center, Main.player[Projectile.owner].Center) >= 110f && Vector2.Distance(target.Center, Main.player[Projectile.owner].Center) <= 170f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player player = Main.player[Projectile.owner];
        SoundEngine.PlaySound(SoundID.Item8, Projectile.position);
        if (Vector2.Distance(target.Center, player.Center) < 135f)
            return;
        if (target.friendly || target.lifeMax <= 5 || target.knockBackResist <= 0f || target.dontTakeDamage)
            return;
        float hitDirectionX = target.position.X > player.position.X ? 1f : -1f;
        float hitDirectionY = target.position.Y > player.position.Y ? 1f : -1f;
        float knockback2 = hit.Knockback * Main.rand.NextFloat(3f, 8f) * target.knockBackResist;
        target.velocity.X += knockback2 * hitDirectionX;
        target.velocity.Y += knockback2 * hitDirectionY;
    }

    public override bool? CanCutTiles()
        => false;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.BeginBlendState(BlendState.AlphaBlend, SamplerState.LinearClamp);
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 position = Projectile.Center - Main.screenPosition;
        Player player = Main.player[Projectile.owner];
        var stats = player.GetModPlayer<WreathHandler>();
        float projOpacity = 1f - (float)_alpha / 255f;
        Color color = stats.BaseColor * (0.75f + 0.4f * (1f - Projectile.ai[0]));
        float multiplier = 0.035f;
        //for (int i = 0; i < 2; i++)
        //spriteBatch.Draw(texture, position, null, color * _fading2, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale + (i < 1 ? multiplier : -multiplier) * _fading2, SpriteEffects.None, 0);

        spriteBatch.Draw(texture, position, null, color * projOpacity, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0);

        float progress = MathHelper.Clamp(Projectile.ai[2], 0f, 1f);
        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);
        float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f) * Projectile.ai[0];
        color *= 1.4f;
        color.A = 80;
        color *= opacity;
        opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
        color *= factor * opacity * 2f;
        color *= 3f;
        float scale = Projectile.scale + factor * 0.035f;
        for (int i = 0; i < 2; i++) {
            spriteBatch.Draw(texture, position, null, color * projOpacity, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), scale, SpriteEffects.None, 0);
        }

        spriteBatch.EndBlendState();
        return false;
    }
}
