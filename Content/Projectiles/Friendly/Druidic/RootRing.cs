using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class RootRing : NatureProjectile {
    public override Color? GetAlpha(Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 0) * (1f - (float)Projectile.alpha / 255f);

    protected override void SafeSetDefaults() {
        int width = 296; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.hostile = false;
        Projectile.friendly = true;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.alpha = 255;

        Projectile.light = 0.25f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    private float _fading, _fading2;
    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write((float)_fading);
        writer.Write((float)_fading2);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _fading = reader.ReadSingle();
        _fading2 = reader.ReadSingle();
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        if (Projectile.alpha >= 65)
            Projectile.ai[1] += Projectile.ai[1] + 0.000000005f;
        float _acc = 0.01f;
        _fading2 += _acc * (_fading == 0f ? 1f : -1f);
        if (_fading2 >= 0.8f) _fading = 1f;
        else if (_fading2 <= 0.2f) _fading = 0f;
        FadeIn();

        Player player = Main.player[Projectile.owner];
        Lighting.AddLight(Projectile.Center, 0.8f, 0.1f, 0.3f);

        Vector2 position = new((float)(player.Center.X - (Projectile.width / 2)), (float)(player.Center.Y - 90 + (float)(player.gfxOffY - 60.0)));
        Projectile.position = new Vector2((int)position.X, (int)position.Y);
        if (player.gravDir == -1.0) Projectile.position.Y += 120f;
        Projectile.rotation += 0.06f;

        if (!player.GetModPlayer<DruidStats>().SoulOfTheWoods || !player.GetModPlayer<WreathHandler>().IsFull2 || player.dead) {
            Projectile.alpha += 15;
            if (Projectile.alpha > 255) {
                Projectile.alpha = 255;
                Projectile.Kill();
            }
        }
        Projectile.netUpdate = true;
    }

    private void FadeIn() {
        if (Projectile.ai[1] <= 50f) {
            Projectile.alpha -= 15;
            if (Projectile.alpha < 70)
                Projectile.alpha = 70;
            return;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        => target.immune[Projectile.owner] = 5;

    public override bool? CanHitNPC(NPC target)
        => target.immune[Projectile.owner] <= 0 && Vector2.Distance(target.Center, Main.player[Projectile.owner].Center) >= 110f && Vector2.Distance(target.Center, Main.player[Projectile.owner].Center) <= 170f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool flag = target.life <= target.lifeMax / 2;
        if (flag) {
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
    }

    public override bool? CanCutTiles()
        => false;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.BeginBlendState(BlendState.AlphaBlend, SamplerState.LinearClamp);
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 position = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor) * 0.75f;
        float multiplier = 0.075f * _fading2;
        for (int i = 0; i < 2; i++)
            spriteBatch.Draw(texture, position, null, color * _fading2, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale + (i < 1 ? multiplier : -multiplier) * _fading2, SpriteEffects.None, 0);
        spriteBatch.EndBlendState();
        return true;
    }
}
