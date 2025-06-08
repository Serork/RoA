using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Items.Weapons;
using RoA.Content.Items.Weapons.Nature.Claws;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

sealed partial class WeaponOverlay : PlayerDrawLayer {
    //private const string CLAWSTEXTURESPATH = $"/{ResourceManager.TEXTURESPATH}/Items/Weapons/Druidic/Claws";

    private static readonly Dictionary<string, Asset<Texture2D>?> _clawsOutfitTextures = [];

    private static void LoadClawsOutfitTextures() {
        for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
            ModItem item = ItemLoader.GetItem(i);
            if (item is BaseClawsItem) {
                _clawsOutfitTextures.Add(item.Name, ModContent.Request<Texture2D>(item.Texture + REQUIREMENT));
            }
        }

        //foreach (Asset<Texture2D> texture in ResourceManager.GetAllTexturesInPath(CLAWSTEXTURESPATH, REQUIREMENT)) {
        //    string getName() {
        //        return texture.Name.Split("\\").Last().Replace(REQUIREMENT, string.Empty);
        //    }
        //    _clawsOutfitTextures.Add(getName(), texture);
        //}
    }

    private static void DrawClawsOnPlayer(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;

        Item item = player.GetSelectedItem();
        if (item.IsEmpty()) {
            return;
        }

        WeaponOverlayAttribute? weaponAttribute = item.GetAttribute<WeaponOverlayAttribute>();
        if (weaponAttribute == null || weaponAttribute.WeaponType != WeaponType.Claws) {
            return;
        }

        Asset<Texture2D>? asset = _clawsOutfitTextures[GetItemNameForTexture(item)];
        if (asset?.IsLoaded != true) {
            return;
        }

        float offsetX = (int)(drawInfo.Position.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2),
              offsetY = (int)(drawInfo.Position.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f);
        Vector2 offset = new Vector2(offsetX, offsetY) + drawInfo.drawPlayer.bodyFrame.Size() / 2f;
        Vector2 drawPosition = drawInfo.drawPlayer.bodyPosition + offset;
        Color immuneAlphaPure = drawInfo.drawPlayer.GetImmuneAlphaPure(weaponAttribute.Hex != null ? Helper.FromHexRgb(weaponAttribute.Hex.Value) : drawInfo.colorArmorBody, drawInfo.shadow);
        immuneAlphaPure *= drawInfo.drawPlayer.stealth;
        DrawData drawData = new(asset.Value, drawPosition - Main.screenPosition, player.bodyFrame, immuneAlphaPure, player.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect);
        drawInfo.DrawDataCache.Add(drawData);
    }
}
