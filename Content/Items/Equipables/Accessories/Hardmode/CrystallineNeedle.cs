using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class CrystallineNeedle : NatureItem {
    private static Asset<Texture2D> _needleTexture = null!;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _needleTexture = ModContent.Request<Texture2D>(Texture + "_Player");
    }

    public override void Load() {
        WreathHandler.OnHitByAnythingEvent += WreathHandler_OnHitByAnythingEvent1;
        ExtraDrawLayerSupport.PostEyebrellaCloudDrawEvent += ExtraDrawLayerSupport_PostEyebrellaCloudDrawEvent;
    }

    private void ExtraDrawLayerSupport_PostEyebrellaCloudDrawEvent(ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.IsAlive()) {
            return;
        }
        var handler = player.GetCommon();
        for (int i = 0; i < handler.CrystallineNeedleTime.Length; i++) {
            if (handler.CrystallineNeedleTime[i].Item1 <= 0f) {
                continue;
            }
            Color color = WreathHandler.GetCurrentColor(player);
            Texture2D texture = _needleTexture.Value;
            Rectangle sourceRectangle = texture.Bounds;
            int opacityTime = 20;
            int inTime = 10;
            float appearanceProgress = Utils.GetLerpValue(handler.CrystallineNeedleTime[i].Item2 - opacityTime, handler.CrystallineNeedleTime[i].Item2 - inTime - opacityTime, handler.CrystallineNeedleTime[i].Item1, true);
            float appearanceProgress2 = Utils.GetLerpValue(handler.CrystallineNeedleTime[i].Item2, handler.CrystallineNeedleTime[i].Item2 - opacityTime, handler.CrystallineNeedleTime[i].Item1, true);
            appearanceProgress = Ease.CubeInOut(appearanceProgress);
            appearanceProgress2 = Ease.CubeOut(appearanceProgress2);
            sourceRectangle.Y = (int)(sourceRectangle.Height * 0.175f * appearanceProgress);
            sourceRectangle.Height /= 2;
            float rotation = handler.CrystallineNeedleRotation[i];
            int direction = (int)(player.direction * player.gravDir);
            bool facedLeft = direction == -1;
            bool reversedGravity = player.gravDir < 0f;
            if (facedLeft) {
                rotation = MathHelper.TwoPi - rotation;
                if (reversedGravity) {
                    rotation += MathHelper.Pi;
                }
            }
            else {
                if (reversedGravity) {
                    rotation += MathHelper.Pi;
                }
            }
            Vector2 position = drawinfo.Position - Main.screenPosition + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2);
            Vector2 origin = sourceRectangle.BottomCenter();

            position = position.Floor();
            Vector2 extraPosition = player.GetCommon().CrystallineNeedleExtraPosition[i];
            position += extraPosition * new Vector2(drawinfo.drawPlayer.direction * drawinfo.drawPlayer.gravDir, 1f).RotatedBy(rotation);
            position += -(Vector2.UnitY * origin.Y * MathHelper.Lerp(0.125f, 0.25f, appearanceProgress2)).RotatedBy(rotation);
            position += (Vector2.UnitY * origin.Y * 0.125f).RotatedBy(rotation);
            Vector2 scale = Vector2.One * 1f;
            SpriteEffects effect = facedLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (reversedGravity) {
                if (!facedLeft) {
                    effect = SpriteEffects.None;
                }
                else {
                    effect |= SpriteEffects.FlipHorizontally;
                }
                position.Y += 16f;
            }
            effect |= SpriteEffects.FlipVertically;

            Rectangle sourceRectangle2 = sourceRectangle;
            sourceRectangle2.Height += 2;
            DrawData item2 = new(texture, position + Vector2.UnitY.RotatedBy(rotation) * 1f + Vector2.UnitX.RotatedBy(rotation) * 2f + player.MovementOffset(), 
                sourceRectangle2,
                color * appearanceProgress2, rotation, origin, scale, effect);
            drawinfo.DrawDataCache.Add(item2);

            float waveOffset = player.whoAmI + extraPosition.Length() * 1f;
            for (float num10 = -0.02f; num10 <= 0.02f; num10 += 0.01f) {
                float num11 = (float)Math.PI * 2f * num10 * 0.5f;
                Vector2 vector2 = position + num11.ToRotationVector2().RotatedBy(rotation) * 2f;
                float alpha = Helper.Wave(0.5f, 0.75f, 10f, waveOffset + num10 * 200f);
                DrawData item = new(texture, vector2 + player.MovementOffset(), sourceRectangle,
                    color.MultiplyAlpha(alpha) * MathHelper.Lerp(0.5f, 0.625f, 0.5f) * 0.5f * appearanceProgress2, rotation + num11, origin, scale, effect);
                drawinfo.DrawDataCache.Add(item);
            }
            for (float num10 = -0.01f; num10 <= 0.01f; num10 += 0.005f) {
                float num11 = (float)Math.PI * 2f * num10 * 0.5f;
                Vector2 vector2 = position + num11.ToRotationVector2().RotatedBy(rotation) * 2f;
                float alpha = Helper.Wave(0.25f, 0.5f, 10f, waveOffset + num10 * 200f);
                DrawData item = new(texture, vector2 + player.MovementOffset(), sourceRectangle, 
                    color.MultiplyAlpha(alpha) * MathHelper.Lerp(0.5f, 0.625f, 0.5f) * 0.75f * appearanceProgress2, rotation + num11, origin, scale, effect);
                drawinfo.DrawDataCache.Add(item);
            }

            position += Vector2.UnitX.RotatedBy(rotation) * 2f;
            if (!Main.gamePaused && Main.instance.IsActive) {
                for (int i2 = 0; i2 < 1; i2++) {
                    if (Main.rand.NextBool(3)) {
                        continue;
                    }
                    Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(6f, 6f) - Vector2.UnitY.RotatedBy(rotation) * 34f + Main.screenPosition, ModContent.DustType<Dusts.CrystallineNeedleDust>(), Vector2.Zero);
                    dust.color = color;
                    dust.alpha = (byte)(255 - appearanceProgress2 * 255);
                    dust.noGravity = true;
                    dust.rotation = MathHelper.TwoPi * Main.rand.NextFloat();
                    dust.velocity -= Vector2.UnitY.RotatedBy(rotation + 0.25f * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(2f, 3f);
                    dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);
                    dust.velocity *= Main.rand.NextFloat(0.8f, 1f) * 0.5f;
                    dust.customData = Main.rand.NextFloat(1f, 10f);
                }
                if (handler.CrystallineNeedleTime[i].Item1 == 1) {
                    int count = 12;
                    for (int i2 = 0; i2 < count; i2++) {
                        for (int k = 0; k < 2; k++) {
                            if (Main.rand.NextBool()) {
                                continue;
                            }
                            Dust dust = Dust.NewDustPerfect(position - Vector2.UnitY.RotatedBy(rotation) * 34f * (i2 / (float)count) + Main.screenPosition, ModContent.DustType<Dusts.CrystallineNeedleDust>(), Vector2.Zero);
                            dust.color = color;
                            dust.alpha = (byte)(255 - appearanceProgress2 * 255);
                            dust.noGravity = true;
                            dust.rotation = MathHelper.TwoPi * Main.rand.NextFloat();
                            dust.velocity -= Vector2.UnitY.RotatedBy(rotation + 0.75f * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(2f, 3f);
                            dust.scale *= Main.rand.NextFloat(0.8f, 1.2f) * 1.375f;
                            dust.velocity *= Main.rand.NextFloat(0.8f, 1f);
                            dust.customData = Main.rand.NextFloat(1f, 10f);
                        }
                    }
                }
            }
        }
    }

    private void WreathHandler_OnHitByAnythingEvent1(Player player, Player.HurtInfo hurtInfo) {
        if (!player.GetDruidStats().IsCrystallineNeedleEffectActive) {
            return;
        }

        if (!player.GetWreathHandler().IsFull1) {
            return;
        }

        if (player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        player.GetWreathHandler().ForcedHardReset(makeDusts: true);

        player.GetCommon()
            .AddCrystallineNeedle(
            (ushort)(MathUtils.SecondsToFrames(10) * player.GetWreathHandler().ActualProgress2),
            MathHelper.TwoPi * Main.rand.NextFloatDirection(),
            Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f);
    }

    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(38, 38);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsCrystallineNeedleEffectActive = true;
    }
}
