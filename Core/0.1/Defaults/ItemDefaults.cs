using RoA.Common.Items;
using RoA.Content;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Core.Defaults;

static partial class ItemDefaults {
    public static void SetSizeValues(this Item item, int width, int? height = null) {
        item.width = width;
        item.height = height ?? width;
    }

    public static void MakeItemNatureWeapon(this Item item) {
        if (item.IsAWeapon()) {
            item.SetDefaultsToNatureWeapon();
        }
    }

    public static void SetDefaultsToNatureWeapon(this Item item) => item.DamageType = DruidClass.Nature;

    public static void SetDefaultsToNatureWeapon(this Item item, int damage, float knockback = 0f) {
        item.SetDefaultsToNatureWeapon();
        item.SetWeaponValues(damage, knockback);
    }

    public static void SetDefaultsToStackable(this Item item, int maxStack, bool consumable = true) {
        item.maxStack = maxStack;
        item.consumable = consumable;
    }

    public static void SetDefaultsToUsable(this Item item, int vanillaUseStyleID, int useTime, int animationTime, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false, SoundStyle? useSound = null) {
        item.useStyle = vanillaUseStyleID;
        item.useTime = useTime;
        item.useAnimation = animationTime;
        item.noUseGraphic = !showItemOnUse;
        item.useTurn = useTurn;
        item.autoReuse = autoReuse;
        if (useSound != null) {
            item.UseSound = useSound;
        }
    }

    public static void SetUsableValues(this Item item, int vanillaUseStyleID, int timeToUse, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false, SoundStyle? useSound = null) 
        => item.SetDefaultsToUsable(vanillaUseStyleID, timeToUse, timeToUse, showItemOnUse, useTurn, autoReuse, useSound);

    public static void SetOtherValues(this Item item, int value, int rare = ItemRarityID.White) {
        item.value = value;
        item.rare = rare;
    }

    public static void SetShootableValues(this Item item, ushort shootType = 0, float shootSpeed = 1f, bool noMelee = true) {
        item.shoot = shootType == 0 ? (ushort)ProjectileID.WoodenArrowFriendly : shootType;
        item.shootSpeed = shootSpeed;
        item.noMelee = noMelee;
    }
}
