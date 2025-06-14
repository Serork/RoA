using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

static class MousePositionStorageExtensions {
    public static void SyncMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().ShouldSyncMousePosition = true;
    public static Vector2 GetMousePosition(this Player player) => player.GetModPlayer<MousePositionStorage>().MousePosition;
}

sealed class MousePositionStorage : ModPlayer {
    private Vector2 _oldMouseWorld;

    internal bool ShouldSyncMousePosition;

    public Vector2 MousePosition { get; private set; }

    public override void PreUpdate() {
        void updateAndSyncMousePosition() {
            if (!Player.IsLocal()) {
                return;
            }

            MousePosition = Player.GetViableMousePosition();

            bool syncControls = false;
            if (ShouldSyncMousePosition && MathUtils.Approximately(MousePosition, _oldMouseWorld, 1E+1f)) {
                _oldMouseWorld = MousePosition;

                syncControls = true;
                ShouldSyncMousePosition = false;
            }

            if (syncControls) {
                MultiplayerSystem.SendPacket(new SyncMousePositionHandler(Player, MousePosition));
            }
        }

        updateAndSyncMousePosition();
    }

    private class SyncMousePositionHandler : NetPacket {
        public SyncMousePositionHandler(Player player, Vector2 mousePosition) {
            Writer.TryWriteSenderPlayer(player);
            Writer.WriteVector2(mousePosition);
        }

        public override void Read(BinaryReader reader, int sender) {
            if (!reader.TryReadSenderPlayer(sender, out Player player)) {
                return;
            }

            Vector2 mousePosition = reader.ReadVector2();   
            MousePositionStorage mousePositionStorage = player.GetModPlayer<MousePositionStorage>();
            mousePositionStorage.MousePosition = mousePosition;
        }
    }
}
