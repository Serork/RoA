using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoA.Common.Druid;
using RoA.Common.InterfaceElements;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.GUI;

[Autoload(Side = ModSide.Client)]
sealed class TooltipFallingLeafs() : InterfaceElement(RoA.ModName + ": Tooltip Falling Leafs", InterfaceScaleType.UI) {
    public const float DELAY = 0f;

    internal struct FallingLeafData {
        public byte Index;
        public SpriteData SpriteInfo;
    }

    private static float _counter;
    private static FallingLeafData[] _fallingLeafs;

    private static float CounterMax => 1f + DELAY;

    internal static void MatchData(FallingLeafData data) {
        _counter = CounterMax;

        _fallingLeafs[data.Index] = data;
    }

    public override void Load(Mod mod) {
        _fallingLeafs = new FallingLeafData[ItemTooltipLeafs.LEAFSCOUNT];
    }

    public override void Unload() {
        _fallingLeafs = null;
    }

    protected override bool DrawSelf() {
        Helper.AddClamp(ref _counter, -TimeSystem.LogicDeltaTime, 0f, CounterMax);

        float maxY = 28f;

        foreach (FallingLeafData fallingLeafData in _fallingLeafs) {
            float counter = Math.Clamp(_counter, 0f, 1f);

            float maxX = 5f * Ease.SineOut(counter), speedX = 2.5f;
            float offsetY = 30f;
            Vector2 velocity = new(Helper.Wave(-maxX, maxX, speedX, (fallingLeafData.Index + 1) * ItemTooltipLeafs.LEAFSCOUNT), offsetY * Ease.QuadOut(counter));
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
            }, samplerState: SamplerState.PointClamp);
        }

        return true;
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }
}
