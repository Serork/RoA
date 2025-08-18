using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

static class MousePositionStorageExtensions {
    public static void SyncMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().ShouldSyncMousePosition = true;
    public static Vector2 GetWorldMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().MousePosition;
}

sealed class MousePositionStorage : ModPlayer {
    private Vector2 _oldMouseWorld;

    internal bool ShouldSyncMousePosition;
    internal Vector2 MousePosition;

    public override void PreUpdate() {
        void updateAndSyncMousePosition() {
            if (!Player.IsLocal()) {
                return;
            }

            MousePosition = Player.GetViableMousePosition();

            bool syncControls = false;
            if (ShouldSyncMousePosition && MousePosition.Distance(_oldMouseWorld) > 10f) {
                _oldMouseWorld = MousePosition;

                syncControls = true;
                ShouldSyncMousePosition = false;
            }

            if (syncControls) {
                MultiplayerSystem.SendPacket(new SyncMousePositionPacket(Player, MousePosition));
            }
        }

        updateAndSyncMousePosition();
    } 
}
