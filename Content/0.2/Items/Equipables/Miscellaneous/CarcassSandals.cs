using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Legs)]
sealed class CarcassSandals : ModItem {
    public static int Alternative { get; private set; } = -1;

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        Alternative = EquipLoader.AddEquipTexture(Mod, $"{Texture}_Thin{EquipType.Legs}", EquipType.Legs, name: "ThinLegs");
    }

    public override void SetMatch(bool male, ref int equipSlot, ref bool robes) {
        if (!male) {
            equipSlot = Alternative;
        }
    }

    public override void SetDefaults() {
        int width = 26, height = 16;
        Item.Size = new Vector2(width, height);
    }
}
