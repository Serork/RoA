using RoA.Content;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Core.Defaults;

static class ItemDefaults {
    public static void SetSize(this Item item, int width, int height) {
        item.width = width;
        item.height = height;
    }

    public static void SetSize(this Item item, int width) => item.width = item.height = width;

    public static void MakeItemNatureWeapon(this Item item) {
        if (item.IsAWeapon()) {
            item.SetDefaultsToNatureWeapon();
        }
    }

    public static void SetDefaultsToNatureWeapon(this Item item) => item.DamageType = DruidClass.NatureDamage;

    public static void SetDefaultsToNatureWeapon(this Item item, int damage, float knockback = 0f) {
        item.SetDefaultsToNatureWeapon();
        item.SetWeaponValues(damage, knockback);
    }

    public static void SetDefaultsToStackable(this Item item, int maxStack, bool consumable = true) {
        item.maxStack = maxStack;
        item.consumable = consumable;
    }

    public static void SetDefaultsToUsable(this Item item, int useStyleID, int useTime, int animationTime, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false, SoundStyle? useSound = null) {
        item.useStyle = useStyleID;
        item.useTime = useTime;
        item.useAnimation = animationTime;
        item.noUseGraphic = !showItemOnUse;
        item.useTurn = useTurn;
        item.autoReuse = autoReuse;
        if (useSound != null) {
            item.UseSound = useSound;
        }
    }

    public static void SetDefaultsToUsable(this Item item, int useStyleID, int timeToUse, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false, SoundStyle? useSound = null) => item.SetDefaultsToUsable(useStyleID, timeToUse, timeToUse, showItemOnUse, useTurn, autoReuse, useSound);

    public static void SetDefaultsOthers(this Item item, int value, int rare = ItemRarityID.White) {
        item.value = value;
        item.rare = rare;
    }

    public static void SetDefaultsToShootable(this Item item, ushort shootType, float shootSpeed = 0f, bool noMelee = true) {
        item.shoot = shootType;
        item.shootSpeed = shootSpeed;
        item.noMelee = noMelee;
    }
}
