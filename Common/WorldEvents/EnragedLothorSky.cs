using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Placeable;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;
using SteelSeries.GameSense;

using System;

using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace RoA.Common.WorldEvents;

sealed class EnragedLothorSky : CustomSky {
	private bool _hasTile;
	private float _intensity, _intensity2;
	private float _alpha = 0;

	private void DrawSunAndMoon(Main.SceneArea sceneArea, Microsoft.Xna.Framework.Color moonColor, Microsoft.Xna.Framework.Color sunColor, float tempMushroomInfluence) {
        Texture2D value = TextureAssets.Sun.Value;
        int num = Main.moonType;
        if (!TextureAssets.Moon.IndexInRange(num))
            num = Utils.Clamp(num, 0, 8);

        Texture2D value2 = TextureAssets.Moon[num].Value;
        int num2 = sceneArea.bgTopY;
        int num3 = (int)(Main.time / 54000.0 * (double)(sceneArea.totalWidth + (float)(value.Width * 2))) - value.Width;
        int num4 = 0;
        float num5 = 1f;
        float rotation = (float)(Main.time / 54000.0) * 2f - 7.3f;
        int num6 = (int)(Main.time / 32400.0 * (double)(sceneArea.totalWidth + (float)(value2.Width * 2))) - value2.Width;
        int num7 = 0;
        float num8 = 1f;
        float num9 = (float)(Main.time / 32400.0) * 2f - 7.3f;
        if (Main.dayTime) {
            double num10;
            if (Main.time < 27000.0) {
                num10 = Math.Pow(1.0 - Main.time / 54000.0 * 2.0, 2.0);
                num4 = (int)((double)num2 + num10 * 250.0 + 180.0);
            }
            else {
                num10 = Math.Pow((Main.time / 54000.0 - 0.5) * 2.0, 2.0);
                num4 = (int)((double)num2 + num10 * 250.0 + 180.0);
            }

            num5 = (float)(1.2 - num10 * 0.4);
        }
        else {
            double num11;
            if (Main.time < 16200.0) {
                num11 = Math.Pow(1.0 - Main.time / 32400.0 * 2.0, 2.0);
                num7 = (int)((double)num2 + num11 * 250.0 + 180.0);
            }
            else {
                num11 = Math.Pow((Main.time / 32400.0 - 0.5) * 2.0, 2.0);
                num7 = (int)((double)num2 + num11 * 250.0 + 180.0);
            }

            num8 = (float)(1.2 - num11 * 0.4);
        }

        num5 *= Main.ForcedMinimumZoom;
        num8 *= Main.ForcedMinimumZoom;
        if (Main.starGame) {
            if (WorldGen.generatingWorld) {
                Main.alreadyGrabbingSunOrMoon = true;
                if (Main.rand.Next(60) == 0) {
                    for (int i = 0; i < Main.numStars; i++) {
                        if (Main.star[i].hidden)
                            Star.SpawnStars(i);
                    }
                }

                if (Main.dayTime) {
                    Main.dayTime = false;
                    Main.time = 0.0;
                }
            }
            else {
                Main.starGame = false;
            }
        }
        else {
            Main.starsHit = 0;
        }

        if (Main.dayTime) {
            if ((Main.remixWorld && !Main.gameMenu) || WorldGen.remixWorldGen)
                return;

            num5 *= 1.1f;
            float num12 = 1f - tempMushroomInfluence;
            num12 -= Main.cloudAlpha * 1.5f * Main.atmo;
            if (num12 < 0f)
                num12 = 0f;

            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color((byte)(255f * num12), (byte)((float)(int)sunColor.G * num12), (byte)((float)(int)sunColor.B * num12), (byte)(255f * num12));
            Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color((byte)((float)(int)sunColor.R * num12), (byte)((float)(int)sunColor.G * num12), (byte)((float)(int)sunColor.B * num12), (byte)((float)(int)sunColor.B * num12));
            bool flag = false;
            if (Main.eclipse) {
                value = TextureAssets.Sun3.Value;
                flag = true;
            }
            else if (!Main.gameMenu && Main.player[Main.myPlayer].head == 12) {
                value = TextureAssets.Sun2.Value;
                flag = true;
            }

            if (flag)
                color2 = new Microsoft.Xna.Framework.Color((byte)((float)(int)sunColor.R * num12), (byte)((float)(int)sunColor.G * num12), (byte)((float)(int)sunColor.B * num12), (byte)((float)(sunColor.B - 60) * num12));

            Vector2 origin = value.Size() / 2f;
            Vector2 position = new Vector2(num3, num4 - 150 + Main.sunModY) + sceneArea.SceneLocalScreenPositionOffset;
            Main.spriteBatch.Draw(value, position, null, color, rotation, origin, num5, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(value, position, null, Color.Black * 0.75f * _intensity2, rotation, origin, num5 * 0.7f, SpriteEffects.None, 0f);
        }

        if (!Main.dayTime) {
            float num13 = 1f - Main.cloudAlpha * 1.5f * Main.atmo;
            if (num13 < 0f)
                num13 = 0f;

            moonColor *= num13;
            Vector2 position2 = new Vector2(num6, num7 - num7 * 0.75f + Main.moonModY) + sceneArea.SceneLocalScreenPositionOffset;
            if (WorldGen.drunkWorldGen)
                Main.spriteBatch.Draw(TextureAssets.SmileyMoon.Value, position2, new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.SmileyMoon.Width(), TextureAssets.SmileyMoon.Height()), Color.Black * 0.75f * _intensity2, num9 / 2f + (float)Math.PI, new Vector2(TextureAssets.SmileyMoon.Width() / 2, TextureAssets.SmileyMoon.Width() / 2), num8 * 0.8f, SpriteEffects.None, 0f);
            else if (Main.pumpkinMoon)
                Main.spriteBatch.Draw(TextureAssets.PumpkinMoon.Value, position2, new Microsoft.Xna.Framework.Rectangle(0, TextureAssets.PumpkinMoon.Width() * Main.moonPhase, TextureAssets.PumpkinMoon.Width(), TextureAssets.PumpkinMoon.Width()), Color.Black * 0.75f * _intensity2, num9, new Vector2(TextureAssets.PumpkinMoon.Width() / 2, TextureAssets.PumpkinMoon.Width() / 2), num8 * 0.8f, SpriteEffects.None, 0f);
            else if (Main.snowMoon)
                Main.spriteBatch.Draw(TextureAssets.SnowMoon.Value, position2, new Microsoft.Xna.Framework.Rectangle(0, TextureAssets.SnowMoon.Width() * Main.moonPhase, TextureAssets.SnowMoon.Width(), TextureAssets.SnowMoon.Width()), Color.Black * 0.75f * _intensity2, num9, new Vector2(TextureAssets.SnowMoon.Width() / 2, TextureAssets.SnowMoon.Width() / 2), num8 * 0.8f, SpriteEffects.None, 0f);
            else
                Main.spriteBatch.Draw(TextureAssets.Moon[num].Value, position2, new Microsoft.Xna.Framework.Rectangle(0, TextureAssets.Moon[num].Width() * Main.moonPhase, TextureAssets.Moon[num].Width(), TextureAssets.Moon[num].Width()), Color.Black * 0.75f * _intensity2, num9, new Vector2(TextureAssets.Moon[num].Width() / 2, TextureAssets.Moon[num].Width() / 2), num8 * 0.8f, SpriteEffects.None, 0f);
        }

        Microsoft.Xna.Framework.Rectangle value3 = ((!Main.dayTime) ? new Microsoft.Xna.Framework.Rectangle((int)((double)num6 - (double)TextureAssets.Moon[num].Width() * 0.5 * (double)num8), (int)((double)num7 - (double)TextureAssets.Moon[num].Width() * 0.5 * (double)num8 + (double)Main.moonModY), (int)((float)TextureAssets.Moon[num].Width() * num8), (int)((float)TextureAssets.Moon[num].Width() * num8)) : new Microsoft.Xna.Framework.Rectangle((int)((double)num3 - (double)TextureAssets.Sun.Width() * 0.5 * (double)num5), (int)((double)num4 - (double)TextureAssets.Sun.Height() * 0.5 * (double)num5 + (double)Main.sunModY), (int)((float)TextureAssets.Sun.Width() * num5), (int)((float)TextureAssets.Sun.Width() * num5)));
        value3.Offset((int)sceneArea.SceneLocalScreenPositionOffset.X, (int)sceneArea.SceneLocalScreenPositionOffset.Y);
        Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(Main.mouseX, Main.mouseY, 1, 1);
        Main.sunModY = (short)((double)Main.sunModY * 0.999);
        Main.moonModY = (short)((double)Main.moonModY * 0.999);
        if (!Main.gameMenu || Main.netMode == 1)
            return;

        if (rectangle.Intersects(value3) || Main.alreadyGrabbingSunOrMoon)
            Main.player[Main.myPlayer].mouseInterface = true;

        if ((Main.mouseLeft || Main.starGame) && Main.hasFocus) {
            if (!rectangle.Intersects(value3) && !Main.alreadyGrabbingSunOrMoon)
                return;

            if (Main.dayTime) {
                Main.time = 54000.0 * (double)((float)(Main.mouseX + TextureAssets.Sun.Width()) / ((float)Main.screenWidth + (float)(TextureAssets.Sun.Width() * 2)));
                Main.sunModY = (short)(Main.mouseY - num4);
                if (Main.time > 53990.0)
                    Main.time = 53990.0;
            }
            else {
                Main.time = 32400.0 * (double)((float)(Main.mouseX + TextureAssets.Moon[num].Width()) / ((float)Main.screenWidth + (float)(TextureAssets.Moon[num].Width() * 2)));
                Main.moonModY = (short)(Main.mouseY - num7);
                if (Main.time > 32390.0)
                    Main.time = 32390.0;
            }

            if (Main.time < 10.0)
                Main.time = 10.0;

            Main.alreadyGrabbingSunOrMoon = true;
        }
        else {
            Main.alreadyGrabbingSunOrMoon = false;
        }
    }

    public override void Update(GameTime gameTime) {
		if (!_hasTile) {
			if (_intensity > 0f) {
				_intensity -= 0.05f;
                _intensity2 -= 0.1f;
                _intensity2 = Math.Max(0f, _intensity2);
            }
			else {
				_intensity = 0f;
			}

			return;
        }
		if (_intensity < 1f) {
			_intensity += 0.05f;
            _intensity2 += 0.1f;
            _intensity2 = Math.Min(1f, _intensity2);
        }
        else {
			_intensity = 1f;
		}
    }

	public override Color OnTileColor(Color inColor) {
		float amt = Helper.EaseInOut3(_intensity) * .5f * LothorSummoningHandler._alpha;
		return inColor.MultiplyRGB(new Color(1f - amt, 1f - amt, 1f - amt));
	}

	public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
		if (!Main.gameMenu) {
			if ((double)maxDepth >= 3.00000000549776E+38 && (double)minDepth < 3.00000000549776E+38) {
				Color color = Color.Lerp(Color.Red, Color.Black, 0.75f) * 0.75f;
				spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color * Math.Min(1f, (float)((Main.screenPosition.Y - 800.0) / 1000.0)) * _intensity);
			}

            if (Main.dayTime) {
                int num13 = Main.screenWidth;
                int num14 = Main.screenHeight;
                Vector2 zero = Vector2.Zero;
                if (num13 < 800) {
                    int num15 = 800 - num13;
                    zero.X -= (float)num15 * 0.5f;
                    num13 = 800;
                }

                if (num14 < 600) {
                    int num16 = 600 - num14;
                    zero.Y -= (float)num16 * 0.5f;
                    num14 = 600;
                }

                Main.SceneArea sceneArea2 = default(Main.SceneArea);
                sceneArea2.bgTopY = 0;
                sceneArea2.totalWidth = num13;
                sceneArea2.totalHeight = num14;
                sceneArea2.SceneLocalScreenPositionOffset = zero;
                Main.SceneArea sceneArea3 = sceneArea2;
                Main.SceneArea sceneArea = sceneArea3;

                Color sunColor = Microsoft.Xna.Framework.Color.Red;
                Color moonColor = Microsoft.Xna.Framework.Color.Red;
                if ((double)(Main.screenPosition.Y / 16f) < Main.worldSurface + 2.0)
                    DrawSunAndMoon(sceneArea3, moonColor * _intensity2, sunColor * _intensity2, 0.95f);
            }
        }
	}
	
	public override void Activate(Vector2 position, params object[] args) => _hasTile = true;

	public override void Deactivate(params object[] args) => _hasTile = false;
	
	public override void Reset() {
		_intensity = 0f;
		_hasTile = false;
	}	

	public override bool IsActive() => _intensity > 0f;
}
