using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class EmptyPlanterBox : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
    }

    public override void SetDefaults() {
        int width = 28; int height = 16;
        Item.Size = new Microsoft.Xna.Framework.Vector2(width, height);

        Item.value = Item.buyPrice(0, 0, 1);
        Item.rare = ItemRarityID.White;
        Item.maxStack = Item.CommonMaxStack;
    }

    private sealed class AddRecipesSystem : ModSystem {
        public override void AddRecipes() {
            void addRecipe(short itemType) {
                Recipe result = Recipe.Create(itemType);
                result.AddIngredient(ModContent.ItemType<EmptyPlanterBox>(), 1);
                result.Register();
            }
            short[] planterBoxes = [ItemID.BlinkrootPlanterBox, ItemID.CorruptPlanterBox, ItemID.CrimsonPlanterBox, ItemID.DayBloomPlanterBox,
                                    ItemID.FireBlossomPlanterBox, ItemID.MoonglowPlanterBox, ItemID.ShiverthornPlanterBox, ItemID.WaterleafPlanterBox];
            foreach (short planterBox in planterBoxes) {
                addRecipe(planterBox);
            }
            for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
                ModItem item = ItemLoader.GetItem(i);
                if (item.Type == ModContent.ItemType<EmptyPlanterBox>()) {
                    continue;
                }
                if (item.GetType().ToString().Contains("PlanterBox")) {
                    addRecipe((short)item.Type);
                }
            }
        }
    }
}
