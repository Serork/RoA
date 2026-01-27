using System.IO;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common;

sealed class TimeSystem : ModSystem {
    public const float FULLDAYLENGTH = (float)(Main.nightLength + Main.dayLength);
    public const int TARGETFPS = 60;

    public static float RenderDeltaTime { get; private set; } = 1f / TARGETFPS;
    public static float LogicDeltaTime { get; private set; } = 1f / TARGETFPS;
    public static ulong UpdateCount { get; private set; }
    public static double GlobalTime { get; private set; }

    public static float RealTime => (float)(Main.time + (Main.dayTime ? 0.0 : Main.dayLength));
    public static float TimeForVisualEffects => (float)(Main.timeForVisualEffects / TARGETFPS);

    public override void PostUpdatePlayers() {
        UpdateCount++;
        GlobalTime += LogicDeltaTime;
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(UpdateCount);
    }

    public override void NetReceive(BinaryReader reader) {
        UpdateCount = reader.ReadUInt64();
    }

    public override void ClearWorld() {
        UpdateCount = 0;
    }

    public override void SaveWorldData(TagCompound tag) {
        tag[RoA.ModName + "updater" + nameof(UpdateCount)] = UpdateCount;
    }

    public override void LoadWorldData(TagCompound tag) {
        UpdateCount = tag.Get<ulong>(RoA.ModName + "updater" + nameof(UpdateCount));
    }
}
