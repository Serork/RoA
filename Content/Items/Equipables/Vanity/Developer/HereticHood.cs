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
                void drawself<T>(ref PlayerDrawSet drawInfo) where T : ModItem {
                    if (player.head == EquipLoader.GetEquipSlot(RoA.Instance, typeof(T).Name, EquipType.Head)) {
                        Color glowMaskColor = Color.White;
                        glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                        Texture2D glowMaskTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<T>()).Texture + "_Head_Glow2").Value;
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
                drawself<HereticHood>(ref drawInfo);
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
    }
}
