using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
sealed class HereticHood : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => head == EquipLoader.GetEquipSlot(Mod, nameof(HereticHood), EquipType.Head) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(HereticChestguard), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(HereticPants), EquipType.Legs);

    private class HereticVisuals {
        public sealed class HereticHeadGlowing : PlayerDrawLayer {
            public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

            protected override void Draw(ref PlayerDrawSet drawInfo) {
                if (drawInfo.hideEntirePlayer) {
                    return;
                }

                Player player = drawInfo.drawPlayer;
                if (!player.active || player.invis) {
                    return;
                }
                DrawHeadGlowMask(ref drawInfo);
            }

            private static void DrawHeadGlowMask(ref PlayerDrawSet drawInfo) {
                Player player = drawInfo.drawPlayer;
                bool flag = ItemLoader.GetItem(ModContent.ItemType<HereticHood>()).IsVanitySet(player.head, player.body, player.legs);
                if (/*flag && */player.head == EquipLoader.GetEquipSlot(RoA.Instance, typeof(HereticHood).Name, EquipType.Head)) {
                    //DrawColor glowMaskColor = DrawColor.White;
                    //glowMaskColor = DrawColor.White;
                    //glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                    //Texture2D glowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<HereticHood>()).Texture + "_Head_Glow").Value;
                    //DrawData drawData = GetHeadGlowMask(ref drawInfo, glowMaskTexture, glowMaskColor);
                    //glowMaskColor = DrawColor.White;
                    //glowMaskColor = drawInfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawInfo.shadow);
                    //drawData.color = glowMaskColor;
                    //drawData.shader = drawInfo.cHead;
                    //drawInfo.DrawDataCache.Add(drawData);

                    Color glowMaskColor = Color.White * 0.9f;
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                    Texture2D glowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<HereticHood>()).Texture + "_Head_Glow2").Value;
                    DrawData drawData = GetHeadGlowMask(ref drawInfo, glowMaskTexture, glowMaskColor);
                    glowMaskColor = Color.White;
                    glowMaskColor = drawInfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawInfo.shadow);
                    float value = Helper.Wave(0f, 1.5f, speed: 1f);
                    glowMaskColor *= Utils.GetLerpValue(0.5f, 1f, value, true);
                    drawData.color = glowMaskColor;
                    drawData.shader = drawInfo.cHead;
                    drawInfo.DrawDataCache.Add(drawData);
                }
            }

            public static DrawData GetHeadGlowMask(ref PlayerDrawSet drawInfo, Texture2D glowMaskTexture, Color glowMaskColor) {
                Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
                bodyFrame.Width += 2;
                Vector2 helmetOffset = drawInfo.helmetOffset;
                if (drawInfo.drawPlayer.direction == -1) {
                    helmetOffset.X -= 2f;
                }
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

        //private class HereticVisualsLegsGlowing : PlayerDrawLayer {
        //    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);

        //    protected override void DrawSelf(ref PlayerDrawSet drawInfo) {
        //        if (drawInfo.hideEntirePlayer) {
        //            return;
        //        }

        //        Player player = drawInfo.drawPlayer;
        //        if (!player.active || player.invis) {
        //            return;
        //        }
        //        DrawArmorGlowMask(ref drawInfo);
        //    }

        //    private static void DrawArmorGlowMask(ref PlayerDrawSet drawInfo) {
        //        Player player = drawInfo.drawPlayer;
        //        if (player.legs == EquipLoader.GetEquipSlot(RoA.Instance, nameof(HereticPants), EquipType.Legs)) {
        //            Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2 - player.legFrame.Width / 2, player.height - player.legFrame.Height + 4f) + player.legPosition;
        //            Vector2 legsOffset = drawInfo.legsOffset;
        //            var drawinfo = drawInfo;
        //            DrawColor glowMaskColor = DrawColor.White;
        //            glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
        //            Texture2D glowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<HereticPants>()).Texture + "_Legs_Glow").Value;
        //            DrawData drawData = new(glowMaskTexture, drawPos.Floor() + legsOffset, player.legFrame, default, player.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0);
        //            glowMaskColor = DrawColor.White;
        //            glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
        //            drawData.color = glowMaskColor;
        //            drawData.shader = drawinfo.cLegs;
        //            drawinfo.DrawDataCache.Add(drawData);
        //        }
        //    }
        //}

        //void ILoadable.Load(Mod mod) {
        //    On_PlayerDrawLayers.DrawCompositeArmorPiece += On_PlayerDrawLayers_DrawCompositeArmorPiece;
        //}

        //void ILoadable.Unload() { }

        //private void On_PlayerDrawLayers_DrawCompositeArmorPiece(On_PlayerDrawLayers.orig_DrawCompositeArmorPiece orig, ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context, DrawData data) {
        //    orig(ref drawinfo, context, data);

        //    var drawPlayer = drawinfo.drawPlayer;
        //    if (drawPlayer.body != EquipLoader.GetEquipSlot(RoA.Instance, nameof(HereticChestguard), EquipType.Body)) {
        //        return;
        //    }
        //    switch (context) {
        //        case CompositePlayerDrawContext.BackShoulder:
        //        case CompositePlayerDrawContext.BackArm:
        //        case CompositePlayerDrawContext.FrontArm:
        //        case CompositePlayerDrawContext.FrontShoulder: {
        //            DrawData item2 = data;
        //            item2.texture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<HereticChestguard>()).Texture + "_Body_Glow").Value;
        //            DrawColor glowMaskColor = DrawColor.White;
        //            glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
        //            Rectangle value2 = item2.sourceRect.Value;
        //            item2.color = glowMaskColor;
        //            item2.sourceRect = value2;
        //            item2.shader = drawinfo.cBody;
        //            drawinfo.DrawDataCache.Add(item2);
        //            break;
        //        }

        //        case CompositePlayerDrawContext.Torso: {
        //            DrawData item = data;
        //            item.texture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<HereticChestguard>()).Texture + "_Body_Glow").Value;
        //            DrawColor glowMaskColor = DrawColor.White;
        //            glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
        //            item.color = glowMaskColor;
        //            Rectangle value = item.sourceRect.Value;
        //            item.sourceRect = value;
        //            item.shader = drawinfo.cBody;
        //            drawinfo.DrawDataCache.Add(item);
        //            break;
        //        }
        //    }
        //}
    }
}
