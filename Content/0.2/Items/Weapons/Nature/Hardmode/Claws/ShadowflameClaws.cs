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

    public override bool ResetOnHit => true;

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldReset = false;
    }

    protected override (Color, Color) SetSlashColors(Player player) 
        => (new Color(160, 85, 255), new Color(120, 50, 255));

    public sealed class ShadowflameClawsSlash : ClawsSlash {
        private bool Charged => (!CanFunction && Projectile.GetOwnerAsPlayer().GetWreathHandler().IsActualFull6) || Opacity == 1f;

        public ref float Opacity => ref Projectile.localAI[1];

        protected override void SetCollisionScale(ref float coneLength) {
            if (!Charged) {
                return;
            }

            coneLength *= 1.2f;
        }

        protected override void SpawnVisualHitEffect(Entity target) {
            base.SpawnVisualHitEffect(target);
        }

        protected override void SafeOnHitPlayer(Player target, Player.HurtInfo info) {
            target.AddBuff(153, Main.rand.Next(60, 180) * 2);
        }

        protected override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(153, Main.rand.Next(60, 180) * 2);

            if (!target.CanActivateOnHitEffect()) {
                return;
            }

            SpawnStem(target);

            if (!Charged) {
                return;
            }

            Projectile.GetOwnerAsPlayer().GetWreathHandler().HandleOnHitNPCForNatureProjectile(Projectile, true);
        }

        private void SpawnStem(NPC target) {
            if (!Charged) {
                return;
            }

            if (!Projectile.IsOwnerLocal()) {
                return;
            }

            ProjectileUtils.SpawnPlayerOwnedProjectile<ShadowflameStem>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                Position = target.Center,
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack,
                AI0 = target.whoAmI
            }, centered: true);
        }

        public override void SafePostAI() {
            if (Charged) {
                Opacity = 1f;
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            Player player = Projectile.GetOwnerAsPlayer();
            float rot = GetRotation();

            Projectile proj = Projectile;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 vector = proj.Center - Main.screenPosition;
            Main.instance.LoadProjectile(ProjectileID.NightsEdge);
            Asset<Texture2D> asset = TextureAssets.Projectile[ProjectileID.NightsEdge];
            Microsoft.Xna.Framework.Rectangle rectangle = asset.Frame(1, 4);
            Vector2 origin = rectangle.Size() / 2f;
            float num = proj.scale * 1.1f;
            SpriteEffects effects = ((!(proj.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float num2 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
            float num3 = Utils.Remap(num2, 0f, 0.6f, 0f, 1f) * Utils.Remap(num2, 0.6f, 1f, 1f, 0f);
            //float num4 = 0.975f;
            float fromValue = Lighting.GetColor(proj.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            fromValue = Utils.Remap(fromValue, 0.2f, 1f, 0f, 1f);
            Microsoft.Xna.Framework.Color color = SecondSlashColor!.Value;
            Microsoft.Xna.Framework.Color color2 = FirstSlashColor!.Value;
            Microsoft.Xna.Framework.Color color3 = Microsoft.Xna.Framework.Color.White * num3 * 0.5f;
            color3.A = (byte)((float)(int)color3.A * (1f - fromValue));
            Microsoft.Xna.Framework.Color color4 = color3 * fromValue * 0.5f;
            color4.G = (byte)((float)(int)color4.G * fromValue);
            color4.R = (byte)((float)(int)color4.R * (0.25f + fromValue * 0.75f));

            float offsetRotation = -MathHelper.PiOver2 * 0.75f * Projectile.direction;
            bool charged = Opacity > 0f;
            if (charged) {
                for (int k = 0; k < 2; k++) {
                    for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                        float progress = i / MathHelper.PiOver2;
                        Color baseColor = Color.Lerp(FirstSlashColor!.Value with { A = 150 }, SecondSlashColor!.Value with { A = 150 }, Helper.Wave(0f, 1f, 20f, Projectile.whoAmI + 3 * i))
                            .ModifyRGB(0.75f);
                        float rotation = Main.rand.NextFloatRange(0.1f) + rot + i * Projectile.direction + offsetRotation;
                        spriteBatch.Draw(asset.Value, vector, rectangle,
                            baseColor * 0.375f * fromValue * num3 * 0.3f * 1f * Opacity * progress * 1f, rotation, origin,
                            num * 0.8f * 1f * MathHelper.Lerp(1f, 1.25f, MathUtils.Clamp01(num2 * 1.5f)), effects, 0f);
                    }
                }
            }

            DrawItself(ref lightColor);

            if (charged) {
                spriteBatch.DrawWithSnapshot(() => {
                    for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                        spriteBatch.Draw(asset.Value, vector, rectangle, GetLightingColor() * fromValue * num3 * 0.3f * Opacity * 1f * 0.5f, rot + i * Projectile.direction + offsetRotation, origin, num * 0.8f, effects, 0f);
                    }
                }, blendState: BlendState.Additive);
            }

            if (charged) {
                float dir = Projectile.ai[0] * GravDir();
                Color baseColor = Color.Lerp(FirstSlashColor!.Value with { A = 150 }, SecondSlashColor!.Value with { A = 150 }, Helper.Wave(0f, 1f, 20f, Projectile.whoAmI));
                baseColor = Color.Lerp(baseColor, Color.White, 0.25f);
                baseColor *= fromValue * num3 * 0.75f;
                spriteBatch.Draw(asset.Value, vector, new Rectangle?(asset.Frame(verticalFrames: 4, frameY: 3)), baseColor * 0.65f * num2, rot, origin, num * 0.8f * MathHelper.Lerp(1f, 1.25f, MathUtils.Clamp01(num2 * 1.5f)), effects, 0.0f);

                if (player.gravDir < 0) {
                    rot -= MathHelper.PiOver4 * 0.3725f * Projectile.direction;
                }
                Vector2 drawpos = vector + (rot + offsetRotation * 0.75f + Utils.Remap(num2, 0f, 1f, 0f, (float)Math.PI / 2f) * proj.ai[0]).ToRotationVector2() * ((float)asset.Width() * 0.5f - 5.5f) * num * 0.975f;
                DrawPrettyStarSparkle(proj.Opacity, SpriteEffects.None, drawpos, GetLightingColor() with { A = 0 } * num3 * 0.5f, color2, num2, 0f, 0.5f, 0.5f, 1f, (float)Math.PI / 4f, new Vector2(2f, 2f), Vector2.One);
            }

            return false;
        }

        protected override bool OnSlashDustSpawn(float progress) {
            float max = Projectile.ai[1] + Projectile.ai[1] * 0.5f;
            float scale = Projectile.scale;
            Projectile.scale *= Charged ? 1.05f : 1f;
            Player player = Projectile.GetOwnerAsPlayer();
            float rot = GetRotation();
            if (Projectile.localAI[0] >= Projectile.ai[1] * 0.25f && Projectile.localAI[0] < max) {
                float startProgress = Utils.Remap(Utils.GetLerpValue(Projectile.ai[1] * 0.5f, Projectile.ai[1] * 0.7f, Projectile.localAI[0], true), 0f, 1f, 0.5f, 1f, true);
                float endProgress = Utils.GetLerpValue(max, max * 0.75f, Projectile.localAI[0], true);
                startProgress *= endProgress;
                float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
                float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
                float offset = 0f;
                float f = rot + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
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
                    dust.scale *= Utils.GetLerpValue(0.25f, 1f, position.Distance(player.Center) / 50f, true) * startProgress;
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
