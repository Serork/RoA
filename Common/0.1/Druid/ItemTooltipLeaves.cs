using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.InterfaceElements;
using RoA.Common.UI;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed class ItemTooltipLeaves : GlobalItem {
    public const byte LEAVESCOUNT = 6;

    private static SpriteData _leavesSpriteData;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _leavesSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.UITextures + "ClassUI_Nature"), new SpriteFrame(3, 1));
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
        if (!item.IsNature()) {
            TooltipFallingLeaves.ResetData();

            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        bool isNameLine = line.Name.Contains("Name");
        if (isNameLine) {
            void drawTooltipLineLeaves(DrawableTooltipLine line, byte leavesCount, ulong seedForRandomness) {
                string text = line.Text;
                Vector2 size = line.Font.MeasureString(text);

                SpriteBatch batch = Main.spriteBatch;
                SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
                batch.BeginBlendState(snapshot.blendState, snapshot.samplerState, isUI: true);

                for (int i = 0; i < leavesCount; i++) {
                    int mid = leavesCount / 2;
                    bool inFirstHalf = i < mid;
                    int inversed = i - mid;
                    bool isFirstOrLast = i == 0 || i == mid - 1 || i == mid || i == leavesCount - 1;

                    int direction = inFirstHalf ? 1 : -1;

                    Vector2 originalSpot = new(line.X, line.Y),
                            position = originalSpot;

                    position.X += inFirstHalf ? FontAssets.MouseText.Value.MeasureString(line.Text).X : 0f;
                    float offset = 3f;
                    position.X += isFirstOrLast ? offset * -direction : offset * direction;
                    float indent = 4.25f, interval = size.Y / mid;
                    position.Y -= size.Y / 8f;
                    position.Y += indent + interval * (inFirstHalf ? i : inversed);

                    int current = (inFirstHalf ? i : i - mid) + 1;
                    double counter = TimeSystem.TimeForVisualEffects * 0.5 + (double)Utils.RandomFloat(ref seedForRandomness);
                    double factor = counter * MathHelper.TwoPi + (double)(float)counter * 0.5f + (float)(counter / 100.0) * 0.5f;
                    float multiplier = (float)Math.Cos(factor) * 0.5f * Utils.GetLerpValue(0.015f * current, 0.225f * current, (float)Math.Abs(counter), clamped: true);

                    float baseRotation = 0.3f * (inFirstHalf ? -i : inversed),
                          extraRotation = 0.5f * multiplier * direction,
                          rotation = baseRotation * -i + extraRotation;

                    _leavesSpriteData.Effects = (SpriteEffects)inFirstHalf.ToInt();
                    _leavesSpriteData.VisualPosition = position - _leavesSpriteData.Origin;
                    _leavesSpriteData = _leavesSpriteData.Framed((byte)Utils.RandomInt(ref seedForRandomness, 3), 0);
                    _leavesSpriteData.Rotation = rotation;

                    TooltipFallingLeaves.FallingLeafData data;
                    data.Index = (byte)i;
                    data.SpriteInfo = _leavesSpriteData;
                    TooltipFallingLeaves.MatchData(data);

                    _leavesSpriteData.DrawSelf();
                }

                batch.End();
                batch.Begin(snapshot);
            }

            if (DamageClassVisualsInItemName.CanDrawClassUIVisuals) {
                ulong seed = (ulong)item.type;
                drawTooltipLineLeaves(line, LEAVESCOUNT, seed);
            }
        }

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }
}
