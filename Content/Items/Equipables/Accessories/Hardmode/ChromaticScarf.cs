using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

// also see ExtraDrawLayerSupport.cs
[AutoloadEquip(EquipType.Neck)]
sealed class ChromaticScarf : ModItem {
    public static Asset<Texture2D> Neck1 { get; private set; } = null!;
    public static Asset<Texture2D> Neck2 { get; private set; }  = null!;
    public static Asset<Texture2D> Neck3 { get; private set; } = null!;

    public static int ChromaticScarfAsNeck { get; private set; } = -1;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        Neck1 = ModContent.Request<Texture2D>(Texture + "_Neck1");
        Neck2 = ModContent.Request<Texture2D>(Texture + "_Neck2");
        Neck3 = ModContent.Request<Texture2D>(Texture + "_Neck3");
    }

    public override void Load() {
        ChromaticScarfAsNeck = EquipLoader.AddEquipTexture(RoA.Instance, $"{Texture}_Neck", EquipType.Neck, this, Name + "_Neck");
    }


    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 24);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsChromaticScarfEffectActive = true;
    }
}
