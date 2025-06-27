using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Core.Defaults;

static partial class ItemDefaults {
    public static void DefaultToMagicWeapon(this Item item, bool staff = true, int shootType = -1, float shootSpeed = 0f) {
        item.useStyle = ItemUseStyleID.Shoot;
        item.noMelee = true;
        if (shootType == -1) {
            item.shoot = ProjectileID.WoodenArrowFriendly;
            item.shootSpeed = shootSpeed <= 0f ? 1f : shootSpeed;
        }
        else {
            item.shoot = shootType;
            item.shootSpeed = shootSpeed;
        }
        item.DamageType = Terraria.ModLoader.DamageClass.Magic;
        Item.staff[item.type] = staff;
    }

    public static void SetDefaultsToUsable(this Item item, int timeToUse, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false, SoundStyle? useSound = null) {
        item.SetUsableValues(item.useStyle, timeToUse, showItemOnUse, useTurn, autoReuse, useSound);
    }
}
