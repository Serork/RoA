using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.Common.GUI;

[Autoload(Side = ModSide.Client)]
public sealed class DruidItemTooltipLeafs : GlobalItem {
    private const byte LEAFS_COUNT = 8;

    private static SpriteData _leafsSpriteData;

    public override void SetStaticDefaults() {
        _leafsSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.GUITextures + "Leafs"), new SpriteFrame(3, 1));
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
        if (!item.IsDruidic()) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        bool isNameLine = line.Name.Contains("Name");
        if (isNameLine) {
            void drawTooltipLineLeafs(DrawableTooltipLine line, byte leafsCount, ulong seedForRandomness) {
                string text = line.Text;
                Vector2 size = line.Font.MeasureString(text);

                for (byte i = 0; i < leafsCount; i++) {
                    int mid = leafsCount / 2;
                    bool inFirstHalf = i < mid;
                    int inversed = i - mid;
                    bool isFirstOrLast = i == 0 || i == mid - 1 || i == mid || i == leafsCount - 1;

                    int direction = inFirstHalf ? 1 : -1;

                    Vector2 originalSpot = new(line.OriginalX, line.OriginalY),
                            position = originalSpot;

                    position.X += inFirstHalf ? FontAssets.MouseText.Value.MeasureString(line.Text).X : 0f;
                    float offset = 3f;
                    position.X += isFirstOrLast ? offset * -direction : offset * direction;
                    float indent = 4.25f, interval = size.Y / mid;
                    position.Y -= size.Y / 8f;
                    position.Y += indent + interval * (inFirstHalf ? i : inversed);

                    int current = (inFirstHalf ? i : i - mid) + 1;
                    double counter = TimeSystem.TimeForVisualEffects * 0.5 + (double)Utils.RandomFloat(ref seedForRandomness);
                    double f = counter * MathHelper.TwoPi + (double)(float)counter * 0.5f + (float)(counter / 100.0) * 0.5f;
                    float multiplier = (float)Math.Cos(f) * 0.5f * Utils.GetLerpValue(0.015f * current, 0.225f * current, (float)Math.Abs(counter), clamped: true);

                    float baseRotation = 0.3f * (inFirstHalf ? -i : inversed),
                          extraRotation = 0.5f * multiplier * direction,
                          rotation = baseRotation * -i + extraRotation;

                    _leafsSpriteData.VisualPosition = position - _leafsSpriteData.Origin;
                    _leafsSpriteData.Framed((byte)Utils.RandomInt(ref seedForRandomness, 3), 0);
                    _leafsSpriteData.Rotation = rotation;

                    Main.spriteBatch.With(BlendState.AlphaBlend, true, () => {
                        _leafsSpriteData.DrawSelf();
                    }, SamplerState.PointClamp);

                    DruidTooltipFallingLeafsVisualSystem.FallingLeafData data;
                    data.Index = i;
                    data.SpriteInfo = _leafsSpriteData;
                    DruidTooltipFallingLeafsVisualSystem.MatchData(data);
                }
            }

            drawTooltipLineLeafs(line, LEAFS_COUNT, (ulong)item.type);
        }

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    private sealed class DruidTooltipFallingLeafsVisualSystem : ModSystem {
        internal struct FallingLeafData {
            public byte Index;
            public SpriteData SpriteInfo;
        }

        const float DELAY = 0f;

        static float _counter;
        static FallingLeafData[] _fallingLeafs;
        static LegacyGameInterfaceLayer _layer;

        static float CounterMax => 1f + DELAY;

        public override void Load() {
            _fallingLeafs = new FallingLeafData[LEAFS_COUNT];

            _layer = new LegacyGameInterfaceLayer($"{RoA.ModName}: Falling Leafs", FallingLeafsLayer);
        }

        public override void Unload() {
            _fallingLeafs = null;

            _layer = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            bool isLayerLoaded = _layer != null;
            if (!isLayerLoaded) {
                return;
            }

            int preferredIndex = layers.FindIndex(l => l.Name == "Vanilla: Cursor");
            layers.Insert(preferredIndex < 0 ? 0 : preferredIndex, _layer);
        }

        internal static void MatchData(FallingLeafData data) {
            _counter = CounterMax;

            _fallingLeafs[data.Index] = data;
        }

        private static bool FallingLeafsLayer() {
            Helper.AddClamp(ref _counter, -TimeSystem.LogicDeltaTime, 0f, CounterMax);

            float maxY = 28f;

            foreach (FallingLeafData fallingLeafData in _fallingLeafs) {
                float counter = Math.Clamp(_counter, 0f, 1f);

                float maxX = 5f * Ease.SineOut(counter), speedX = 2.5f;
                float offsetY = 30f;
                Vector2 velocity = new(Helper.Wave(-maxX, maxX, speedX, (fallingLeafData.Index + 1) * LEAFS_COUNT), offsetY * Ease.QuadOut(counter));
                float oscMultiplier = (1f - counter) * 5f;
                velocity.X *= oscMultiplier;

                float alpha = Main.mouseItem.IsAir ? 0f : Utils.GetLerpValue(0f, 0.3f, Ease.SineIn(counter));

                float velocityAffectedExtraRotation = velocity.X * 0.05f;

                Main.spriteBatch.With(BlendState.AlphaBlend, true, () => {
                    SpriteData spriteInfo = fallingLeafData.SpriteInfo;
                    spriteInfo.VisualPosition += Vector2.UnitY * maxY - velocity;
                    spriteInfo.Color *= alpha;
                    spriteInfo.Rotation += velocityAffectedExtraRotation;
                    spriteInfo.DrawSelf();
                }, SamplerState.PointClamp);
            }

            return true;
        }
    }
}
