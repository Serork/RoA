using Terraria.ModLoader;

namespace RoA.Common.Hooks;

sealed partial class Hooks : ModSystem {
    public override void Load() {
        LoadPlayerHooks();
    }

    public partial void LoadPlayerHooks();
}
