using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

// also see ExtraDrawLayerSupport.cs
sealed class ConjurersEye : ModItem {
    public static Asset<Texture2D> EyeTexture { get; private set; } = null!;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        EyeTexture = ModContent.Request<Texture2D>(Texture + "_Eye");
    }

    public override void SetDefaults() {
        Item.DefaultToAccessory(26, 26);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsConjurersEyeEffectActive = true;

        player.GetCommon().IsConjurersEyeEffectActive_Hidden = hideVisual;
    }

    public override void UpdateVanity(Player player) {
        player.GetCommon().ConjurersEyeVanity = true;
    }
}
