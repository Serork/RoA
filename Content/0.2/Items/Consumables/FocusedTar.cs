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
        switch (Main.rand.Next(3)) {
            case 0:
                hp = 0;
                damageModifier = 1f;
                break;
            case 1:
                damageModifier = 1f;
                defense = 0;
                break;
            case 2:
                hp = 0;
                defense = 0;
                break;
        }
        ItemCommon.TarEnchantmentStat tarEnchantmentStat = new(HP: hp, DamageModifier: damageModifier, Defense: defense);
        return tarEnchantmentStat;
    }
}
