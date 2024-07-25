using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Common;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.Wreath;

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
        if (Player.dead || Player.ghost) {
            return true;
        }

        Item selectedItem = Player.GetSelectedItem();
        if (!(selectedItem.IsDruidic() && selectedItem.IsAWeapon())) {
            return true;
        }

        Vector2 position;
        Vector2 playerPosition = Utils.Floor(Player.Top + Vector2.UnitY * Player.gfxOffY);
        playerPosition.Y -= 60f;
        bool breathUI = Player.breath < Player.breathMax || Player.lavaTime < Player.lavaMax;
        float offsetX = _wreathSpriteData.FrameWidth / 2f + 5, offsetY = -3f;
        playerPosition.X += offsetX;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((Player.breathMax - 1) / 200 + 1)) : -offsetY;
        position = Vector2.Transform(Vector2.Lerp(_oldPosition, playerPosition, 0.3f) - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.UIScale;
        position += _wreathSpriteData.Texture.Size() / 2f;
        _oldPosition = playerPosition;
        Color mainColor = new(255, 255, 200, 200);
        Color color = Utils.MultiplyRGB(mainColor, Lighting.GetColor(new Point((int)Player.Center.X / 16, (int)Player.Center.Y / 16)));

        SpriteData wreathSpriteData = _wreathSpriteData.Framed(0, 2);
        wreathSpriteData.Color = color;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.DrawSelf();

        SpriteData wreathSpriteData2 = wreathSpriteData.Framed(0, 1);
        wreathSpriteData2.DrawSelf();

        return true;
    }
}
