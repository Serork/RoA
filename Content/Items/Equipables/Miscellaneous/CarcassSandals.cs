using Microsoft.Xna.Framework;

using RoA.Core.Defaults;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Legs)]
sealed class CarcassSandals : ModItem {
    public static int FemaleLegs { get; private set; } = -1;

    public override void Load() {
        FemaleLegs = EquipLoader.AddEquipTexture(Mod, $"{Texture}_Thin{EquipType.Legs}", EquipType.Legs, this, Name + "_Female");
    }

    public override void SetMatch(bool male, ref int equipSlot, ref bool robes) {
        equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
        if (!male) {
            equipSlot = EquipLoader.GetEquipSlot(Mod, Name + "_Female", EquipType.Legs);
        }
    }

    public override void SetDefaults() {
        int width = 26, height = 16;
        Item.SetSizeValues(width, height);
    }
}
