using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

static class MousePositionStorageExtensions {
    public static void SyncMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().ShouldSyncMousePosition = true;
    public static void SyncCappedMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().ShouldSyncCappedMousePosition = true;
    public static Vector2 GetWorldMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().MousePosition;
    public static Vector2 GetCappedWorldMousePosition(this Player player, float width, float height) {
        MousePositionStorage storage = player.GetModPlayer<MousePositionStorage>();
        storage.CappedMousePositionWidth = width;
        storage.CappedMousePositionHeight = height;
        return storage.CappedMousePosition;
    }
}

sealed class MousePositionStorage : ModPlayer {
    private Vector2 _oldMouseWorld, _oldCappedMouseWorld;

    internal float CappedMousePositionWidth;
    internal float CappedMousePositionHeight;

    internal bool ShouldSyncMousePosition, ShouldSyncCappedMousePosition;

    public Vector2 MousePosition { get; internal set; }
    public Vector2 CappedMousePosition { get; internal set; }

    public override void PreUpdate() {
        void updateAndSyncMousePosition() {
            if (!Player.IsLocal()) {
                return;
            }

            MousePosition = Player.GetViableMousePosition();
            CappedMousePosition = Player.GetViableMousePosition(CappedMousePositionWidth, CappedMousePositionHeight);

            bool syncControls = false, syncControls2 = false;
            if (ShouldSyncMousePosition && MousePosition.Distance(_oldMouseWorld) > 10f) {
                _oldMouseWorld = MousePosition;

                syncControls = true;
                ShouldSyncMousePosition = false;
            }

            if (ShouldSyncCappedMousePosition && CappedMousePosition.Distance(_oldCappedMouseWorld) > 10f) {
                _oldCappedMouseWorld = CappedMousePosition;

                syncControls2 = true;
                ShouldSyncCappedMousePosition = false;
            }

            if (syncControls) {
                MultiplayerSystem.SendPacket(new SyncMousePositionPacket(Player, MousePosition));
            }
            if (syncControls2) {
                MultiplayerSystem.SendPacket(new SyncMousePositionPacket2(Player, CappedMousePosition));
            }
        }

        updateAndSyncMousePosition();
    } 
}
