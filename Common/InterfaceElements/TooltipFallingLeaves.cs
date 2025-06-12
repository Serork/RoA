﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

sealed class TooltipFallingLeaves() : InterfaceElement(RoA.ModName + ": Tooltip Falling Leaves", InterfaceScaleType.UI) {
    public const float DELAY = 0f;

    internal struct FallingLeafData {
        public byte Index;
        public SpriteData SpriteInfo;
    }

    private static float _counter;
    private static FallingLeafData[] _fallingLeaves;
    private static bool _draw;

    private static float CounterMax => 1f + DELAY;

    internal static void ResetData() => _draw = false;

    internal static void MatchData(FallingLeafData data) {
        _draw = true;

        _counter = CounterMax;

        _fallingLeaves[data.Index] = data;
    }

    public override void Load(Mod mod) {
        _fallingLeaves = new FallingLeafData[ItemTooltipLeaves.LEAVESCOUNT];
    }

    public override void Unload() {
        _fallingLeaves = null;
    }

    protected override bool DrawSelf() {
        if (!_draw) {
            return true;
        }

        Helper.AddClamp(ref _counter, -TimeSystem.LogicDeltaTime, 0f, CounterMax);

        float maxY = 28f;

        foreach (FallingLeafData fallingLeafData in _fallingLeaves) {
            float counter = Math.Clamp(_counter, 0f, 1f);

            float maxX = 5f * Ease.SineOut(counter), speedX = 2.5f;
            float offsetY = 30f;
            Vector2 velocity = new(Helper.Wave(-maxX, maxX, speedX, (fallingLeafData.Index + 1) * ItemTooltipLeaves.LEAVESCOUNT), offsetY * Ease.QuadOut(counter));
            float oscMultiplier = (1f - counter) * 5f;
            velocity.X *= oscMultiplier;

            float alpha = Main.mouseItem.IsAir ? 0f : Utils.GetLerpValue(0f, 0.3f, Ease.SineIn(counter));

            float velocityAffectedExtraRotation = velocity.X * 0.05f;

            Main.spriteBatch.With(BlendState.AlphaBlend, true, () => {
                SpriteData spriteInfo = fallingLeafData.SpriteInfo;
                spriteInfo.VisualPosition += Vector2.UnitY * maxY - velocity + Vector2.UnitY;
                spriteInfo.Color *= alpha;
                spriteInfo.Rotation += velocityAffectedExtraRotation;
                spriteInfo.DrawSelf();
            }, samplerState: SamplerState.AnisotropicClamp);
        }

        return true;
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }
}
