using RoA.Content.Items;

using System;
using System.Reflection;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class ItemExtensions {
    public static bool IsAWeapon(this Item item) => item.damage > 0;

    public static bool IsEmpty(this Item item) => item == null || item.stack <= 0 || item.type <= ItemID.None || item.IsAir;

    public static T GetAttribute<T>(this Item item) where T : Attribute => item.ModItem?.GetType().GetCustomAttribute<T>();

    public static bool IsNature(this Item item) => item.ModItem is NatureItem || CrossmodNatureContent.IsItemNature(item);

    public static bool IsANatureWeapon(this Item item) => item.IsNature() && item.IsAWeapon();

    public static T As<T>(this Item item) where T : ModItem => item.ModItem as T;
}
