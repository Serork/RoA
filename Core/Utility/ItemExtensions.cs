using System;
using System.Reflection;

using Terraria;
using Terraria.ID;

namespace RoA.Core.Utility;

static class ItemExtensions {
    public static bool IsAWeapon(this Item item) => item.damage > 0;

    public static bool IsEmpty(this Item item) => item == null || item.stack <= 0 || item.type <= ItemID.None || item.IsAir;

    public static T GetAttribute<T>(this Item item) where T : Attribute => item.ModItem?.GetType().GetCustomAttribute<T>();
}
