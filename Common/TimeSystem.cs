using Terraria;
using Terraria.ModLoader;

namespace RoA.Common;

[Autoload(Side = ModSide.Client)]
sealed class TimeSystem : ModSystem {
    public const float FULLDAYLENGTH = (float)(Main.nightLength + Main.dayLength);
    public const int TARGETFPS = 60;

    public static float RenderDeltaTime { get; private set; } = 1f / TARGETFPS;
    public static float LogicDeltaTime { get; private set; } = 1f / TARGETFPS;
    public static ulong UpdateCount { get; private set; }
    public static double GlobalTime { get; private set; }

    public static float RealTime => (float)(Main.time + (Main.dayTime ? 0.0 : Main.dayLength));
    public static double TimeForVisualEffects => Main.timeForVisualEffects / TARGETFPS;

    public override void PostUpdateEverything() {
        if (!Main.gameMenu && !Main.InGameUI.IsVisible) {
            UpdateCount++;
        }
        GlobalTime += LogicDeltaTime;
    }
}
