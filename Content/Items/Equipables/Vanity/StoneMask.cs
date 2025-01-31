using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class StoneMask : ModItem {
	public override void SetStaticDefaults() {
		//Tooltip.SetDefault("'Cursed'");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 24; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(gold: 1);
		Item.vanity = true;
	}

    internal sealed class StoneMaskHandler : PlayerDrawLayer {
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo) {

        }
    }
}
