using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.Configs;
using RoA.Common.InterfaceElements;
using RoA.Content;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
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
    private static float _mainDrawTimer;
    private static float _postMainDrawTimer, _postMainDrawOpacityValue;

    public static float OpacityUpdatedInDraws {
        get => _opacityUpdatedInDraws;
        internal set => _opacityUpdatedInDraws = MathUtils.Clamp01(value);
    }
    public static float PostMainDrawOpacity => 1f - MathUtils.Clamp01(_postMainDrawOpacityValue);

    public static bool CanDrawClassUIVisuals => Main.gameMenu || Main.InGameUI.IsVisible ? BooleanElement.Value2 : ModContent.GetInstance<RoAClientConfig>().ClassUIVisuals;

    public override void Load() {
        LoadClassUITextures();
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
        if (!CanDrawClassUIVisuals) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

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
        float sizeOffsetModifier = 0.025f;
        tooltipLineSize.X *= 1f - sizeOffsetModifier * 2.25f;
        ushort itemType = (ushort)item.type;
        Vector2 originalSpot = new(line.X + tooltipLineSize.X * sizeOffsetModifier, line.Y);
        originalSpot.X += 1f;
        void resetGlobalClassVisualInfo() {
            _mainDrawTimer += 1f;
            _postMainDrawTimer = 0f;
            _postMainDrawOpacityValue = 0f;
        }

        if (!isItemValidToBeHandledForDamageClass(item, out DamageClassType? checkDamageClassTypeOfThisItem)) {
            DamageClassVisualsInItemNamePostTooltipDrawing.ResetData();

            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        resetGlobalClassVisualInfo();

        DamageClassVisualsInItemNamePostTooltipDrawing.ResetData();

        Asset<Texture2D>? classUIAsset = _classTexturesMap[checkDamageClassTypeOfThisItem!.Value];
        if (classUIAsset?.IsLoaded != true) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        DamageClassType damageClassTypeOfThisItem = checkDamageClassTypeOfThisItem.Value;
        if (checkDamageClassTypeOfThisItem != _previousDamageClassTypeDraw) {
            OpacityUpdatedInDraws = 0f;

            if (NeedMainTimerReset(damageClassTypeOfThisItem)) {
                _mainDrawTimer = 0f;
            }
        }

        Texture2D classUITexture = classUIAsset.Value;
        DrawClassUIVisuals(new(damageClassTypeOfThisItem, classUITexture, itemType, originalSpot, tooltipLineSize));
        DamageClassVisualsInItemNamePostTooltipDrawing.IsHoveringItem = true;

        _previousDamageClassTypeDraw = damageClassTypeOfThisItem;

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    public static bool NeedMainTimerReset(DamageClassType damageClassType) => damageClassType != DamageClassType.Summon;

    public static void UpdateGlobalClassVisualsInfo(DamageClassType damageClassType) {
        OpacityUpdatedInDraws = 1f;
        if (damageClassType == DamageClassType.Melee) {
            _postMainDrawTimer += 0.0275f;
            _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, 1.35f, _postMainDrawTimer, true));
        }
        else if (damageClassType == DamageClassType.Ranged) {
            _postMainDrawTimer += 1.5f;
            _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(40f, 85f, _postMainDrawTimer, true));
        }
        else if (damageClassType == DamageClassType.Magic) {
            _postMainDrawTimer += 0.0275f;
            _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, 1.35f, _postMainDrawTimer, true));
        }
        if (damageClassType == DamageClassType.Summon) {
            _mainDrawTimer += 1f;
            _postMainDrawTimer += 0.0275f;
            _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, 1.35f, _postMainDrawTimer, true));
        }
    }

    public static void DrawClassUIVisuals(in DamageClassNameVisualsInfo damageClassVisualsInfo) {
        DamageClassType damageClassTypeOfThisItem = damageClassVisualsInfo.DamageClassType;
        SpriteBatch batch = Main.spriteBatch;
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
            case DamageClassType.Summon:
                DrawSlimes(batch, damageClassVisualsInfo);
                break;
        }
        DamageClassVisualsInItemNamePostTooltipDrawing.MatchData(damageClassVisualsInfo);
        batch.End();
        batch.Begin(snapshot);
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

    public static void DrawSlimes(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte SLIMECOUNT = 2;
        const byte SLIMEFRAMECOUNT = 3;
        Texture2D slimeTexture = damageClassNameVisualsInfo.Texture;
        for (byte i = 0; i < SLIMECOUNT; i++) {
            byte half = SLIMECOUNT / 2;
            bool firstPair = i < half;
            int slimeHeight = slimeTexture.Height;
            float slimeRotation = 0f;
            Vector2 slimePositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            float offsetX = tooltipLineSize.X * 0.1f;
            int randomValue = GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType);
            float factor = MathHelper.WrapAngle(_mainDrawTimer / 20f % MathHelper.TwoPi + randomValue);
            ulong seedForRandomness = (ulong)((i + 1) * randomValue);
            float randomValue2 = Utils.RandomFloat(ref seedForRandomness) * 100f;
            seedForRandomness = (ulong)randomValue;
            float randomValue4 = Utils.RandomFloat(ref seedForRandomness) * 100f;
            float frameFactor = 1f - Math.Abs(MathF.Sin(factor + randomValue2 * MathHelper.Pi));
            Color starColor = Color.White;
            float randomValue3 = randomValue / 100f;
            float slimeColor = randomValue3;
            starColor = Color.Lerp(starColor, new Color(slimeColor, slimeColor, slimeColor), randomValue2);
            if (!firstPair) {
                slimePositionToDraw += new Vector2(tooltipLineSize.X + offsetX, 0f);
            }
            else {
                slimePositionToDraw -= Vector2.UnitX * offsetX;
            }
            slimePositionToDraw.Y += slimeHeight / 2f;
            SpriteFrame spriteFrame = new(SLIMEFRAMECOUNT, 1);
            spriteFrame = spriteFrame.With((byte)(SLIMEFRAMECOUNT * frameFactor), 0);
            SpriteEffects flipStarDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            starColor *= PostMainDrawOpacity;
            float jumpHeight = 10f + 30f * randomValue4;
            Vector2 bezierPoint1 = slimePositionToDraw, bezierPoint2 = bezierPoint1 - Vector2.UnitY * jumpHeight, bezierPoint3 = bezierPoint1 + Vector2.UnitY * jumpHeight * 0.5f;
            float bezierFactor = 1f - _postMainDrawTimer;
            Rectangle starSourceRectangle = spriteFrame.GetSourceRectangle(slimeTexture);
            Vector2 slimeOrigin = starSourceRectangle.Size() / 2f;
            Vector2 positionToDraw = MathF.Pow(1 - bezierFactor, 2) * bezierPoint1 + 2 * (1 - bezierFactor) * bezierFactor * bezierPoint2 + MathF.Pow(bezierFactor, 2) * bezierPoint3;
            slimePositionToDraw = positionToDraw;
            slimePositionToDraw -= Vector2.UnitY * jumpHeight * 0.5f;
            batch.Draw(slimeTexture, slimePositionToDraw, DrawInfo.Default with {
                Color = starColor,
                Rotation = slimeRotation,
                Origin = slimeOrigin,
                ImageFlip = flipStarDrawing,
                Clip = starSourceRectangle
            }, false);
        }
    }

    public static void DrawStars(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte STARCOUNT = 2;
        OpacityUpdatedInDraws += 0.035f;
        Texture2D starTexture = damageClassNameVisualsInfo.Texture;
        for (byte i = 0; i < STARCOUNT; i++) {
            byte half = STARCOUNT / 2;
            bool firstPair = i < half;
            int starHeight = starTexture.Height;
            float starRotation = 0f;
            Vector2 starPositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            float offsetX = tooltipLineSize.X * 0.07f;
            if (!firstPair) {
                starPositionToDraw += new Vector2(tooltipLineSize.X + offsetX, 0f);
            }
            else {
                starPositionToDraw -= Vector2.UnitX * offsetX;
            }
            float starDirection = firstPair.ToDirectionInt();
            starPositionToDraw.Y += starHeight / 2f;
            starPositionToDraw.Y -= 2f;
            float factor = MathHelper.WrapAngle(_mainDrawTimer / 20f % MathHelper.TwoPi + GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType));
            float starProgress = 1f - OpacityUpdatedInDraws;
            float sin = MathF.Sin(factor);
            Rectangle starSourceRectangle = starTexture.Bounds;
            Color starColor = Color.White;
            SpriteEffects flipStarDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 starOrigin = starTexture.Size() / 2f;
            float extraScale = 0.2f * starDirection * sin;
            starColor *= PostMainDrawOpacity;
            Vector2 starScale = Vector2.One + Vector2.One * extraScale;
            float pulseEffectStrength = 1f - 0.5f * OpacityUpdatedInDraws + Math.Abs(0.5f * sin) * OpacityUpdatedInDraws;
            float alphaProgress = Ease.QuartIn(PostMainDrawOpacity);
            batch.Draw(starTexture, starPositionToDraw, DrawInfo.Default with {
                Color = starColor,
                Rotation = starRotation,
                Origin = starOrigin,
                ImageFlip = flipStarDrawing,
                Clip = starSourceRectangle,
                Scale = starScale
            }, false);
            batch.Draw(starTexture, starPositionToDraw, DrawInfo.Default with {
                Color = starColor.MultiplyAlpha(pulseEffectStrength),
                Rotation = starRotation,
                Origin = starOrigin,
                ImageFlip = flipStarDrawing,
                Clip = starSourceRectangle,
                Scale = starScale
            }, false);
            starColor.A = (byte)(255 * Ease.QuartIn(1f - MathUtils.Clamp01(_postMainDrawTimer)));
            for (int i2 = 0; i2 < 2; i2++) {
                batch.Draw(starTexture, starPositionToDraw, DrawInfo.Default with {
                    Color = starColor.MultiplyAlpha(pulseEffectStrength) * PostMainDrawOpacity,
                    Rotation = starRotation,
                    Origin = starOrigin,
                    ImageFlip = flipStarDrawing,
                    Clip = starSourceRectangle,
                    Scale = starScale
                }, false);
            }
        }
    }

    public static void DrawSwords(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte SWORDCOUNT = 4;
        OpacityUpdatedInDraws += 0.035f;
        Texture2D swordTexture = damageClassNameVisualsInfo.Texture;
        for (byte i = 0; i < SWORDCOUNT; i++) {
            bool firstPair = i < SWORDCOUNT / 2;
            bool topSwords = (i + 1) % 2 != 0;
            int swordWidth = swordTexture.Width,
                swordHeight = swordTexture.Height;
            Vector2 swordPositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            switch (i) {
                case 0:
                    swordPositionToDraw += new Vector2(-swordWidth, -swordHeight);
                    break;
                case 1:
                    swordPositionToDraw += new Vector2(-swordWidth, swordHeight / 2f);
                    break;
                case 2:
                    swordPositionToDraw += new Vector2(tooltipLineSize.X + swordWidth, -swordHeight);
                    break;
                case 3:
                    swordPositionToDraw += new Vector2(tooltipLineSize.X + swordWidth, swordHeight / 2f);
                    break;
            }
            swordPositionToDraw.Y += swordHeight / 1.25f;
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
            float factor0 = 1f - OpacityUpdatedInDraws;
            float swordExtraRotation = MathF.Sin(factor0 + Utils.RandomFloat(ref seedForRandomness));
            swordExtraRotation = Ease.ExpoIn(Ease.QuadOut(swordExtraRotation));
            swordExtraRotation = MathUtils.Clamp01(swordExtraRotation);
            swordExtraRotation *= swordExtraRotationDirection;
            swordExtraRotation *= MathHelper.PiOver2;
            swordExtraRotation += Ease.CircOut(MathHelper.WrapAngle(_postMainDrawTimer)) * swordExtraRotationDirection * 2f;
            swordRotation += swordExtraRotation;
            Rectangle swordSourceRectangle = swordTexture.Bounds;
            Color swordColor = Color.White * PostMainDrawOpacity;
            SpriteEffects flipSwordDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 swordOrigin = new(firstPair ? 4f : swordWidth - 4f, swordHeight - 4f);
            batch.Draw(swordTexture, swordPositionToDraw, DrawInfo.Default with {
                Color = swordColor,
                Rotation = swordRotation,
                Origin = swordOrigin,
                ImageFlip = flipSwordDrawing,
                Clip = swordSourceRectangle
            }, false);
        }
    }

    public static void DrawArrows(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte ARROWCOUNT = 4;
        Texture2D arrowTexture = damageClassNameVisualsInfo.Texture;
        OpacityUpdatedInDraws += 0.075f;
        for (byte i = 0; i < ARROWCOUNT; i++) {
            byte half = ARROWCOUNT / 2;
            byte nextArrowIndex = (byte)(i + 1);
            bool firstPair = i < half;
            bool rightArrows = nextArrowIndex > half;
            int arrowHeight = arrowTexture.Height;
            Vector2 arrowPositionToDraw = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            if (rightArrows) {
                arrowPositionToDraw += new Vector2(tooltipLineSize.X, 0f);
            }
            arrowPositionToDraw.Y += arrowHeight / 2f;
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
            float factor = MathHelper.WrapAngle(_mainDrawTimer / 30f + Utils.RandomInt(ref seedForRandomness, 100));
            float arrowExtraRotation = MathF.Sin(factor + Utils.RandomFloat(ref seedForRandomness) + Utils.RandomInt(ref seedForRandomness2, 100));
            float arrowProgress = 1f - OpacityUpdatedInDraws;
            arrowExtraRotation *= MathF.Max(0.25f, arrowProgress) * 1f;
            arrowExtraRotation *= rightDirection;
            arrowExtraRotation *= 0.2f;
            arrowExtraRotation *= (nextArrowIndex % 2 == 0).ToDirectionInt();
            arrowRotation += arrowExtraRotation;
            Vector2 arrowVelocity = Vector2.UnitY.RotatedBy(arrowRotation);
            arrowPositionToDraw += arrowVelocity * 50f * Ease.QuadIn(arrowProgress);
            arrowPositionToDraw += -arrowVelocity * Ease.QuadOut(MathUtils.Clamp01(_postMainDrawTimer / 60f)) * 100f;
            Rectangle arrowSourceRectangle = arrowTexture.Bounds;
            Color arrowColor = Color.White * PostMainDrawOpacity;
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

    public static int GetRandomIntBasedOnItemType(ushort itemType) {
        ulong seedForRandomness = itemType;
        return Utils.RandomInt(ref seedForRandomness, 100);
    }
}

sealed class DamageClassVisualsInItemNamePostTooltipDrawing() : InterfaceElement(RoA.ModName + ": Damage Class Tooltip UI", InterfaceScaleType.UI) {
    private static float CANPLAYINANIMATIONAGAINFOR => 10f;

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
        if (!CanDrawClassUIVisuals) {
            return true;
        }

        void resetGlobalClassVisualInfo() {
            if (_canResetGlobalVisualInfoTimer-- <= 0f) {
                OpacityUpdatedInDraws = 0f;
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

        UpdateGlobalClassVisualsInfo(_damageClassNameVisualsInfo.DamageClassType);
        DrawClassUIVisuals(_damageClassNameVisualsInfo);

        return true;
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }
}
