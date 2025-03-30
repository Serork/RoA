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

    private static Point _point1, _point2;
    private static int _durationIn;
    private static int _durationOut;
    private static int _durationHold;

    private static float EaseFunctionValue(float value1, float value2, float amount) {
        float result = MathHelper.Clamp(amount, 0f, 1f);
        //result = MathHelper.Hermite(value1, 0f, value2, 0f, result);

        result = MathHelper.Lerp(value1, value2, Ease.SineIn(Ease.SineOut(amount)));

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
            //if (CameraSystem.AsymetricalPanModifier.Progress > 0) {
            //    float value = 1f - CameraSystem.AsymetricalPanModifier.Progress;
            //    r = MathHelper.Clamp(r + value, 0, 1);
            //    g = MathHelper.Clamp(g + value, 0, 1);
            //    b = MathHelper.Clamp(b + value, 0, 1);
            //}
        }
    }

    private class SetPoint1Command : ModCommand {
        public override CommandType Type => CommandType.World;
        public override string Command => "point1";
        public override string Usage => "/point1";

        public override void Action(CommandCaller caller, string input, string[] args) => _point1 = Main.LocalPlayer.Center.ToTileCoordinates();
    }

    private class SetPoint2Command : ModCommand {
        public override CommandType Type => CommandType.World;
        public override string Command => "point2";
        public override string Usage => "/point2";

        public override void Action(CommandCaller caller, string input, string[] args) => _point2 = Main.LocalPlayer.Center.ToTileCoordinates();
    }

    private class SetDurationInCommand : ModCommand {
        public override CommandType Type => CommandType.World;
        public override string Command => "duration";
        public override string Usage => "/duration in out hold";

        public override void Action(CommandCaller caller, string input, string[] args) {
            if (!int.TryParse(args[0], out int durationIn)) {
                throw new UsageException(args[0] + " is not an integer");
            }

            if (!int.TryParse(args[1], out int durationOut)) {
                throw new UsageException(args[1] + " is not an integer");
            }

            if (!int.TryParse(args[2], out int durationHold)) {
                throw new UsageException(args[2] + " is not an integer");
            }

            _durationIn = durationIn;
            _durationOut = durationOut;
            _durationHold = durationHold;
        }
    }

    public override void PostUpdate() {
        if (Helper.JustPressed(Keys.F5)) {
            _shifted = false;
        }

        if (!_shifted) {
            Point point1 = _point1;
            Point point2 = _point2;
            CameraSystem.AsymetricalPan(_durationIn, _durationHold, _durationOut,
                point1.ToWorldCoordinates(), point2.ToWorldCoordinates(), EaseFunction);
            _shifted = true;
        }
    }
}
