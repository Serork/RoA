using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class ElderShell : ModItem {
    public static ushort BUFFTIME => (ushort)MathUtils.SecondsToFrames(10);

    public override void SetDefaults() {
        Item.DefaultToAccessory(26, 20);

        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsElderShellEffectActive = true;
    }
}
