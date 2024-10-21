using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

sealed class WreathDrawingOnPlayer : PlayerDrawLayer {
    private const string WREATHSTEXTURESPATH = $"/{ResourceManager.TEXTURESPATH}/Items/Equipables/Wreaths";
    private const string REQUIREMENT = "_Outfit";

    private static readonly Dictionary<string, Asset<Texture2D>?> _wreathsOutfitTextures = [];

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => true;

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.WaistAcc);

    public override bool IsHeadLayer => false;

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        LoadWreathsOutfitTextures();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        DrawWreathsOnPlayer(drawInfo);
    }

    private static void LoadWreathsOutfitTextures() {
        foreach (Asset<Texture2D> texture in ResourceManager.GetAllTexturesInPath(WREATHSTEXTURESPATH, REQUIREMENT)) {
            string getName() {
                return texture.Name.Split("\\").Last().Replace(REQUIREMENT, string.Empty);
            }
            _wreathsOutfitTextures.Add(getName(), texture);
        }
    }

    private static void DrawWreathsOnPlayer(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;

        WreathItemToShowHandler wreathItemToShowHandler = player.GetModPlayer<WreathItemToShowHandler>();
        Item wreathItem = wreathItemToShowHandler.WreathToShow;
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
        DrawData drawData = new(texture,
                                position + drawInfo.drawPlayer.PlayerMovementOffset(),
                                null, drawInfo.colorArmorLegs, drawInfo.drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0) {
            shader = wreathItemToShowHandler.DyeItem.dye
        };
        drawInfo.DrawDataCache.Add(drawData);
    }
}
