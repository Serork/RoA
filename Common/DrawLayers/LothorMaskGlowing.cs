using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Content.Items.Special.Lothor;
using RoA.Core.Utility;
using RoA.Utilities;

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

        Player player = drawinfo.drawPlayer;
        if (player.active) {
            int lothorMask = ModContent.ItemType<LothorMask>();
            bool flag = false;
            if (player.CheckArmorSlot(lothorMask, 0, 10) || player.CheckVanitySlot(lothorMask, 10)) {
                flag = true;
            }
            if (flag) {
                var drawInfo = drawinfo;
                if (!(player.dead || player.invis || player.ShouldNotDraw)) { 
                    float lifeProgress = 1f;
                    for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                        Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
                        bodyFrame.Width += 2;
                        Vector2 helmetOffset = drawInfo.helmetOffset;
                        Color immuneAlphaPure = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
                        immuneAlphaPure *= drawInfo.drawPlayer.stealth;
                        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
                        Main.spriteBatch.BeginBlendState(BlendState.Additive);
                        Main.spriteBatch.Draw(_lothorGlowMaskTexture.Value,
                            helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                            (float)(drawInfo.drawPlayer.width / 2)),
                            (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                            (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect +
                            Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                            * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress,
                             bodyFrame, immuneAlphaPure.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress, drawInfo.drawPlayer.headRotation + Main.rand.NextFloatRange(0.05f) * lifeProgress,
                             drawInfo.headVect, 1f, drawInfo.playerEffect, 0f);

                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(in snapshot);
                    }
                }
            }
        }
    }
}
