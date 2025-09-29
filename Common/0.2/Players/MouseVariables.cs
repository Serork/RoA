using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

static class MousePositionStorageExtensions {
    public static void SyncMousePosition(this Player player) => player.GetModPlayer<MouseVariables>().ShouldSyncMousePosition = true;
    public static void SyncCappedMousePosition(this Player player) => player.GetModPlayer<MouseVariables>().ShouldSyncCappedMousePosition = true;
    public static void SyncLMB(this Player player) => player.GetModPlayer<MouseVariables>().ShouldSyncLMB = true;
    public static bool HoldingLMB(this Player player) => player.GetModPlayer<MouseVariables>().HoldingLMB;
    public static Vector2 GetWorldMousePosition(this Player player) => player.GetModPlayer<MouseVariables>().MousePosition;
    public static Vector2 GetCappedWorldMousePosition(this Player player, float width, float height) {
        MouseVariables storage = player.GetModPlayer<MouseVariables>();
        storage.CappedMousePositionWidth = width;
        storage.CappedMousePositionHeight = height;
        return storage.CappedMousePosition;
    }
}

sealed class MouseVariables : ModPlayer {
    private Vector2 _oldMouseWorld, _oldCappedMouseWorld;
    private bool _oldHoldingLMB;

    internal float CappedMousePositionWidth;
    internal float CappedMousePositionHeight;

    internal bool ShouldSyncMousePosition, ShouldSyncCappedMousePosition, ShouldSyncLMB;

    public Vector2 MousePosition { get; internal set; }
    public Vector2 CappedMousePosition { get; internal set; }
    public bool HoldingLMB { get; internal set; }

    public delegate void OnHoldingLMBDelegate(Player player);
    public static event OnHoldingLMBDelegate? OnHoldingLMBEvent;

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
        void updateAndSyncMouseClicks() {
            if (!Player.IsLocal()) {
                return;
            }

            bool syncControls = false;
            if (Main.mouseLeft && !Main.mouseText) {
                HoldingLMB = true;
                OnHoldingLMBEvent?.Invoke(Player);
            }
            else {
                HoldingLMB = false;
            }

            if (ShouldSyncLMB && HoldingLMB != _oldHoldingLMB) {
                _oldHoldingLMB = HoldingLMB;

                syncControls = true;
                ShouldSyncLMB = false;
            }

            if (syncControls) {
                MultiplayerSystem.SendPacket(new SyncLMBPacket(Player, HoldingLMB));
            }
        }
        updateAndSyncMousePosition();
        updateAndSyncMouseClicks();
    }

    public override void PostUpdate() {
    }
}
