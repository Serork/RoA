using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoA.Common.InterfaceElements;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.Druid.Wreath;

[Autoload(Side = ModSide.Client)]
sealed class WreathSystem() : InterfaceElement(RoA.ModName + ": Wreath", InterfaceScaleType.Game) {
    private static SpriteData _wreathSpriteData;

    private Vector2 _oldPosition;

    private static Player Player => Main.LocalPlayer;
    private static WreathStats Stats => Player.GetModPlayer<WreathStats>();  

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Active && layer.Name.Equals("Vanilla: Ingame Options"));

    public override void Load(Mod mod) {
        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(3, 3));
    }

    protected override bool DrawSelf() {
        Vector2 playerPosition = Utils.Floor(Player.Top + Vector2.UnitY * Player.gfxOffY);
        playerPosition.Y -= 15f;

        Item selectedItem = Player.GetSelectedItem();
        if (Player.dead || Player.ghost || !selectedItem.IsADruidicWeapon()) {
            _oldPosition = playerPosition;

            return true;
        }

        Vector2 position;
        bool breathUI = Player.breath < Player.breathMax || Player.lavaTime < Player.lavaMax;
        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2, offsetY = _wreathSpriteData.FrameHeight;
        playerPosition.X += offsetX;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((Player.breathMax - 1) / 200 + 1)) : -offsetY;
        position = Vector2.Lerp(_oldPosition, playerPosition, 0.3f) - Main.screenPosition;
        _oldPosition = playerPosition;
        Color mainColor = new(255, 255, 200, 200);
        Color color = Utils.MultiplyRGB(mainColor, Lighting.GetColor(new Point((int)Player.Center.X / 16, (int)Player.Center.Y / 16)));

        SpriteData wreathSpriteData = _wreathSpriteData;
        wreathSpriteData.Rotation = MathHelper.Pi;
        wreathSpriteData.Color = color;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.DrawSelf();

        float progress = Stats.Progress;
        SpriteData wreathSpriteData2 = wreathSpriteData.Framed(0, 1);
        wreathSpriteData2.DrawSelf(new Rectangle(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY, wreathSpriteData2.FrameWidth, (int)(wreathSpriteData2.FrameHeight * progress)));

        return true;
    }
}
