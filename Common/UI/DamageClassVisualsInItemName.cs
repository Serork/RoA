using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.Configs;
using RoA.Common.InterfaceElements;
using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Content;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

using static RoA.Common.UI.DamageClassVisualsInItemName;

namespace RoA.Common.UI;

sealed class DamageClassItemsStorage : IInitializer {
    private static Player? _testPlayer;

    public static Dictionary<DamageClass, HashSet<int>>? ItemsPerDamageClass { get; private set; }

    public static IEnumerable<DamageClass> AllSupportedDamageClasses => DamageClassUtils.GetDamagesClasses();

    public static bool IsItemValid(Item item, out DamageClass? damageClassOfItem) {
        if (ItemsPerDamageClass == null) {
            goto End;
        }
        foreach (DamageClass damageClass in AllSupportedDamageClasses) {
            if (ItemsPerDamageClass![damageClass].Contains(item.type)) {
                damageClassOfItem = damageClass;
                return true;
            }
        }
        End:
        damageClassOfItem = null;
        return false;
    }

    public static List<DamageClass> GetDamageClasses(Item item) {
        List<DamageClass> damageClassOfItem = [];
        foreach (DamageClass damageClass in AllSupportedDamageClasses) {
            if (ItemsPerDamageClass![damageClass].Contains(item.type)) {
                damageClassOfItem.Add(damageClass);
            }
        }
        return damageClassOfItem;
    }

    public static bool IsHybridItem(Item item) {
        bool repeat = false;
        List<DamageClass> countedDamageClasses = [];
        bool result = false;
        foreach (DamageClass damageClass in AllSupportedDamageClasses) {
            if (result) {
                return true;
            }
            if (ItemsPerDamageClass![damageClass].Contains(item.type)) {
                bool checkForCanRepeat(DamageClass checkDamageClass1, DamageClass checkDamageClass2)
                    => (damageClass == checkDamageClass1 && !countedDamageClasses.Contains(checkDamageClass2)) || (damageClass == checkDamageClass2 && !countedDamageClasses.Contains(checkDamageClass1));
                bool canRepeat = checkForCanRepeat(DamageClass.Summon, DamageClass.SummonMeleeSpeed) ||
                                 checkForCanRepeat(DamageClass.Melee, DamageClass.MeleeNoSpeed);
                if (canRepeat && repeat) {
                    result = true;
                }
                repeat = true;
                countedDamageClasses.Add(damageClass);
            }
        }
        return result;
    }

    public static bool IsAlreadyAdded(Item item) {
        foreach (DamageClass damageClass in AllSupportedDamageClasses) {
            if (ItemsPerDamageClass![damageClass].Contains(item.type)) {
                return true;
            }
        }
        return false;
    }

    private static void AddItem(Item item, DamageClass damageClass) {
        if (!ItemsPerDamageClass!.TryGetValue(damageClass, out HashSet<int>? damageClassItemIDSet)) {
            return;
        }

        damageClassItemIDSet.Add(item.type);
    }

    void ILoadable.Load(Mod mod) {
        On_Item.SetDefaults_int_bool_ItemVariant += On_Item_SetDefaults_int_bool_ItemVariant;
    }

    void ILoadable.Unload() {
        ItemsPerDamageClass?.Clear();
        ItemsPerDamageClass = null;
    }

    private void On_Item_SetDefaults_int_bool_ItemVariant(On_Item.orig_SetDefaults_int_bool_ItemVariant orig, Item self, int Type, bool noMatCheck, Terraria.GameContent.Items.ItemVariant variant) {
        orig(self, Type, noMatCheck, variant);

        if (self.type == ItemID.SporeSac) {
            return;
        }

        IEnumerable<DamageClass> damageClasses = AllSupportedDamageClasses;
        if (ItemsPerDamageClass == null) {
            ItemsPerDamageClass = [];
            foreach (DamageClass damageClass in damageClasses) {
                ItemsPerDamageClass[damageClass] = [];
            }

            _testPlayer ??= new Player();
            _testPlayer.dead = false;
            _testPlayer.statLife = _testPlayer.statLifeMax = 100;
            _testPlayer.statMana = _testPlayer.statManaMax = 20;
            _testPlayer.immune = true;
        }

        bool exit = false;
        void generateItemsListAndPopulate() {
            if (self.type >= ItemID.CopperCoin && self.type <= ItemID.PlatinumCoin) {
                return;
            }

            void checkForDamageClassEquips(DamageClass damageClass) {
                Player testPlayer = _testPlayer!;
                List<object> classValuesBefore = [], classValuesAfter = [];
                void setupClassValues(List<object> values) {
                    values.Add(testPlayer.GetDamage(damageClass));
                    values.Add(testPlayer.GetCritChance(damageClass));
                    values.Add(testPlayer.GetAttackSpeed(damageClass));
                    values.Add(testPlayer.GetArmorPenetration(damageClass));
                    values.Add(testPlayer.GetKnockback(damageClass));

                    if (damageClass == DamageClass.Magic) {
                        values.Add(testPlayer.manaMagnet);
                        values.Add(testPlayer.magicCuffs);
                        values.Add(testPlayer.statManaMax2);
                        values.Add(testPlayer.manaCost);
                        values.Add(testPlayer.manaFlower);
                        values.Add(testPlayer.manaRegen);
                        values.Add(testPlayer.manaRegenBuff);
                    }
                    else if (damageClass == DamageClass.Melee) {
                        values.Add(testPlayer.autoReuseGlove);
                        values.Add(testPlayer.meleeScaleGlove);
                        values.Add(testPlayer.magmaStone);
                        values.Add(testPlayer.kbGlove);
                        values.Add(testPlayer.yoyoGlove);
                        values.Add(testPlayer.stringColor);
                    }
                    else if (damageClass == DamageClass.Summon) {
                        values.Add(testPlayer.maxMinions);
                        values.Add(testPlayer.dd2Accessory);
                        values.Add(testPlayer.whipRangeMultiplier);
                        values.Add(testPlayer.maxTurrets);
                    }
                    else if (damageClass == DamageClass.Ranged) {
                        values.Add(testPlayer.magicQuiver);
                        values.Add(testPlayer.hasMoltenQuiver);
                        values.Add(testPlayer.arrowDamage);
                        values.Add(testPlayer.bulletDamage);
                        values.Add(testPlayer.specialistDamage);
                        values.Add(testPlayer.ammoCost75);
                        values.Add(testPlayer.ammoCost80);
                        values.Add(testPlayer.GetModPlayer<RangedArmorSetPlayer>().AllAmmoConsumptionReduce);
                    }
                }
                try {
                    setupClassValues(classValuesBefore);
                    //if (self.createTile > -1) {
                    //    Tile tile = Main.tile[-1, -1];
                    //    bool active = tile.HasTile;
                    //    ushort tileType = tile.TileType;
                    //    tile.HasTile = true;
                    //    tile.TileType = (ushort)self.createTile;
                    //    testPlayer.tileInteractAttempted = true;
                    //    testPlayer.releaseUseTile = true;
                    //    testPlayer.TileInteractionsCheck(-1, -1);
                    //    tile.HasTile = active;
                    //    tile.TileType = tileType;
                    //    testPlayer.UpdateBuffs(self.whoAmI);
                    //    testPlayer.tileInteractAttempted = false;
                    //    testPlayer.releaseUseTile = false;
                    //}
                    if (self.buffType > 0) {
                        testPlayer.AddBuff(self.buffType, 2);
                        testPlayer.UpdateBuffs(self.whoAmI);
                    }
                    if (self.accessory) {
                        testPlayer.ApplyEquipFunctional(self, true);
                    }
                    testPlayer.GrantArmorBenefits(self);
                    setupClassValues(classValuesAfter);

                    testPlayer.ResetEffects();
                }
                catch {
                    exit = true;
                    return;
                }
                int lengthOfCheckValues = classValuesBefore.Count;
                for (int i = 0; i < lengthOfCheckValues; i++) {
                    object checkValueBefore = classValuesBefore[i], checkValueAfter = classValuesAfter[i];
                    if (checkValueBefore is bool v && v != (bool)checkValueAfter) {
                        AddItem(self, damageClass);
                    }
                    else if (checkValueBefore is StatModifier v2 && (v2.Additive != ((StatModifier)checkValueAfter).Additive || v2.Multiplicative != ((StatModifier)checkValueAfter).Multiplicative)) {
                        AddItem(self, damageClass);
                    }
                    else if (checkValueBefore is int v3 && v3 != (int)checkValueAfter) {
                        AddItem(self, damageClass);
                    }
                    else if (checkValueBefore is float v4 && v4 != (float)checkValueAfter) {
                        AddItem(self, damageClass);
                    }
                }
            }

            if (self.IsAWeapon()) {
                AddItem(self, self.DamageType);
            }
            else if (self.createTile > -1 || self.accessory || (self.buffType > 0 && self.buffTime >= 1800) || self.headSlot != -1 || self.bodySlot != -1 || self.legSlot != -1) {
                foreach (DamageClass damageClass in damageClasses) {
                    if (exit) {
                        return;
                    }
                    checkForDamageClassEquips(damageClass);
                }
            }
        }

        generateItemsListAndPopulate();
    }
}

sealed class DamageClassVisualsInItemName : GlobalItem {
    public readonly struct DamageClassNameVisualsInfo(List<DamageClass> damageClasses, Texture2D texture, ushort itemType, Vector2 tooltipLinePosition, Vector2 tooltipLineSize) {
        public readonly List<DamageClass> DamageClasses = damageClasses;
        public readonly Texture2D Texture = texture;
        public readonly ushort ItemType = itemType;
        public readonly Vector2 TooltipLinePosition = tooltipLinePosition;
        public readonly Vector2 TooltipLineSize = tooltipLineSize;
    }

    private static readonly Dictionary<DamageClass, Asset<Texture2D>?> _classTexturesMap = [];

    private static Asset<Texture2D>? _multiclassTexture, _neutralClassTexture;

    private static DamageClass? _previousDamageClassDraw;
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

        string tooltipLineText = line.Text;
        Vector2 tooltipLineSize = line.Font.MeasureString(tooltipLineText);
        float sizeOffsetModifier = 0.025f;
        tooltipLineSize.X *= 1f - sizeOffsetModifier * 2.25f;
        ushort itemType = (ushort)item.type;
        Vector2 originalSpot = new(line.X + tooltipLineSize.X * sizeOffsetModifier, line.Y);
        originalSpot.X += 1f;

        if (!DamageClassItemsStorage.IsItemValid(item, out DamageClass? itemDamageClass)) {
            DamageClassVisualsInItemNamePostTooltipDrawing.ResetData();

            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        List<DamageClass> damageClassesOfThisItem = DamageClassItemsStorage.GetDamageClasses(item);

        _mainDrawTimer += 1f;
        _postMainDrawTimer = 0f;
        _postMainDrawOpacityValue = 0f;

        DamageClassVisualsInItemNamePostTooltipDrawing.ResetData();

        DamageClass damageClassTypeOfThisItem = itemDamageClass!;
        if (!_classTexturesMap.TryGetValue(damageClassTypeOfThisItem, out Asset<Texture2D>? classUIAsset)) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        if (classUIAsset?.IsLoaded != true) {
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        if (itemDamageClass != _previousDamageClassDraw) {
            OpacityUpdatedInDraws = 0f;

            if (NeedMainTimerReset(damageClassTypeOfThisItem)) {
                _mainDrawTimer = 0f;
            }
        }

        Texture2D classUITexture = classUIAsset.Value;
        DrawClassUIVisuals(new(damageClassesOfThisItem, classUITexture, itemType, originalSpot, tooltipLineSize));
        DamageClassVisualsInItemNamePostTooltipDrawing.IsHoveringItem = true;

        _previousDamageClassDraw = damageClassTypeOfThisItem;

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    public static bool NeedMainTimerReset(DamageClass damageClass) => damageClass != DamageClass.Summon;

    public static void DrawClassUIVisuals(in DamageClassNameVisualsInfo damageClassVisualsInfo, bool mainDraw = true) {
        if (!mainDraw) {
            OpacityUpdatedInDraws = 1f;
        }

        List<DamageClass> damageClassesTypeOfThisItem = damageClassVisualsInfo.DamageClasses;
        SpriteBatch batch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
        batch.BeginBlendState(snapshot.blendState, snapshot.samplerState, isUI: true);
        bool shouldDrawNeutralIcon = damageClassesTypeOfThisItem.Contains(DamageClass.Generic) || damageClassesTypeOfThisItem.Count > 2;
        if (shouldDrawNeutralIcon || damageClassesTypeOfThisItem.Count == 2) {
            if (!mainDraw) {
                float max = 1.35f;
                _postMainDrawTimer += 0.0275f;
                _postMainDrawTimer = MathF.Min(max, _postMainDrawTimer);
                _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, max, _postMainDrawTimer, true));
            }

            DrawMulticlassIcon(batch, damageClassVisualsInfo, shouldDrawNeutralIcon);
        }
        else if (damageClassesTypeOfThisItem.Contains(DamageClass.Melee) || damageClassesTypeOfThisItem.Contains(DamageClass.MeleeNoSpeed)) {
            if (!mainDraw) {
                float max = 1.35f;
                _postMainDrawTimer += 0.0275f;
                _postMainDrawTimer = MathF.Min(max, _postMainDrawTimer);
                _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, max, _postMainDrawTimer, true));
            }

            DrawSwords(batch, damageClassVisualsInfo);
        }
        else if (damageClassesTypeOfThisItem.Contains(DamageClass.Ranged)) {
            if (!mainDraw) {
                _postMainDrawTimer += 1.5f;
                _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(40f, 85f, _postMainDrawTimer, true));
            }

            DrawArrows(batch, damageClassVisualsInfo);
        }
        else if (damageClassesTypeOfThisItem.Contains(DamageClass.Magic)) {
            if (!mainDraw) {
                float max = 1.35f;
                _postMainDrawTimer += 0.0275f;
                _postMainDrawTimer = MathF.Min(max, _postMainDrawTimer);
                _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, max, _postMainDrawTimer, true));
            }

            DrawStars(batch, damageClassVisualsInfo);
        }
        else if (damageClassesTypeOfThisItem.Contains(DamageClass.Summon) || damageClassesTypeOfThisItem.Contains(DamageClass.SummonMeleeSpeed)) {
            if (!mainDraw) {
                float max = 1.35f;
                _mainDrawTimer += 1f;
                _postMainDrawTimer += 0.0275f;
                _postMainDrawTimer = MathF.Min(max, _postMainDrawTimer);
                _postMainDrawOpacityValue = Ease.SineIn(Utils.GetLerpValue(0.5f, max, _postMainDrawTimer, true));
            }

            DrawSlimes(batch, damageClassVisualsInfo);
        }
        DamageClassVisualsInItemNamePostTooltipDrawing.MatchData(damageClassVisualsInfo);
        batch.End();
        batch.Begin(snapshot);
    }

    private static void LoadClassUITextures() {
        if (Main.dedServ) {
            return;
        }

        foreach (DamageClass damageClass in DamageClassItemsStorage.AllSupportedDamageClasses) {
            string classDamageTypeName = damageClass.Name.Replace("DamageClass", string.Empty);
            if (classDamageTypeName.Contains("Summon")) {
                classDamageTypeName = "Summon";
            }
            else if (classDamageTypeName.Contains("Melee")) {
                classDamageTypeName = "Melee";
            }
            else if (classDamageTypeName.Contains("Generic")) {
                classDamageTypeName = "Neutral";
            }
            string classUITextureName = ResourceManager.UITextures + $"ClassUI_{classDamageTypeName}";
            if (ModContent.RequestIfExists(classUITextureName, out Asset<Texture2D> classUITextureAsset)) {
                _classTexturesMap[damageClass] = classUITextureAsset;
            }
        }

        _multiclassTexture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "ClassUI_Multiclass");
        _neutralClassTexture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "ClassUI_Neutral");
    }

    public static void DrawSlimes(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte SLIMECOUNT = 2;
        const byte SLIMEFRAMECOUNT = 3;
        Texture2D slimeTexture = damageClassNameVisualsInfo.Texture;
        for (int i = 0; i < SLIMECOUNT; i++) {
            byte half = SLIMECOUNT / 2;
            bool firstPair = i < half;
            int height = slimeTexture.Height;
            float rotation = 0f;
            Vector2 position = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            float offsetX = 10f;
            int randomValue = GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType);
            float factor = MathHelper.WrapAngle(_mainDrawTimer / 20f % MathHelper.TwoPi + randomValue);
            ulong seedForRandomness = (ulong)((i + 1) * randomValue);
            float randomValue2 = Utils.RandomFloat(ref seedForRandomness) * 100f;
            seedForRandomness = (ulong)randomValue;
            float randomValue4 = Utils.RandomFloat(ref seedForRandomness) * 100f;
            float frameFactor = 1f - Math.Abs(MathF.Sin(factor + randomValue2 * MathHelper.Pi));
            Color color = Color.White;
            float randomValue3 = randomValue / 100f;
            float colorRGB = randomValue3;
            color = Color.Lerp(color, new Color(colorRGB, colorRGB, colorRGB), randomValue2);
            if (!firstPair) {
                position += new Vector2(tooltipLineSize.X + offsetX, 0f);
            }
            else {
                position -= Vector2.UnitX * offsetX;
            }
            position.Y += height / 2f;
            SpriteFrame spriteFrame = new SpriteFrame(SLIMEFRAMECOUNT, 1).With((byte)(SLIMEFRAMECOUNT * frameFactor), 0);
            SpriteEffects flipSlimeDrawing = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            color *= PostMainDrawOpacity;
            float jumpHeight = 10f + 30f * randomValue4;
            Vector2 bezierPoint1 = position, bezierPoint2 = bezierPoint1 - Vector2.UnitY * jumpHeight, bezierPoint3 = bezierPoint1 + Vector2.UnitY * jumpHeight * 0.5f;
            float bezierFactor = 1f - _postMainDrawTimer;
            Rectangle starSourceRectangle = spriteFrame.GetSourceRectangle(slimeTexture);
            Vector2 origin = starSourceRectangle.Size() / 2f;
            Vector2 finalPosition = MathF.Pow(1 - bezierFactor, 2) * bezierPoint1 + 2 * (1 - bezierFactor) * bezierFactor * bezierPoint2 + MathF.Pow(bezierFactor, 2) * bezierPoint3;
            position = finalPosition;
            position -= Vector2.UnitY * jumpHeight * 0.5f;
            batch.Draw(slimeTexture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Origin = origin,
                ImageFlip = flipSlimeDrawing,
                Clip = starSourceRectangle
            }, false);
        }
    }

    public static void DrawStars(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte STARCOUNT = 2;
        OpacityUpdatedInDraws += 0.035f;
        Texture2D starTexture = damageClassNameVisualsInfo.Texture;
        for (int i = 0; i < STARCOUNT; i++) {
            byte half = STARCOUNT / 2;
            bool firstPair = i < half;
            int height = starTexture.Height;
            float rotation = 0f;
            Vector2 position = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            float offsetX = 8f;
            if (!firstPair) {
                position += new Vector2(tooltipLineSize.X + offsetX, 0f);
            }
            else {
                position -= Vector2.UnitX * offsetX;
            }
            float direction = firstPair.ToDirectionInt();
            position.Y += height / 2f;
            position.Y -= 2f;
            float factor = MathHelper.WrapAngle(_mainDrawTimer / 20f % MathHelper.TwoPi + GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType));
            float progress = 1f - OpacityUpdatedInDraws;
            float sin = MathF.Sin(factor);
            Rectangle clip = starTexture.Bounds;
            Color color = Color.White;
            SpriteEffects flip = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = starTexture.Size() / 2f;
            float extraScale = 0.2f * direction * sin;
            color *= PostMainDrawOpacity;
            Vector2 scale = Vector2.One + Vector2.One * extraScale;
            float pulseEffectStrength = 1f - 0.5f * OpacityUpdatedInDraws + Math.Abs(0.5f * sin) * OpacityUpdatedInDraws;
            float alphaProgress = Ease.QuartIn(PostMainDrawOpacity);
            batch.Draw(starTexture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Origin = origin,
                ImageFlip = flip,
                Clip = clip,
                Scale = scale
            }, false);
            batch.Draw(starTexture, position, DrawInfo.Default with {
                Color = color.MultiplyAlpha(pulseEffectStrength),
                Rotation = rotation,
                Origin = origin,
                ImageFlip = flip,
                Clip = clip,
                Scale = scale
            }, false);
            color.A = (byte)(255 * Ease.QuartIn(1f - MathUtils.Clamp01(_postMainDrawTimer)));
            for (int i2 = 0; i2 < 2; i2++) {
                batch.Draw(starTexture, position, DrawInfo.Default with {
                    Color = color.MultiplyAlpha(pulseEffectStrength) * PostMainDrawOpacity,
                    Rotation = rotation,
                    Origin = origin,
                    ImageFlip = flip,
                    Clip = clip,
                    Scale = scale
                }, false);
            }
        }
    }

    public static void DrawSwords(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte SWORDCOUNT = 4;
        const byte SWORDFRAMECOUNT = 3;
        OpacityUpdatedInDraws += 0.035f;
        Texture2D swordTexture = damageClassNameVisualsInfo.Texture;
        float originOffsetFactor = 6f;
        ulong seedForRandomness2 = (ulong)(GetRandomIntBasedOnItemType(damageClassNameVisualsInfo.ItemType));
        byte usedFrame = (byte)Utils.RandomInt(ref seedForRandomness2, SWORDFRAMECOUNT);
        for (int i = 0; i < SWORDCOUNT; i++) {
            int nextSwordIndex = i + 1;
            bool firstPair = i < SWORDCOUNT / 2;
            bool topSwords = (i + 1) % 2 != 0;
            SpriteFrame spriteFrame = new SpriteFrame(SWORDFRAMECOUNT, 1).With(usedFrame, 0);
            Rectangle clip = spriteFrame.GetSourceRectangle(swordTexture);
            int width = clip.Width,
                height = clip.Height;
            Vector2 position = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            Vector2 offsetXToCenter = Vector2.UnitX * firstPair.ToDirectionInt() * 4f;
            switch (i) {
                case 0:
                    position += new Vector2(-width, -height);
                    break;
                case 1:
                    position += new Vector2(-width, height / 2f);
                    break;
                case 2:
                    position += new Vector2(tooltipLineSize.X + width, -height);
                    break;
                case 3:
                    position += new Vector2(tooltipLineSize.X + width, height / 2f);
                    break;
            }
            position += offsetXToCenter;
            position.Y += height / 1.25f;
            int topDirection = topSwords.ToDirectionInt(),
                pairDirection = firstPair.ToDirectionInt();
            float offsetYPerSword = topDirection * 8f;
            position.Y += offsetYPerSword;
            float extraRotationDirection = pairDirection * topDirection * -1f;
            float rotationBase = 0f * extraRotationDirection;
            float rotation = rotationBase;
            if (topSwords) {
                rotation = rotationBase + MathHelper.PiOver2 * pairDirection;
            }
            ulong seedForRandomness = (ulong)nextSwordIndex;
            float factor0 = 1f - OpacityUpdatedInDraws;
            float extraRotation = MathF.Sin(factor0 + Utils.RandomFloat(ref seedForRandomness));
            extraRotation = Ease.ExpoIn(Ease.QuadOut(extraRotation));
            extraRotation = MathUtils.Clamp01(extraRotation);
            extraRotation *= extraRotationDirection;
            extraRotation *= MathHelper.PiOver2;
            float extraRotation2 = Ease.CircOut(MathHelper.WrapAngle(Utils.Clamp(_postMainDrawTimer, 0f, 1f))) * extraRotationDirection * 2f;
            extraRotation += extraRotation2;
            rotation += extraRotation;
            Color color = Color.White * PostMainDrawOpacity;
            SpriteEffects flip = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new(firstPair ? originOffsetFactor : width - originOffsetFactor, height - (originOffsetFactor - 2f));
            batch.Draw(swordTexture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Origin = origin,
                ImageFlip = flip,
                Clip = clip
            }, false);
        }
    }

    public static void DrawArrows(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo) {
        const byte ARROWCOUNT = 4;
        const byte ARROWFRAMECOUNT = 2;
        Texture2D arrowTexture = damageClassNameVisualsInfo.Texture;
        OpacityUpdatedInDraws = 1f;
        ushort itemType = damageClassNameVisualsInfo.ItemType;
        ulong seedForRandomness3 = itemType;
        int usedFrame = (int)MathF.Min(Utils.RandomInt(ref seedForRandomness3, ARROWFRAMECOUNT + 1), ARROWFRAMECOUNT - 1);
        for (int i = 0; i < ARROWCOUNT; i++) {
            byte half = ARROWCOUNT / 2;
            byte nextArrowIndex = (byte)(i + 1);
            bool firstPair = i < half;
            bool rightArrows = nextArrowIndex > half;
            int height = arrowTexture.Height;
            Vector2 position = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            if (rightArrows) {
                position += new Vector2(tooltipLineSize.X, 0f);
            }
            position.Y += height / 2f;
            position.Y -= 4f;
            float rotation = MathHelper.PiOver2;
            if (!firstPair) {
                rotation -= MathHelper.Pi;
            }
            int checkIndex = nextArrowIndex % half;
            float offsetYValue = 2f;
            float offsetY = offsetYValue * (checkIndex % 2 == 0).ToDirectionInt() + offsetYValue * (checkIndex % 3 == 0).ToDirectionInt();
            position.Y += offsetY;
            int rightDirection = (!firstPair).ToDirectionInt();
            rotation += offsetY * 0.05f * rightDirection;
            ulong seedForRandomness = itemType,
                  seedForRandomness2 = nextArrowIndex;
            SpriteFrame spriteFrame = new SpriteFrame(ARROWFRAMECOUNT, 1).With((byte)(!firstPair ? ARROWFRAMECOUNT - usedFrame - 1 : usedFrame), 0);
            if (++usedFrame >= ARROWFRAMECOUNT) {
                usedFrame = 0;
            }
            float factor = MathHelper.WrapAngle(_mainDrawTimer / 30f + Utils.RandomInt(ref seedForRandomness, 100));
            float extraRotation = MathF.Sin(factor + Utils.RandomFloat(ref seedForRandomness) + Utils.RandomInt(ref seedForRandomness2, 100));
            float progress = 1f - OpacityUpdatedInDraws;
            extraRotation *= MathF.Max(0.25f, progress) * 1f;
            extraRotation *= rightDirection;
            extraRotation *= 0.35f;
            extraRotation *= (nextArrowIndex % 2 == 0).ToDirectionInt();
            rotation += extraRotation;
            Vector2 velocity = Vector2.UnitY.RotatedBy(rotation);
            position += velocity * 50f * Ease.QuadIn(progress);
            position += -velocity * Ease.QuadOut(MathUtils.Clamp01(_postMainDrawTimer / 60f)) * 100f;
            Rectangle clip = spriteFrame.GetSourceRectangle(arrowTexture);
            Color color = Color.White * PostMainDrawOpacity;
            SpriteEffects flip = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new(clip.Width / 2f, 0f);
            batch.Draw(arrowTexture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Origin = origin,
                ImageFlip = flip,
                Clip = clip
            }, false);
        }
    }

    public static void DrawMulticlassIcon(SpriteBatch batch, in DamageClassNameVisualsInfo damageClassNameVisualsInfo, bool neutralClass = false) {
        if (_neutralClassTexture?.IsLoaded != true || _multiclassTexture?.IsLoaded != true) {
            return;
        }

        List<DamageClass> damageClassesOfItem = damageClassNameVisualsInfo.DamageClasses;
        if (!neutralClass && damageClassesOfItem.Count < 2) {
            return;
        }

        const byte MULTICLASSICONCOUNT = 2;
        const byte SPRITESHEETCOLUMNS = 5;
        const byte SPRITESHEETROWS = 5;
        Texture2D multiclassTexture = neutralClass ? _neutralClassTexture.Value : _multiclassTexture.Value;
        DamageClass firstDamageClass = damageClassesOfItem[0];
        DamageClass? secondDamageClass = neutralClass ? null : damageClassesOfItem[1];
        byte usedRow = 0;
        if (!neutralClass) {
            if (firstDamageClass == DamageClass.Melee || firstDamageClass == DamageClass.MeleeNoSpeed) {
                usedRow = 0;
            }
            else if (firstDamageClass == DamageClass.Ranged) {
                usedRow = 1;
            }
            else if (firstDamageClass == DamageClass.Magic) {
                usedRow = 2;
            }
            else if (firstDamageClass == DamageClass.Summon || firstDamageClass == DamageClass.SummonMeleeSpeed) {
                usedRow = 3;
            }
            else if (firstDamageClass == DruidClass.Nature) {
                usedRow = 4;
            }
        }
        byte usedColumn = 0;
        if (!neutralClass) {
            if (secondDamageClass == DamageClass.Melee || secondDamageClass == DamageClass.MeleeNoSpeed) {
                usedColumn = 0;
            }
            else if (secondDamageClass == DamageClass.Ranged) {
                usedColumn = 1;
            }
            else if (secondDamageClass == DamageClass.Magic) {
                usedColumn = 2;
            }
            else if (secondDamageClass == DamageClass.Summon || secondDamageClass == DamageClass.SummonMeleeSpeed) {
                usedColumn = 3;
            }
            else if (secondDamageClass == DruidClass.Nature) {
                usedColumn = 4;
            }
        }
        OpacityUpdatedInDraws += 0.03f;
        for (int i = 0; i < MULTICLASSICONCOUNT; i++) {
            byte half = MULTICLASSICONCOUNT / 2;
            bool firstPair = i < half;
            SpriteFrame spriteFrame = neutralClass ? new(1, 1) : new SpriteFrame(SPRITESHEETCOLUMNS, SPRITESHEETROWS).With(usedRow, usedColumn);
            Rectangle clip = spriteFrame.GetSourceRectangle(multiclassTexture);
            int height = clip.Height,
                width = clip.Width;
            Vector2 position = damageClassNameVisualsInfo.TooltipLinePosition;
            Vector2 tooltipLineSize = damageClassNameVisualsInfo.TooltipLineSize;
            float offsetX = 20f;
            float newSizeX = tooltipLineSize.X;
            position.X += newSizeX / 2f;
            float direction = firstPair.ToDirectionInt();
            float progress = Ease.CircOut(OpacityUpdatedInDraws),
                  progress2 = progress * MathF.Pow(_postMainDrawTimer + PostMainDrawOpacity, 0.4f);
            float rotation = MathHelper.TwoPi * 1.25f * progress2 * -direction;
            position.X -= direction * (newSizeX / 2f + offsetX / 2f) * 1f/*progress2*/;
            position.Y += height / 1.25f;
            position.Y -= 3f;
            Color color = Color.White * PostMainDrawOpacity;
            SpriteEffects flip = firstPair ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = clip.Size() / 2f;
            color *= PostMainDrawOpacity;
            batch.Draw(multiclassTexture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Origin = origin,
                ImageFlip = flip,
                Clip = clip,
                Scale = Vector2.One * Utils.Remap(progress2, 0f, 1f, 0.25f, 1f)
            }, false);
        }
    }

    private static int GetRandomIntBasedOnItemType(ushort itemType) {
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

        DrawClassUIVisuals(_damageClassNameVisualsInfo, false);

        return true;
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }
}
