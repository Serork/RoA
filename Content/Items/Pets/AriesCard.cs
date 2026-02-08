using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Pets;

sealed class AriesCard : ModItem {
    public override void SetDefaults() {
        Item.DefaultToVanitypet(ModContent.ProjectileType<Projectiles.Friendly.Pets.Aries>(), ModContent.BuffType<Buffs.Aries>());
        Item.value = Item.buyPrice(0, 50);
    }

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer) {
            player.AddBuff(Item.buffType, 3600);
        }

        return true;
    }
}
