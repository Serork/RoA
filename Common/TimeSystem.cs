using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Common;

[Autoload(Side = ModSide.Client)]
sealed class TimeSystem : ModSystem {
    public const float FullDayLength = (float)(Main.nightLength + Main.dayLength);
    public const float DayLength = 54000f;
    public const float NightLength = 32400f;
    public const int Target = 60;
    public const float LogicDeltaTime = 1f / Target;

    public static ulong UpdateCount { get; private set; }
    public static double GlobalTime { get; private set; }

    public static float RealTime => (float)(Main.time + (Main.dayTime ? 0.0 : Main.dayLength));
    public static double TimeForVisualEffects => Main.timeForVisualEffects / Target;

    public override void PostUpdateEverything() {
        UpdateCount++;
        GlobalTime += LogicDeltaTime;
    }
}
