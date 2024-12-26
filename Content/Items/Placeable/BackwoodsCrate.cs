using Humanizer;

using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Content.Items.Weapons.Summon;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

sealed class BackwoodsCrate : ModItem {
    private class CatchThisCratePlayer : ModPlayer {
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
            bool inWater = !attempt.inLava && !attempt.inHoney;
            bool inBackwoods = Player.InModBiome<BackwoodsBiome>();
            if (inWater && inBackwoods && attempt.crate) {
                if (!attempt.veryrare && !attempt.legendary && attempt.rare) {
                    itemDrop = ModContent.ItemType<BackwoodsCrate>();
                }
            }
        }
    }

    public override void SetStaticDefaults() {
        ItemID.Sets.IsFishingCrate[Type] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
    }

    public override void SetDefaults() {
        Item.width = 34;
        Item.height = 34;
        Item.rare = 2;
        Item.maxStack = Item.CommonMaxStack;
        Item.createTile = ModContent.TileType<Tiles.Decorations.BackwoodsCrate>();
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.autoReuse = true;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = Item.sellPrice(0, 1);
    }

    public override bool CanRightClick() {
        return true;
    }

    public override void RightClick(Player player) {
        IEntitySource source = player.GetSource_OpenItem(Type);
        bool flag = ItemID.Sets.IsFishingCrateHardmode[Type];

        int type20;
        switch (Main.rand.Next(5)) {
            case 0:
                type20 = ModContent.ItemType<WandCore>();
                break;
            case 1:
                type20 = ModContent.ItemType<OvergrownSpear>();
                break;
            case 2:
                type20 = ModContent.ItemType<MothStaff>();
                break;
            case 3:
                type20 = ModContent.ItemType<DoubleFocusCharm>();
                break;
            default:
                type20 = ModContent.ItemType<BeastBow>();
                break;
        }

        int number37 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type20, 1, noBroadcast: false, -1);
        if (Main.netMode == 1)
            NetMessage.SendData(21, -1, -1, null, number37, 1f);

    }
}