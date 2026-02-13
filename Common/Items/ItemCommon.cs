using Microsoft.Xna.Framework;

using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    public override bool InstancePerEntity => true;

    public delegate void UseItemDelegate(Item item, Player player);
    public static event UseItemDelegate UseItemEvent = null!;
    public override bool? UseItem(Item item, Player player) {
        UseItemEvent?.Invoke(item, player);

        return base.UseItem(item, player);
    }

    public delegate void ModifyShootStatsDelegate(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
    public static event ModifyShootStatsDelegate ModifyShootStatsEvent = null!;
    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        ModifyShootStatsEvent?.Invoke(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override void SetStaticDefaults() {
        VanillaSkullSetStaticDefaults();

        TarEnchantmentSetStaticDefaults();
    }

    public partial void TarEnchantmentSetStaticDefaults();
    public partial void VanillaSkullSetStaticDefaults();

    public override void Load() {
        VanillaSkullLoad();

        TarEnchantmentLoad();
    }

    public partial void TarEnchantmentLoad();
    public partial void VanillaSkullLoad(); 
    
    public override void UpdateEquip(Item item, Player player) {
        VanillaSkullUpdateEquip(item, player);

        TarEnchantmentUpdateEquip(item, player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
        VanillaSkullUpdateAccessory(item, player, hideVisual);

        VanillaSkullUpdateAccessory(item, player, hideVisual);
    }

    public partial void TarEnchantmentUpdateEquip(Item item, Player player);
    public partial void TarEnchantmentUpdateAccessory(Item item, Player player, bool hideVisual);

    public partial void VanillaSkullUpdateEquip(Item item, Player player);
    public partial void VanillaSkullUpdateAccessory(Item item, Player player, bool hideVisual);
}
