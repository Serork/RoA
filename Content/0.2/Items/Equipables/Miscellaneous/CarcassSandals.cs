using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Legs)]
sealed class CarcassSandals : ModItem {
    public override void SetDefaults() {
        int width = 26, height = 16;
        Item.Size = new Vector2(width, height);
    }
}
