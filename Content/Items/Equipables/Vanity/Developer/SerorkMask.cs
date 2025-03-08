using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
sealed class SerorkMask : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => (head == EquipLoader.GetEquipSlot(Mod, nameof(SerorkHelmet), EquipType.Head) || head == EquipLoader.GetEquipSlot(Mod, nameof(SerorkMask), EquipType.Head)) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(SerorkBreastplate), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(SerorkGreaves), EquipType.Legs);

    private sealed class SerorkVisuals : ILoadable {
        public sealed class SerorkVisualsHeadGlowing : PlayerDrawLayer {
            public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

            protected override void Draw(ref PlayerDrawSet drawInfo) {
                Player player = drawInfo.drawPlayer;
                if (!player.active || player.invis) {
                    return;
                }
                DrawHeadGlowMask(ref drawInfo);
            }

            private static void DrawHeadGlowMask(ref PlayerDrawSet drawInfo) {
                Player player = drawInfo.drawPlayer;
                void drawself<T>(ref PlayerDrawSet drawInfo) where T : ModItem {
                    if (player.head == EquipLoader.GetEquipSlot(RoA.Instance, typeof(T).Name, EquipType.Head)) {
                        Texture2D glowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<T>()).Texture + "_Head_Glow").Value;
                        Color glowMaskColor = Color.White * (1f - drawInfo.shadow);
                        glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                        DrawData drawData = GetHeadGlowMask(ref drawInfo, glowMaskTexture, glowMaskColor);
                        var drawinfo = drawInfo;
                        glowMaskColor = Color.White * (1f - drawinfo.shadow);
                        glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                        drawData.color = glowMaskColor * 0.25f;
                        drawData.color = Color.Lerp(drawData.color, Color.Blue, 0.5f);
                        drawData.color.A = (byte)((float)(int)drawData.color.A * 0.2f);
                        var handler = drawinfo.drawPlayer.GetModPlayer<SerorkVisualsStorage>();
                        float num2 = handler.ghostFade;
                        for (int l = 0; l < 4; l++) {
                            float num3;
                            float num4;
                            switch (l) {
                                default:
                                    num3 = num2;
                                    num4 = 0f;
                                    break;
                                case 1:
                                    num3 = 0f - num2;
                                    num4 = 0f;
                                    break;
                                case 2:
                                    num3 = 0f;
                                    num4 = num2;
                                    break;
                                case 3:
                                    num3 = 0f;
                                    num4 = 0f - num2;
                                    break;
                            }
                            drawData.position = new Vector2(drawData.position.X + num3, drawData.position.Y + num4);
                            drawData.shader = drawinfo.cHead;
                            drawinfo.DrawDataCache.Add(drawData);
                        }
                    }
                }
                drawself<SerorkMask>(ref drawInfo);
                drawself<SerorkHelmet>(ref drawInfo);
            }

            public static DrawData GetHeadGlowMask(ref PlayerDrawSet drawInfo, Texture2D glowMaskTexture, Color glowMaskColor) {
                Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
                bodyFrame.Width += 2;
                Vector2 helmetOffset = drawInfo.helmetOffset;
                DrawData item = new(glowMaskTexture,
                    helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                    (float)(drawInfo.drawPlayer.width / 2)),
                    (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                    (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect,
                    bodyFrame, glowMaskColor, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect) {
                };
                return item;
            }
        }

        private sealed class SerorkVisualsLegsGlowing : PlayerDrawLayer {
            public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);

            protected override void Draw(ref PlayerDrawSet drawInfo) {
                Player player = drawInfo.drawPlayer;
                if (!player.active || player.invis) {
                    return;
                }
                DrawArmorGlowMask(ref drawInfo);
            }

            private static void DrawArmorGlowMask(ref PlayerDrawSet drawInfo) {
                Player player = drawInfo.drawPlayer;
                if (player.legs == EquipLoader.GetEquipSlot(RoA.Instance, nameof(SerorkGreaves), EquipType.Legs)) {
                    Texture2D glowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<SerorkGreaves>()).Texture + "_Legs_Glow").Value;
                    Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2 - player.legFrame.Width / 2, player.height - player.legFrame.Height + 4f) + player.legPosition;
                    Vector2 legsOffset = drawInfo.legsOffset;
                    DrawData drawData = new(glowMaskTexture, drawPos.Floor() + legsOffset, player.legFrame, default, player.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0);
                    var drawinfo = drawInfo;
                    Color glowMaskColor = Color.White * (1f - drawinfo.shadow);
                    glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                    drawData.color = glowMaskColor * 0.25f;
                    drawData.color = Color.Lerp(drawData.color, Color.Blue, 0.5f);
                    drawData.color.A = (byte)((float)(int)drawData.color.A * 0.2f);
                    var handler = drawinfo.drawPlayer.GetModPlayer<SerorkVisualsStorage>();
                    float num2 = handler.ghostFade;
                    for (int l = 0; l < 4; l++) {
                        float num3;
                        float num4;
                        switch (l) {
                            default:
                                num3 = num2;
                                num4 = 0f;
                                break;
                            case 1:
                                num3 = 0f - num2;
                                num4 = 0f;
                                break;
                            case 2:
                                num3 = 0f;
                                num4 = num2;
                                break;
                            case 3:
                                num3 = 0f;
                                num4 = 0f - num2;
                                break;
                        }
                        drawData.position = new Vector2(drawData.position.X + num3, drawData.position.Y + num4);
                        drawData.shader = drawinfo.cLegs;
                        drawinfo.DrawDataCache.Add(drawData);
                    }
                }
            }
        }

        private sealed class SerorkVisualsStorage : ModPlayer {
            public float ghostFade;
            public float ghostDir;
        }

        void ILoadable.Load(Mod mod) {
            On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += On_PlayerDrawLayers_DrawPlayer_RenderAllLayers;
            On_PlayerDrawLayers.DrawCompositeArmorPiece += On_PlayerDrawLayers_DrawCompositeArmorPiece;
        }

        private void On_PlayerDrawLayers_DrawCompositeArmorPiece(On_PlayerDrawLayers.orig_DrawCompositeArmorPiece orig, ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context, DrawData data) {
            orig(ref drawinfo, context, data);

            var drawPlayer = drawinfo.drawPlayer;
            bool flag = ItemLoader.GetItem(ModContent.ItemType<SerorkMask>()).IsVanitySet(drawPlayer.head, drawPlayer.body, drawPlayer.legs);
            if (drawPlayer.body != EquipLoader.GetEquipSlot(RoA.Instance, nameof(SerorkBreastplate), EquipType.Body)) {
                return;
            }
            switch (context) {
                case CompositePlayerDrawContext.BackShoulder:
                case CompositePlayerDrawContext.BackArm:
                case CompositePlayerDrawContext.FrontArm:
                case CompositePlayerDrawContext.FrontShoulder: {
                    DrawData item2 = data;
                    item2.texture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<SerorkBreastplate>()).Texture + "_Body_Glow").Value;
                    Color glowMaskColor = Color.White * (1f - drawinfo.shadow);
                    glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                    item2.color = glowMaskColor * 0.25f;
                    item2.color = Color.Lerp(item2.color, Color.Blue, 0.5f);
                    item2.color.A = (byte)((float)(int)item2.color.A * 0.2f);
                    var handler = drawPlayer.GetModPlayer<SerorkVisualsStorage>();
                    float num2 = handler.ghostFade;
                    for (int l = 0; l < 4; l++) {
                        float num3;
                        float num4;
                        switch (l) {
                            default:
                                num3 = num2;
                                num4 = 0f;
                                break;
                            case 1:
                                num3 = 0f - num2;
                                num4 = 0f;
                                break;
                            case 2:
                                num3 = 0f;
                                num4 = num2;
                                break;
                            case 3:
                                num3 = 0f;
                                num4 = 0f - num2;
                                break;
                        }
                        item2.position = new Vector2(item2.position.X + num3, item2.position.Y + num4);
                        Rectangle value2 = item2.sourceRect.Value;
                        item2.sourceRect = value2;
                        item2.shader = drawinfo.cBody;
                        drawinfo.DrawDataCache.Add(item2);
                    }
                    break;
                }

                case CompositePlayerDrawContext.Torso: {
                    DrawData item = data;
                    item.texture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<SerorkBreastplate>()).Texture + "_Body_Glow2").Value;
                    Color glowMaskColor = Color.White * (1f - drawinfo.shadow);
                    glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                    item.color = glowMaskColor;
                    Rectangle value = item.sourceRect.Value;
                    item.sourceRect = value;
                    item.shader = drawinfo.cBody;
                    drawinfo.DrawDataCache.Add(item);
                    break;
                }
            }
        }

        private void On_PlayerDrawLayers_DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo) {
            var drawPlayer = drawinfo.drawPlayer;
            if (drawPlayer.active) {
                var handler = drawPlayer.GetModPlayer<SerorkVisualsStorage>();
                bool flag = ItemLoader.GetItem(ModContent.ItemType<SerorkMask>()).IsVanitySet(drawPlayer.head, drawPlayer.body, drawPlayer.legs);
                if (flag && drawinfo.shadow == handler.ghostFade) {
                    for (int i = 0; i < drawinfo.DrawDataCache.Count; i++) {
                        DrawData value = drawinfo.DrawDataCache[i];
                        value.color = Color.Lerp(value.color, Color.Blue, 0.5f);
                        value.color.A = (byte)((float)(int)value.color.A * 0.2f);
                        drawinfo.DrawDataCache[i] = value;
                    }
                }
            }

            orig(ref drawinfo);
        }

        private void On_LegacyPlayerRenderer_DrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
            SpriteBatch spriteBatch = camera.SpriteBatch;
            SamplerState samplerState = camera.Sampler;
            if (drawPlayer.mount.Active && drawPlayer.fullRotation != 0f)
                samplerState = LegacyPlayerRenderer.MountedSamplerState;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, camera.Rasterizer, null, camera.GameViewMatrix.TransformationMatrix);
            if (Main.gamePaused)
                drawPlayer.PlayerFrame();

            Vector2 position = default(Vector2);
            var handler = drawPlayer.GetModPlayer<SerorkVisualsStorage>();
            bool flag = ItemLoader.GetItem(ModContent.ItemType<SerorkMask>()).IsVanitySet(drawPlayer.head, drawPlayer.body, drawPlayer.legs);
            if (flag) {
                if (!drawPlayer.ghost) {
                    if (!drawPlayer.invis) {
                        _ = drawPlayer.position;
                        //if (!Main.gamePaused)
                        //    handler.ghostFade += handler.ghostDir * 0.03f;

                        //if ((double)handler.ghostFade < 0.05) {
                        //    handler.ghostDir = 1f;
                        //    handler.ghostFade = 0.15f;
                        //}
                        //else if ((double)handler.ghostFade > 0.95) {
                        //    handler.ghostDir = -1f;
                        //    handler.ghostFade = 0.95f;
                        //}

                        if (!Main.gamePaused) {
                            handler.ghostFade = MathHelper.SmoothStep(handler.ghostFade, 
                                (float)Main.rand.NextFloatDirection() * (float)Math.Floor(Main.rand.NextFloatDirection()) * Main.rand.NextFloat(20f),
                                0.25f);
                        }

                        float num2 = handler.ghostFade;
                        for (int l = 0; l < 4; l++) {
                            float num3;
                            float num4;
                            switch (l) {
                                default:
                                    num3 = num2;
                                    num4 = 0f;
                                    break;
                                case 1:
                                    num3 = 0f - num2;
                                    num4 = 0f;
                                    break;
                                case 2:
                                    num3 = 0f;
                                    num4 = num2;
                                    break;
                                case 3:
                                    num3 = 0f;
                                    num4 = 0f - num2;
                                    break;
                            }

                            position = new Vector2(drawPlayer.position.X + num3, drawPlayer.position.Y + drawPlayer.gfxOffY + num4);
                            //self.DrawPlayer(camera, drawPlayer, position, drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, handler.ghostFade);
                        }
                    }
                }
            }

            spriteBatch.End();

            orig(self, camera, drawPlayer);
        }

        void ILoadable.Unload() { }
    }
}
