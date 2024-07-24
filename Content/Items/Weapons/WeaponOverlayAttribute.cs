using System;

namespace RoA.Content.Items.Weapons;

enum WeaponType {
    Claws
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
sealed class WeaponOverlayAttribute(WeaponType weaponType) : Attribute {
    public readonly WeaponType WeaponType = weaponType;
}
