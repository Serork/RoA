using Humanizer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Threading;

using RoA.Core;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.GameContent.Shaders;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Common.Liquids;

sealed class CustomLiquidRenderer : IInitializer {
    private static readonly int[] WATERFALL_LENGTH = new int[6] {
        10,
        3,
        2,
        10,

        3,
        2,
    };
    private static readonly float[] DEFAULT_OPACITY = new float[6] {
        0.6f,
        0.95f,
        0.95f,
        0.75f,

        0.95f,
        0.95f
    };
    private static readonly byte[] WAVE_MASK_STRENGTH = new byte[7];
    private static readonly byte[] VISCOSITY_MASK = new byte[7] {
        0,
        200,
        240,
        0,

        200,
        240,

        0
    };

    void ILoadable.Load(Mod mod) {
        LoadContent();

        On_SceneMetrics.Reset += On_SceneMetrics_Reset;

        On_Liquid.SettleWaterAt += On_Liquid_SettleWaterAt;
        On_Liquid.Update += On_Liquid_Update;
        On_Liquid.LiquidCheck += On_Liquid_LiquidCheck;

        On_LiquidRenderer.PrepareDraw += On_LiquidRenderer_PrepareDraw;
        On_LiquidRenderer.DrawNormalLiquids += On_LiquidRenderer_DrawNormalLiquids;
        On_LiquidRenderer.DrawShimmer += On_LiquidRenderer_DrawShimmer;
        On_LiquidRenderer.Update += On_LiquidRenderer_Update;
        On_LiquidRenderer.HasFullWater += On_LiquidRenderer_HasFullWater;
        On_LiquidRenderer.LoadContent += On_LiquidRenderer_LoadContent;
        On_LiquidRenderer.PrepareAssets += On_LiquidRenderer_PrepareAssets;
        On_LiquidRenderer.GetVisibleLiquid += On_LiquidRenderer_GetVisibleLiquid;
        On_LiquidRenderer.SetWaveMaskData += On_LiquidRenderer_SetWaveMaskData;
        On_LiquidRenderer.GetCachedDrawArea += On_LiquidRenderer_GetCachedDrawArea;

        On_TileDrawing.DrawPartialLiquid += On_TileDrawing_DrawPartialLiquid;
        On_TileDrawing.DrawTile_LiquidBehindTile += On_TileDrawing_DrawTile_LiquidBehindTile;

        On_TileLightScanner.GetTileMask += On_TileLightScanner_GetTileMask;
        On_TileLightScanner.ApplyLiquidLight += On_TileLightScanner_ApplyLiquidLight;

        On_WaterShaderData.DrawWaves += On_WaterShaderData_DrawWaves;
        On_WaterShaderData.QueueRipple_Vector2_Color_Vector2_RippleShape_float += On_WaterShaderData_QueueRipple_Vector2_Color_Vector2_RippleShape_float;
        On_LightMap.SetSize += On_LightMap_SetSize;
        On_LightMap.SetMaskAt += On_LightMap_SetMaskAt;
        On_LightMap.BlurLine += On_LightMap_BlurLine;
        On_LightMap.Clear += On_LightMap_Clear;
        On_TileLightScanner.ExportTo += On_TileLightScanner_ExportTo;

        On_MapHelper.CreateMapTile += On_MapHelper_CreateMapTile;
        On_MapHelper.GetMapTileXnaColor += On_MapHelper_GetMapTileXnaColor;

        MonoModHooks.Add(typeof(MapLegend).GetMethod("FromTile", BindingFlags.Instance | BindingFlags.Public), OnFromTile);
    }

    private delegate string orig_FromTile(MapLegend self, MapTile mapTile, int x, int y);

    private string OnFromTile(orig_FromTile orig, MapLegend self, MapTile mapTile, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (tile != null && tile.LiquidAmount > 32 && tile.LiquidType >= 4) {
            return string.Empty;
        }

        return orig(self, mapTile, x, y);
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MapColor")]
    public extern static void MapHelper_MapColor(ushort type, ref Color oldColor, byte colorType);

    private Color On_MapHelper_GetMapTileXnaColor(On_MapHelper.orig_GetMapTileXnaColor orig, ref MapTile tile) {
        int actualID = 0;
        if (tile.Type < 10000) {
            return orig(ref tile);
        }

        actualID = tile.Type - 10000;

        Color liquidColor;
        if (actualID == 0) {
            liquidColor = new Color(109, 234, 214);
        }
        else {
            liquidColor = new Color(46, 34, 47);
        }

        byte color = tile.Color;
        if (color > 0) {
            MapHelper_MapColor(tile.Type, ref liquidColor, color);
        }

        if (tile.Light == byte.MaxValue) {
            return liquidColor;
        }

        float num = (float)(int)tile.Light / 255f;
        liquidColor.R = (byte)((float)(int)liquidColor.R * num);
        liquidColor.G = (byte)((float)(int)liquidColor.G * num);
        liquidColor.B = (byte)((float)(int)liquidColor.B * num);
        return liquidColor;
    }

    private MapTile On_MapHelper_CreateMapTile(On_MapHelper.orig_CreateMapTile orig, int i, int j, byte Light) {
        Tile tile = Main.tile[i, j];

        int num2 = Light;
        int num7 = 0;

        if (tile.LiquidAmount > 32 && tile.LiquidType >= 4) {
            num7 = tile.LiquidType - 4 + 10000;
        }
        else {
            return orig(i, j, Light);
        }

        return MapTile.Create((ushort)num7, (byte)num2, (byte)0);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_drawInvisibleWalls")]
    public extern static ref bool TileLightScanner__drawInvisibleWalls(TileLightScanner self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetTileMask")]
    public extern static LightMaskMode TileLightScanner_GetTileMask(TileLightScanner self, Tile tile);

    private void On_TileLightScanner_ExportTo(On_TileLightScanner.orig_ExportTo orig, TileLightScanner self, Rectangle area, LightMap outputMap, TileLightScannerOptions options) {
        ref bool _drawInvisibleWalls = ref TileLightScanner__drawInvisibleWalls(self);
        _drawInvisibleWalls = options.DrawInvisibleWalls;
        FastParallel.For(area.Left, area.Right, delegate (int start, int end, object context) {
            for (int i = start; i < end; i++) {
                for (int j = area.Top; j <= area.Bottom; j++) {
                    if (IsTileNullOrTouchingNull(i, j)) {
                        outputMap.SetMaskAt(i - area.X, j - area.Y, LightMaskMode.None);
                        outputMap[i - area.X, j - area.Y] = Vector3.Zero;
                    }
                    else {
                        LightMaskMode tileMask = TileLightScanner_GetTileMask(self, Main.tile[i, j]);
                        outputMap.SetMaskAt(i - area.X, j - area.Y, tileMask);
                        self.GetTileLight(i, j, out var outputColor);
                        outputMap[i - area.X, j - area.Y] = outputColor;
                        if (Main.tile[i, j].LiquidAmount > 0 && Main.tile[i, j].LiquidType == 5) {
                            _mask[LightMap_IndexOf(outputMap, i - area.X, j - area.Y)] = ExtraLightMaskMode.Tar;
                        }
                    }
                }
            }
        });
    }

    private bool IsTileNullOrTouchingNull(int x, int y) {
        if (WorldGen.InWorld(x, y, 1)) {
            if (Main.tile[x, y] != null && Main.tile[x + 1, y] != null && Main.tile[x - 1, y] != null && Main.tile[x, y - 1] != null)
                return Main.tile[x, y + 1] == null;

            return true;
        }

        return true;
    }

    private void On_LightMap_Clear(On_LightMap.orig_Clear orig, LightMap self) {
        orig(self);

        //for (int i = 0; i < LightMap__colors(self).Length; i++) {
        //    if (_mask[i] != ExtraLightMaskMode.None) {
        //        _mask[i] = ExtraLightMaskMode.None;
        //    }
        //}
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_colors")]
    public extern static ref Vector3[] LightMap__colors(LightMap self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mask")]
    public extern static ref LightMaskMode[] LightMap__masks(LightMap self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_random")]
    public extern static ref FastRandom LightMap__random(LightMap self);

    public static Vector3 LightDecayThroughTar { get; set; } = new Vector3(0.88f, 0.96f, 1.015f) * 0.6f;

    private void On_LightMap_BlurLine(On_LightMap.orig_BlurLine orig, LightMap self, int startIndex, int endIndex, int stride) {
        Vector3 zero = Vector3.Zero;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        ref Vector3[] _colors = ref LightMap__colors(self);
        for (int i = startIndex; i != endIndex + stride; i += stride) {
            if (zero.X < _colors[i].X) {
                zero.X = _colors[i].X;
                flag = false;
            }
            else if (!flag) {
                if (zero.X < 0.0185f)
                    flag = true;
                else
                    _colors[i].X = zero.X;
            }

            if (zero.Y < _colors[i].Y) {
                zero.Y = _colors[i].Y;
                flag2 = false;
            }
            else if (!flag2) {
                if (zero.Y < 0.0185f)
                    flag2 = true;
                else
                    _colors[i].Y = zero.Y;
            }

            if (zero.Z < _colors[i].Z) {
                zero.Z = _colors[i].Z;
                flag3 = false;
            }
            else if (!flag3) {
                if (zero.Z < 0.0185f)
                    flag3 = true;
                else
                    _colors[i].Z = zero.Z;
            }

            if (flag && flag3 && flag2)
                continue;

            bool flag4 = false;
            switch (_mask[i]) {
                case ExtraLightMaskMode.Tar:
                    if (!flag)
                        zero.X *= LightDecayThroughTar.X;
                    if (!flag2)
                        zero.Y *= LightDecayThroughTar.Y;
                    if (!flag3)
                        zero.Z *= LightDecayThroughTar.Z;
                    flag4 = true;
                    break;
            }

            if (!flag4) {
                switch (LightMap__masks(self)[i]) {
                    case LightMaskMode.None:
                        if (!flag)
                            zero.X *= self.LightDecayThroughAir;
                        if (!flag2)
                            zero.Y *= self.LightDecayThroughAir;
                        if (!flag3)
                            zero.Z *= self.LightDecayThroughAir;
                        break;
                    case LightMaskMode.Solid:
                        if (!flag)
                            zero.X *= self.LightDecayThroughSolid;
                        if (!flag2)
                            zero.Y *= self.LightDecayThroughSolid;
                        if (!flag3)
                            zero.Z *= self.LightDecayThroughSolid;
                        break;
                    case LightMaskMode.Water: {
                        float num = (float)LightMap__random(self).WithModifier((ulong)i).Next(98, 100) / 100f;
                        if (!flag)
                            zero.X *= self.LightDecayThroughWater.X * num;

                        if (!flag2)
                            zero.Y *= self.LightDecayThroughWater.Y * num;

                        if (!flag3)
                            zero.Z *= self.LightDecayThroughWater.Z * num;

                        break;
                    }
                    case LightMaskMode.Honey:
                        if (!flag)
                            zero.X *= self.LightDecayThroughHoney.X;
                        if (!flag2)
                            zero.Y *= self.LightDecayThroughHoney.Y;
                        if (!flag3)
                            zero.Z *= self.LightDecayThroughHoney.Z;
                        break;
                }
            }
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IndexOf")]
    public extern static int LightMap_IndexOf(LightMap self, int x, int y);

    private void On_LightMap_SetMaskAt(On_LightMap.orig_SetMaskAt orig, LightMap self, int x, int y, LightMaskMode mode) {
        orig(self, x, y, mode);
    }

    private void On_LightMap_SetSize(On_LightMap.orig_SetSize orig, LightMap self, int width, int height) {
        orig(self, width, height);

        int neededSize = (width + 1) * (height + 1);
        _mask = new ExtraLightMaskMode[neededSize];
    }

    public enum ExtraLightMaskMode : byte {
        None,
        Tar
    }

    private static Ripple[] _rippleQueue = new Ripple[200];
    private static ExtraLightMaskMode[] _mask = new ExtraLightMaskMode[41209]; // scary

    private void On_WaterShaderData_QueueRipple_Vector2_Color_Vector2_RippleShape_float(On_WaterShaderData.orig_QueueRipple_Vector2_Color_Vector2_RippleShape_float orig, WaterShaderData self, Vector2 position, Color waveData, Vector2 size, RippleShape shape, float rotation) {
        //orig(self, position, waveData, size, shape, rotation);

        ref bool _useRippleWaves = ref WaterShaderData__useRippleWaves(self);
        ref int _rippleQueueCount = ref WaterShaderData__rippleQueueCount(self);
        if (!_useRippleWaves || Main.drawToScreen)
            _rippleQueueCount = 0;
        else if (_rippleQueueCount < _rippleQueue.Length)
            _rippleQueue[_rippleQueueCount++] = new Ripple(position, waveData, size, shape, rotation);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_lastDistortionDrawOffset")]
    public extern static ref Vector2 WaterShaderData__lastDistortionDrawOffset(WaterShaderData self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_useNPCWaves")]
    public extern static ref bool WaterShaderData__useNPCWaves(WaterShaderData self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_usePlayerWaves")]
    public extern static ref bool WaterShaderData__usePlayerWaves(WaterShaderData self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_useRippleWaves")]
    public extern static ref bool WaterShaderData__useRippleWaves(WaterShaderData self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rippleQueueCount")]
    public extern static ref int WaterShaderData__rippleQueueCount(WaterShaderData self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rippleShapeTexture")]
    public extern static ref Asset<Texture2D> WaterShaderData__rippleShapeTexture(WaterShaderData self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_useCustomWaves")]
    public extern static ref bool WaterShaderData__useCustomWaves(WaterShaderData self);

    private void On_WaterShaderData_DrawWaves(On_WaterShaderData.orig_DrawWaves orig, WaterShaderData self) {
        Vector2 screenPosition = Main.screenPosition;
        Vector2 vector = (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
        var _lastDistortionDrawOffset = WaterShaderData__lastDistortionDrawOffset(self);
        Vector2 vector2 = -_lastDistortionDrawOffset / 0.25f + vector;
        TileBatch tileBatch = Main.tileBatch;
        _ = Main.instance.GraphicsDevice;
        Vector2 dimensions = new Vector2(Main.screenWidth, Main.screenHeight);
        Vector2 vector3 = new Vector2(16f, 16f);
        tileBatch.Begin();
        GameShaders.Misc["WaterDistortionObject"].Apply();
        var _useNPCWaves = WaterShaderData__useNPCWaves(self);
        if (_useNPCWaves) {
            for (int i = 0; i < 200; i++) {
                if (Main.npc[i] == null || !Main.npc[i].active || (!Main.npc[i].wet && Main.npc[i].wetCount == 0) || !Collision.CheckAABBvAABBCollision(screenPosition, dimensions, Main.npc[i].position - vector3, Main.npc[i].Size + vector3))
                    continue;

                NPC nPC = Main.npc[i];
                Vector2 vector4 = nPC.Center - vector2;
                Vector2 vector5 = nPC.velocity.RotatedBy(0f - nPC.rotation) / new Vector2(nPC.height, nPC.width);
                float num = vector5.LengthSquared();
                num = num * 0.3f + 0.7f * num * (1024f / (float)(nPC.height * nPC.width));
                num = Math.Min(num, 0.08f);
                num += (nPC.velocity - nPC.oldVelocity).Length() * 0.5f;
                vector5.Normalize();
                Vector2 velocity = nPC.velocity;
                velocity.Normalize();
                vector4 -= velocity * 10f;
                if (!self._useViscosityFilter && (nPC.honeyWet || nPC.lavaWet))
                    num *= 0.3f;

                if (nPC.wet)
                    tileBatch.Draw(TextureAssets.MagicPixel.Value, new Vector4(vector4.X, vector4.Y, (float)nPC.width * 2f, (float)nPC.height * 2f) * 0.25f, null, new VertexColors(new Color(vector5.X * 0.5f + 0.5f, vector5.Y * 0.5f + 0.5f, 0.5f * num)), new Vector2((float)TextureAssets.MagicPixel.Width() / 2f, (float)TextureAssets.MagicPixel.Height() / 2f), SpriteEffects.None, nPC.rotation);

                if (nPC.wetCount != 0) {
                    num = nPC.velocity.Length();
                    num = 0.195f * (float)Math.Sqrt(num);
                    float num2 = 5f;
                    if (!nPC.wet)
                        num2 = -20f;

                    self.QueueRipple(nPC.Center + velocity * num2, new Color(0.5f, (nPC.wet ? num : (0f - num)) * 0.5f + 0.5f, 0f, 1f) * 0.5f, new Vector2(nPC.width, (float)nPC.height * ((float)(int)nPC.wetCount / 9f)) * MathHelper.Clamp(num * 10f, 0f, 1f), RippleShape.Circle);
                }
            }
        }
        var _usePlayerWaves = WaterShaderData__usePlayerWaves(self);
        if (_usePlayerWaves) {
            for (int j = 0; j < 255; j++) {
                if (Main.player[j] == null || !Main.player[j].active || ((!Main.player[j].wet && Main.player[j].wetCount == 0) && (!Main.player[j].GetModPlayer<CustomLiquidCollision_Player>().wet && Main.player[j].GetModPlayer<CustomLiquidCollision_Player>().wetCount == 0)) || !Collision.CheckAABBvAABBCollision(screenPosition, dimensions, Main.player[j].position - vector3, Main.player[j].Size + vector3))
                    continue;

                Player player = Main.player[j];
                Vector2 vector6 = player.Center - vector2;
                float num3 = player.velocity.Length();
                num3 = 0.05f * (float)Math.Sqrt(num3);
                Vector2 velocity2 = player.velocity;
                velocity2.Normalize();
                vector6 -= velocity2 * 10f;

                var handler = Main.player[j].GetModPlayer<CustomLiquidCollision_Player>();
                if (!self._useViscosityFilter) {
                    if (player.honeyWet || player.lavaWet || handler.permafrostWet || handler.tarWet) {
                        num3 *= handler.tarWet ? 0.1f : 0.3f;
                    }
                }

                if (handler.tarWet) {
                    num3 *= 0.35f;
                }

                if (player.wet || handler.wet)
                    tileBatch.Draw(TextureAssets.MagicPixel.Value, new Vector4(vector6.X - (float)player.width * 2f * 0.5f, vector6.Y - (float)player.height * 2f * 0.5f, (float)player.width * 2f, (float)player.height * 2f) * 0.25f, new VertexColors(new Color(velocity2.X * 0.5f + 0.5f, velocity2.Y * 0.5f + 0.5f, 0.5f * num3)));

                if (player.wetCount != 0 || handler.wetCount != 0) {
                    float num4 = 5f;
                    if (!player.wet)
                        num4 = -20f;

                    num3 *= 3f;
                    float factor = ((float)(int)player.wetCount / 9f);
                    if (handler.tarWet) {
                        factor = ((float)(int)handler.wetCount / 9f);
                        num3 += 0.25f * factor;
                    }
                    self.QueueRipple(player.Center + velocity2 * num4, player.wet ? num3 : (0f - num3), new Vector2(player.width, (float)player.height * factor) * MathHelper.Clamp(num3 * 10f, 0f, 1f), RippleShape.Circle);
                }
            }
        }

        if (self._useProjectileWaves) {
            for (int k = 0; k < 1000; k++) {
                Projectile projectile = Main.projectile[k];
                if (projectile.wet && !projectile.lavaWet)
                    _ = !projectile.honeyWet;
                else
                    _ = 0;

                bool flag = projectile.lavaWet;
                bool flag2 = projectile.honeyWet;
                bool flag3 = projectile.wet;
                if (projectile.ignoreWater)
                    flag3 = true;

                if (!(projectile != null && projectile.active && ProjectileID.Sets.CanDistortWater[projectile.type] && flag3) || ProjectileID.Sets.NoLiquidDistortion[projectile.type] || !Collision.CheckAABBvAABBCollision(screenPosition, dimensions, projectile.position - vector3, projectile.Size + vector3))
                    continue;

                if (projectile.ignoreWater) {
                    bool num5 = Collision.LavaCollision(projectile.position, projectile.width, projectile.height);
                    flag = Collision.WetCollision(projectile.position, projectile.width, projectile.height);
                    flag2 = Collision.honey;
                    if (!(num5 || flag || flag2))
                        continue;
                }

                Vector2 vector7 = projectile.Center - vector2;
                float num6 = projectile.velocity.Length();
                num6 = 2f * (float)Math.Sqrt(0.05f * num6);
                Vector2 velocity3 = projectile.velocity;
                velocity3.Normalize();
                if (!self._useViscosityFilter && (flag2 || flag))
                    num6 *= 0.3f;

                float num7 = Math.Max(12f, (float)projectile.width * 0.75f);
                float num8 = Math.Max(12f, (float)projectile.height * 0.75f);
                tileBatch.Draw(TextureAssets.MagicPixel.Value, new Vector4(vector7.X - num7 * 0.5f, vector7.Y - num8 * 0.5f, num7, num8) * 0.25f, new VertexColors(new Color(velocity3.X * 0.5f + 0.5f, velocity3.Y * 0.5f + 0.5f, num6 * 0.5f)));
            }
        }

        tileBatch.End();

        var _useRippleWaves = WaterShaderData__useRippleWaves(self);
        ref int _rippleQueueCount = ref WaterShaderData__rippleQueueCount(self);
        var _rippleShapeTexture = WaterShaderData__rippleShapeTexture(self);
        if (_useRippleWaves) {
            tileBatch.Begin();
            for (int l = 0; l < _rippleQueueCount; l++) {
                Vector2 vector8 = _rippleQueue[l].Position - vector2;
                Vector2 size = _rippleQueue[l].Size;
                Rectangle sourceRectangle = _rippleQueue[l].SourceRectangle;
                Texture2D value = _rippleShapeTexture.Value;
                tileBatch.Draw(value, new Vector4(vector8.X, vector8.Y, size.X, size.Y) * 0.25f, sourceRectangle,
                    new VertexColors(_rippleQueue[l].WaveData), new Vector2(sourceRectangle.Width / 2, sourceRectangle.Height / 2), SpriteEffects.None,
                    _rippleQueue[l].Rotation);
            }

            tileBatch.End();
        }

        _rippleQueueCount = 0;
        //var _useCustomWaves = WaterShaderData__useCustomWaves(self);
        //if (_useCustomWaves && self.OnWaveDraw != null) {
        //    tileBatch.Begin();
        //    self.OnWaveDraw(tileBatch);
        //    tileBatch.End();
        //}
    }

    private struct Ripple {
        private static readonly Rectangle[] RIPPLE_SHAPE_SOURCE_RECTS = new Rectangle[3] {
            new Rectangle(0, 0, 0, 0),
            new Rectangle(1, 1, 62, 62),
            new Rectangle(1, 65, 62, 62)
        };
        public readonly Vector2 Position;
        public readonly Color WaveData;
        public readonly Vector2 Size;
        public readonly RippleShape Shape;
        public readonly float Rotation;

        public Rectangle SourceRectangle => RIPPLE_SHAPE_SOURCE_RECTS[(int)Shape];

        public Ripple(Vector2 position, Color waveData, Vector2 size, RippleShape shape, float rotation) {
            Position = position;
            WaveData = waveData;
            Size = size;
            Shape = shape;
            Rotation = rotation;
        }
    }

    private void On_TileLightScanner_ApplyLiquidLight(On_TileLightScanner.orig_ApplyLiquidLight orig, TileLightScanner self, Tile tile, ref Vector3 lightColor) {
        bool permafrost = tile.LiquidType == 4;
        if (tile.LiquidAmount > 0 && (permafrost || tile.LiquidType == 5)) {
            if (permafrost) {
                float num = 0.55f;
                num += (float)(270 - Main.mouseTextColor) / 900f;
                if (lightColor.X < num)
                    lightColor.X = num * 0.4f;

                if (lightColor.Y < num)
                    lightColor.Y = num * 0.9f;

                if (lightColor.Z < num)
                    lightColor.Z = num * 0.8f;
            }

            return;
        }

        orig(self, tile, ref lightColor);
    }

    private LightMaskMode On_TileLightScanner_GetTileMask(On_TileLightScanner.orig_GetTileMask orig, TileLightScanner self, Tile tile) {
        if ((tile.LiquidType == 4/* || tile.LiquidType == 5*/) && tile.LiquidAmount > 128) {
            //if (tile.LiquidType == 5) {
            //    return LightMaskMode.Honey;
            //}
            return LightMaskMode.None;
        }

        return orig(self, tile);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawPartialLiquid")]
    public extern static void TileDrawing_DrawPartialLiquid(TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors);

    private void On_TileDrawing_DrawTile_LiquidBehindTile(On_TileDrawing.orig_DrawTile_LiquidBehindTile orig, TileDrawing self, bool solidLayer, bool inFrontOfPlayers, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, Tile tileCache) {
        Tile tile = Main.tile[tileX + 1, tileY];
        Tile tile2 = Main.tile[tileX - 1, tileY];
        Tile tile3 = Main.tile[tileX, tileY - 1];
        Tile tile4 = Main.tile[tileX, tileY + 1];
        //if (tile == null) {
        //    tile = new Tile();
        //    Main.tile[tileX + 1, tileY] = tile;
        //}

        //if (tile2 == null) {
        //    tile2 = new Tile();
        //    Main.tile[tileX - 1, tileY] = tile2;
        //}

        //if (tile3 == null) {
        //    tile3 = new Tile();
        //    Main.tile[tileX, tileY - 1] = tile3;
        //}

        //if (tile4 == null) {
        //    tile4 = new Tile();
        //    Main.tile[tileX, tileY + 1] = tile4;
        //}

        if (!tileCache.HasTile || tileCache.IsActuated || Main.tileSolidTop[tileCache.TileType] || (tileCache.IsHalfBlock && (tile2.LiquidAmount > 160 || tile.LiquidAmount > 160) && Main.instance.waterfallManager.CheckForWaterfall(tileX, tileY)) || (TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType] && tileCache.Slope == 0))
            return;

        int num = 0;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = false;
        int num2 = 0;
        bool flag6 = false;
        int num3 = (int)tileCache.Slope;
        int num4 = (int)tileCache.BlockType;
        if (tileCache.TileType == 546 && tileCache.LiquidAmount > 0) {
            flag5 = true;
            flag4 = true;
            flag = true;
            flag2 = true;
            switch (tileCache.LiquidType) {
                case 0:
                    flag6 = true;
                    break;
                case 1:
                    num2 = 1;
                    break;
                case 2:
                    num2 = 11;
                    break;
                case 3:
                    num2 = 14;
                    break;

                case 4:
                    num2 = 50;
                    break;
                case 5:
                    num2 = 51;
                    break;
            }

            num = tileCache.LiquidAmount;
        }
        else {
            if (tileCache.LiquidAmount > 0 && num4 != 0 && (num4 != 1 || tileCache.LiquidAmount > 160)) {
                flag5 = true;
                switch (tileCache.LiquidType) {
                    case 0:
                        flag6 = true;
                        break;
                    case 1:
                        num2 = 1;
                        break;
                    case 2:
                        num2 = 11;
                        break;
                    case 3:
                        num2 = 14;
                        break;

                    case 4:
                        num2 = 50;
                        break;
                    case 5:
                        num2 = 51;
                        break;
                }

                if (tileCache.LiquidAmount > num)
                    num = tileCache.LiquidAmount;
            }

            if (tile2.LiquidAmount > 0 && num3 != 1 && num3 != 3) {
                flag = true;
                switch (tile2.LiquidType) {
                    case 0:
                        flag6 = true;
                        break;
                    case 1:
                        num2 = 1;
                        break;
                    case 2:
                        num2 = 11;
                        break;
                    case 3:
                        num2 = 14;
                        break;

                    case 4:
                        num2 = 50;
                        break;
                    case 5:
                        num2 = 51;
                        break;
                }

                if (tile2.LiquidAmount > num)
                    num = tile2.LiquidAmount;
            }

            if (tile.LiquidAmount > 0 && num3 != 2 && num3 != 4) {
                flag2 = true;
                switch (tile.LiquidType) {
                    case 0:
                        flag6 = true;
                        break;
                    case 1:
                        num2 = 1;
                        break;
                    case 2:
                        num2 = 11;
                        break;
                    case 3:
                        num2 = 14;
                        break;

                    case 4:
                        num2 = 50;
                        break;
                    case 5:
                        num2 = 51;
                        break;
                }

                if (tile.LiquidAmount > num)
                    num = tile.LiquidAmount;
            }

            if (tile3.LiquidAmount > 0 && num3 != 3 && num3 != 4) {
                flag3 = true;
                switch (tile3.LiquidType) {
                    case 0:
                        flag6 = true;
                        break;
                    case 1:
                        num2 = 1;
                        break;
                    case 2:
                        num2 = 11;
                        break;
                    case 3:
                        num2 = 14;
                        break;

                    case 4:
                        num2 = 50;
                        break;
                    case 5:
                        num2 = 51;
                        break;
                }
            }

            if (tile4.LiquidAmount > 0 && num3 != 1 && num3 != 2) {
                if (tile4.LiquidAmount > 240)
                    flag4 = true;

                switch (tile4.LiquidType) {
                    case 0:
                        flag6 = true;
                        break;
                    case 1:
                        num2 = 1;
                        break;
                    case 2:
                        num2 = 11;
                        break;
                    case 3:
                        num2 = 14;
                        break;

                    case 4:
                        num2 = 50;
                        break;
                    case 5:
                        num2 = 51;
                        break;
                }
            }
        }

        if (!flag3 && !flag4 && !flag && !flag2 && !flag5)
            return;

        if (waterStyleOverride != -1)
            Main.waterStyle = waterStyleOverride;

        if (num2 == 0)
            num2 = Main.waterStyle;

        Lighting.GetCornerColors(tileX, tileY, out var vertices);
        Vector2 vector = new Vector2(tileX * 16, tileY * 16);
        Rectangle liquidSize = new Rectangle(0, 4, 16, 16);
        if (flag4 && (flag || flag2)) {
            flag = true;
            flag2 = true;
        }

        if (tileCache.HasTile && (Main.tileSolidTop[tileCache.TileType] || !Main.tileSolid[tileCache.TileType]))
            return;

        if ((!flag3 || !(flag || flag2)) && !(flag4 && flag3)) {
            if (flag3) {
                liquidSize = new Rectangle(0, 4, 16, 4);
                if (tileCache.IsHalfBlock || tileCache.Slope != 0)
                    liquidSize = new Rectangle(0, 4, 16, 12);
            }
            else if (flag4 && !flag && !flag2) {
                vector = new Vector2(tileX * 16, tileY * 16 + 12);
                liquidSize = new Rectangle(0, 4, 16, 4);
            }
            else {
                float num5 = (float)(256 - num) / 32f;
                int y = 4;
                if (tile3.LiquidAmount == 0 && (num4 != 0 || !WorldGen.SolidTile(tileX, tileY - 1)))
                    y = 0;

                int num6 = (int)num5 * 2;
                if (tileCache.Slope != 0) {
                    vector = new Vector2(tileX * 16, tileY * 16 + num6);
                    liquidSize = new Rectangle(0, num6, 16, 16 - num6);
                }
                else if ((flag && flag2) || tileCache.IsHalfBlock) {
                    vector = new Vector2(tileX * 16, tileY * 16 + num6);
                    liquidSize = new Rectangle(0, y, 16, 16 - num6);
                }
                else if (flag) {
                    vector = new Vector2(tileX * 16, tileY * 16 + num6);
                    liquidSize = new Rectangle(0, y, 4, 16 - num6);
                }
                else {
                    vector = new Vector2(tileX * 16 + 12, tileY * 16 + num6);
                    liquidSize = new Rectangle(0, y, 4, 16 - num6);
                }
            }
        }

        Vector2 position = vector - screenPosition + screenOffset;
        float num7 = 0.5f;
        switch (num2) {
            case 1:
                num7 = 1f;
                break;
            case 11:
                num7 = Math.Max(num7 * 1.7f, 1f);
                break;
        }

        if ((double)tileY <= Main.worldSurface || num7 > 1f) {
            num7 = 1f;
            if (tileCache.WallType == 21)
                num7 = 0.9f;
            else if (tileCache.WallType > 0)
                num7 = 0.6f;
        }

        if (tileCache.IsHalfBlock && tile3.LiquidAmount > 0 && tileCache.WallType > 0)
            num7 = 0f;

        if (num3 == 4 && tile2.LiquidAmount == 0 && !WorldGen.SolidTile(tileX - 1, tileY))
            num7 = 0f;

        if (num3 == 3 && tile.LiquidAmount == 0 && !WorldGen.SolidTile(tileX + 1, tileY))
            num7 = 0f;

        vertices.BottomLeftColor *= num7;
        vertices.BottomRightColor *= num7;
        vertices.TopLeftColor *= num7;
        vertices.TopRightColor *= num7;
        bool flag7 = false;
        if (flag6) {
            int totalCount = (int)typeof(WaterStylesLoader).GetProperty("TotalCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(LoaderManager.Get<WaterStylesLoader>());/*TotalCount*/;
            for (int i = 0; i < totalCount; i++) {
                if (Main.IsLiquidStyleWater(i) && Main.liquidAlpha[i] > 0f && i != num2) {
                    TileDrawing_DrawPartialLiquid(self, !solidLayer, tileCache, ref position, ref liquidSize, i, ref vertices);
                    flag7 = true;
                    break;
                }
            }
        }

        VertexColors colors = vertices;
        bool flag8 = false;
        bool permafrost = false;
        bool tar = false;
        if (num2 == 50 || num2 == 51) {
            if (num2 == 50) {
                permafrost = true;
            }
            if (num2 == 51) {
                tar = true;
            }
            num2 -= 46;
            flag8 = true;
        }
        float num8 = (!flag8 && flag7 ? Main.liquidAlpha[num2] : 1f);
        colors.BottomLeftColor *= num8;
        colors.BottomRightColor *= num8;
        colors.TopLeftColor *= num8;
        colors.TopRightColor *= num8;
        if (num2 == 14)
            SetShimmerVertexColors(ref colors, solidLayer ? 0.75f : 1f, tileX, tileY);

        if (permafrost)
            SetPermafrostVertexColors(ref colors, 1f, tileX, tileY);

        if (tar)
            SetTarVertexColors(ref colors, 1f, tileX, tileY);

        TileDrawing_DrawPartialLiquid(self, !solidLayer, tileCache, ref position, ref liquidSize, flag8 ? num2 + 46 : num2, ref colors);
    }

    private void On_TileDrawing_DrawPartialLiquid(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors) {
        if (liquidType == 50 || liquidType == 51) {
            liquidType -= 46;

            int num = (int)tileCache.Slope;
            bool flag = !TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType];
            if (!behindBlocks)
                flag = false;

            if (flag || num == 0) {
                Main.tileBatch.Draw(_liquidBlockTextures[liquidType - 4].Value, position, liquidSize, colors, default(Vector2), 1f, SpriteEffects.None);
                return;
            }

            liquidSize.X += 18 * (num - 1);
            Texture2D slopeTexture = _liquidSlopeTextures[liquidType - 4].Value;
            switch (num) {
                case 1:
                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                    break;
                case 2:
                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                    break;
                case 3:
                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                    break;
                case 4:
                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                    break;
            }

            return;
        }

        orig(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
    }

    private Rectangle On_LiquidRenderer_GetCachedDrawArea(On_LiquidRenderer.orig_GetCachedDrawArea orig, LiquidRenderer self) {
        return Instance.GetCachedDrawArea();
    }
    private void On_LiquidRenderer_SetWaveMaskData(On_LiquidRenderer.orig_SetWaveMaskData orig, LiquidRenderer self, ref Texture2D texture) {
        Instance.SetWaveMaskData(ref texture);
    }
    private float On_LiquidRenderer_GetVisibleLiquid(On_LiquidRenderer.orig_GetVisibleLiquid orig, LiquidRenderer self, int x, int y) {
        return Instance.GetVisibleLiquid(x, y);
    }
    private void On_LiquidRenderer_PrepareAssets(On_LiquidRenderer.orig_PrepareAssets orig, LiquidRenderer self) {

    }
    private void On_LiquidRenderer_LoadContent(On_LiquidRenderer.orig_LoadContent orig) {

    }
    private void On_LiquidRenderer_Update(On_LiquidRenderer.orig_Update orig, LiquidRenderer self, GameTime gameTime) {
        Instance.Update(gameTime);
    }
    private void On_LiquidRenderer_DrawShimmer(On_LiquidRenderer.orig_DrawShimmer orig, LiquidRenderer self, SpriteBatch spriteBatch, Vector2 drawOffset, bool isBackgroundDraw) {
        Instance.DrawShimmer(spriteBatch, drawOffset, isBackgroundDraw);
    }
    private void On_LiquidRenderer_PrepareDraw(On_LiquidRenderer.orig_PrepareDraw orig, LiquidRenderer self, Rectangle drawArea) {
        //orig(self, drawArea);

        Instance.PrepareDraw(drawArea);
    }
    private void On_LiquidRenderer_DrawNormalLiquids(On_LiquidRenderer.orig_DrawNormalLiquids orig, LiquidRenderer self, SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float globalAlpha, bool isBackgroundDraw) {
        //orig(self, spriteBatch, drawOffset, waterStyle, globalAlpha, isBackgroundDraw);

        Instance.DrawNormalLiquids(spriteBatch, drawOffset, waterStyle, globalAlpha, isBackgroundDraw);
    }
    private bool On_LiquidRenderer_HasFullWater(On_LiquidRenderer.orig_HasFullWater orig, LiquidRenderer self, int x, int y) => HasFullWater(x, y);

    private void On_Liquid_LiquidCheck(On_Liquid.orig_LiquidCheck orig, int x, int y, int thisLiquidType) {
        if (thisLiquidType != 4 && thisLiquidType != 5) {

            orig(x, y, thisLiquidType);
            return;
        }

        return;

        if (WorldGen.SolidTile(x, y))
            return;

        Tile tile = Main.tile[x - 1, y];
        Tile tile2 = Main.tile[x + 1, y];
        Tile tile3 = Main.tile[x, y - 1];
        Tile tile4 = Main.tile[x, y + 1];
        Tile tile5 = Main.tile[x, y];
        if ((tile.LiquidAmount > 0 && tile.LiquidType != thisLiquidType) || (tile2.LiquidAmount > 0 && tile2.LiquidType != thisLiquidType) || (tile3.LiquidAmount > 0 && tile3.LiquidType != thisLiquidType)) {
            int num = 0;
            if (tile.LiquidType != thisLiquidType) {
                num += tile.LiquidAmount;
                tile.LiquidAmount = 0;
            }

            if (tile2.LiquidType != thisLiquidType) {
                num += tile2.LiquidAmount;
                tile2.LiquidAmount = 0;
            }

            if (tile3.LiquidType != thisLiquidType) {
                num += tile3.LiquidAmount;
                tile3.LiquidAmount = 0;
            }

            int liquidMergeTileType = 56;
            int liquidMergeType = 0;
            bool waterNearby = tile.LiquidType == 0 || tile2.LiquidType == 0 || tile3.LiquidType == 0;
            bool lavaNearby = tile.LiquidType == LiquidID.Lava || tile2.LiquidType == LiquidID.Lava || tile3.LiquidType == LiquidID.Lava;
            bool honeyNearby = tile.LiquidType == LiquidID.Honey || tile2.LiquidType == LiquidID.Honey || tile3.LiquidType == LiquidID.Honey;
            bool shimmerNearby = tile.LiquidType == LiquidID.Shimmer || tile2.LiquidType == LiquidID.Shimmer || tile3.LiquidType == LiquidID.Shimmer;
            Liquid.GetLiquidMergeTypes(thisLiquidType, out liquidMergeTileType, out liquidMergeType, waterNearby, lavaNearby, honeyNearby, shimmerNearby);
            if (num < 24 || liquidMergeType == thisLiquidType)
                return;

            if (tile5.HasTile && Main.tileObsidianKill[tile5.TileType]) {
                WorldGen.KillTile(x, y);
                if (Main.netMode == 2)
                    NetMessage.SendData(17, -1, -1, null, 0, x, y);
            }

            if (!tile5.HasTile) {
                tile5.LiquidAmount = 0;
                tile.LiquidType = 0;

                TileChangeType liquidChangeType = WorldGen.GetLiquidChangeType(thisLiquidType, liquidMergeType);
                if (!WorldGen.gen)
                    WorldGen.PlayLiquidChangeSound(liquidChangeType, x, y);

                WorldGen.PlaceTile(x, y, liquidMergeTileType, mute: true, forced: true);
                WorldGen.SquareTileFrame(x, y);

                // TODO: multiplayer packets
                //if (Main.netMode == 2)
                    //NetMessage.SendTileSquare(-1, x - 1, y - 1, 3, liquidChangeType);
            }
        }
        else {
            if (tile4.LiquidAmount <= 0 || tile4.LiquidType == thisLiquidType)
                return;

            bool flag = false;
            if (tile5.HasTile && TileID.Sets.IsAContainer[tile5.TileType] && !TileID.Sets.IsAContainer[tile4.TileType])
                flag = true;

            if (thisLiquidType != 0 && Main.tileCut[tile4.TileType]) {
                WorldGen.KillTile(x, y + 1);
                if (Main.netMode == 2)
                    NetMessage.SendData(17, -1, -1, null, 0, x, y + 1);
            }
            else if (tile4.HasTile && Main.tileObsidianKill[tile4.TileType]) {
                WorldGen.KillTile(x, y + 1);
                if (Main.netMode == 2)
                    NetMessage.SendData(17, -1, -1, null, 0, x, y + 1);
            }

            if (!(!tile4.HasTile || flag))
                return;

            if (tile5.LiquidAmount < 24) {
                tile5.LiquidAmount = 0;
                tile5.LiquidType = 0;
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, x - 1, y, 3);

                return;
            }

            int liquidMergeTileType2 = 56;
            int liquidMergeType2 = 0;
            bool waterNearby2 = tile4.LiquidType == 0;
            bool lavaNearby2 = tile4.LiquidType == LiquidID.Lava;
            bool honeyNearby2 = tile4.LiquidType == LiquidID.Honey;
            bool shimmerNearby2 = tile4.LiquidType == LiquidID.Shimmer;
            Liquid.GetLiquidMergeTypes(thisLiquidType, out liquidMergeTileType2, out liquidMergeType2, waterNearby2, lavaNearby2, honeyNearby2, shimmerNearby2);
            tile5.LiquidAmount = 0;
            tile5.LiquidType = 0;

            tile4.LiquidAmount = 0;
            TileChangeType liquidChangeType2 = WorldGen.GetLiquidChangeType(thisLiquidType, liquidMergeType2);
            if (!Main.gameMenu)
                WorldGen.PlayLiquidChangeSound(liquidChangeType2, x, y);

            WorldGen.PlaceTile(x, y + 1, liquidMergeTileType2, mute: true, forced: true);
            WorldGen.SquareTileFrame(x, y + 1);

            // TODO: multiplayer packets
            //if (Main.netMode == 2)
                //NetMessage.SendTileSquare(-1, x - 1, y, 3, liquidChangeType2);
        }
    }
    private void On_Liquid_Update(On_Liquid.orig_Update orig, Liquid self) {
        int x = self.x, y = self.y;
        Tile tile = Main.tile[x - 1, y];
        Tile tile2 = Main.tile[x + 1, y];
        Tile tile3 = Main.tile[x, y - 1];
        Tile tile4 = Main.tile[x, y + 1];
        Tile tile5 = Main.tile[x, y];
        if (!(tile5.LiquidType == 4 || tile5.LiquidType == 5)) {
            orig(self);
            return;
        }

        if (tile5.HasUnactuatedTile && Main.tileSolid[tile5.TileType] && !Main.tileSolidTop[tile5.TileType]) {
            _ = tile5.TileType;
            _ = 10;
            self.kill = 999;
            return;
        }

        byte liquid = tile5.LiquidAmount;
        float num = 0f;
        if (y > Main.UnderworldLayer && tile5.LiquidType == 0 && tile5.LiquidAmount > 0) {
            byte b = 2;
            if (tile5.LiquidAmount < b)
                b = tile5.LiquidAmount;

            tile5.LiquidAmount -= b;
        }

        if (tile5.LiquidAmount == 0) {
            self.kill = 999;
            return;
        }

        if (tile5.LiquidType == 4 || tile5.LiquidType == 5) {
            LavaCheck(x, y, tile5.LiquidType);
            if (!Liquid.quickFall) {
                if (tile5.LiquidType == 4) {
                    if (self.delay < 7) {
                        self.delay++;
                        return;
                    }

                    self.delay = 0;
                }
                if (tile5.LiquidType == 5) {
                    if (self.delay < 13) {
                        self.delay++;
                        return;
                    }

                    self.delay = 0;
                }
            }
        }
        else {
            if (tile.LiquidType == 4 || tile.LiquidType == 5)
                Liquid.AddWater(x - 1, y);

            if (tile2.LiquidType == 4 || tile2.LiquidType == 5)
                Liquid.AddWater(x + 1, y);

            if (tile3.LiquidType == 4 || tile3.LiquidType == 5)
                Liquid.AddWater(x, y - 1);

            if (tile4.LiquidType == 4 || tile4.LiquidType == 5)
                Liquid.AddWater(x, y + 1);

            //if (tile5.honey()) {
            //    HoneyCheck(x, y);
            //    if (!quickFall) {
            //        if (delay < 10) {
            //            delay++;
            //            return;
            //        }

            //        delay = 0;
            //    }
            //}
            //else {
            //    if (tile.honey())
            //        AddWater(x - 1, y);

            //    if (tile2.honey())
            //        AddWater(x + 1, y);

            //    if (tile3.honey())
            //        AddWater(x, y - 1);

            //    if (tile4.honey())
            //        AddWater(x, y + 1);

            //    if (tile5.shimmer()) {
            //        ShimmerCheck(x, y);
            //    }
            //    else {
            //        if (tile.shimmer())
            //            AddWater(x - 1, y);

            //        if (tile2.shimmer())
            //            AddWater(x + 1, y);

            //        if (tile3.shimmer())
            //            AddWater(x, y - 1);

            //        if (tile4.shimmer())
            //            AddWater(x, y + 1);
            //    }
            //}
        }

        if ((!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) && (tile4.LiquidAmount <= 0 || tile4.LiquidType == tile5.LiquidType) && tile4.LiquidAmount < byte.MaxValue) {
            bool flag = false;
            num = 255 - tile4.LiquidAmount;
            if (num > (float)(int)tile5.LiquidAmount)
                num = (int)tile5.LiquidAmount;

            if (num == 1f && tile5.LiquidAmount == byte.MaxValue)
                flag = true;

            if (!flag)
                tile5.LiquidAmount -= (byte)num;

            tile4.LiquidAmount += (byte)num;
            tile4.LiquidType = tile5.LiquidType;
            Liquid.AddWater(x, y + 1);
            tile4.SkipLiquid = true;
            tile5.SkipLiquid = true;
            if (Liquid.quickSettle && tile5.LiquidAmount > 250) {
                tile5.LiquidAmount = byte.MaxValue;
            }
            else if (!flag) {
                Liquid.AddWater(x - 1, y);
                Liquid.AddWater(x + 1, y);
            }
        }

        if (tile5.LiquidAmount > 0) {
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            bool flag5 = true;
            if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                flag2 = false;
            else if (tile.LiquidAmount > 0 && tile.LiquidType != tile5.LiquidType)
                flag2 = false;
            else if (Main.tile[x - 2, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x - 2, y].TileType] && !Main.tileSolidTop[Main.tile[x - 2, y].TileType])
                flag4 = false;
            else if (Main.tile[x - 2, y].LiquidAmount == 0)
                flag4 = false;
            else if (Main.tile[x - 2, y].LiquidAmount > 0 && Main.tile[x - 2, y].LiquidType != tile5.LiquidType)
                flag4 = false;

            if (tile2.HasUnactuatedTile && Main.tileSolid[tile2.TileType] && !Main.tileSolidTop[tile2.TileType])
                flag3 = false;
            else if (tile2.LiquidAmount > 0 && tile2.LiquidType != tile5.LiquidType)
                flag3 = false;
            else if (Main.tile[x + 2, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x + 2, y].TileType] && !Main.tileSolidTop[Main.tile[x + 2, y].TileType])
                flag5 = false;
            else if (Main.tile[x + 2, y].LiquidAmount == 0)
                flag5 = false;
            else if (Main.tile[x + 2, y].LiquidAmount > 0 && Main.tile[x + 2, y].LiquidType != tile5.LiquidType)
                flag5 = false;

            int num2 = 0;
            if (tile5.LiquidAmount < 3)
                num2 = -1;

            if (tile5.LiquidAmount > 250) {
                flag4 = false;
                flag5 = false;
            }

            if (flag2 && flag3) {
                if (flag4 && flag5) {
                    bool flag6 = true;
                    bool flag7 = true;
                    if (Main.tile[x - 3, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x - 3, y].TileType] && !Main.tileSolidTop[Main.tile[x - 3, y].TileType])
                        flag6 = false;
                    else if (Main.tile[x - 3, y].LiquidAmount == 0)
                        flag6 = false;
                    else if (Main.tile[x - 3, y].LiquidType != tile5.LiquidType)
                        flag6 = false;

                    if (Main.tile[x + 3, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x + 3, y].TileType] && !Main.tileSolidTop[Main.tile[x + 3, y].TileType])
                        flag7 = false;
                    else if (Main.tile[x + 3, y].LiquidAmount == 0)
                        flag7 = false;
                    else if (Main.tile[x + 3, y].LiquidType != tile5.LiquidType)
                        flag7 = false;

                    if (flag6 && flag7) {
                        num = tile.LiquidAmount + tile2.LiquidAmount + Main.tile[x - 2, y].LiquidAmount + Main.tile[x + 2, y].LiquidAmount + Main.tile[x - 3, y].LiquidAmount + Main.tile[x + 3, y].LiquidAmount + tile5.LiquidAmount + num2;
                        num = (float)Math.Round(num / 7f);
                        int num3 = 0;
                        tile.LiquidType = tile5.LiquidType;
                        if (tile.LiquidAmount != (byte)num) {
                            tile.LiquidAmount = (byte)num;
                            Liquid.AddWater(x - 1, y);
                        }
                        else {
                            num3++;
                        }

                        tile2.LiquidType = tile5.LiquidType;
                        if (tile2.LiquidAmount != (byte)num) {
                            tile2.LiquidAmount = (byte)num;
                            Liquid.AddWater(x + 1, y);
                        }
                        else {
                            num3++;
                        }

                        Tile checkTile = Main.tile[x - 2, y];
                        checkTile.LiquidType = tile5.LiquidType;
                        if (Main.tile[x - 2, y].LiquidAmount != (byte)num) {
                            Main.tile[x - 2, y].LiquidAmount = (byte)num;
                            Liquid.AddWater(x - 2, y);
                        }
                        else {
                            num3++;
                        }

                        checkTile = Main.tile[x + 2, y];
                        checkTile.LiquidType = tile5.LiquidType;
                        if (Main.tile[x + 2, y].LiquidAmount != (byte)num) {
                            Main.tile[x + 2, y].LiquidAmount = (byte)num;
                            Liquid.AddWater(x + 2, y);
                        }
                        else {
                            num3++;
                        }

                        checkTile = Main.tile[x - 3, y];
                        checkTile.LiquidType = tile5.LiquidType;
                        if (Main.tile[x - 3, y].LiquidAmount != (byte)num) {
                            Main.tile[x - 3, y].LiquidAmount = (byte)num;
                            Liquid.AddWater(x - 3, y);
                        }
                        else {
                            num3++;
                        }

                        checkTile = Main.tile[x + 3, y];
                        checkTile.LiquidType = tile5.LiquidType;
                        if (Main.tile[x + 3, y].LiquidAmount != (byte)num) {
                            Main.tile[x + 3, y].LiquidAmount = (byte)num;
                            Liquid.AddWater(x + 3, y);
                        }
                        else {
                            num3++;
                        }

                        if (tile.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x - 1, y);

                        if (tile2.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x + 1, y);

                        if (Main.tile[x - 2, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x - 2, y);

                        if (Main.tile[x + 2, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x + 2, y);

                        if (Main.tile[x - 3, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x - 3, y);

                        if (Main.tile[x + 3, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x + 3, y);

                        if (num3 != 6 || tile3.LiquidAmount <= 0)
                            tile5.LiquidAmount = (byte)num;
                    }
                    else {
                        int num4 = 0;
                        num = tile.LiquidAmount + tile2.LiquidAmount + Main.tile[x - 2, y].LiquidAmount + Main.tile[x + 2, y].LiquidAmount + tile5.LiquidAmount + num2;
                        num = (float)Math.Round(num / 5f);
                        tile.LiquidType = tile5.LiquidType;
                        if (tile.LiquidAmount != (byte)num) {
                            tile.LiquidAmount = (byte)num;
                            Liquid.AddWater(x - 1, y);
                        }
                        else {
                            num4++;
                        }

                        tile2.LiquidType = tile5.LiquidType;
                        if (tile2.LiquidAmount != (byte)num) {
                            tile2.LiquidAmount = (byte)num;
                            Liquid.AddWater(x + 1, y);
                        }
                        else {
                            num4++;
                        }

                        Tile checkTile = Main.tile[x - 2, y];
                        checkTile.LiquidType = tile5.LiquidType;
                        if (Main.tile[x - 2, y].LiquidAmount != (byte)num) {
                            Main.tile[x - 2, y].LiquidAmount = (byte)num;
                            Liquid.AddWater(x - 2, y);
                        }
                        else {
                            num4++;
                        }

                        checkTile = Main.tile[x + 2, y];
                        checkTile.LiquidType = tile5.LiquidType;
                        if (Main.tile[x + 2, y].LiquidAmount != (byte)num) {
                            Main.tile[x + 2, y].LiquidAmount = (byte)num;
                            Liquid.AddWater(x + 2, y);
                        }
                        else {
                            num4++;
                        }

                        if (tile.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x - 1, y);

                        if (tile2.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x + 1, y);

                        if (Main.tile[x - 2, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x - 2, y);

                        if (Main.tile[x + 2, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num)
                            Liquid.AddWater(x + 2, y);

                        if (num4 != 4 || tile3.LiquidAmount <= 0)
                            tile5.LiquidAmount = (byte)num;
                    }
                }
                else if (flag4) {
                    num = tile.LiquidAmount + tile2.LiquidAmount + Main.tile[x - 2, y].LiquidAmount + tile5.LiquidAmount + num2;
                    num = (float)Math.Round(num / 4f);
                    tile.LiquidType = tile5.LiquidType;
                    if (tile.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num) {
                        tile.LiquidAmount = (byte)num;
                        Liquid.AddWater(x - 1, y);
                    }

                    tile2.LiquidType = tile5.LiquidType;
                    if (tile2.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num) {
                        tile2.LiquidAmount = (byte)num;
                        Liquid.AddWater(x + 1, y);
                    }

                    Tile checkTile = Main.tile[x - 2, y];
                    checkTile.LiquidType = tile5.LiquidType;
                    if (Main.tile[x - 2, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num) {
                        Main.tile[x - 2, y].LiquidAmount = (byte)num;
                        Liquid.AddWater(x - 2, y);
                    }

                    tile5.LiquidAmount = (byte)num;
                }
                else if (flag5) {
                    num = tile.LiquidAmount + tile2.LiquidAmount + Main.tile[x + 2, y].LiquidAmount + tile5.LiquidAmount + num2;
                    num = (float)Math.Round(num / 4f);
                    tile.LiquidType = tile5.LiquidType;
                    if (tile.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num) {
                        tile.LiquidAmount = (byte)num;
                        Liquid.AddWater(x - 1, y);
                    }

                    tile2.LiquidType = tile5.LiquidType;
                    if (tile2.LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num) {
                        tile2.LiquidAmount = (byte)num;
                        Liquid.AddWater(x + 1, y);
                    }

                    Tile checkTile = Main.tile[x + 2, y];
                    checkTile.LiquidType = tile5.LiquidType;
                    if (Main.tile[x + 2, y].LiquidAmount != (byte)num || tile5.LiquidAmount != (byte)num) {
                        Main.tile[x + 2, y].LiquidAmount = (byte)num;
                        Liquid.AddWater(x + 2, y);
                    }

                    tile5.LiquidAmount = (byte)num;
                }
                else {
                    num = tile.LiquidAmount + tile2.LiquidAmount + tile5.LiquidAmount + num2;
                    num = (float)Math.Round(num / 3f);
                    if (num == 254f && WorldGen.genRand.Next(30) == 0)
                        num = 255f;

                    tile.LiquidType = tile5.LiquidType;
                    if (tile.LiquidAmount != (byte)num) {
                        tile.LiquidAmount = (byte)num;
                        Liquid.AddWater(x - 1, y);
                    }

                    tile2.LiquidType = tile5.LiquidType;
                    if (tile2.LiquidAmount != (byte)num) {
                        tile2.LiquidAmount = (byte)num;
                        Liquid.AddWater(x + 1, y);
                    }

                    tile5.LiquidAmount = (byte)num;
                }
            }
            else if (flag2) {
                num = tile.LiquidAmount + tile5.LiquidAmount + num2;
                num = (float)Math.Round(num / 2f);
                if (tile.LiquidAmount != (byte)num)
                    tile.LiquidAmount = (byte)num;

                tile.LiquidType = tile5.LiquidType;
                if (tile5.LiquidAmount != (byte)num || tile.LiquidAmount != (byte)num)
                    Liquid.AddWater(x - 1, y);

                tile5.LiquidAmount = (byte)num;
            }
            else if (flag3) {
                num = tile2.LiquidAmount + tile5.LiquidAmount + num2;
                num = (float)Math.Round(num / 2f);
                if (tile2.LiquidAmount != (byte)num)
                    tile2.LiquidAmount = (byte)num;

                tile2.LiquidType = tile5.LiquidType;
                if (tile5.LiquidAmount != (byte)num || tile2.LiquidAmount != (byte)num)
                    Liquid.AddWater(x + 1, y);

                tile5.LiquidAmount = (byte)num;
            }
        }

        if (tile5.LiquidAmount != liquid) {
            if (tile5.LiquidAmount == 254 && liquid == byte.MaxValue) {
                if (Liquid.quickSettle) {
                    tile5.LiquidAmount = byte.MaxValue;
                    self.kill++;
                }
                else {
                    self.kill++;
                }
            }
            else {
                Liquid.AddWater(x, y - 1);
                self.kill = 0;
            }
        }
        else {
            self.kill++;
        }
    }
    private void On_Liquid_SettleWaterAt(On_Liquid.orig_SettleWaterAt orig, int originX, int originY) {
        Tile tile = Main.tile[originX, originY];
        Liquid.tilesIgnoreWater(ignoreSolids: true);
        if (tile.LiquidAmount == 0) {
            return;
        }
        int num = originX;
        int num2 = originY;
        if (tile.LiquidType == 4 || tile.LiquidType == 5) {
            int num3 = tile.LiquidAmount;
            int b = tile.LiquidType;
            bool tileAtXYHasLava = b == 4;
            bool flag = b == 5;
            //bool flag2 = b == LiquidID.Shimmer;
            tile.LiquidAmount = 0;
            bool flag3 = true;
            while (true) {
                Tile tile2 = Main.tile[num, num2 + 1];
                bool flag4 = false;
                while (num2 < Main.maxTilesY - 5 && tile2.LiquidAmount == 0 && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType])) {
                    num2++;
                    flag4 = true;
                    flag3 = false;
                    tile2 = Main.tile[num, num2 + 1];
                }

                // here we replace liquids upon worldgen
                if (WorldGen.gen && flag4 && !flag && !tileAtXYHasLava) {
                    if (WorldGen.remixWorldGen)
                        b = (byte)((num2 > GenVars.lavaLine && ((double)num2 < Main.rockLayer - 80.0 || num2 > Main.maxTilesY - 350)) ? ((!WorldGen.oceanDepths(num, num2)) ? 1 : 0) : 0);
                    else if (num2 > GenVars.waterLine)
                        b = 1;
                }

                int num4 = -1;
                int num5 = 0;
                int num6 = -1;
                int num7 = 0;
                bool flag5 = false;
                bool flag6 = false;
                bool flag7 = false;
                while (true) {
                    if (Main.tile[num + num5 * num4, num2].LiquidAmount == 0) {
                        num6 = num4;
                        num7 = num5;
                    }

                    if (num4 == -1 && num + num5 * num4 < 5)
                        flag6 = true;
                    else if (num4 == 1 && num + num5 * num4 > Main.maxTilesX - 5)
                        flag5 = true;

                    tile2 = Main.tile[num + num5 * num4, num2 + 1];
                    if (tile2.LiquidAmount != 0 && tile2.LiquidAmount != byte.MaxValue && tile2.LiquidType == b) {
                        int num8 = 255 - tile2.LiquidAmount;
                        if (num8 > num3)
                            num8 = num3;

                        tile2.LiquidAmount += (byte)num8;
                        num3 -= num8;
                        if (num3 == 0)
                            break;
                    }

                    if (num2 < Main.maxTilesY - 5 && tile2.LiquidAmount == 0 && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType])) {
                        flag7 = true;
                        break;
                    }

                    Tile tile3 = Main.tile[num + (num5 + 1) * num4, num2];
                    if ((tile3.LiquidAmount != 0 && (!flag3 || num4 != 1)) || (tile3.HasUnactuatedTile && Main.tileSolid[tile3.TileType] && !Main.tileSolidTop[tile3.TileType])) {
                        if (num4 == 1)
                            flag5 = true;
                        else
                            flag6 = true;
                    }

                    if (flag6 && flag5)
                        break;

                    if (flag5) {
                        num4 = -1;
                        num5++;
                    }
                    else if (flag6) {
                        if (num4 == 1)
                            num5++;

                        num4 = 1;
                    }
                    else {
                        if (num4 == 1)
                            num5++;

                        num4 = -num4;
                    }
                }

                num += num7 * num6;
                if (num3 == 0 || !flag7)
                    break;

                num2++;
            }

            Main.tile[num, num2].LiquidAmount = (byte)num3;
            tile.LiquidType = b;

            AttemptToMoveLava(num, num2, tileAtXYHasLava, 4);
            AttemptToMoveLava(num, num2, flag, 5);

            Liquid.tilesIgnoreWater(ignoreSolids: false);

            return;
        }
        orig(originX, originY);
    }
    private static void AttemptToMoveLava(int X, int Y, bool tileAtXYHasLava, int checkLiquidType) {
        if (Main.tile[X - 1, Y].LiquidAmount > 0 && ((!tileAtXYHasLava && Main.tile[X - 1, Y].LiquidType == checkLiquidType) || (tileAtXYHasLava && Main.tile[X - 1, Y].LiquidType != checkLiquidType))) {
            if (tileAtXYHasLava)
                LavaCheck(X, Y, checkLiquidType);
            else
                LavaCheck(X - 1, Y, checkLiquidType);
        }
        else if (Main.tile[X + 1, Y].LiquidAmount > 0 && ((!tileAtXYHasLava && Main.tile[X + 1, Y].LiquidType == checkLiquidType) || (tileAtXYHasLava && Main.tile[X + 1, Y].LiquidType != checkLiquidType))) {
            if (tileAtXYHasLava)
                LavaCheck(X, Y, checkLiquidType);
            else
                LavaCheck(X + 1, Y, checkLiquidType);
        }
        else if (Main.tile[X, Y - 1].LiquidAmount > 0 && ((!tileAtXYHasLava && Main.tile[X, Y - 1].LiquidType == checkLiquidType) || (tileAtXYHasLava && Main.tile[X, Y - 1].LiquidType != checkLiquidType))) {
            if (tileAtXYHasLava)
                LavaCheck(X, Y, checkLiquidType);
            else
                LavaCheck(X, Y - 1, checkLiquidType);
        }
        else if (Main.tile[X, Y + 1].LiquidAmount > 0 && ((!tileAtXYHasLava && Main.tile[X, Y + 1].LiquidType == checkLiquidType) || (tileAtXYHasLava && Main.tile[X, Y + 1].LiquidType != checkLiquidType))) {
            if (tileAtXYHasLava)
                LavaCheck(X, Y, checkLiquidType);
            else
                LavaCheck(X, Y + 1, checkLiquidType);
        }
    }
    public static void LavaCheck(int x, int y, int checkLiquid) {
        if (!WorldGen.remixWorldGen && WorldGen.generatingWorld/* && UndergroundDesertCheck(x, y)*/) {
            for (int i = x - 3; i <= x + 3; i++) {
                for (int j = y - 3; j <= y + 3; j++) {
                    Tile tile = Main.tile[i, j];
                    tile.LiquidType = checkLiquid;
                }
            }
        }

        Liquid.LiquidCheck(x, y, checkLiquid);
    }

    private void On_SceneMetrics_Reset(On_SceneMetrics.orig_Reset orig, SceneMetrics self) {
        var fieldInfo = typeof(SceneMetrics).GetField("_liquidCounts", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo?.SetValue(self, new int[4 + 2]);

        orig(self);
    }

    private struct LiquidCache {
        public float LiquidLevel;
        public float VisibleLiquidLevel;
        public float Opacity;
        public bool IsSolid;
        public bool IsHalfBrick;
        public bool HasLiquid;
        public bool HasVisibleLiquid;
        public bool HasWall;
        public Point FrameOffset;
        public bool HasLeftEdge;
        public bool HasRightEdge;
        public bool HasTopEdge;
        public bool HasBottomEdge;
        public float LeftWall;
        public float RightWall;
        public float BottomWall;
        public float TopWall;
        public float VisibleLeftWall;
        public float VisibleRightWall;
        public float VisibleBottomWall;
        public float VisibleTopWall;
        public byte Type;
        public byte VisibleType;
    }

    private struct LiquidDrawCache {
        public Rectangle SourceRectangle;
        public Vector2 LiquidOffset;
        public bool IsVisible;
        public float Opacity;
        public byte Type;
        public bool IsSurfaceLiquid;
        public bool HasWall;
        public int X;
        public int Y;
    }

    private struct SpecialLiquidDrawCache {
        public int X;
        public int Y;
        public Rectangle SourceRectangle;
        public Vector2 LiquidOffset;
        public bool IsVisible;
        public float Opacity;
        public byte Type;
        public bool IsSurfaceLiquid;
        public bool HasWall;
    }

    private const int ANIMATION_FRAME_COUNT = 16;
    private const int CACHE_PADDING = 2;
    private const int CACHE_PADDING_2 = 4;
    public const float MIN_LIQUID_SIZE = 0.25f;
    public static CustomLiquidRenderer Instance;
    public Asset<Texture2D>[] _liquidTextures = new Asset<Texture2D>[15 + 2];
    public static Asset<Texture2D>[] _liquidSlopeTextures = new Asset<Texture2D>[2];
    public static Asset<Texture2D>[] _liquidBlockTextures = new Asset<Texture2D>[2];
    private LiquidCache[] _cache = new LiquidCache[1];
    private LiquidDrawCache[] _drawCache = new LiquidDrawCache[1];
    private SpecialLiquidDrawCache[] _drawCacheForShimmer = new SpecialLiquidDrawCache[1];
    private int _animationFrame;
    private Rectangle _drawArea = new Rectangle(0, 0, 1, 1);
    private readonly UnifiedRandom _random = new UnifiedRandom();
    private Color[] _waveMask = new Color[1];
    private float _frameState;

    /*
	private static Tile[,] Tiles => Main.tile;
	*/

    public event Action<Color[], Rectangle> WaveFilters;

    public static void LoadContent() {
        Instance = new CustomLiquidRenderer();
        Instance.PrepareAssets();
    }

    private void PrepareAssets() {
        if (!Main.dedServ) {
            for (int i = 0; i < 15; i++) {
                _liquidTextures[i] = Main.Assets.Request<Texture2D>("Images/Misc/water_" + i);
            }

            _liquidTextures[15] = ModContent.Request<Texture2D>(ResourceManager.WaterTextures + "Permafrost");
            _liquidSlopeTextures[0] = ModContent.Request<Texture2D>(ResourceManager.WaterTextures + "Permafrost_Slope");
            _liquidBlockTextures[0] = ModContent.Request<Texture2D>(ResourceManager.WaterTextures + "Permafrost_Block");
            _liquidTextures[16] = ModContent.Request<Texture2D>(ResourceManager.WaterTextures + "Tar");
            _liquidSlopeTextures[1] = ModContent.Request<Texture2D>(ResourceManager.WaterTextures + "Tar_Slope");
            _liquidBlockTextures[1] = ModContent.Request<Texture2D>(ResourceManager.WaterTextures + "Tar_Block");
        }
    }

    private unsafe void InternalPrepareDraw(Rectangle drawArea) {
        Rectangle rectangle = new Rectangle(drawArea.X - 2, drawArea.Y - 2, drawArea.Width + 4, drawArea.Height + 4);
        _drawArea = drawArea;
        if (_cache.Length < rectangle.Width * rectangle.Height + 1)
            _cache = new LiquidCache[rectangle.Width * rectangle.Height + 1];

        if (_drawCache.Length < drawArea.Width * drawArea.Height + 1)
            _drawCache = new LiquidDrawCache[drawArea.Width * drawArea.Height + 1];

        if (_drawCacheForShimmer.Length < drawArea.Width * drawArea.Height + 1)
            _drawCacheForShimmer = new SpecialLiquidDrawCache[drawArea.Width * drawArea.Height + 1];

        if (_waveMask.Length < drawArea.Width * drawArea.Height)
            _waveMask = new Color[drawArea.Width * drawArea.Height];

        Tile tile = default;
        fixed (LiquidCache* ptr = &_cache[1]) {
            LiquidCache* ptr2 = ptr;
            int num = rectangle.Height * 2 + 2;
            ptr2 = ptr;
            for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++) {
                for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++) {
                    tile = Main.tile[i, j];
                    if (tile == null)
                        tile = new Tile();

                    //if (tile.LiquidType < 4) {
                    //    ptr2->LiquidLevel = 0f;
                    //    ptr2->IsHalfBrick = tile.IsHalfBlock && ptr2[-1].HasLiquid && !TileID.Sets.Platforms[tile.TileType];
                    //    ptr2->IsSolid = WorldGen.SolidOrSlopedTile(tile);
                    //    ptr2->HasLiquid = false;
                    //    ptr2->VisibleLiquidLevel = 0f;
                    //    ptr2->HasWall = tile.WallType != 0;
                    //    ptr2->Type = (byte)tile.LiquidType;
                    //    if (ptr2->IsHalfBrick && !ptr2->HasLiquid)
                    //        ptr2->Type = ptr2[-1].Type;

                    //    ptr2++;

                    //    continue;
                    //}

                    ptr2->LiquidLevel = (float)(int)tile.LiquidAmount / 255f;
                    ptr2->IsHalfBrick = tile.IsHalfBlock && ptr2[-1].HasLiquid && !TileID.Sets.Platforms[tile.TileType];
                    ptr2->IsSolid = WorldGen.SolidOrSlopedTile(tile);
                    ptr2->HasLiquid = tile.LiquidAmount != 0;
                    ptr2->VisibleLiquidLevel = 0f;
                    ptr2->HasWall = tile.WallType != 0;
                    ptr2->Type = (byte)tile.LiquidType;
                    if (ptr2->IsHalfBrick && !ptr2->HasLiquid)
                        ptr2->Type = ptr2[-1].Type;

                    ptr2++;
                }
            }

            ptr2 = ptr;
            float num2 = 0f;
            ptr2 += num;
            for (int k = 2; k < rectangle.Width - 2; k++) {
                for (int l = 2; l < rectangle.Height - 2; l++) {
                    num2 = 0f;
                    if (ptr2->IsHalfBrick && ptr2[-1].HasLiquid) {
                        num2 = 1f;
                    }
                    else if (!ptr2->HasLiquid) {
                        LiquidCache liquidCache = ptr2[-1];
                        LiquidCache liquidCache2 = ptr2[1];
                        LiquidCache liquidCache3 = ptr2[-rectangle.Height];
                        LiquidCache liquidCache4 = ptr2[rectangle.Height];
                        if (liquidCache.HasLiquid && liquidCache2.HasLiquid && liquidCache.Type == liquidCache2.Type && !liquidCache.IsSolid && !liquidCache2.IsSolid) {
                            num2 = liquidCache.LiquidLevel + liquidCache2.LiquidLevel;
                            ptr2->Type = liquidCache.Type;
                        }

                        if (liquidCache3.HasLiquid && liquidCache4.HasLiquid && liquidCache3.Type == liquidCache4.Type && !liquidCache3.IsSolid && !liquidCache4.IsSolid) {
                            num2 = Math.Max(num2, liquidCache3.LiquidLevel + liquidCache4.LiquidLevel);
                            ptr2->Type = liquidCache3.Type;
                        }

                        num2 *= 0.5f;
                    }
                    else {
                        num2 = ptr2->LiquidLevel;
                    }

                    ptr2->VisibleLiquidLevel = num2;
                    ptr2->HasVisibleLiquid = num2 != 0f;
                    ptr2++;
                }

                ptr2 += 4;
            }

            ptr2 = ptr;
            for (int m = 0; m < rectangle.Width; m++) {
                for (int n = 0; n < rectangle.Height - 10; n++) {
                    if (ptr2->HasVisibleLiquid && (!ptr2->IsSolid || ptr2->IsHalfBrick)) {
                        ptr2->Opacity = 1f;
                        ptr2->VisibleType = ptr2->Type;
                        float num3 = 1f / (float)(WATERFALL_LENGTH[ptr2->Type] + 1);
                        float num4 = 1f;
                        for (int num5 = 1; num5 <= WATERFALL_LENGTH[ptr2->Type]; num5++) {
                            num4 -= num3;
                            if (ptr2[num5].IsSolid)
                                break;

                            ptr2[num5].VisibleLiquidLevel = Math.Max(ptr2[num5].VisibleLiquidLevel, ptr2->VisibleLiquidLevel * num4);
                            ptr2[num5].Opacity = num4;
                            ptr2[num5].VisibleType = ptr2->Type;
                        }
                    }

                    if (ptr2->IsSolid && !ptr2->IsHalfBrick) {
                        ptr2->VisibleLiquidLevel = 1f;
                        ptr2->HasVisibleLiquid = false;
                    }
                    else {
                        ptr2->HasVisibleLiquid = ptr2->VisibleLiquidLevel != 0f;
                    }

                    ptr2++;
                }

                ptr2 += 10;
            }

            ptr2 = ptr;
            ptr2 += num;
            for (int num6 = 2; num6 < rectangle.Width - 2; num6++) {
                for (int num7 = 2; num7 < rectangle.Height - 2; num7++) {
                    if (!ptr2->HasVisibleLiquid) {
                        ptr2->HasLeftEdge = false;
                        ptr2->HasTopEdge = false;
                        ptr2->HasRightEdge = false;
                        ptr2->HasBottomEdge = false;
                    }
                    else {
                        LiquidCache liquidCache = ptr2[-1];
                        LiquidCache liquidCache2 = ptr2[1];
                        LiquidCache liquidCache3 = ptr2[-rectangle.Height];
                        LiquidCache liquidCache4 = ptr2[rectangle.Height];
                        float num8 = 0f;
                        float num9 = 1f;
                        float num10 = 0f;
                        float num11 = 1f;
                        float visibleLiquidLevel = ptr2->VisibleLiquidLevel;
                        if (!liquidCache.HasVisibleLiquid)
                            num10 += liquidCache2.VisibleLiquidLevel * (1f - visibleLiquidLevel);

                        if (!liquidCache2.HasVisibleLiquid && !liquidCache2.IsSolid && !liquidCache2.IsHalfBrick)
                            num11 -= liquidCache.VisibleLiquidLevel * (1f - visibleLiquidLevel);

                        if (!liquidCache3.HasVisibleLiquid && !liquidCache3.IsSolid && !liquidCache3.IsHalfBrick)
                            num8 += liquidCache4.VisibleLiquidLevel * (1f - visibleLiquidLevel);

                        if (!liquidCache4.HasVisibleLiquid && !liquidCache4.IsSolid && !liquidCache4.IsHalfBrick)
                            num9 -= liquidCache3.VisibleLiquidLevel * (1f - visibleLiquidLevel);

                        ptr2->LeftWall = num8;
                        ptr2->RightWall = num9;
                        ptr2->BottomWall = num11;
                        ptr2->TopWall = num10;
                        Point zero = Point.Zero;
                        ptr2->HasTopEdge = (!liquidCache.HasVisibleLiquid && !liquidCache.IsSolid) || num10 != 0f;
                        ptr2->HasBottomEdge = (!liquidCache2.HasVisibleLiquid && !liquidCache2.IsSolid) || num11 != 1f;
                        ptr2->HasLeftEdge = (!liquidCache3.HasVisibleLiquid && !liquidCache3.IsSolid) || num8 != 0f;
                        ptr2->HasRightEdge = (!liquidCache4.HasVisibleLiquid && !liquidCache4.IsSolid) || num9 != 1f;
                        if (!ptr2->HasLeftEdge) {
                            if (ptr2->HasRightEdge)
                                zero.X += 32;
                            else
                                zero.X += 16;
                        }

                        if (ptr2->HasLeftEdge && ptr2->HasRightEdge) {
                            zero.X = 16;
                            zero.Y += 32;
                            if (ptr2->HasTopEdge)
                                zero.Y = 16;
                        }
                        else if (!ptr2->HasTopEdge) {
                            if (!ptr2->HasLeftEdge && !ptr2->HasRightEdge)
                                zero.Y += 48;
                            else
                                zero.Y += 16;
                        }

                        if (zero.Y == 16 && (ptr2->HasLeftEdge ^ ptr2->HasRightEdge) && (num7 + rectangle.Y) % 2 == 0)
                            zero.Y += 16;

                        ptr2->FrameOffset = zero;
                    }

                    ptr2++;
                }

                ptr2 += 4;
            }

            ptr2 = ptr;
            ptr2 += num;
            for (int num12 = 2; num12 < rectangle.Width - 2; num12++) {
                for (int num13 = 2; num13 < rectangle.Height - 2; num13++) {
                    if (ptr2->HasVisibleLiquid) {
                        LiquidCache liquidCache = ptr2[-1];
                        LiquidCache liquidCache2 = ptr2[1];
                        LiquidCache liquidCache3 = ptr2[-rectangle.Height];
                        LiquidCache liquidCache4 = ptr2[rectangle.Height];
                        ptr2->VisibleLeftWall = ptr2->LeftWall;
                        ptr2->VisibleRightWall = ptr2->RightWall;
                        ptr2->VisibleTopWall = ptr2->TopWall;
                        ptr2->VisibleBottomWall = ptr2->BottomWall;
                        if (liquidCache.HasVisibleLiquid && liquidCache2.HasVisibleLiquid) {
                            if (ptr2->HasLeftEdge)
                                ptr2->VisibleLeftWall = (ptr2->LeftWall * 2f + liquidCache.LeftWall + liquidCache2.LeftWall) * 0.25f;

                            if (ptr2->HasRightEdge)
                                ptr2->VisibleRightWall = (ptr2->RightWall * 2f + liquidCache.RightWall + liquidCache2.RightWall) * 0.25f;
                        }

                        if (liquidCache3.HasVisibleLiquid && liquidCache4.HasVisibleLiquid) {
                            if (ptr2->HasTopEdge)
                                ptr2->VisibleTopWall = (ptr2->TopWall * 2f + liquidCache3.TopWall + liquidCache4.TopWall) * 0.25f;

                            if (ptr2->HasBottomEdge)
                                ptr2->VisibleBottomWall = (ptr2->BottomWall * 2f + liquidCache3.BottomWall + liquidCache4.BottomWall) * 0.25f;
                        }
                    }

                    ptr2++;
                }

                ptr2 += 4;
            }

            ptr2 = ptr;
            ptr2 += num;
            for (int num14 = 2; num14 < rectangle.Width - 2; num14++) {
                for (int num15 = 2; num15 < rectangle.Height - 2; num15++) {
                    if (ptr2->HasLiquid) {
                        LiquidCache liquidCache = ptr2[-1];
                        LiquidCache liquidCache2 = ptr2[1];
                        LiquidCache liquidCache3 = ptr2[-rectangle.Height];
                        LiquidCache liquidCache4 = ptr2[rectangle.Height];
                        if (ptr2->HasTopEdge && !ptr2->HasBottomEdge && (ptr2->HasLeftEdge ^ ptr2->HasRightEdge)) {
                            if (ptr2->HasRightEdge) {
                                ptr2->VisibleRightWall = liquidCache2.VisibleRightWall;
                                ptr2->VisibleTopWall = liquidCache3.VisibleTopWall;
                            }
                            else {
                                ptr2->VisibleLeftWall = liquidCache2.VisibleLeftWall;
                                ptr2->VisibleTopWall = liquidCache4.VisibleTopWall;
                            }
                        }
                        else if (liquidCache2.FrameOffset.X == 16 && liquidCache2.FrameOffset.Y == 32) {
                            if (ptr2->VisibleLeftWall > 0.5f) {
                                ptr2->VisibleLeftWall = 0f;
                                ptr2->FrameOffset = new Point(0, 0);
                            }
                            else if (ptr2->VisibleRightWall < 0.5f) {
                                ptr2->VisibleRightWall = 1f;
                                ptr2->FrameOffset = new Point(32, 0);
                            }
                        }
                    }

                    ptr2++;
                }

                ptr2 += 4;
            }

            ptr2 = ptr;
            ptr2 += num;
            for (int num16 = 2; num16 < rectangle.Width - 2; num16++) {
                for (int num17 = 2; num17 < rectangle.Height - 2; num17++) {
                    if (ptr2->HasLiquid) {
                        LiquidCache liquidCache = ptr2[-1];
                        LiquidCache liquidCache2 = ptr2[1];
                        LiquidCache liquidCache3 = ptr2[-rectangle.Height];
                        LiquidCache liquidCache4 = ptr2[rectangle.Height];
                        if (!ptr2->HasBottomEdge && !ptr2->HasLeftEdge && !ptr2->HasTopEdge && !ptr2->HasRightEdge) {
                            if (liquidCache3.HasTopEdge && liquidCache.HasLeftEdge) {
                                ptr2->FrameOffset.X = Math.Max(4, (int)(16f - liquidCache.VisibleLeftWall * 16f)) - 4;
                                ptr2->FrameOffset.Y = 48 + Math.Max(4, (int)(16f - liquidCache3.VisibleTopWall * 16f)) - 4;
                                ptr2->VisibleLeftWall = 0f;
                                ptr2->VisibleTopWall = 0f;
                                ptr2->VisibleRightWall = 1f;
                                ptr2->VisibleBottomWall = 1f;
                            }
                            else if (liquidCache4.HasTopEdge && liquidCache.HasRightEdge) {
                                ptr2->FrameOffset.X = 32 - Math.Min(16, (int)(liquidCache.VisibleRightWall * 16f) - 4);
                                ptr2->FrameOffset.Y = 48 + Math.Max(4, (int)(16f - liquidCache4.VisibleTopWall * 16f)) - 4;
                                ptr2->VisibleLeftWall = 0f;
                                ptr2->VisibleTopWall = 0f;
                                ptr2->VisibleRightWall = 1f;
                                ptr2->VisibleBottomWall = 1f;
                            }
                        }
                    }

                    ptr2++;
                }

                ptr2 += 4;
            }

            ptr2 = ptr;
            ptr2 += num;
            fixed (LiquidDrawCache* ptr3 = &_drawCache[0]) {
                fixed (Color* ptr5 = &_waveMask[0]) {
                    LiquidDrawCache* ptr4 = ptr3;
                    Color* ptr6 = ptr5;
                    for (int num18 = 2; num18 < rectangle.Width - 2; num18++) {
                        for (int num19 = 2; num19 < rectangle.Height - 2; num19++) {
                            if (ptr2->HasVisibleLiquid) {
                                float num20 = Math.Min(0.75f, ptr2->VisibleLeftWall);
                                float num21 = Math.Max(0.25f, ptr2->VisibleRightWall);
                                float num22 = Math.Min(0.75f, ptr2->VisibleTopWall);
                                float num23 = Math.Max(0.25f, ptr2->VisibleBottomWall);
                                if (ptr2->IsHalfBrick && ptr2->IsSolid && num23 > 0.5f)
                                    num23 = 0.5f;

                                ptr4->X = num18;
                                ptr4->Y = num19;
                                ptr4->IsVisible = ptr2->HasWall || !ptr2->IsHalfBrick || !ptr2->HasLiquid || !(ptr2->LiquidLevel < 1f);
                                ptr4->SourceRectangle = new Rectangle((int)(16f - num21 * 16f) + ptr2->FrameOffset.X, (int)(16f - num23 * 16f) + ptr2->FrameOffset.Y, (int)Math.Ceiling((num21 - num20) * 16f), (int)Math.Ceiling((num23 - num22) * 16f));
                                ptr4->IsSurfaceLiquid = ptr2->FrameOffset.X == 16 && ptr2->FrameOffset.Y == 0 && (double)(num19 + rectangle.Y) > Main.worldSurface - 40.0;
                                ptr4->Opacity = ptr2->Opacity;
                                ptr4->LiquidOffset = new Vector2((float)Math.Floor(num20 * 16f), (float)Math.Floor(num22 * 16f));
                                ptr4->Type = ptr2->VisibleType;
                                ptr4->HasWall = ptr2->HasWall;
                                byte b = WAVE_MASK_STRENGTH[ptr2->VisibleType];
                                byte g = (ptr6->R = (byte)(b >> 1));
                                ptr6->G = g;
                                ptr6->B = VISCOSITY_MASK[ptr2->VisibleType];
                                ptr6->A = b;
                                LiquidCache* ptr7 = ptr2 - 1;
                                if (num19 != 2 && !ptr7->HasVisibleLiquid && !ptr7->IsSolid && !ptr7->IsHalfBrick)
                                    *(ptr6 - 1) = *ptr6;
                            }
                            else {
                                ptr4->IsVisible = false;
                                int num24 = ((!ptr2->IsSolid && !ptr2->IsHalfBrick) ? 4 : 3);
                                byte b3 = WAVE_MASK_STRENGTH[num24];
                                byte g2 = (ptr6->R = (byte)(b3 >> 1));
                                ptr6->G = g2;
                                ptr6->B = VISCOSITY_MASK[num24];
                                ptr6->A = b3;
                            }

                            ptr2++;
                            ptr4++;
                            ptr6++;
                        }

                        ptr2 += 4;
                    }
                }
            }

            ptr2 = ptr;
            for (int num25 = rectangle.X; num25 < rectangle.X + rectangle.Width; num25++) {
                for (int num26 = rectangle.Y; num26 < rectangle.Y + rectangle.Height; num26++) {
                    if (ptr2->VisibleType == 1 && ptr2->HasVisibleLiquid && Dust.lavaBubbles < 200) {
                        if (_random.Next(700) == 0)
                            Dust.NewDust(new Vector2(num25 * 16, num26 * 16), 16, 16, 35, 0f, 0f, 0, Color.White);

                        if (_random.Next(350) == 0) {
                            int num27 = Dust.NewDust(new Vector2(num25 * 16, num26 * 16), 16, 8, 35, 0f, 0f, 50, Color.White, 1.5f);
                            Main.dust[num27].velocity *= 0.8f;
                            Main.dust[num27].velocity.X *= 2f;
                            Main.dust[num27].velocity.Y -= (float)_random.Next(1, 7) * 0.1f;
                            if (_random.Next(10) == 0)
                                Main.dust[num27].velocity.Y *= _random.Next(2, 5);

                            Main.dust[num27].noGravity = true;
                        }
                    }

                    ptr2++;
                }
            }

            fixed (LiquidDrawCache* ptr8 = &_drawCache[0]) {
                fixed (SpecialLiquidDrawCache* ptr10 = &_drawCacheForShimmer[0]) {
                    LiquidDrawCache* ptr9 = ptr8;
                    SpecialLiquidDrawCache* ptr11 = ptr10;
                    for (int num28 = 2; num28 < rectangle.Width - 2; num28++) {
                        for (int num29 = 2; num29 < rectangle.Height - 2; num29++) {
                            if (ptr9->IsVisible && ptr9->Type == 3) {
                                ptr11->X = num28;
                                ptr11->Y = num29;
                                ptr11->IsVisible = ptr9->IsVisible;
                                ptr11->HasWall = ptr9->HasWall;
                                ptr11->IsSurfaceLiquid = ptr9->IsSurfaceLiquid;
                                ptr11->LiquidOffset = ptr9->LiquidOffset;
                                ptr11->Opacity = ptr9->Opacity;
                                ptr11->SourceRectangle = ptr9->SourceRectangle;
                                ptr11->Type = ptr9->Type;
                                ptr9->IsVisible = false;
                                ptr11++;
                            }

                            ptr9++;
                        }
                    }

                    ptr11->IsVisible = false;
                }
            }
        }

        if (this.WaveFilters != null)
            this.WaveFilters(_waveMask, GetCachedDrawArea());
    }

    public unsafe void DrawNormalLiquids(SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float globalAlpha, bool isBackgroundDraw) {
        Rectangle drawArea = _drawArea;
        Main.tileBatch.Begin();
        fixed (LiquidDrawCache* ptr = &_drawCache[0]) {
            LiquidDrawCache* ptr2 = ptr;
            for (int i = drawArea.X; i < drawArea.X + drawArea.Width; i++) {
                for (int j = drawArea.Y; j < drawArea.Y + drawArea.Height; j++) {
                    if (ptr2->IsVisible) {
                        Rectangle sourceRectangle = ptr2->SourceRectangle;
                        if (ptr2->IsSurfaceLiquid)
                            sourceRectangle.Y = 1280;
                        else
                            sourceRectangle.Y += _animationFrame * 80;

                        Vector2 liquidOffset = ptr2->LiquidOffset;
                        float num = ptr2->Opacity * (isBackgroundDraw ? 1f : DEFAULT_OPACITY[ptr2->Type]);
                        int num2 = ptr2->Type;

                        Lighting.GetCornerColors(i, j, out var vertices);
                        bool tar = false, permafrost = false;
                        if (num2 == 4) {
                            num2 = 15;
                            int num3 = ptr2->X + drawArea.X - 2;
                            int num4 = ptr2->Y + drawArea.Y - 2;
                            SetPermafrostVertexColors(ref vertices, num, num3, num4);
                            permafrost = true;
                        }
                        if (num2 == 5) {
                            num2 = 16;
                            int num3 = ptr2->X + drawArea.X - 2;
                            int num4 = ptr2->Y + drawArea.Y - 2;
                            SetTarVertexColors(ref vertices, num, num3, num4);
                            tar = true;
                        }

                        switch (num2) {
                            case 0:
                                num2 = waterStyle;
                                num *= globalAlpha;
                                break;
                            case 2:
                                num2 = 11;
                                break;
                        }


                        num = Math.Min(1f, num);

                        if (!permafrost && !tar) {
                            vertices.BottomLeftColor *= num;
                            vertices.BottomRightColor *= num;
                            vertices.TopLeftColor *= num;
                            vertices.TopRightColor *= num;
                        }
                        Main.DrawTileInWater(drawOffset, i, j);

                        Main.tileBatch.Draw(_liquidTextures[num2].Value, new Vector2(i << 4, j << 4) + drawOffset + liquidOffset, sourceRectangle, vertices, Vector2.Zero, 1f, SpriteEffects.None);
                    }

                    ptr2++;
                }
            }
        }

        Main.tileBatch.End();
    }

    public unsafe void DrawShimmer(SpriteBatch spriteBatch, Vector2 drawOffset, bool isBackgroundDraw) {
        Rectangle drawArea = _drawArea;
        Main.tileBatch.Begin();
        fixed (SpecialLiquidDrawCache* ptr = &_drawCacheForShimmer[0]) {
            SpecialLiquidDrawCache* ptr2 = ptr;
            int num = _drawCacheForShimmer.Length;
            for (int i = 0; i < num; i++) {
                if (!ptr2->IsVisible)
                    break;

                Rectangle sourceRectangle = ptr2->SourceRectangle;
                if (ptr2->IsSurfaceLiquid)
                    sourceRectangle.Y = 1280;
                else
                    sourceRectangle.Y += _animationFrame * 80;

                Vector2 liquidOffset = ptr2->LiquidOffset;
                float val = ptr2->Opacity * (isBackgroundDraw ? 1f : 0.75f);
                int num2 = 14;
                val = Math.Min(1f, val);
                int num3 = ptr2->X + drawArea.X - 2;
                int num4 = ptr2->Y + drawArea.Y - 2;
                Lighting.GetCornerColors(num3, num4, out var vertices);
                SetShimmerVertexColors(ref vertices, val, num3, num4);
                Main.DrawTileInWater(drawOffset, num3, num4);
                Main.tileBatch.Draw(_liquidTextures[num2].Value, new Vector2(num3 << 4, num4 << 4) + drawOffset + liquidOffset, sourceRectangle, vertices, Vector2.Zero, 1f, SpriteEffects.None);
                sourceRectangle = ptr2->SourceRectangle;
                bool flag = sourceRectangle.X != 16 || sourceRectangle.Y % 80 != 48;
                if (flag || (num3 + num4) % 2 == 0) {
                    sourceRectangle.X += 48;
                    sourceRectangle.Y += 80 * GetShimmerFrame(flag, num3, num4);
                    SetShimmerVertexColors_Sparkle(ref vertices, ptr2->Opacity, num3, num4, flag);
                    Main.tileBatch.Draw(_liquidTextures[num2].Value, new Vector2(num3 << 4, num4 << 4) + drawOffset + liquidOffset, sourceRectangle, vertices, Vector2.Zero, 1f, SpriteEffects.None);
                }

                ptr2++;
            }
        }

        Main.tileBatch.End();
    }

    public static void SetTarVertexColors(ref VertexColors colors, float opacity, int x, int y) {
        colors.BottomLeftColor = Color.White;
        colors.BottomRightColor = Color.White;
        colors.TopLeftColor = Color.White;
        colors.TopRightColor = Color.White;
        colors.BottomLeftColor *= opacity;
        colors.BottomRightColor *= opacity;
        colors.TopLeftColor *= opacity;
        colors.TopRightColor *= opacity;
        colors.BottomLeftColor = new Color(colors.BottomLeftColor.ToVector4() * GetTarBaseColor(x, y + 1));
        colors.BottomRightColor = new Color(colors.BottomRightColor.ToVector4() * GetTarBaseColor(x + 1, y + 1));
        colors.TopLeftColor = new Color(colors.TopLeftColor.ToVector4() * GetTarBaseColor(x, y));
        colors.TopRightColor = new Color(colors.TopRightColor.ToVector4() * GetTarBaseColor(x + 1, y));
    }

    public static void SetPermafrostVertexColors(ref VertexColors colors, float opacity, int x, int y) {
        colors.BottomLeftColor = Color.White;
        colors.BottomRightColor = Color.White;
        colors.TopLeftColor = Color.White;
        colors.TopRightColor = Color.White;
        colors.BottomLeftColor *= opacity;
        colors.BottomRightColor *= opacity;
        colors.TopLeftColor *= opacity;
        colors.TopRightColor *= opacity;
        colors.BottomLeftColor = new Color(colors.BottomLeftColor.ToVector4() * GetPermafrostBaseColor(x, y + 1));
        colors.BottomRightColor = new Color(colors.BottomRightColor.ToVector4() * GetPermafrostBaseColor(x + 1, y + 1));
        colors.TopLeftColor = new Color(colors.TopLeftColor.ToVector4() * GetPermafrostBaseColor(x, y));
        colors.TopRightColor = new Color(colors.TopRightColor.ToVector4() * GetPermafrostBaseColor(x + 1, y));
    }

    public static Vector4 GetTarBaseColor(float worldPositionX, float worldPositionY) {
        float shimmerWave = GetTarWave(ref worldPositionX, ref worldPositionY);
        float brightness = Lighting.Brightness((int)worldPositionX, (int)worldPositionY);
        Vector4 brightColor1 = new(0.3f, 0.3f, 0.3f, 1f),
                brightColor2 = new(1f, 1f, 1f, 1f);
        Vector4 darkColor1 = new(0.2f, 0.2f, 0.2f, 1f),
                darkColor2 = new(0.9f, 0.9f, 0.9f, 1f);
        var output = Vector4.Lerp(Vector4.Lerp(darkColor1, darkColor2, brightness), Vector4.Lerp(brightColor1, brightColor2, brightness), shimmerWave);
        return output * 1f;
    }

    public static Vector4 GetPermafrostBaseColor(float worldPositionX, float worldPositionY) {
        float shimmerWave = GetPermaforstWave(ref worldPositionX, ref worldPositionY);
        var output = Vector4.Lerp(new Vector4(0.25f, 0.9f, 1f, 0.9f), new Vector4(1f, 1f, 1f, 1f), shimmerWave);
        return output * 0.75f;
    }

    public static VertexColors SetShimmerVertexColors_Sparkle(ref VertexColors colors, float opacity, int x, int y, bool top) {
        colors.BottomLeftColor = GetShimmerGlitterColor(top, x, y + 1);
        colors.BottomRightColor = GetShimmerGlitterColor(top, x + 1, y + 1);
        colors.TopLeftColor = GetShimmerGlitterColor(top, x, y);
        colors.TopRightColor = GetShimmerGlitterColor(top, x + 1, y);
        colors.BottomLeftColor *= opacity;
        colors.BottomRightColor *= opacity;
        colors.TopLeftColor *= opacity;
        colors.TopRightColor *= opacity;
        return colors;
    }

    public static void SetShimmerVertexColors(ref VertexColors colors, float opacity, int x, int y) {
        colors.BottomLeftColor = Color.White;
        colors.BottomRightColor = Color.White;
        colors.TopLeftColor = Color.White;
        colors.TopRightColor = Color.White;
        colors.BottomLeftColor *= opacity;
        colors.BottomRightColor *= opacity;
        colors.TopLeftColor *= opacity;
        colors.TopRightColor *= opacity;
        colors.BottomLeftColor = new Color(colors.BottomLeftColor.ToVector4() * GetShimmerBaseColor(x, y + 1));
        colors.BottomRightColor = new Color(colors.BottomRightColor.ToVector4() * GetShimmerBaseColor(x + 1, y + 1));
        colors.TopLeftColor = new Color(colors.TopLeftColor.ToVector4() * GetShimmerBaseColor(x, y));
        colors.TopRightColor = new Color(colors.TopRightColor.ToVector4() * GetShimmerBaseColor(x + 1, y));
    }

    public static float GetShimmerWave(ref float worldPositionX, ref float worldPositionY) => (float)Math.Sin(((double)((worldPositionX + worldPositionY / 6f) / 10f) - Main.timeForVisualEffects / 360.0) * 6.2831854820251465);

    public static float GetTarWave(ref float worldPositionX, ref float worldPositionY)
       => (float)Math.Sin(((double)((Math.Cos(worldPositionX + Main.timeForVisualEffects / 180) + Math.Sin(worldPositionY + Main.timeForVisualEffects / 180))) - Main.timeForVisualEffects / 360) * 6.2831854820251465);

    public static float GetPermaforstWave(ref float worldPositionX, ref float worldPositionY)
       => (float)Math.Sin(((double)((Math.Cos(worldPositionX + Main.timeForVisualEffects / 180) + Math.Sin(worldPositionY + Main.timeForVisualEffects / 180))) - Main.timeForVisualEffects / 180) * 6.2831854820251465);

    public static Color GetShimmerGlitterColor(bool top, float worldPositionX, float worldPositionY) {
        Color color = Main.hslToRgb((float)(((double)(worldPositionX + worldPositionY / 6f) + Main.timeForVisualEffects / 30.0) / 6.0) % 1f, 1f, 0.5f);
        color.A = 0;
        return new Color(color.ToVector4() * GetShimmerGlitterOpacity(top, worldPositionX, worldPositionY));
    }

    public static float GetShimmerGlitterOpacity(bool top, float worldPositionX, float worldPositionY) {
        if (top)
            return 0.5f;

        float num = Utils.Remap((float)Math.Sin(((double)((worldPositionX + worldPositionY / 6f) / 10f) - Main.timeForVisualEffects / 360.0) * 6.2831854820251465), -0.5f, 1f, 0f, 0.35f);
        float num2 = (float)Math.Sin((double)((float)SimpleWhiteNoise((uint)worldPositionX, (uint)worldPositionY) / 10f) + Main.timeForVisualEffects / 180.0);
        return Utils.Remap(num * num2, 0f, 0.5f, 0f, 1f);
    }

    private static uint SimpleWhiteNoise(uint x, uint y) {
        x = 36469 * (x & 0xFFFF) + (x >> 16);
        y = 18012 * (y & 0xFFFF) + (y >> 16);
        return (x << 16) + y;
    }

    public int GetShimmerFrame(bool top, float worldPositionX, float worldPositionY) {
        worldPositionX += 0.5f;
        worldPositionY += 0.5f;
        double num = (double)((worldPositionX + worldPositionY / 6f) / 10f) - Main.timeForVisualEffects / 360.0;
        if (!top)
            num += (double)(worldPositionX + worldPositionY);

        return ((int)num % 16 + 16) % 16;
    }

    public static Vector4 GetShimmerBaseColor(float worldPositionX, float worldPositionY) {
        float shimmerWave = GetShimmerWave(ref worldPositionX, ref worldPositionY);
        return Vector4.Lerp(new Vector4(0.64705884f, 26f / 51f, 14f / 15f, 1f), new Vector4(41f / 51f, 41f / 51f, 1f, 1f), 0.1f + shimmerWave * 0.4f);
    }

    public bool HasFullWater(int x, int y) {
        x -= _drawArea.X;
        y -= _drawArea.Y;
        int num = x * _drawArea.Height + y;
        if (num >= 0 && num < _drawCache.Length) {
            if (_drawCache[num].IsVisible)
                return !_drawCache[num].IsSurfaceLiquid;

            return false;
        }

        return true;
    }

    public float GetVisibleLiquid(int x, int y) {
        x -= _drawArea.X;
        y -= _drawArea.Y;
        if (x < 0 || x >= _drawArea.Width || y < 0 || y >= _drawArea.Height)
            return 0f;

        int num = (x + 2) * (_drawArea.Height + 4) + y + 2;
        if (!_cache[num].HasVisibleLiquid)
            return 0f;

        return _cache[num].VisibleLiquidLevel;
    }

    public void Update(GameTime gameTime) {
        if (!Main.gamePaused && Main.hasFocus) {
            float num = Main.windSpeedCurrent * 25f;
            num = ((!(num < 0f)) ? (num + 6f) : (num - 6f));
            _frameState += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_frameState < 0f)
                _frameState += 16f;

            _frameState %= 16f;
            _animationFrame = (int)_frameState;
        }
    }

    public void PrepareDraw(Rectangle drawArea) {
        InternalPrepareDraw(drawArea);
    }

    public void SetWaveMaskData(ref Texture2D texture) {
        try {
            if (texture == null || texture.Width < _drawArea.Height || texture.Height < _drawArea.Width) {
                Console.WriteLine("WaveMaskData texture recreated. {0}x{1}", _drawArea.Height, _drawArea.Width);
                if (texture != null) {
                    try {
                        texture.Dispose();
                    }
                    catch {
                    }
                }

                texture = new Texture2D(Main.instance.GraphicsDevice, _drawArea.Height, _drawArea.Width, mipMap: false, SurfaceFormat.Color);
            }

            texture.SetData(0, new Rectangle(0, 0, _drawArea.Height, _drawArea.Width), _waveMask, 0, _drawArea.Width * _drawArea.Height);
        }
        catch {
            texture = new Texture2D(Main.instance.GraphicsDevice, _drawArea.Height, _drawArea.Width, mipMap: false, SurfaceFormat.Color);
            texture.SetData(0, new Rectangle(0, 0, _drawArea.Height, _drawArea.Width), _waveMask, 0, _drawArea.Width * _drawArea.Height);
        }
    }

    public Rectangle GetCachedDrawArea() => _drawArea;
}
