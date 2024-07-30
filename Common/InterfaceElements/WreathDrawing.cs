using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.InterfaceElements;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

[Autoload(Side = ModSide.Client)]
sealed class WreathDrawing() : InterfaceElement(RoA.ModName + ": Wreath", InterfaceScaleType.Game) {
    private const byte HORIZONTALFRAMECOUNT = 6;

    private static SpriteData _wreathSpriteData;

    private Vector2 _oldPosition;

    private static Player Player => Main.LocalPlayer;
    private static WreathStats Stats => Player.GetModPlayer<WreathStats>();  

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Active && layer.Name.Equals("Vanilla: Ingame Options"));

    public override void Load(Mod mod) {
        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(HORIZONTALFRAMECOUNT, 3));
    }

    protected override bool DrawSelf() {
        Vector2 playerPosition = Utils.Floor(Player.Top + Vector2.UnitY * Player.gfxOffY);
        playerPosition.Y -= 15f;
        Vector2 position;
        bool breathUI = Player.breath < Player.breathMax || Player.lavaTime < Player.lavaMax;
        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2, offsetY = _wreathSpriteData.FrameHeight;
        playerPosition.X += offsetX;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((Player.breathMax - 1) / 200 + 1)) : -offsetY;

        Item selectedItem = Player.GetSelectedItem();
        if (Player.dead || Player.ghost || !selectedItem.IsADruidicWeapon()) {
            _oldPosition = playerPosition;

            return true;
        }

        position = Vector2.Lerp(_oldPosition, playerPosition, 0.3f) - Main.screenPosition;
        _oldPosition = playerPosition;
        float progress = Stats.Progress;
        Color color = Stats.DrawColor;
        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.75f);
        SpriteData wreathSpriteData = _wreathSpriteData;
        wreathSpriteData.Rotation = MathHelper.Pi;
        wreathSpriteData.Color = color * opacity;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.DrawSelf();

        SpriteData wreathSpriteData2 = wreathSpriteData.Framed(0, 1);
        wreathSpriteData2.Color = color;
        wreathSpriteData2.DrawSelf(new Rectangle(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY, wreathSpriteData2.FrameWidth, (int)(wreathSpriteData2.FrameHeight * progress)));

        SpriteData wreathSpriteData3 = wreathSpriteData.Framed(3, 1);
        opacity = Math.Min(progress * 1.25f, 0.75f);
        wreathSpriteData3.Color = color * opacity;
        wreathSpriteData3.DrawSelf();

        return true;
    }
}
