using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Core;

static class DrawUtils {
    public struct SingleTileDrawInfo(Texture2D texture, Point position, Rectangle clip, Color? color = null, SlopeType slope = SlopeType.Solid, bool isHalfBlock = false) {
        public Texture2D Texture = texture;
        public Point Position = position;
        public Rectangle Clip = clip;
        public Color Color = color ?? Color.White;
        public SlopeType Slope = slope;
        public bool IsHalfBlock = isHalfBlock;
    }

    // adapted vanilla
    public static void DrawSingleTile(in SingleTileDrawInfo singleTileInfo, float scale = 1f, Vector2 drawOffset = default) {
        int num12 = (int)singleTileInfo.Slope;
        bool halfBlock = singleTileInfo.IsHalfBlock;
        Vector2 position = singleTileInfo.Position.ToWorldCoordinates() - Vector2.One * 8f + drawOffset;
        Vector2 scale2 = Vector2.One * scale;
        Texture2D texture = singleTileInfo.Texture;
        if (num12 == 0 && !halfBlock) {
            Rectangle clip2 = singleTileInfo.Clip;
            Vector2 origin = clip2.Centered();
            Main.spriteBatch.Draw(texture, position + origin / 1f, DrawInfo.Default with {
                Color = singleTileInfo.Color,
                Clip = clip2,
                Scale = scale2,
                Origin = origin
            });
        }
        else if (halfBlock) {
            Rectangle clip2 = singleTileInfo.Clip.AdjustHeight(-singleTileInfo.Clip.Height / 2);
            Vector2 origin = clip2.Centered();
            Main.spriteBatch.Draw(texture, position + Vector2.UnitY * 8f + origin / 1f, DrawInfo.Default with {
                Color = singleTileInfo.Color,
                Clip = clip2,
                Scale = scale2,
                Origin = origin
            });
        }
        else {
            int num13 = 2;
            for (int i2 = 0; i2 < 8; i2++) {
                int num14 = i2 * -2;
                int num15 = 16 - i2 * 2;
                int num16 = 16 - num15;
                int num17;
                switch (num12) {
                    case 1:
                        num14 = 0;
                        num17 = i2 * 2;
                        num15 = 14 - i2 * 2;
                        num16 = 0;
                        break;
                    case 2:
                        num14 = 0;
                        num17 = 16 - i2 * 2 - 2;
                        num15 = 14 - i2 * 2;
                        num16 = 0;
                        break;
                    case 3:
                        num17 = i2 * 2;
                        break;
                    default:
                        num17 = 16 - i2 * 2 - 2;
                        break;
                }
                Rectangle clip2 = new Rectangle(singleTileInfo.Clip.X + num17, singleTileInfo.Clip.Y + num16, num13, num15);
                Vector2 origin = clip2.Centered();
                Main.spriteBatch.Draw(texture, position + new Vector2(num17, i2 * num13 + num14) + origin / 1f, DrawInfo.Default with {
                    Color = singleTileInfo.Color,
                    Clip = clip2,
                    Scale = scale2,
                    Origin = origin
                });
            }
            int num18 = ((num12 <= 2) ? 14 : 0);
            Rectangle clip = new Rectangle(singleTileInfo.Clip.X, singleTileInfo.Clip.Y + num18, 16, 2);
            Vector2 origin2 = clip.Centered();
            Main.spriteBatch.Draw(texture, position + new Vector2(0f, num18) + origin2 / 1f, DrawInfo.Default with {
                Color = singleTileInfo.Color,
                Clip = clip,
                Scale = scale2,
                Origin = origin2
            });
        }
    }

    // recipe browser
    public static Asset<Texture2D> ToAsset(this Texture2D texture) {
        using MemoryStream stream = new();

        if (!Program.IsMainThread) {
            Main.RunOnMainThread(() => {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                stream.Position = 0;
            }).GetAwaiter().GetResult();
        }
        else {
            texture.SaveAsPng(stream, texture.Width, texture.Height);
            stream.Position = 0;
        }

        return Main.Assets.CreateUntracked<Texture2D>(stream, texture.Name ?? "NoName.png");
    }

    // recipe browser
    internal static Asset<Texture2D> ResizeImage(Asset<Texture2D> texture2D, int desiredWidth, int desiredHeight) {
        texture2D.Wait();
        RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, desiredWidth, desiredHeight);
        Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
        Main.spriteBatch.Begin();

        float scale = 1;
        if (texture2D.Value.Width > desiredWidth || texture2D.Value.Height > desiredHeight) {
            if (texture2D.Value.Height > texture2D.Value.Width)
                scale = (float)desiredWidth / texture2D.Value.Height;
            else
                scale = (float)desiredWidth / texture2D.Value.Width;
        }

        //new Vector2(texture2D.Width / 2 * scale, texture2D.Height / 2 * scale) desiredWidth/2, desiredHeight/2
        Main.spriteBatch.Draw(texture2D.Value, new Vector2(desiredWidth / 2, desiredHeight / 2), null, Color.White, 0f, new Vector2(texture2D.Value.Width / 2, texture2D.Value.Height / 2), scale, SpriteEffects.None, 0f);

        Main.spriteBatch.End();
        Main.instance.GraphicsDevice.SetRenderTarget(null);

        Texture2D mergedTexture = new Texture2D(Main.instance.GraphicsDevice, desiredWidth, desiredHeight);
        Color[] content = new Color[desiredWidth * desiredHeight];
        renderTarget.GetData<Color>(content);
        mergedTexture.SetData<Color>(content);

        return mergedTexture.ToAsset();
    }
}
