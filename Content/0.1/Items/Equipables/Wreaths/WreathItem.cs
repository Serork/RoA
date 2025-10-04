using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Utilities;

using static Terraria.GameContent.Creative.ItemFilters.AAccessories;

namespace RoA.Content.Items.Equipables.Wreaths;

abstract class WreathItem : NatureItem {
    public override bool? PrefixChance(int pre, UnifiedRandom rand) {
        if (pre == -3 || pre == -1) {
            return false;
        }

        return base.PrefixChance(pre, rand);
    }

    public override bool AllowPrefix(int pre) => false;

    protected void DefaultsToTier3Wreath() {
        Item.maxStack = 1;
        Item.rare = ItemRarityID.LightRed;

        Item.value = Item.sellPrice(0, 1, 50, 0);
    }

    internal interface IWreathGlowMask {
        public Color GlowColor { get; }
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => modded;

    public sealed override void SetStaticDefaults() {

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    protected sealed override void SafeSetDefaults2() {
        Item.accessory = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)640;
    }
}
