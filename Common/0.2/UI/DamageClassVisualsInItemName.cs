using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.InterfaceElements;
using RoA.Content;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

using static RoA.Common.UI.DamageClassVisualsInItemName;

namespace RoA.Common.UI;

sealed class DamageClassVisualsInItemName : GlobalItem {
    private static byte DAMAGECLASSAMOUNT => (byte)DamageClassType.Count;

    public readonly struct DamageClassNameVisualsInfo(DamageClassType damageClassType, Texture2D texture, ushort itemType, Vector2 tooltipLinePosition, Vector2 tooltipLineSize) {
        public readonly DamageClassType DamageClassType = damageClassType;
        public readonly Texture2D Texture = texture;
        public readonly ushort ItemType = itemType;
        public readonly Vector2 TooltipLinePosition = tooltipLinePosition;
        public readonly Vector2 TooltipLineSize = tooltipLineSize;
    }

    private static readonly Dictionary<DamageClassType, Asset<Texture2D>?> _classTexturesMap = [];

    private static DamageClassType _previousDamageClassTypeDraw;
    private static float _opacityUpdatedInDraws;

    internal static SpriteBatch UsedBatch => Main.spriteBatch;

    internal static float VisualTimer;
    internal static float VisualRotation, VisualOpacity;
    
    internal static float OpacityUpdatedInDraws {
        get => _opacityUpdatedInDraws;
        set => _opacityUpdatedInDraws = MathUtils.Clamp01(value);
    }

    public static float AfterMainDrawOpacity => 1f - MathUtils.Clamp01(VisualOpacity);
    public static float AfterMainDrawRotation => Ease.CircOut(MathHelper.WrapAngle(VisualRotation));

    public override void Load() {
        LoadClassUITextures();
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
        bool isNameLine = line.Name.Contains("Name");
        if (!isNameLine) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }   

        bool isItemValidToBeHandledForDamageClass(Item item, out DamageClassType? damageClassType) {
            damageClassType = null;
            if (item.IsAWeapon()) {
                if (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed) {
                    damageClassType = DamageClassType.Melee;
                }
                else if (item.DamageType == DamageClass.Ranged) {
                    damageClassType = DamageClassType.Ranged;
                }
                else if (item.DamageType == DamageClass.Magic) {
                    damageClassType = DamageClassType.Magic;
                }
                else if (item.DamageType == DamageClass.Summon || item.DamageType == DamageClass.SummonMeleeSpeed) {
                    damageClassType = DamageClassType.Summon;
                }
                else if (item.DamageType == DruidClass.Nature) {
                    damageClassType = DamageClassType.Nature;
                }
            }

            return damageClassType != null;
        }
        string tooltipLineText = line.Text;
        Vector2 tooltipLineSize = line.Font.MeasureString(tooltipLineText);
        ushort itemType = (ushort)item.type;
        Vector2 originalSpot = new(line.X, line.Y);
        void resetGlobalClassVisualInfo() {
            VisualTimer += 1f;
            VisualRotation = 0f;
            VisualOpacity = 0f;
        }

        if (!isItemValidToBeHandledForDamageClass(item, out DamageClassType? checkDamageClassTypeOfThisItem)) {
            DamageClassVisualsInItemNameAfterTooltipDrawing.ResetData();

            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        resetGlobalClassVisualInfo();

        DamageClassVisualsInItemNameAfterTooltipDrawing.ResetData();

        Asset<Texture2D>? classUIAsset = _classTexturesMap[checkDamageClassTypeOfThisItem!.Value];
        if (classUIAsset?.IsLoaded != true) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        if (checkDamageClassTypeOfThisItem != _previousDamageClassTypeDraw) {
            OpacityUpdatedInDraws = 0f;
            VisualTimer = 0f;
        }

        Texture2D classUITexture = classUIAsset.Value;
        DamageClassType damageClassTypeOfThisItem = checkDamageClassTypeOfThisItem.Value;
        DamageClassNameVisualsInfo damageClassVisualsInfo = new(damageClassTypeOfThisItem, classUITexture, itemType, originalSpot, tooltipLineSize);
        SpriteBatch batch = UsedBatch;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
        batch.BeginBlendState(BlendState.AlphaBlend, SamplerState.AnisotropicClamp, isUI: true);
        switch (damageClassTypeOfThisItem) {
            case DamageClassType.Melee:
                DrawSwords(batch, damageClassVisualsInfo);
                break;
            case DamageClassType.Ranged:
                DrawArrows(batch, damageClassVisualsInfo);
                break;
            case DamageClassType.Magic:
                DrawStars(batch, damageClassVisualsInfo);
                break;
        }
        DamageClassVisualsInItemNameAfterTooltipDrawing.MatchData(damageClassVisualsInfo);
        batch.End();
        batch.Begin(snapshot);

        DamageClassVisualsInItemNameAfterTooltipDrawing.IsHoveringItem = true;

        _previousDamageClassTypeDraw = damageClassTypeOfThisItem;

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    private static void LoadClassUITextures() {
        if (Main.dedServ) {
            return;
        }

        for (int i = 0; i < DAMAGECLASSAMOUNT; i++) {
            DamageClassType damageClassType = (DamageClassType)i;
            _classTexturesMap[damageClassType] = ModContent.Request<Texture2D>(ResourceManager.UITextures + $"ClassUI_{damageClassType}");
        }
    }

    public static void DrawStars(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        if (damageClassNameVisualsInfo.DamageClassType != DamageClassType.Magic) {
            return;
        }

        const byte STARCOUNT = 2;
        OpacityUpdatedInDraws += 0.035f;
        Texture2D starTexture = damageClassNameVisualsInfo.Texture;
        for (byte i = 0; i < STARCOUNT; i++) {
            byte half = STARCOUNT / 2;
            bool firstPair = i < half;
            int width = starTexture.Width,
                height = starTexture.Height;
            float starRotation = 0f;
            Vector2 starPositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            if (!firstPair) {
                starPositionToDraw += new Vector2(tooltipLineSize.X, 0f);
            }
            float starDirection = firstPair.ToDirectionInt();
            starPositionToDraw.X += 1f;
            starPositionToDraw.Y += height / 2f;
            starPositionToDraw.Y -= 2f;
            float factor = MathHelper.WrapAngle(VisualTimer / 20f % MathHelper.TwoPi + GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType));
            float starProgress = 1f - OpacityUpdatedInDraws;
            ulong seedForRandomess = damageClassNameVisualsInfo.ItemType;
            Vector2 tooltipCenter = damageClassNameVisualsInfo.TooltipLinePosition + tooltipLineSize / 2f;
            float randomValueForX = 1f - Utils.RandomFloat(ref seedForRandomess) + 0.5f;
            randomValueForX *= starDirection;
            Vector2 fallStartPosition = tooltipCenter + Vector2.UnitX * 100f * randomValueForX - Vector2.UnitY * 0f;
            Vector2 starAngle = fallStartPosition.DirectionTo(starPositionToDraw).SafeNormalize();
            Vector2 starVelocity = -starAngle.RotatedBy(starRotation);
            float sin = MathF.Sin(factor);
            //starRotation = MathHelper.TwoPi * 0.25f * starProgress * starDirection;
            //starProgress = Ease.QuartIn(starProgress);
            //starPositionToDraw += starVelocity * 100f * starProgress;
            Rectangle starSourceRectangle = starTexture.Bounds;
            Color starColor = Color.White * AfterMainDrawOpacity;
            SpriteEffects flipStarDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 starOrigin = starTexture.Size() / 2f;
            starRotation += 0.2f * starDirection * sin;
            if (AfterMainDrawOpacity == 1f) {
                batch.Draw(starTexture, starPositionToDraw, DrawInfo.Default with {
                    Color = starColor,
                    Rotation = starRotation,
                    Origin = starOrigin,
                    ImageFlip = flipStarDrawing,
                    Clip = starSourceRectangle
                }, false);
                batch.Draw(starTexture, starPositionToDraw, DrawInfo.Default with {
                    Color = starColor.MultiplyAlpha(1f - 0.5f * OpacityUpdatedInDraws + Math.Abs(0.5f * sin) * OpacityUpdatedInDraws),
                    Rotation = starRotation,
                    Origin = starOrigin,
                    ImageFlip = flipStarDrawing,
                    Clip = starSourceRectangle
                }, false);
            }
        }
    }

    public static void DrawSwords(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        if (damageClassNameVisualsInfo.DamageClassType != DamageClassType.Melee) {
            return;
        }

        const byte SWORDCOUNT = 4;
        Texture2D swordTexture = damageClassNameVisualsInfo.Texture;
        for (byte i = 0; i < SWORDCOUNT; i++) {
            bool firstPair = i < SWORDCOUNT / 2;
            bool topSwords = (i + 1) % 2 != 0;
            int width = swordTexture.Width,
                height = swordTexture.Height;
            Vector2 swordPositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            switch (i) {
                case 0:
                    swordPositionToDraw += new Vector2(-width, -height);
                    break;
                case 1:
                    swordPositionToDraw += new Vector2(-width, height / 2f);
                    break;
                case 2:
                    swordPositionToDraw += new Vector2(tooltipLineSize.X + width, -height);
                    break;
                case 3:
                    swordPositionToDraw += new Vector2(tooltipLineSize.X + width, height / 2f);
                    break;
            }
            swordPositionToDraw.X += 1f;
            swordPositionToDraw.Y += height / 1.25f;
            int topDirection = topSwords.ToDirectionInt(),
                pairDirection = firstPair.ToDirectionInt();
            swordPositionToDraw.Y += topDirection * 8f;
            float swordExtraRotationDirection = pairDirection * topDirection * -1f;
            float swordRotationBase = 0f * swordExtraRotationDirection;
            float swordRotation = swordRotationBase;
            if (topSwords) {
                swordRotation = swordRotationBase + MathHelper.PiOver2 * pairDirection;
            }
            ulong seedForRandomness = (ulong)(i + 1);
            float factor = MathHelper.WrapAngle(VisualTimer / 35f % MathHelper.TwoPi + (VisualRotation <= 0f ? GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType) : 0f));
            float penalty = 1f - Utils.GetLerpValue(MathHelper.PiOver4, MathHelper.Pi, factor, true);
            factor *= penalty;
            float swordExtraRotation = MathF.Sin(factor + Utils.RandomFloat(ref seedForRandomness));
            swordExtraRotation = Ease.ExpoIn(Ease.QuadOut(swordExtraRotation));
            swordExtraRotation = MathUtils.Clamp01(swordExtraRotation);
            swordExtraRotation *= swordExtraRotationDirection;
            swordExtraRotation *= MathHelper.PiOver2;
            swordExtraRotation += AfterMainDrawRotation * swordExtraRotationDirection;
            swordRotation += swordExtraRotation;
            Rectangle swordSourceRectangle = swordTexture.Bounds;
            Color swordColor = Color.White * AfterMainDrawOpacity;
            SpriteEffects flipSwordDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 swordOrigin = new(firstPair ? 4f : width - 4f, height - 4f);
            batch.Draw(swordTexture, swordPositionToDraw, DrawInfo.Default with {
                Color = swordColor,
                Rotation = swordRotation,
                Origin = swordOrigin,
                ImageFlip = flipSwordDrawing,
                Clip = swordSourceRectangle
            }, false);
        }
    }

    public static int GetRandomIntBasedOnItemType(ushort itemType) {
        ulong seedForRandomness = itemType;
        return Utils.RandomInt(ref seedForRandomness, 100);
    }

    public static void DrawArrows(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        if (damageClassNameVisualsInfo.DamageClassType != DamageClassType.Ranged) {
            return;
        }

        const byte ARROWCOUNT = 4;
        Texture2D arrowTexture = damageClassNameVisualsInfo.Texture;
        OpacityUpdatedInDraws += 0.1f;
        for (byte i = 0; i < ARROWCOUNT; i++) {
            byte half = ARROWCOUNT / 2;
            byte nextArrowIndex = (byte)(i + 1);
            bool firstPair = i < half;
            bool rightArrows = nextArrowIndex > half;
            int width = arrowTexture.Width,
                height = arrowTexture.Height;
            Vector2 arrowPositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            if (rightArrows) {
                arrowPositionToDraw += new Vector2(tooltipLineSize.X, 0f);
            }
            arrowPositionToDraw.X += 1f;
            arrowPositionToDraw.Y += height / 2f;
            arrowPositionToDraw.Y -= 4f;
            float arrowRotation = MathHelper.PiOver2;
            if (!firstPair) {
                arrowRotation -= MathHelper.Pi;
            }
            int checkIndex = nextArrowIndex % half;
            float offsetYValue = 2f;
            float arrowOffsetY = offsetYValue * (checkIndex % 2 == 0).ToDirectionInt() + offsetYValue * (checkIndex % 3 == 0).ToDirectionInt();
            arrowPositionToDraw.Y += arrowOffsetY;
            int rightDirection = (!firstPair).ToDirectionInt();
            arrowRotation += arrowOffsetY * 0.05f * rightDirection;
            ushort itemType = damageClassNameVisualsInfo.ItemType;
            ulong seedForRandomness = itemType,
                  seedForRandomness2 = nextArrowIndex;
            float factor = MathHelper.WrapAngle(VisualTimer / 30f + Utils.RandomInt(ref seedForRandomness, 100));
            float arrowExtraRotation = MathF.Sin(factor + Utils.RandomFloat(ref seedForRandomness) + Utils.RandomInt(ref seedForRandomness2, 100));
            float arrowProgress = 1f - OpacityUpdatedInDraws;
            arrowExtraRotation *= MathF.Max(0.25f, arrowProgress) * 1f;
            arrowExtraRotation *= rightDirection;
            arrowExtraRotation *= 0.2f;
            arrowExtraRotation *= (nextArrowIndex % 2 == 0).ToDirectionInt();
            arrowRotation += arrowExtraRotation;
            Vector2 arrowVelocity = Vector2.UnitY.RotatedBy(arrowRotation);
            arrowPositionToDraw += arrowVelocity * 50f * Ease.QuadIn(arrowProgress);
            arrowPositionToDraw += -arrowVelocity * Ease.QuadOut(MathUtils.Clamp01(VisualRotation / 60f)) * 100f;
            Rectangle arrowSourceRectangle = arrowTexture.Bounds;
            Color arrowColor = Color.White * AfterMainDrawOpacity;
            SpriteEffects flipArrowDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 arrowOrigin = new(arrowTexture.Width / 2f, 0f);
            batch.Draw(arrowTexture, arrowPositionToDraw, DrawInfo.Default with {
                Color = arrowColor,
                Rotation = arrowRotation,
                Origin = arrowOrigin,
                ImageFlip = flipArrowDrawing,
                Clip = arrowSourceRectangle
            }, false);
        }
    }
}

sealed class DamageClassVisualsInItemNameAfterTooltipDrawing() : InterfaceElement(RoA.ModName + ": Damage Class Tooltip UI", InterfaceScaleType.UI) {
    private static float CANPLAYINANIMATIONAGAINFOR => 0f;

    private static bool _shouldDraw;
    private static DamageClassNameVisualsInfo _damageClassNameVisualsInfo;
    private static float _canResetGlobalVisualInfoTimer;

    internal static bool IsHoveringItem;

    public static void ResetData() => _shouldDraw = false;

    public static void MatchData(in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        _shouldDraw = true;

        _damageClassNameVisualsInfo = damageClassNameVisualsInfo;
    }

    protected override bool DrawSelf() {
        void resetGlobalClassVisualInfo() {
            if (_canResetGlobalVisualInfoTimer-- <= 0f) {
                OpacityUpdatedInDraws = 0f;
            }
        }
        void updateGlobalClassVisualsInfo() {
            OpacityUpdatedInDraws = 1f;
            if (_damageClassNameVisualsInfo.DamageClassType == DamageClassType.Melee) {
                VisualTimer = 80f;
                VisualRotation += 0.025f;
                VisualOpacity = Ease.SineIn(Utils.GetLerpValue(0.4f, 1.25f, VisualRotation, true));
            }
            else if (_damageClassNameVisualsInfo.DamageClassType == DamageClassType.Ranged) {
                VisualRotation += 1.5f;
                VisualOpacity = Ease.SineIn(Utils.GetLerpValue(40f, 85f, VisualRotation, true));
            }
            else if (_damageClassNameVisualsInfo.DamageClassType == DamageClassType.Magic) {
                VisualOpacity = 1f;
            }
        }

        if (!IsHoveringItem) {
            resetGlobalClassVisualInfo();
        }
        else {
            _canResetGlobalVisualInfoTimer = CANPLAYINANIMATIONAGAINFOR;
        }
        IsHoveringItem = false;

        if (!_shouldDraw || Main.mouseItem.IsAir) {
            return true;
        }

        updateGlobalClassVisualsInfo();

        DrawSwords(UsedBatch, _damageClassNameVisualsInfo);
        DrawArrows(UsedBatch, _damageClassNameVisualsInfo);
        DrawStars(UsedBatch, _damageClassNameVisualsInfo);

        return true;
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }
}
