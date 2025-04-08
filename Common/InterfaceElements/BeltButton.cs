using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

sealed class BeltButton() : InterfaceElement(RoA.ModName + ": Belt Button", InterfaceScaleType.UI) {
    private static Asset<Texture2D> _beltButtonTexture, _beltButtonTextureOutline;

    private static bool _isUsedInternal;

    public static bool IsUsed { get; private set; }

    internal static void ToggleTo(bool option) => IsUsed = _isUsedInternal = option;

    public override void Load(Mod mod) {
        if (Main.dedServ) {
            return;
        }

        string textureName = ResourceManager.UITextures + "BeltButton";
        _beltButtonTexture = ModContent.Request<Texture2D>(textureName);
        _beltButtonTextureOutline = ModContent.Request<Texture2D>(textureName + "_Outline");
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }

    protected override bool DrawSelf() {
        if (Main.playerInventory && Main.EquipPage < 1) {
            if (!_isUsedInternal) {
                IsUsed = WreathSlot.IsItemValidForSlot(Main.mouseItem);
            }

            SpriteFrame beltButtonFrame = new(2, 1);
            Texture2D texture = (Texture2D)_beltButtonTexture;
            int num9 = 8 + Main.LocalPlayer.GetAmountOfExtraAccessorySlotsToShow();
            int inventoryTop = 174;
            int num10 = 950;
            if (Main.screenHeight < num10 && num9 >= 10) {
                inventoryTop -= (int)(56.0 * Main.inventoryScale * (num9 - 9));
            }
            float yPos = inventoryTop - 32;
            int height = _beltButtonTexture.Height() / beltButtonFrame.RowCount;
            int width = _beltButtonTexture.Width() / beltButtonFrame.ColumnCount;
            Vector2 origin = new Vector2(width, height);
            float extraY = Main.screenWidth / 8f;
            //Vector2 vector2 = new Vector2(Main.screenWidth - 162 - 6f - 12f, (float)yPos + 14f + (!Main.mapFullscreen && Main.mapStyle == 1 && Main.mapEnabled ? extraY : 0f)) + origin / 2f;
            Vector2 vector2 = new Vector2(Main.screenWidth - 162 - 6f - 12f, WreathSlot.GetCustomLocation().Y);
            vector2.Y -= texture.Height / 2;
            vector2.X -= 33f;
            vector2.X -= Main.netMode == NetmodeID.MultiplayerClient ? 25f : 0f;
            vector2.Y += 4f;
            Rectangle sourceRectangle = beltButtonFrame.GetSourceRectangle(texture);
            sourceRectangle.Width += 2;
            float scale = 0.9f;
            bool flag = Collision.CheckAABBvAABBCollision(vector2 - origin, origin, new Vector2(Main.mouseX, Main.mouseY), Vector2.One) && Main.mouseItem.IsEmpty();
            sourceRectangle.X += IsUsed ? width : 0;
            Color color = Color.White;
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Draw(texture, vector2, sourceRectangle, color, 0f, origin, scale, SpriteEffects.None, 0f);
            texture = (Texture2D)_beltButtonTextureOutline;
            if (flag) {
                Main.mouseText = true;
                Main.LocalPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease) {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    IsUsed = !IsUsed;
                    _isUsedInternal = IsUsed;
                }
                Main.instance.MouseText(string.Concat(new object[] {
                            Language.GetTextValue("Mods.RoA.WreathUI")
                        }), 0, 0, -1, -1, -1, -1);
                spriteBatch.Draw(texture, vector2 - Vector2.One * 2f, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        return true;
    }
}
