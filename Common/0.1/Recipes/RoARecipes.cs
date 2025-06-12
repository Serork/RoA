using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed partial class RoARecipes : ModSystem {
    public override void AddRecipes() {
        V01Recipes();
        V02Recipes();
    }
}
