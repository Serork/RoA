using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Items.Equipables.Armor.Magic;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

sealed class ArmorCape : PlayerDrawLayer {
    private Asset<Texture2D> _acolyteCapeTexture;

    public override void Load() => _acolyteCapeTexture = ModContent.Request<Texture2D>(ResourceManager.MagicArmorTextures + "AcolyteCape_Front");

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        int copperJacket = ModContent.ItemType<CopperAcolyteJacket>();
        int tinJacket = ModContent.ItemType<TinAcolyteJacket>();
        Player player = drawInfo.drawPlayer;
        if (player.CheckArmorSlot(copperJacket, 1, 11) || player.CheckVanitySlot(copperJacket, 11) ||
            player.CheckArmorSlot(tinJacket, 1, 11) || player.CheckVanitySlot(tinJacket, 11))
            return true;
        return false;
    }

    public override Position GetDefaultPosition()
        => new AfterParent(PlayerDrawLayers.FrontAccFront);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.hideEntirePlayer) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis || player.front != -1 || player.ShouldNotDraw) return;

        Texture2D texture = _acolyteCapeTexture.Value;
        Vector2 position = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2 - player.bodyFrame.Width / 2, player.height - player.bodyFrame.Height + 4f) + player.bodyPosition;
        Vector2 origin = drawInfo.bodyVect;

        Color immuneAlphaPure = drawInfo.drawPlayer.GetImmuneAlphaPure(drawInfo.colorArmorBody, drawInfo.shadow);
        immuneAlphaPure *= drawInfo.drawPlayer.stealth;

        DrawData drawData = new DrawData(texture, position.Floor() + origin, player.bodyFrame, immuneAlphaPure, player.bodyRotation, origin, 1f, drawInfo.playerEffect, 0);
        drawData.shader = player.cBody;
        drawInfo.DrawDataCache.Add(drawData);
    }
}