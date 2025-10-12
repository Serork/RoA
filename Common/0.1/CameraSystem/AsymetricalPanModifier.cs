//using Microsoft.Xna.Framework;

//using System;

//using Terraria;
//using Terraria.Graphics.CameraModifiers;
//using Terraria.ModLoader;

//namespace RoA.Common.CameraSystem;

//[Autoload(false)]
//// starlight river
//internal class AsymetricalPanModifier : ICameraModifier {
//    public Func<Vector2, Vector2, float, Vector2> EaseInFunction = Vector2.SmoothStep;
//    public Func<Vector2, Vector2, float, Vector2> EaseOutFunction = Vector2.SmoothStep;

//    public int timeOut = 0;
//    public int timeHold = 0;
//    public int timeIn = 0;

//    public Vector2 _temp;

//    public int timer = 0;
//    public Vector2 target = new(0, 0);
//    public Vector2 from = new(0, 0);

//    public int TotalDuration => timeOut + timeHold + timeIn;

//    public string UniqueIdentity => "Starlight River AsymPan";

//    public bool Finished => false;

//    public float DashTime { get; private set; }

//    public void PassiveUpdate() {
//        if (TotalDuration > 0 && from != Vector2.Zero && target != Vector2.Zero) {
//            //cutscene timers
//            if (timer >= TotalDuration) {
//                timeOut = 0;
//                timeHold = 0;
//                timeIn = 0;

//                timer = 0;

//                target = from = Vector2.Zero;
//            }

//            if (timer < TotalDuration && !TrailerCameraTest._pause)
//                timer++;
//        }
//    }

//    public void Update(ref CameraInfo cameraPosition) {
//        if (TotalDuration > 0 && target != Vector2.Zero) {
//            var offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

//            if (timer <= timeOut) { //go out
//                DashTime = timer / (float)timeOut;
//                cameraPosition.CameraPosition = EaseOutFunction(from + offset, target + offset, DashTime);
//            }
//            else if (timer >= TotalDuration - timeIn) { //go in
//                DashTime = (timer - (TotalDuration - timeIn)) / (float)timeIn;
//                cameraPosition.CameraPosition = EaseInFunction(target + offset, from + offset, (timer - (TotalDuration - timeIn)) / (float)timeIn);
//            }
//            else {
//                cameraPosition.CameraPosition = offset + target;
//            } //stay on target

//            _temp = cameraPosition.CameraPosition + -offset;
//        }
//    }

//    public void Reset() {
//        timeOut = 0;
//        timeHold = 0;
//        timeIn = 0;

//        timer = 0;

//        from = Vector2.Zero;
//        target = Vector2.Zero;
//    }
//}