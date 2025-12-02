using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class ResetEffectsManager : ModPlayer {
    public delegate void ResetEffectsDelegate();
    public static event ResetEffectsDelegate? ResetEffectsEvent;

    public override void ResetEffects() {
        ResetEffectsEvent?.Invoke();
    }

    public override void Unload() => ResetEffectsEvent = null;
}
