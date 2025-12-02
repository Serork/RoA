using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Tools;
sealed class DynastyWoodHammer : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 40; int height = width;
        Item.Size = new Vector2(width, height);

        Item.damage = 4;
        Item.DamageType = DamageClass.Melee;

        Item.useAnimation = 34;
        Item.useTime = 20;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;

        Item.knockBack = 5f;
        Item.hammer = 45;

        Item.value = Item.sellPrice(copper: 20);
        Item.rare = ItemRarityID.White;
        Item.UseSound = SoundID.Item1;
    }
}
