using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Accessories.Hardmode;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Balloon)]
sealed class FeathersInABalloon : NatureItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 22; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.accessory = true;
        Item.rare = ItemRarityID.LightRed;

        Item.value = Item.sellPrice(0, 3, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsFeathersInABalloonEffectActive = true;

        player.GetJumpState<FeathersInABottle.FeathersInABottleExtraJump>().Enable();
        player.jumpBoost = true;
    }

    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
        return !(equippedItem.type == ModContent.ItemType<ThunderKingsGrace>() && player.GetCommon().IsThunderKingsGraceEffectActive) &&
            !(equippedItem.type == ModContent.ItemType<FeathersInABottle>() && player.GetCommon().IsFeathersInABottleEffectActive);
    }
}