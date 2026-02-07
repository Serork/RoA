using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Runtime.InteropServices;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[AutoloadGlowMask]
[WeaponOverlay(WeaponType.Claws)]
sealed class HiTechCattleProd : ClawsBaseItem<HiTechCattleProd.HiTechCattleProdSlash> {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 0.5f;
    public override float SecondAttackSpeedModifier => 0.5f;
    public override float ThirdAttackSpeedModifier => 1f;

    public override float FirstAttackScaleModifier => 1.25f;
    public override float SecondAttackScaleModifier => 1.25f;
    public override float ThirdAttackScaleModifier => 1.5f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 34);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Pink;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);

        Item.autoReuse = true;
    }

    public override bool ResetOnHit => true;

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldReset = false;
    }

    protected override (Color, Color) SetSlashColors(Player player)
        => (new Color(97, 200, 225), new Color(98, 154, 179));

    public sealed class HiTechCattleProdSlash : ClawsSlash {
        private bool Charged => (!CanFunction && Projectile.GetOwnerAsPlayer().GetWreathHandler().IsActualFull6) || Opacity == 1f;

        public ref float Opacity => ref Projectile.localAI[1];

        protected override void SetCollisionScale(ref float coneLength) {
            if (!Charged) {
                return;
            }

            coneLength *= 1.2f;
        }

        protected override void SafeOnHitPlayer(Player target, Player.HurtInfo info) {
            //target.AddBuff(BuffID.Electrified, Main.rand.Next(60, 180) * 2);
        }

        protected override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            //target.AddBuff(BuffID.Electrified, Main.rand.Next(60, 180) * 2);

            if (Projectile.localAI[2] != 0f) {
                return;
            }

            if (!target.CanActivateOnHitEffect()) {
                return;
            }

            if (!Charged) {
                return;
            }

            SpawnStar(target);
            Projectile.GetOwnerAsPlayer().GetWreathHandler().HandleOnHitNPCForNatureProjectile(Projectile, true);
            Projectile.localAI[2] = 1f;
        }

        private void SpawnStar(NPC target) {
            if (!Projectile.IsOwnerLocal()) {
                return;
            }

            ProjectileUtils.SpawnPlayerOwnedProjectile<HiTechStar>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                Position = target.Center,
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack
            });
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
            float num2_2 = Projectile.localAI[0] / Projectile.ai[1];
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
                            .ModifyRGB(0.825f);
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

                float to = (float)Math.PI / 2f * GravDir() * 1.1f;
                if (GravDir() < 0) {
                    rot += MathHelper.PiOver4 / 4.75f * -Projectile.direction;
                }
                if (GravDir() < 0) {
                    rot += MathHelper.PiOver4 * -Projectile.direction;
                    rot -= MathHelper.PiOver2 * 0.75f * -Projectile.direction;
                }
                if (GravDir() < 0) {
                    rot -= MathHelper.PiOver2 * 0.875f * -Projectile.direction;
                }
                Vector2 drawpos = vector + (rot + offsetRotation * 0.75f + Utils.Remap(num2, 0f, 1f, 0f, to) * proj.ai[0]).ToRotationVector2() * ((float)asset.Width() * 0.5f - 5.5f) 
                    * num * 0.825f * MathHelper.Lerp(1f, 1.25f, MathUtils.Clamp01(num2 * 1.5f));
                DrawPrettyStarSparkle(proj.Opacity, SpriteEffects.None, drawpos, GetLightingColor() with { A = 0 } * num3 * 0.5f, color2, num2, 0f, 0.5f, 0.5f, 1f, (float)Math.PI / 4f, new Vector2(2f, 2f), Vector2.One);
            }

            return false;
        }
    }
}
