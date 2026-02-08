using Newtonsoft.Json.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Pets;

sealed class AriesCard : ModItem {
    public override void SetDefaults() {
        Item.DefaultToVanitypet(875, 274);
        Item.value = Item.buyPrice(0, 50);
    }
}
