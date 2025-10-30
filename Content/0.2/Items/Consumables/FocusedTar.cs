using RoA.Common.Items;

using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Items.Consumables;

class FocusedTar : ModItem {
    public override void SetDefaults() {
        Item.consumable = true;
        Item.width = 28;
        Item.height = 28;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 2;
    }

    public virtual ItemCommon.TarEnchantmentStat GetAppliedEnchantment() {
        UnifiedRandom random = Main.rand;
        ushort[] hpOptions = [5, 10, 20];
        ushort hp = random.NextFromList(hpOptions);
        float[] damageModifierOptions = [1.04f, 1.08f, 1.1f];
        float damageModifier = random.NextFromList(damageModifierOptions);
        ushort[] defenseOptions = [1, 2, 4];
        ushort defense = random.NextFromList(defenseOptions);
        bool shouldGiveHP = Main.rand.NextBool();
        bool shouldGiveDamage = Main.rand.NextBool();
        bool shouldGiveDefense = Main.rand.NextBool();
        while (!shouldGiveHP && !shouldGiveDamage && !shouldGiveDefense) {
            shouldGiveHP = Main.rand.NextBool();
            shouldGiveDamage = Main.rand.NextBool();
            shouldGiveDefense = Main.rand.NextBool();
        }
        ItemCommon.TarEnchantmentStat tarEnchantmentStat = new(HP: (ushort)(shouldGiveHP ? hp : 0), DamageModifier: shouldGiveDamage ? damageModifier : 1f, Defense: (ushort)(shouldGiveDefense ? defense : 0));
        return tarEnchantmentStat;
    }
}
