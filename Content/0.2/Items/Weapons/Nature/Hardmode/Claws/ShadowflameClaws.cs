using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class ShadowflameClaws : ClawsBaseItem<ShadowflameClaws.ShadowflameClawsSlash> {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 0f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 0.85f;
    public override float SecondAttackSpeedModifier => 0.925f;
    public override float ThirdAttackSpeedModifier => 1f;

    public override float FirstAttackScaleModifier => 1f;
    public override float SecondAttackScaleModifier => 1.25f;
    public override float ThirdAttackScaleModifier => 1.5f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 36);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Pink;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldReset = false;
    }

    protected override (Color, Color) SlashColors(Player player) => (Color.Lerp(new Color(169, 85, 240), new Color(88, 63, 163).ModifyRGB(1f), 0.75f), Color.Lerp(new Color(115, 30, 200), new Color(88, 63, 163).ModifyRGB(0.75f), 0.75f));

    public sealed class ShadowflameClawsSlash : ClawsSlash {
        private bool Charged => Projectile.GetOwnerAsPlayer().GetWreathHandler().IsActualFull6;

        public ref float Opacity => ref Projectile.localAI[1];

        protected override void SetCollisionScale(ref float coneLength) {
            if (!Charged) {
                return;
            }

            coneLength *= 1.2f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) {
            target.AddBuff(153, Main.rand.Next(60, 180) * 2);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(153, Main.rand.Next(60, 180) * 2);

            if (!Charged) {
                return;
            }

            Projectile.GetOwnerAsPlayer().GetWreathHandler().HandleOnHitNPCForNatureProjectile(Projectile, true);
        }

        public override void SafePostAI() {
            float lerpValue = 0.1f;
            if (Charged) {
                Opacity = Helper.Approach(Opacity, 1f, lerpValue);
            }
            else {
                Opacity = Helper.Approach(Opacity, 0f, lerpValue);
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            Projectile proj = Projectile;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 vector = proj.Center - Main.screenPosition;
            Asset<Texture2D> asset = TextureAssets.Projectile[ProjectileID.NightsEdge];
            Microsoft.Xna.Framework.Rectangle rectangle = asset.Frame(1, 4);
            Vector2 origin = rectangle.Size() / 2f;
            float num = proj.scale * 1.1f;
            SpriteEffects effects = ((!(proj.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float num2 = proj.localAI[0] / proj.ai[1];
            float num3 = Utils.Remap(num2, 0f, 0.6f, 0f, 1f) * Utils.Remap(num2, 0.6f, 1f, 1f, 0f);
            float num4 = 0.975f;
            float fromValue = Lighting.GetColor(proj.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            fromValue = Utils.Remap(fromValue, 0.2f, 1f, 0f, 1f);
            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(40, 20, 60);
            Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color(80, 40, 180);
            Microsoft.Xna.Framework.Color color3 = Microsoft.Xna.Framework.Color.White * num3 * 0.5f;
            color3.A = (byte)((float)(int)color3.A * (1f - fromValue));
            Microsoft.Xna.Framework.Color color4 = color3 * fromValue * 0.5f;
            color4.G = (byte)((float)(int)color4.G * fromValue);
            color4.R = (byte)((float)(int)color4.R * (0.25f + fromValue * 0.75f));

            if (Opacity > 0f) {
                Color baseColor = FirstSlashColor!.Value;
                baseColor.A = 150;
                for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                    spriteBatch.Draw(asset.Value, vector, rectangle,
                        baseColor * fromValue * num3 * 0.3f * 1f * Opacity, Main.rand.NextFloatRange(0.1f) + proj.rotation + i * Projectile.direction - MathHelper.PiOver4 * Projectile.direction, origin,
                        num * 0.8f * 1f * MathHelper.Lerp(1f, 1.25f, MathUtils.Clamp01(num2 * 1.5f)), effects, 0f);
                }

                for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                    spriteBatch.Draw(asset.Value, vector, rectangle,
                        baseColor * fromValue * num3 * 0.3f * 0.75f * Opacity, Main.rand.NextFloatRange(0.1f) + proj.rotation + i * Projectile.direction - MathHelper.PiOver4 * Projectile.direction, origin,
                        num * 0.8f * 0.5f * MathHelper.Lerp(1f, 1.25f, MathUtils.Clamp01(num2 * 1.5f)), effects, 0f);
                }
            }

            DrawItself(ref lightColor);

            if (Opacity > 0f) {
                spriteBatch.DrawWithSnapshot(() => {
                    for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                        spriteBatch.Draw(asset.Value, vector, rectangle, GetLightingColor() * fromValue * num3 * 0.3f * Opacity, proj.rotation + i * Projectile.direction - MathHelper.PiOver4 * Projectile.direction, origin, num * 0.8f, effects, 0f);
                    }
                }, blendState: BlendState.Additive);
            }

            return false;
        }

        protected override bool OnSlashDustSpawn(float progress) {
            float max = Projectile.ai[1] + Projectile.ai[1] * 0.5f;
            float scale = Projectile.scale;
            Projectile.scale *= Charged ? 1.05f : 1f;
            if (Projectile.localAI[0] >= Projectile.ai[1] * 0.5f && Projectile.localAI[0] < max) {
                float startProgress = Utils.Remap(Utils.GetLerpValue(Projectile.ai[1] * 0.5f, Projectile.ai[1] * 0.7f, Projectile.localAI[0], true), 0f, 1f, 0.5f, 1f, true);
                float endProgress = Utils.GetLerpValue(max, max * 0.75f, Projectile.localAI[0], true);
                startProgress *= endProgress;
                float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
                float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
                Player player = Main.player[Projectile.owner];
                float offset = player.gravDir == 1 ? 0f : (-MathHelper.PiOver4 * progress);
                float f = Projectile.rotation + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
                Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
                Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
                for (int num807 = 0; (float)num807 < Projectile.scale * 10f; num807++) {
                    if (!Main.rand.NextChance(startProgress * 0.9f)) {
                        continue;
                    }
                    int type = 27;
                    Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2 * player.gravDir), 125, default, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                    dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                    dust.scale *= 0.35f * startProgress;
                    dust.scale *= Projectile.scale;
                    dust.scale *= Utils.GetLerpValue(0f, 1f, position.Distance(player.Center) / 50f, true) * startProgress;
                    dust.noGravity = true;
                    Dust dust2 = dust;
                    dust2 = dust;
                    dust2.velocity -= Projectile.velocity * (1.25f - Projectile.scale);
                    dust.fadeIn = 100 + Projectile.owner;
                    dust2 = dust;
                    dust2.scale += Projectile.scale * 0.75f;
                }
            }
            Projectile.scale = scale;

            return false;
        }
    }
}
