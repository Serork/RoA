﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Items.Weapons;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

sealed partial class WeaponOverlay : PlayerDrawLayer {
    private const string CLAWSTEXTURESPATH = $"/{ResourceManager.TEXTURESPATH}/Items/Weapons/Druidic/Claws";

    private static readonly Dictionary<string, Asset<Texture2D>?> _clawsOutfitTextures = [];

    private static void LoadClawsOutfitTextures() {
        foreach (Asset<Texture2D> texture in ResourceManager.GetAllTexturesInPath(CLAWSTEXTURESPATH, REQUIREMENT)) {
            string getName() {
                return texture.Name.Split("\\").Last().Replace(REQUIREMENT, string.Empty);
            }
            _clawsOutfitTextures.Add(getName(), texture);
        }
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
        DrawData drawData = new(asset.Value, drawPosition - Main.screenPosition, player.bodyFrame, drawInfo.colorArmorBody, player.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect);
        drawInfo.DrawDataCache.Add(drawData);
    }
}
