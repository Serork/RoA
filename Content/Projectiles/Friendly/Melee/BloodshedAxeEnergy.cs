using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Melee;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class BloodshedAxeEnergy : ModProjectile {
    private int NPC => (int)Projectile.ai[0];

    private int Direction => (int)Projectile.ai[1];

    public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0) * 0.9f;

    public override void SetDefaults() {
        int width = 54; int height = 50;
        Projectile.Size = new Vector2(width, height);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.timeLeft = 100;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;

        Projectile.alpha = 255;

        Projectile.extraUpdates = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 40;
    }

    public override bool? CanDamage()
        => Projectile.timeLeft < 80 && Projectile.alpha < 125;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 12; i++) {
            var v = Main.rand.NextVector2Unit();
            var d = Dust.NewDustPerfect(target.Center + v * new Vector2(Main.rand.NextFloat(target.width / 2f + 16f),
                Main.rand.NextFloat(target.height / 2f + 16f)), DustID.Blood, v * 5f, Scale: Main.rand.NextFloat(1f, 1.2f));
            d.noGravity = true;
            d.noLightEmittence = true;
        }

        int type = ModContent.ProjectileType<BloodshedAxeHeal>();
        Player player = Main.player[Projectile.owner];
        if (player.ownedProjectileCounts[type] < 5) {
            //Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target), target.Center, VectorHelper.VelocityToPoint(target.Center, player.Center, 1f), type, damage, knockback, Projectile.owner);
        }
        if (_canHeal) {
            /*for (int i = 0; i < 16; i++) {
				int dust = Dust.NewDust(player.Center, 0, 0, DustID.RainbowMk2, 0f, 0f, 0, new DrawColor(50, 20, 20, 0), 3f - i * 0.2f);
				Main.dust[dust].velocity = player.Center - Main.dust[dust].position;
				Main.dust[dust].velocity.Normalize();
				Main.dust[dust].velocity *= i;
				Main.dust[dust].noLightEmittence = true;
				Main.dust[dust].noGravity = true;
			}*/

            player.Heal(Main.DamageVar((int)(Projectile.damage / 2 + Projectile.damage / 4), 30, player.luck));
            _canHeal = false;
        }
    }

    private float _circle = MathHelper.TwoPi;
    private float _progress1, _progress2, _rotation, _rotationTarget, _rotationSpeed;
    private Vector2 _rotationCenter, _offset;
    private bool _canHeal = true;

    public override void AI() {
        NPC npc = Main.npc[NPC];
        if (!npc.active) {
            Projectile.Kill();
            return;
        }
        _offset = new Vector2(0, npc.height * 1.2f);
        _rotationCenter = npc.Center - _offset;

        if (Projectile.timeLeft == 100) {
            _rotation = _circle * -0.2f * Direction;
            _rotationTarget = -_rotation;
        }

        _progress1 = 1f - Helper.EaseInOut3((Projectile.timeLeft - 80) / 20f / 2f + 0.5f);
        _progress2 = 1f - Helper.EaseInOut3(Projectile.timeLeft / 80f);

        if (Projectile.timeLeft >= 80) {
            _rotationSpeed = (_circle * -0.3f * Direction - _rotation) * _progress1;
            Projectile.alpha = (int)(255 - 255 * _progress1);
        }
        else {
            _rotationSpeed = (_rotationTarget - _rotation) * _progress2;
            Projectile.alpha = (int)(255 * _progress2);
        }

        if (Projectile.timeLeft > 45) {
            if (Main.rand.NextBool()) {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.RainbowMk2, 0f, 0f, 0, new Color(150, 50, 50, 0) * (1f - _progress2), Main.rand.NextFloat(1f, 1.5f));
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(2f, 2f);
                Main.dust[dust].position += Main.dust[dust].velocity * Main.rand.NextFloat(10f, 15f);
                Main.dust[dust].velocity = _offset.RotatedBy(_rotation + _circle / 2f - _circle / 4 * Direction) * 0.05f;
                Main.dust[dust].fadeIn = 1f;
                Main.dust[dust].noLightEmittence = true;
                Main.dust[dust].noGravity = true;
            }
        }

        if (Projectile.timeLeft == 80) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "BloodShed") { Volume = 0.3f }, npc.Center);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Heal") { Volume = 0.6f }, npc.Center);
        }
        _rotation += _rotationSpeed;
        Projectile.Center = _rotationCenter + _offset.RotatedBy(_rotation);
        Projectile.rotation = _rotation + _circle / 2f - _circle / 8f * Direction;
        Lighting.AddLight(Projectile.Center, 0.4f * (255 - Projectile.alpha) / 255, 0.2f * (255 - Projectile.alpha) / 255, 0.2f * (255 - Projectile.alpha) / 255);

        Projectile.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor);
        spriteBatch.Draw(texture,
                         position,
                         null,
                         color,
                         Projectile.rotation,
                         new Vector2(Projectile.width / 2, Projectile.height / 2),
                         Projectile.scale,
                         Direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                         0);
        return false;
    }
}
