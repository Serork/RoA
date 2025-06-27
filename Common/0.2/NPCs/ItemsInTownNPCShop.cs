//namespace RoA.Common.NPCs;

//sealed class ItemsInTownNPCShopSystem : GlobalNPC {
//    private static readonly ushort[] _itemsAddedToTownNPCShopCount = new ushort[ItemLoader.ItemCount];
//    private static readonly ushort[][] _itemsAddedToTownNPCShop = new ushort[NPCLoader.NPCCount][];

//    public static void AddItemToTownNPCShop(ushort townNPCType, ushort itemTypeToAddInShop) {
//        _itemsAddedToTownNPCShop[townNPCType][_itemsAddedToTownNPCShopCount[townNPCType]++] = itemTypeToAddInShop;
//    }

//    public override void Load() {
//        for (int i = 0; i < _itemsAddedToTownNPCShop.Length; i++) {
//            _itemsAddedToTownNPCShop[i] = new ushort[ItemLoader.ItemCount];
//        }
//    }

//    public override void ModifyShop(NPCShop shop) {
//        ushort townNPCType = (ushort)shop.NpcType;
//        int itemCountThatShouldBeAddedInShop = _itemsAddedToTownNPCShopCount[townNPCType];
//        if (itemCountThatShouldBeAddedInShop == 0) {
//            return;
//        }

//        for (int i = 0; i < itemCountThatShouldBeAddedInShop; i++) {
//            shop.Add(_itemsAddedToTownNPCShop[townNPCType][i]);
//        }
//    }
//}
