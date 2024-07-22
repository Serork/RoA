using System;

namespace RoA.Content.Items.Weapons;

enum WeaponType {
    Claws
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
sealed class WeaponAttribute(WeaponType weaponType) : Attribute {
    public readonly WeaponType WeaponType = weaponType;
}
