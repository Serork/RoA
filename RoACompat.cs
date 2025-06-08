using Terraria;
using Terraria.ModLoader;

namespace RoA;

sealed class RoACompat : ModSystem {
    private static Mod _riseOfAges;

    internal static Mod RiseOfAges {
        get {
            if (_riseOfAges == null && ModLoader.TryGetMod("RoA", out Mod mod)) {
                _riseOfAges = mod;
            }
            return _riseOfAges;
        }
    }

    public static bool IsRoAEnabled => RiseOfAges != null;

    public override void Unload() => _riseOfAges = null;

    internal static void MakeItemNature(Item item) => RiseOfAges?.Call("MakeItemNature", item);
    internal static void MakeItemNatureWeapon(Item item) => RiseOfAges?.Call("MakeItemNatureWeapon", item);
    internal static void SetNatureWeaponValues(Item item, ushort potentialDamage, float fillingRateModifier = 1f) {
        MakeItemNatureWeapon(item);
        RiseOfAges?.Call("SetNatureWeaponValues", item, potentialDamage, fillingRateModifier);
    }

    internal static ushort GetNatureWeaponBaseDamage(Item item, Player player) => (ushort)RiseOfAges?.Call("GetNatureWeaponBaseDamage", item, player);
    internal static ushort GetNatureWeaponBasePotentialDamage(Item item, Player player) => (ushort)RiseOfAges?.Call("GetNatureWeaponBasePotentialDamage", item, player);
    internal static ushort GetNatureWeaponCurrentDamage(Item item, Player player) => (ushort)RiseOfAges?.Call("GetNatureWeaponCurrentDamage", item, player);

    internal static void MakeProjectileNature(Projectile projectile) => RiseOfAges?.Call("MakeProjectileNature", projectile);
    internal static void SetNatureProjectileValues(Projectile projectile, bool shouldChargeWreathOnDamage = true, bool shouldApplyAttachedNatureWeaponCurrentDamage = true, float wreathFillingFine = 0f) {
        MakeProjectileNature(projectile);
        RiseOfAges?.Call("SetNatureProjectileValues", projectile, shouldChargeWreathOnDamage, shouldApplyAttachedNatureWeaponCurrentDamage, wreathFillingFine);
    }

    internal static void SetAttachedNatureWeaponToNatureProjectile(Projectile projectile, Item item) => RiseOfAges?.Call("SetAttachedNatureWeaponToNatureProjectile", projectile, item);
    internal static Item GetAttachedNatureWeaponToNatureProjectile(Projectile projectile) => (Item)RiseOfAges?.Call("GetAttachedNatureWeaponToNatureProjectile", projectile);
}
