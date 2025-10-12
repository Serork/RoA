//using Microsoft.Xna.Framework;

//using RoA.Common.Networking;
//using RoA.Common.Networking.Packets;
//using RoA.Core;

//using Terraria;
//using Terraria.Audio;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace RoA.Common.CameraSystem;

//[Autoload(false)]
//sealed class TrailerCameraTest : ModSystem {
//    private static bool _shifted = true;

//    private static Point _point1, _point2;
//    private static int _durationIn;
//    private static int _durationOut;
//    private static int _durationHold;
//    private static int _beforeTime;

//    internal static bool _pause;

//    private static float EaseFunctionValue(float value1, float value2, float amount) {
//        float result = MathHelper.Clamp(amount, 0f, 1f);
//        //result = MathHelper.Hermite(value1, 0f, value2, 0f, result);

//        result = MathHelper.Lerp(value1, value2, Ease.SineIn(Ease.SineOut(amount)));

//        return result;
//    }

//    private static Vector2 EaseFunction(Vector2 value1, Vector2 value2, float amount) {
//        return new Vector2(
//            EaseFunctionValue(value1.X, value2.X, amount),
//            EaseFunctionValue(value1.Y, value2.Y, amount)
//        );
//    }

//    private class LightHackGlobalWall : GlobalWall {
//        public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
//            //if (CameraSystem.AsymetricalPanModifier.Opacity > 0) {
//            //    float value = 1f - CameraSystem.AsymetricalPanModifier.Opacity;
//            //    r = MathHelper.Clamp(r + value, 0, 1);
//            //    g = MathHelper.Clamp(g + value, 0, 1);
//            //    b = MathHelper.Clamp(b + value, 0, 1);
//            //}
//        }
//    }

//    //private class SetPoint1Command : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "point1";
//    //    public override string Usage => "/point1";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        Main.NewText(Main.LocalPlayer.Center.ToTileCoordinates());

//    //        if (!int.TryParse(args[0], out int x)) {
//    //            throw new UsageException(args[0] + " is not an integer");
//    //        }

//    //        if (!int.TryParse(args[1], out int y)) {
//    //            throw new UsageException(args[1] + " is not an integer");
//    //        }

//    //        _point2 = new Point(x, y);
//    //    }
//    //}

//    //private class SetPoint2Command : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "point2";
//    //    public override string Usage => "/point2";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        Main.NewText(Main.LocalPlayer.Center.ToTileCoordinates());

//    //        if (!int.TryParse(args[0], out int x)) {
//    //            throw new UsageException(args[0] + " is not an integer");
//    //        }

//    //        if (!int.TryParse(args[1], out int y)) {
//    //            throw new UsageException(args[1] + " is not an integer");
//    //        }

//    //        _point1 = new Point(x, y);
//    //    }
//    //}

//    //private class SetDurationInCommand : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "duration";
//    //    public override string Usage => "/duration in out hold";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        if (!int.TryParse(args[0], out int durationIn)) {
//    //            throw new UsageException(args[0] + " is not an integer");
//    //        }

//    //        if (!int.TryParse(args[1], out int durationOut)) {
//    //            throw new UsageException(args[1] + " is not an integer");
//    //        }

//    //        if (!int.TryParse(args[2], out int durationHold)) {
//    //            throw new UsageException(args[2] + " is not an integer");
//    //        }

//    //        _durationIn = durationIn;
//    //        _durationOut = durationOut;
//    //        _durationHold = durationHold;
//    //    }
//    //}

//    //private class StopCameraCommand : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "stop";
//    //    public override string Usage => "/stop";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        CameraSystem.AsymetricalPanModifier.timer = 0;
//    //        CameraSystem.AsymetricalPanModifier.target = CameraSystem.AsymetricalPanModifier.from = Vector2.Zero;
//    //    }
//    //}

//    //private class StartCameraCommand : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "start";
//    //    public override string Usage => "/start";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        _shifted = false;
//    //    }
//    //}

//    //private class Start2CameraCommand : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "start2";
//    //    public override string Usage => "/start2 time";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        if (!int.TryParse(args[0], out int time)) {
//    //            throw new UsageException(args[0] + " is not an integer");
//    //        }

//    //        _shifted = false;
//    //        _beforeTime = time; 
//    //    }
//    //}

//    //private class BeforeStartTimerCameraCommand : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "beforetimer";
//    //    public override string Usage => "/beforetimer time";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        if (!int.TryParse(args[0], out int time)) {
//    //            throw new UsageException(args[0] + " is not an integer");
//    //        }

//    //        _beforeTime = time;
//    //    }
//    //}

//    //private class PauseCameraCommand : ModCommand {
//    //    public override CommandType Type => CommandType.Chat;
//    //    public override string Command => "pause";
//    //    public override string Usage => "/pause";

//    //    public override void Action(CommandCaller caller, string input, string[] args) {
//    //        _pause = !_pause;
//    //    }
//    //}

//    public override void Load() {
//        On_SceneMetrics.ScanAndExportToMain += On_SceneMetrics_ScanAndExportToMain;
//    }

//    private void On_SceneMetrics_ScanAndExportToMain(On_SceneMetrics.orig_ScanAndExportToMain orig, SceneMetrics self, SceneMetricsScanSettings settings) {
//        if (CameraSystem.AsymetricalPanModifier.timer <= 0) {
//            orig(self, settings);
//            return;
//        }
//        Vector2? temp = settings.BiomeScanCenterPositionInWorld;
//        settings.BiomeScanCenterPositionInWorld = CameraSystem.AsymetricalPanModifier._temp;
//        orig(self, settings);
//        settings.BiomeScanCenterPositionInWorld = temp;
//    }

//    public override void PostUpdatePlayers() {
//        if (!_shifted) {
//            if (_beforeTime > 0f) {
//                _beforeTime--;
//                if (_beforeTime % 60 == 0) {
//                    SoundEngine.PlaySound(SoundID.PlayerHit);
//                    foreach (Player activePlayer in Main.ActivePlayers) {
//                        if (Main.netMode == NetmodeID.MultiplayerClient) {
//                            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(activePlayer, -1, activePlayer.Center), ignoreClient: activePlayer.whoAmI);
//                        }
//                    }
//                }
//                return;
//            }

//            Point point1 = _point1;
//            Point point2 = _point2;
//            CameraSystem.AsymetricalPan(_durationOut, _durationHold, _durationIn,
//                point1.ToWorldCoordinates(), point2.ToWorldCoordinates(), EaseFunction);
//            _shifted = true;
//        }
//    }
//}
