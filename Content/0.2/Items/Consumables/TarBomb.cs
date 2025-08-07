using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class TarBomb : ModItem {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    public override void SetStaticDefaults() {
        ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
        ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.shootSpeed = 5f;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.TarBomb>();
        Item.width = 22;
        Item.height = 26;
        Item.maxStack = Item.CommonMaxStack;
        Item.UseSound = SoundID.Item1;
        Item.consumable = true;
        Item.useAnimation = 25;
        Item.noUseGraphic = true;
        Item.useTime = 25;
        Item.value = Item.sellPrice(0, 0, 5);
        Item.rare = 1;
    }
}
