using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class RoAPlayer : ModPlayer {
    public delegate void PreUpdateDelegate(Player player);
    public static event PreUpdateDelegate PreUpdateEvent;
    public override void PreUpdate() {
        PreUpdateEvent?.Invoke(Player);
    }
}
