using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using RoA.Common.BackwoodsSystems;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.CameraSystem;
sealed class TrailerCameraTest : ModPlayer {
    private bool _shifted = true;

    private static float EaseFunctionValue(float value1, float value2, float amount) {
        float result = MathHelper.Clamp(amount, 0f, 1f);
        //result = MathHelper.Hermite(value1, 0f, value2, 0f, result);

        result = MathHelper.Lerp(value1, value2, Ease.SineIn(Ease.CubeOut(amount)));

        return result;
    }

    private static Vector2 EaseFunction(Vector2 value1, Vector2 value2, float amount) {
        return new Vector2(
            EaseFunctionValue(value1.X, value2.X, amount),
            EaseFunctionValue(value1.Y, value2.Y, amount)
        );
    }

    private class LightHackGlobalWall : GlobalWall {
        public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
            if (CameraSystem.AsymetricalPanModifier.Progress > 0) {
                float value = 1f - CameraSystem.AsymetricalPanModifier.Progress;
                r = MathHelper.Clamp(r + value, 0, 1);
                g = MathHelper.Clamp(g + value, 0, 1);
                b = MathHelper.Clamp(b + value, 0, 1);
            }
        }
    }

    public override void PostUpdate() {
        if (Helper.JustPressed(Keys.F5)) {
            _shifted = false;
        }

        if (!_shifted) {
            Point altarPosition = new Point(BackwoodsVars.BackwoodsStartX + 9, BackwoodsVars.FirstTileYAtCenter - 25);
            Point backwoodsBottomPosition = new Point(BackwoodsVars.BackwoodsStartX + 9, BackwoodsVars.FirstTileYAtCenter + BackwoodsVars.BackwoodsSizeY * 2 - BackwoodsVars.BackwoodsSizeY / 2);
            CameraSystem.AsymetricalPan(700, 240, 100, backwoodsBottomPosition.ToWorldCoordinates(), altarPosition.ToWorldCoordinates(), EaseFunction);
            _shifted = true;
        }
    }
}
