using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

using Terraria;

namespace RoA.Core.Utility.Extensions;

static class Texture2DExtensions {
    public static List<Vector2> GetColorMap(this Texture2D texture, Vector2 texturePosition, float textureRotation, float textureScale, float distanceToPreviousToBeAdded = 0f, float chance = 1f) {
        int textureWidth = texture.Width;
        int textureHeight = texture.Height;
        Color[] colorData = new Color[textureWidth * textureHeight];
        texture.GetData(colorData);
        List<Vector2> result = [];
        Vector2 previousLeafPosition = Vector2.Zero;
        for (int h = 0; h < textureHeight; h++) {
            for (int w = 0; w < textureWidth; w++) {
                Color color = colorData[w + h * textureWidth];
                if (color.A > 0 && color.R > 0 && color.G > 0 && color.B > 0 && Main.rand.NextChance(chance)) {
                    Vector2 positionOffset = textureScale * new Vector2(textureWidth * 0.5f, textureHeight * 0.5f).RotatedBy(textureRotation);
                    Vector2 position = texturePosition - positionOffset + new Vector2(w, h).RotatedBy(textureRotation);
                    if (previousLeafPosition.Distance(position) < distanceToPreviousToBeAdded) {
                        continue;
                    }
                    result.Add(position);
                    previousLeafPosition = position;
                }
            }
        }
        return result;
    }
}
