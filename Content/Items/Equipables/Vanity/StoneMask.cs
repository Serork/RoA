using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class StoneMask : ModItem {
    public override void SetStaticDefaults() {
        //Tooltip.SetDefault("'Cursed'");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 24; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.vanity = true;

        Item.value = Item.sellPrice(0, 0, 30, 0);
    }

    internal sealed class StoneMaskDrawLayer : PlayerDrawLayer {
        private static Asset<Texture2D> _headTexture = null!,
                                        _eyesTexture = null!;

        public override void SetStaticDefaults() {
            if (Main.dedServ) {
                return;
            }

            _headTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<StoneMask>()).Texture + "_Head");
            _eyesTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<StoneMask>()).Texture + "_Head_Eyes");
        }

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.FaceAcc);

        private static bool IsActive(Player player) {
            int itemType = ModContent.ItemType<StoneMask>();
            bool flag = player.head == EquipLoader.GetEquipSlot(RoA.Instance, nameof(StoneMask), EquipType.Head);
            return flag;
        }

        private class DisableHeadDrawing : ModPlayer {
            public override void HideDrawLayers(PlayerDrawSet drawInfo) {
                if (IsActive(drawInfo.drawPlayer)) {
                    //PlayerDrawLayers.Head.Hide();
                    PlayerDrawLayers.HeadBack.Hide();
                }
            }
        }

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.hideEntirePlayer) {
                return;
            }

            Player player = drawInfo.drawPlayer;
            if (!IsActive(drawInfo.drawPlayer)) {
                return;
            }
            Texture2D texture = _headTexture.Value;
            Color color = drawInfo.colorBodySkin * (1f - drawInfo.shadow);
            color = player.GetImmuneAlphaPure(color, drawInfo.shadow);
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            bodyFrame.Width += 2;
            Vector2 helmetOffset = drawInfo.helmetOffset;
            DrawData item = new(texture,
                helmetOffset + Vector2.UnitX * (player.direction == -1 ? -2 : 0) + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                (float)(drawInfo.drawPlayer.width / 2)),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect,
                bodyFrame, color, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect) {
                shader = drawInfo.cHead
            };
            drawInfo.DrawDataCache.Add(item);
            texture = _eyesTexture.Value;
            color = drawInfo.colorArmorBody * (1f - drawInfo.shadow);
            color = player.GetImmuneAlphaPure(color, drawInfo.shadow);
            item = new(texture,
                helmetOffset + Vector2.UnitX * (player.direction == -1 ? -2 : 0) + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                (float)(drawInfo.drawPlayer.width / 2)),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect,
                bodyFrame, color, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect) {
                shader = drawInfo.cHead
            };
            drawInfo.DrawDataCache.Add(item);
        }
    }
}
