﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Content.Items.Special.Lothor;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

sealed class LothorMaskGlowing : ModSystem {
    private Asset<Texture2D> _lothorGlowMaskTexture;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _lothorGlowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<LothorMask>()).Texture + "_Head_Glow");
    }

    public override void Load() {
        On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += On_PlayerDrawLayers_DrawPlayer_RenderAllLayers;
    }

    private void On_PlayerDrawLayers_DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        var layer = PlayerDrawLayerLoader.GetDrawLayers(drawinfo).First(x => x != null && x.IsHeadLayer);
        if (!layer.Visible) {
            return;
        }

        Player player = drawinfo.drawPlayer;
        if (player.head > 0) {
            int lothorMask = ModContent.ItemType<LothorMask>();
            bool flag = false;
            if (player.CheckArmorSlot(lothorMask, 0, 10) || player.CheckVanitySlot(lothorMask, 10)) {
                flag = true;
            }
            if (!player.active || player.isLockedToATile) {
                flag = false;
            }
            if (flag) {
                var drawInfo = drawinfo;
                if (!(player.dead || player.invis || player.ShouldNotDraw)) {
                    float lifeProgress = 1f - MathHelper.Clamp((float)player.statLife / player.statLifeMax2 * 0.5f, 0f, 1f);
                    SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
                    Main.spriteBatch.BeginBlendState(BlendState.Additive);
                    for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                        Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
                        bodyFrame.Width += 2;
                        Vector2 helmetOffset = drawInfo.helmetOffset;
                        Color immuneAlphaPure = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
                        immuneAlphaPure *= drawInfo.drawPlayer.stealth;
                        Vector2 position = drawInfo.Position + new Vector2(-1f, 5f);
                        Vector2 vector = drawInfo.Position + drawInfo.rotationOrigin;
                        Matrix matrix = Matrix.CreateRotationZ(drawInfo.rotation);
                        Vector2 newPosition = position - vector;
                        newPosition = Vector2.Transform(newPosition, matrix);
                        position.X = (newPosition + vector).X;
                        float progress4 = Math.Abs(drawInfo.rotation) / MathHelper.Pi;
                        float drawDistance = 3f/*MathHelper.Lerp(5f, 0f, (float)player.statLife / (float)player.statLifeMax2)*/;
                        position.X -= progress4 * (player.width / 2f + 12f) * player.direction;
                        position.Y += progress4 * (player.height / 2f + 8f);
                        Main.spriteBatch.Draw(_lothorGlowMaskTexture.Value,
                            helmetOffset + new Vector2((int)(position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                            (float)(drawInfo.drawPlayer.width / 2)),
                            (int)(position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                            (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect +
                            Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                            * Helper.Wave(0f, drawDistance, 12f, 0.5f) * lifeProgress +
                            Vector2.UnitY * (player.gravDir == -1f ? -6f : 0f),
                             bodyFrame, immuneAlphaPure.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress,
                             player.fullRotation + drawInfo.drawPlayer.headRotation + Main.rand.NextFloatRange(0.05f) * lifeProgress,
                             new Vector2(40f, 56f) / 2f, 1f, drawInfo.drawPlayer.direction == -1 ? SpriteEffects.None | SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                    }
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(in snapshot);
                }
            }
        }
    }
}
