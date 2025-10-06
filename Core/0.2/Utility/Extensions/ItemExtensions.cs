using RoA.Content.Items.Weapons.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility.Extensions;

static partial class ItemExtensions {
    public static bool IsNatureClaws(this Item item, out ClawsBaseItem clawsBaseItem) {
        bool result = item.IsModded(out ModItem modItem) && modItem is ClawsBaseItem;
        if (result) {
            clawsBaseItem = (modItem as ClawsBaseItem)!;
        }
        else {
            clawsBaseItem = null!;
        }
        return result;
    }

    public static bool IsClaws(this Item item) => item.IsModded(out ModItem modItem) && modItem is ClawsBaseItem;

    public static bool IsModded(this Item item) => item.ModItem is not null;
    public static bool IsModded(this Item item, out ModItem modItem) {
        modItem = item.ModItem;
        return modItem is not null;
    }

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

    public static void SetDefaultsToUsable(this Item item, int timeToUse, bool showItemOnUse = true, bool useTurn = false, bool autoReuse = false, SoundStyle? useSound = null) => item.SetUsableValues(item.useStyle, timeToUse, showItemOnUse, useTurn, autoReuse, useSound);
}
