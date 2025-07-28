using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Crossmod;

sealed class RoALiquidsCompat : ModSystem {
    private static Mod? _riseOfAgesLiquids;

    internal static Mod? RiseOfAgesLiquids {
        get {
            if (_riseOfAgesLiquids == null && ModLoader.TryGetMod("RoALiquids", out Mod mod)) {
                _riseOfAgesLiquids = mod;
            }
            return _riseOfAgesLiquids;
        }
    }

    public static bool IsRoALiquidsEnabled => RiseOfAgesLiquids != null;

    public override void Unload() => _riseOfAgesLiquids = null;

    internal static bool IsTarWet(NPC npc) => (bool)RiseOfAgesLiquids?.Call("IsTarWet", npc)!;
}
