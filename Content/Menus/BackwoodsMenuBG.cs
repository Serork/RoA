using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Utilities.Extensions;
using RoA.Core;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Content.Menus;

// it stinks
sealed class BackwoodsMenuBG : ModSurfaceBackgroundStyle {
    private BackwoodsSkyElementsSystem _skyElements;

    public override void SetStaticDefaults()
        => _skyElements = new BackwoodsSkyElementsSystem(updateSkyElement);

    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
            => -1;

    public override int ChooseMiddleTexture()
        => -1;

    public override int ChooseFarTexture()
        => -1;

    private void updateSkyElement(BackwoodsSkyElement skyElement) {
        skyElement.Position += skyElement.Velocity;
        int height = skyElement.Frame.Height;
        skyElement.Frame = new Rectangle(0, height * (skyElement.Timer / 8 % 4), skyElement.Frame.Width, skyElement.Frame.Height);
        //skyElement.Velocity = new Vector2((float)((3.0 + (double)Math.Abs(Main.WindForVisuals) * 0.8) * skyElement.Direction), 0f);
        float num1 = (float)(Main.rand.NextDouble() * 5.0 + 10.0);
        double num2 = Main.rand.NextDouble() * 0.6 - 0.3;
        if (Main.rand.NextBool(20)) {
            num2 += Math.PI;
        }
        Vector2 vector2 = new Vector2(Main.rand.NextFloat() * 0.5f + 0.5f, Main.rand.NextFloat() * 0.5f + 0.5f);
        Vector2 targetOffset = new Vector2(Main.rand.NextFloat() * 2f - 1f, Main.rand.NextFloat() * 2f - 1f) * 100f;
        skyElement.SetMagnetization(vector2 * (Main.rand.NextFloat() * 0.3f + 0.85f) * 0.05f, targetOffset);

        skyElement.Timer--;

        Vector2 vector = skyElement._magnetAccelerations * new Vector2(Math.Sign(skyElement._magnetPointTarget.X - skyElement._positionVsMagnet.X), Math.Sign(skyElement._magnetPointTarget.Y - skyElement._positionVsMagnet.Y));
        skyElement._velocityVsMagnet += vector;
        skyElement._positionVsMagnet += skyElement._velocityVsMagnet;
        float x = (3f + Math.Abs(Main.WindForVisuals) * 0.8f) * skyElement.Direction;
        skyElement.Velocity = new Vector2(x, 0f) + skyElement._velocityVsMagnet;
    }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
        DrawDarkerBackground(spriteBatch);

        float num1 = 1200f;
        float num2 = 1550f;
        int[] numArray = new int[4] {
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsFar"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMid"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMidMenu"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsClose")
        };
        Color color = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance);

        if (Main.rand.NextBool(10)) {
            int layer = Main.rand.Next(-1, numArray.Length);
            float scale = Main.rand.NextFloat(0.8f, 1.5f) * (Math.Max(0, layer) + 1) / 4f;
            bool isOnLeft = Main.rand.NextBool();
            Vector2 position = new Vector2(isOnLeft ? -10 : Main.screenWidth + 10, Main.rand.Next(10, (int)(Main.screenHeight * 1.5f)));
            Vector2 velocity = new Vector2(-Main.rand.NextFloat(2.7f, 3.2f), Main.rand.NextFloat(2f, 2.3f));
            Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.Textures + "Fleder_Sky").Value;
            Rectangle rectangle = new(0, 0, 78, 56);
            if (Main.rand.NextBool(3)) {
                texture = ModContent.Request<Texture2D>(ResourceManager.Textures + "BabyFleder_Sky").Value;
                rectangle = new(0, 0, 66, 44);
            }
            _skyElements.Add(new BackwoodsSkyElement(texture, position, velocity * scale,
                                                     0f,
                                                     scale,
                                                     color,
                                                     (int)(Main.screenHeight / 2.075f / scale) + 100,
                                                     Vector2.Zero,
                                                     isOnLeft ? 1 : -1,
                                                     1,
                                                     rectangle,
                                                     1f,
                                                     layer));
        }

        int length = numArray.Length;
        _skyElements.Draw(Main.spriteBatch, -1);
        for (int index1 = 0; index1 < numArray.Length; ++index1) {
            _skyElements.Draw(Main.spriteBatch, index1);
            float num3 = (float)(0.5 - 0.1 * (length - index1));
            int i = numArray[index1];
            Main.instance.LoadBackground(i);
            float scale = 2.2f;
            int num4 = (int)(Main.backgroundWidth[i] * (double)scale);
            SkyManager.Instance.DrawToDepth(spriteBatch, 1f / num3);
            float fieldValue1 = typeof(Main).GetFieldValue<float>("screenOff", Main.instance);
            float fieldValue2 = typeof(Main).GetFieldValue<float>("scAdj", Main.instance);
            int num5 = (int)(-Math.IEEERemainder(Main.screenPosition.X * (double)num3, num4) - num4 / 2);
            int num6 = (int)((-(double)Main.screenPosition.Y + (double)fieldValue1 / 2.0) / (Main.worldSurface * 16.0) * (double)num1 + (double)num2) + (int)fieldValue2 - length * 1000;
            if (Main.gameMenu) {
                num6 = -500;
            }
            int num7 = Main.screenWidth / num4 + 2;
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int index2 = 0; index2 < num7; ++index2)
                    spriteBatch.Draw(TextureAssets.Background[i].Value, new Vector2(num5 + num4 * index2, MathHelper.Clamp(num6, -50f, 0f)), new Rectangle?(new Rectangle(0, 0, Main.backgroundWidth[i], Main.backgroundHeight[i])), color, 0.0f, new Vector2(), scale, SpriteEffects.None, 0.0f);
            }
        }

        return false;
    }


    private class BackwoodsSkyElementsSystem {
        private readonly List<BackwoodsSkyElement> _skyElements = new List<BackwoodsSkyElement>();

        internal delegate void Update(BackwoodsSkyElement skyElement);

        private readonly Update _update;

        public BackwoodsSkyElementsSystem(Update updateMethod) {
            _update = updateMethod;
        }

        public void Draw(SpriteBatch spriteBatch, int layer) {
            Color color = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance) * 2f;
            float opacity = (color.R + color.G + color.B) / 255f / 3f;

            for (int k = 0; k < _skyElements.Count; k++) {
                BackwoodsSkyElement skyElement = _skyElements[k];

                if (skyElement is null || skyElement.Layer != layer) {
                    continue;
                }

                _update(skyElement);

                spriteBatch.Draw(skyElement.Texture,
                                 skyElement.Position,
                                 skyElement.Frame,
                                 skyElement.Color * skyElement.Alpha * BackwoodsMenu.GlobalOpacity * opacity,
                                 skyElement.Velocity.X * 0.1f,
                                 skyElement.Frame.Size() / 2,
                                 skyElement.Scale,
                                 skyElement.Direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                                 0);
            }
            _skyElements.RemoveAll(n => n is null || n.Timer <= 0);
        }

        public void Add(BackwoodsSkyElement skyElement) {
            if (Main.gameInactive) {
                return;
            }
            _skyElements.Add(skyElement);
        }

        public void Clear()
            => _skyElements.Clear();
    }

    private class BackwoodsSkyElement {
        internal Texture2D Texture { get; set; }
        internal Vector2 Position { get; set; }
        internal Vector2 Velocity { get; set; }
        internal Vector2 StoredPosition { get; set; }
        internal float Rotation { get; set; }
        internal float Scale { get; set; }
        internal float Alpha { get; set; }
        internal Color Color { get; set; }
        internal int Timer { get; set; }
        internal int Direction { get; set; }
        internal int DirectionY { get; set; }
        internal Rectangle Frame { get; set; }
        internal int Layer { get; set; }

        internal Vector2 _magnetAccelerations;
        internal Vector2 _magnetPointTarget;
        internal Vector2 _positionVsMagnet;
        internal Vector2 _velocityVsMagnet;

        public BackwoodsSkyElement(Texture2D texture, Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int timer, Vector2 storedPosition, int direction, int directionY = 1, Rectangle frame = new Rectangle(), float alpha = 1, int layer = 0) {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            Scale = scale;
            Color = color;
            Timer = timer;
            StoredPosition = storedPosition;
            Direction = direction;
            DirectionY = directionY;
            Frame = frame;
            Alpha = alpha;
            Layer = layer;
        }

        public void SetMagnetization(Vector2 accelerations, Vector2 targetOffset) {
            _magnetAccelerations = accelerations;
            _magnetPointTarget = targetOffset;
        }
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        for (int index = 0; index < fades.Length; ++index) {
            if (index == Slot) {
                fades[index] += transitionSpeed;
                if (fades[index] > 1.0)
                    fades[index] = 1f;
            }
            else {
                fades[index] -= transitionSpeed;
                if (fades[index] < 0.0)
                    fades[index] = 0.0f;
            }
        }
    }

    private void DrawDarkerBackground(SpriteBatch spriteBatch) {
        Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "BackwoodsSky").Value;
        float scale = 2.2f;
        int width = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "BackwoodsMid2").Value.Width;
        int num4 = (int)(width * (double)scale);
        float num1 = 1200f;
        float num2 = 1550f;
        Color color = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance) * 1.35f;
        int num7 = Main.screenWidth / num4 + 2;
        float fieldValue1 = typeof(Main).GetFieldValue<float>("screenOff", Main.instance);
        float fieldValue2 = typeof(Main).GetFieldValue<float>("scAdj", Main.instance);
        float num3 = 0.05f;
        spriteBatch.Draw(texture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color);
        if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
            for (int index2 = 0; index2 < num7; ++index2) {
                int num5 = (int)(-Math.IEEERemainder(Main.screenPosition.X * (double)num3, num4) - num4 / 2);
                int num6 = (int)((-(double)Main.screenPosition.Y + (double)fieldValue1 / 2.0) / (Main.worldSurface * 16.0) * (double)num1 + (double)num2) + (int)fieldValue2;
                if (Main.gameMenu) {
                    num6 = -500;
                }
                Vector2 position = new Vector2(num5 + num4 * index2, MathHelper.Clamp(num6, -50f, 0f));
                texture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "BackwoodsMid2").Value;
                //spriteBatch.Draw(texture, position, new Rectangle(0, 0, texture.Width * 3, texture.Height * 3), color);
                spriteBatch.Draw(texture, new Vector2(num5 + num4 * index2, MathHelper.Clamp(num6, -50f, 0f) + (int)(Main.screenHeight * 0.52f)), new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), color, 0.0f, new Vector2(), scale, SpriteEffects.None, 0.0f);
            }
        }
        float opacity = (color.R + color.G + color.B) / 255f / 3f;
        spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(30, 30, 30).MultiplyRGB(color) * 0.5f * opacity);
    }
}