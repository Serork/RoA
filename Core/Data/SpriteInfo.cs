using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Core.Data;

struct SpriteInfo(Asset<Texture2D> textureAsset, SpriteFrame frame) {
    public Asset<Texture2D> Asset = textureAsset;
    public SpriteFrame Frame = frame;
    public Vector2 VisualPosition;
    public Color Color = Color.White;
    public float Scale = 1f;
    public float Rotation;
    public SpriteEffects Effects;

    public readonly Texture2D? Texture => Asset?.Value;
    public Rectangle SourceRectangle => Frame.GetSourceRectangle(Texture);
    public Vector2 Origin => SourceRectangle.Centered();
    public Vector2 Center => VisualPosition + Origin;

    public readonly SpriteInfo Framed(byte frameIndexX, byte frameIndexY) {
        SpriteInfo result = this;
        result.Frame.CurrentColumn = frameIndexX;
        result.Frame.CurrentRow = frameIndexY;
        return result;
    }

    public void DrawSelf() {
        if (Texture == null) {
            return;
        }

        Main.EntitySpriteDraw(Texture,
                              Center,
                              SourceRectangle,
                              Color,
                              Rotation,
                              Origin,
                              Scale,
                              Effects);
    }
}