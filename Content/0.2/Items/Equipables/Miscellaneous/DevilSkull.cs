using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class DevilSkull : ModItem {
    public override void SetStaticDefaults() {
    }

    public override void SetDefaults() {
        int width = 24; int height = 26;
        Item.Size = new Vector2(width, height);
    }
}
