using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class RoAPlayer : ModPlayer {
    public static ushort CONTROLUSEITEMTIMECHECKBASE => 10;

    private ushort _controlUseItemTimer;

    public ushort ControlUseItemTimeCheck = CONTROLUSEITEMTIMECHECKBASE;
    public bool ControlUseItem;

    public delegate void PreUpdateDelegate(Player player);
    public static event PreUpdateDelegate PreUpdateEvent;
    public override void PreUpdate() {
        PreUpdateEvent?.Invoke(Player);
    }

    public override void PostUpdate() {
        if (Player.controlUseItem) {
            ControlUseItem = true;
        }
        else if (ControlUseItem) {
            if (_controlUseItemTimer++ > ControlUseItemTimeCheck) {
                ControlUseItem = false;
                _controlUseItemTimer = 0;
                ControlUseItemTimeCheck = CONTROLUSEITEMTIMECHECKBASE;
            }
        }
    }

    public static RoAPlayer GetHandler(Player player) => player.GetModPlayer<RoAPlayer>();
}
