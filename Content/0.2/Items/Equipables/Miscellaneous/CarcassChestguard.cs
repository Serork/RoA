using Microsoft.Xna.Framework;

using RoA.Common.Players;

using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Body)]
sealed class CarcassChestguard : ModItem {
    public override void SetStaticDefaults() {
        ExtraDrawLayerSupport.RegisterExtraDrawLayer(EquipType.Back, this);
    }

    public override void SetDefaults() {
        int width = 40, height = 26;
        Item.Size = new Vector2(width, height);
    }
}
