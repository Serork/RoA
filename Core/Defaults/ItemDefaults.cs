using RoA.Content;

using Terraria;

namespace RoA.Core;

static class ItemDefaults {
    public static void SetSize(this Item item, int width, int height) {
        item.width = width;
        item.height = height;
    }

    public static void SetSize(this Item item, int width) => item.width = item.height = width;

    public static void SetDefaultToDruidicWeapon(this Item item) => item.DamageType = DruidClass.NatureDamage;

    public static void SetDefaultToDruidicWeapon(this Item item, int damage, float knockback = 0f) {
        item.SetDefaultToDruidicWeapon();

        item.SetWeaponValues(damage, knockback);
    }

    public static void SetDefaultToStackable(this Item item, int maxStack, bool consumable = true) {
        item.maxStack = maxStack;
        item.consumable = consumable;
    }

    public static void SetUsableValues(this Item item, int useStyleID, int useTime, int animationTime, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false) {
        item.useStyle = useStyleID;
        item.useTime = useTime;
        item.useAnimation = animationTime;
        item.noUseGraphic = !showItemOnUse;
        item.useTurn = useTurn;
        item.autoReuse = autoReuse;
    }

    public static void SetUsableValues(this Item item, int useStyleID, int timeToUse, bool showItemOnUse = true) => item.SetUsableValues(useStyleID, timeToUse, timeToUse, showItemOnUse);
}
