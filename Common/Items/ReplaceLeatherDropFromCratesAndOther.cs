using RoA.Content.Buffs;
using RoA.Content.Items.Miscellaneous;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class ReplaceLeatherDropFromCratesAndOther : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_CommonCode.DropItem_Vector2_IEntitySource_int_int += On_CommonCode_DropItem_Vector2_IEntitySource_int_int;
    }

    private int On_CommonCode_DropItem_Vector2_IEntitySource_int_int(On_CommonCode.orig_DropItem_Vector2_IEntitySource_int_int orig, Microsoft.Xna.Framework.Vector2 position, Terraria.DataStructures.IEntitySource entitySource, int itemId, int stack) {
        if (itemId == ItemID.Leather) {
            itemId = ModContent.ItemType<AnimalLeather>();
            stack = 1;

            int whoAmI = orig(position, entitySource, itemId, stack);
            Main.item[whoAmI].GetGlobalItem<SpoilLeatherHandler>().StartSpoilingTime = TimeSystem.UpdateCount;

            return whoAmI;
        }

        return orig(position, entitySource, itemId, stack);
    }

    void ILoadable.Unload() { }
}