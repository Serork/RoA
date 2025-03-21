﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

sealed class WreathDrawingOnPlayer : PlayerDrawLayer {
    private const string WREATHSTEXTURESPATH = $"/{ResourceManager.TEXTURESPATH}/Items/Equipables/Wreaths";
    private const string REQUIREMENT = "_Outfit";

    private static readonly Dictionary<string, Asset<Texture2D>?> _wreathsOutfitTextures = [];

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => true;

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.WaistAcc);

    public override bool IsHeadLayer => false;

    public override void SetStaticDefaults() => LoadOutfitTextures();

    private static void LoadOutfitTextures() {
        if (Main.dedServ) {
            return;
        }

        LoadWreathsOutfitTextures();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.hideEntirePlayer) {
            return;
        }

        DrawWreathsOnPlayer(drawInfo);
    }

    private static void LoadWreathsOutfitTextures() {
        for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
            ModItem item = ItemLoader.GetItem(i);
            if (item is BaseWreathItem) {
                _wreathsOutfitTextures.Add(item.Name, ModContent.Request<Texture2D>(item.Texture + REQUIREMENT));
            }
        }

        //foreach (Asset<Texture2D> texture in ResourceManager.GetAllTexturesInPath(WREATHSTEXTURESPATH, REQUIREMENT)) {
        //    string getName() {
        //        return texture.Name.Split("\\").Last().Replace(REQUIREMENT, string.Empty);
        //    }
        //    _wreathsOutfitTextures.Add(getName(), texture);
        //}
    }

    private static void DrawWreathsOnPlayer(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;

        WreathItemToShowHandler wreathItemToShowHandler = player.GetModPlayer<WreathItemToShowHandler>();
        Item wreathItem = wreathItemToShowHandler.WreathToShow;
        if (!player.active && Main.gameMenu) {
            wreathItemToShowHandler.ForcedUpdate();
        }
        if (wreathItem.IsEmpty() || wreathItemToShowHandler.HideVisuals) {
            return;
        }

        Asset<Texture2D>? asset = _wreathsOutfitTextures[WeaponOverlay.GetItemNameForTexture(wreathItem)];
        if (asset?.IsLoaded != true) {
            return;
        }

        Texture2D texture = asset.Value;
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width / 2 + drawInfo.drawPlayer.width / 2),
                                        (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect;
        Color immuneAlphaPure = drawInfo.drawPlayer.GetImmuneAlphaPure(wreathItem.ModItem is FenethsBlazingWreath ? Color.White : drawInfo.colorArmorLegs, drawInfo.shadow);
        immuneAlphaPure *= drawInfo.drawPlayer.stealth;
        DrawData drawData = new(texture,
                                position + drawInfo.drawPlayer.PlayerMovementOffset(),
                                null,
                                immuneAlphaPure, drawInfo.drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0) {
            shader = wreathItemToShowHandler.DyeItem.dye
        };
        drawInfo.DrawDataCache.Add(drawData);
    }
}
