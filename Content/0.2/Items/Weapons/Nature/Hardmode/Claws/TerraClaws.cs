using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Runtime.InteropServices;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class TerraClaws : ClawsBaseItem<TerraClaws.TerraClawsSlash> {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 0.75f;
    public override float SecondAttackSpeedModifier => 0.75f;
    public override float ThirdAttackSpeedModifier => 0.75f;

    public override float FirstAttackScaleModifier => 1.4f;
    public override float SecondAttackScaleModifier => 1.5f;
    public override float ThirdAttackScaleModifier => 1.8f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 36);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Yellow;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldSpawn = false;
    }

    protected override (Color, Color) SetSlashColors(Player player)
        => (Color.Transparent, Color.Transparent);

    protected override void SpawnClawsSlash(Player player, Vector2 position, int type, int damage, float knockback, int attackTime) {
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), SpawnClawsProjectileType(player) ?? type, damage, knockback, player.whoAmI,
            player.direction, attackTime);
        //Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), SpawnClawsProjectileType(player) ?? type, damage, knockback, player.whoAmI,
        //    player.direction, attackTime, ai2: 30f);
    }

    public sealed class TerraClawsSlash : ClawsSlash {
        protected override void UpdateMainCycle() {
            base.UpdateMainCycle();

            Projectile proj = Projectile;
            float num2 = proj.localAI[0] / proj.ai[1];
            _firstSlashColor = Color.Lerp(new Color(45, 124, 205), new Color(47, 239, 102), num2).ModifyRGB(1f);
            _secondSlashColor = new Color(181, 230, 29).ModifyRGB(1f);
        }

        protected override bool OnSlashDustSpawn(float progress) {
            float max = Projectile.ai[1] + Projectile.ai[1] * 0.5f;
            float scale = Projectile.scale;
            Projectile.scale *= 1f;
            Player player = Projectile.GetOwnerAsPlayer();
            float rot = GetRotation();
            if (FirstSlashColor != null && SecondSlashColor != null && Projectile.localAI[0] >= Projectile.ai[1] * 0.25f && Projectile.localAI[0] < max) {
                float startProgress = Utils.Remap(Utils.GetLerpValue(Projectile.ai[1] * 0.5f, Projectile.ai[1] * 0.7f, Projectile.localAI[0], true), 0f, 1f, 0.5f, 1f, true);
                float endProgress = Utils.GetLerpValue(max, max * 0.75f, Projectile.localAI[0], true);
                startProgress *= endProgress;
                float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
                float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
                float offset = 0f;
                float f = rot + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
                Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
                Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat(0.7f, 0.75f) * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
                for (int num807 = 0; (float)num807 < Projectile.scale * 10f; num807++) {
                    if (!Main.rand.NextChance(startProgress * 0.9f * 0.25f)) {
                        continue;
                    }
                    if (Main.rand.NextBool()) {
                        continue;
                    }
                    if (Main.rand.NextBool()) {
                        continue;
                    }
                    Color color1 = FirstSlashColor.Value;
                    Color color2 = SecondSlashColor.Value;

                    int type = 278;
                    Color value = new(64, 220, 96);
                    Color value2 = new(15, 84, 125);
                    Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2 * player.gravDir), 125, Color.Lerp(value, value2, Main.rand.NextFloat()), Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                    dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                    dust.scale *= 0.35f * startProgress;
                    dust.scale *= Projectile.scale;
                    dust.scale *= 0.25f;
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

            return true;
        }

        protected override void SpawnSlashDusts(float num1, float num4) {
            Player player = Projectile.GetOwnerAsPlayer();
            float rot = GetRotation();
            var selectedClaws = Owner.GetSelectedItem().As<ClawsBaseItem>();
            if (FirstSlashColor != null && SecondSlashColor != null) {
                if (OnSlashDustSpawn(num1)) {
                    for (int i = 0; i < 2; i++) {
                        float max = Projectile.ai[1] + Projectile.ai[1] * 0.5f;
                        if (Projectile.localAI[0] >= Projectile.ai[1] * 0f && Projectile.localAI[0] < max) {
                            float startProgress = Utils.Remap(Utils.GetLerpValue(Projectile.ai[1] * 0f, Projectile.ai[1] * 1f, Projectile.localAI[0], true), 0f, 1f, 0.75f, 1f, true);
                            float endProgress = Utils.GetLerpValue(max, max * 0.75f, Projectile.localAI[0], true);
                            startProgress *= endProgress;
                            if (!Main.rand.NextChance(startProgress * 2f)) {
                                continue;
                            }
                            if (Main.rand.NextBool()) {
                                continue;
                            }
                            Color color1 = FirstSlashColor.Value;
                            Color color2 = SecondSlashColor.Value;
                            //Point pos = Projectile.Center.ToTileCoordinates();
                            //float brightness = MathHelper.Clamp(Lighting.Brightness(pos.X, pos.Y), 0.5f, 1f);
                            //color1 *= brightness;
                            //color2 *= brightness;
                            float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
                            float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
                            float num42 = num4;
                            if (ShouldFullBright) {
                                num42 = selectedClaws.BrightnessModifier;
                            }
                            color1 *= num42 * num22;
                            color2 *= num42 * num22;

                            float offset = 0f;
                            float f = rot + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
                            Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
                            Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
                            if (position.Distance(player.Center) >= 10f/*45f*/) {
                                int type = ModContent.DustType<Slash>();
                                Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2 * player.gravDir), 0, Color.Lerp(color1, color2, Main.rand.NextFloat(0.5f, 1f) * 0.3f) * 2f, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                                dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                                dust.noLight = dust.noLightEmittence = !selectedClaws.HasLighting;
                                dust.scale *= Projectile.scale;
                                dust.scale *= Utils.GetLerpValue(0f, 1f, position.Distance(player.Center) / 50f, true) * startProgress;
                                dust.noGravity = true;
                                if (ShouldFullBright) {
                                    dust.customData = num42;
                                }
                                AdjustBaseSlashDust(ref dust);
                            }
                        }
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            Projectile proj = Projectile;
            Color baseLightColor = lightColor;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 vector = proj.Center - Main.screenPosition;
            Main.instance.LoadProjectile(ProjectileID.TerraBlade2Shot);
            Asset<Texture2D> asset = TextureAssets.Projectile[ProjectileID.TerraBlade2Shot];
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

            spriteBatch.DrawWithSnapshot(() => {
                for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                    spriteBatch.Draw(asset.Value, vector, rectangle,
                        GetLightingColor2(Color.Blue) *
                        fromValue * num3 * 1f * 0.375f,
                        proj.rotation + MathHelper.PiOver4 * 0.175f * Projectile.direction + i * Projectile.direction - MathHelper.Lerp(MathHelper.PiOver2, MathHelper.PiOver2 * 0.25f, num2) * Projectile.direction, origin, num * 0.8f, effects, 0f);
                }
            }, blendState: BlendState.Additive);

            DrawItself(ref lightColor);

            for (float num6 = 0f; num6 < 12f; num6 += 1f) {
                float num7 = proj.rotation + MathHelper.PiOver2 * 0.25f * Projectile.direction + proj.ai[0] * (num6 - 2f) * ((float)Math.PI * -2f) * 0.025f + Utils.Remap(num2, 0f, 1f, 0f, (float)Math.PI / 4f) * proj.ai[0];
                Vector2 drawpos = vector + num7.ToRotationVector2() * ((float)asset.Width() * 0.5f - 6f) * num * 0.8f;
                float num8 = num6 / 12f;
                DrawPrettyStarSparkle(proj.Opacity * 0.5f, SpriteEffects.None, drawpos, new Microsoft.Xna.Framework.Color(255, 255, 255, 0) * num3 * num8, color3, 
                    num2, 0f, 0.5f, 0.5f, 1f, num7, new Vector2(0f, Utils.Remap(num2, 0f, 1f, 3f, 0f)) * num, Vector2.One * num * 2f);
            }

            spriteBatch.DrawWithSnapshot(() => {
                for (float i = 0; i < MathHelper.PiOver2; i += 0.25f) {
                    spriteBatch.Draw(TextureAssets.Projectile[Type].Value, vector, rectangle,
                        GetLightingColor2(Color.Lerp(new Color(181, 230, 29), new Color(45, 124, 205), Ease.CircOut(MathUtils.Clamp01(i / MathHelper.PiOver2)))) *
                        fromValue * num3 * 1f * 0.5f,
                        proj.rotation + MathHelper.PiOver4 * 0.175f * Projectile.direction + i * Projectile.direction - MathHelper.Lerp(MathHelper.Pi * 0.5f, 0f, num2) * Projectile.direction, origin, num, effects, 0f);
                }
            }, blendState: BlendState.Additive);

            DrawStars(ref baseLightColor);

            return false;
        }

        private Color GetLightingColor2(Color? slashColor2 = null) {
            Color color = Color.Transparent;
            Projectile proj = Projectile;
            float num2 = proj.localAI[0] / proj.ai[1];
            slashColor2 ??= new Color(181, 230, 29);
            Color slashColor = Color.Lerp(slashColor2.Value, Color.Lerp(new Color(181, 230, 29), new Color(34, 177, 76), num2), num2);
            ClawsBaseItem selectedClaws = Owner.GetSelectedItem().As<ClawsBaseItem>();
            color = slashColor;
            return color;
        }
    }
}
