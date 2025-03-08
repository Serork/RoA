using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Back)]
sealed class PeegeonCape : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Peegeon's Cape");
        // Tooltip.SetDefault("Allows flight\n'Great for impersonating RoA devs?' Sure!");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 24; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.accessory = true;

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    //public override void UpdateAccessory (Player player, bool hideVisual) {
    //	player.rocketBoots = player.vanityRocketBoots = 2;
    //	player.rocketTimeMax = 8;
    //	if (player.name == "peege.on") {
    //		player.rocketTimeMax = int.MaxValue;
    //		player.moveSpeed += 0.5f;
    //		player.maxRunSpeed += 25f;
    //		player.iceSkate = true;
    //		player.noFallDmg = true;
    //	}
    //}
}