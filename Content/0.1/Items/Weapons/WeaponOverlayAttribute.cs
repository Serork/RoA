using System;

namespace RoA.Content.Items.Weapons;

enum WeaponType {
    Claws
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
sealed class WeaponOverlayAttribute : Attribute {
    public readonly WeaponType WeaponType;
    public readonly uint? Hex;

    public WeaponOverlayAttribute(WeaponType weaponType, uint hex) {
        WeaponType = weaponType;
        Hex = hex;
    }

    public WeaponOverlayAttribute(WeaponType weaponType) {
        WeaponType = weaponType;
        Hex = null;
    }
}
