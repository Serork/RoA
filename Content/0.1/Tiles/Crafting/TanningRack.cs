using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Items.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class TanningRack : ModTile {
    public class TanningRackGeneratedCountStorage : ModSystem {
        public static int TanningRackCountInWorld;

        public override void PostWorldGen() {
            TanningRackCountInWorld = 0;
        }
    }

    public override void Load() {
        On_HouseBuilder.PlaceBiomeSpecificTool += On_HouseBuilder_PlaceBiomeSpecificTool;
    }

    private void On_HouseBuilder_PlaceBiomeSpecificTool(On_HouseBuilder.orig_PlaceBiomeSpecificTool orig, HouseBuilder self, HouseBuilderContext context) {
        orig(self, context);

        bool flag3 = WorldGen.genRand.NextChance(0.1);
        bool flag4 = TanningRackGeneratedCountStorage.TanningRackCountInWorld < WorldGen.genRand.Next(2, 5);
        if (flag4 || flag3) {
            bool flag2 = false;
            int type = ModContent.TileType<TanningRack>();
            foreach (Rectangle room2 in self.Rooms) {
                int num3 = room2.Height - 2 + room2.Y;
                for (int k = 0; k < 10; k++) {
                    int num4 = WorldGen.genRand.Next(2, room2.Width - 2) + room2.X;
                    WorldGen.PlaceTile(num4, num3, type, mute: true, forced: true);
                    if (flag2 = Main.tile[num4, num3].HasTile && Main.tile[num4, num3].TileType == type) {
                        break;
                    }
                }

                if (flag2) {
                    TanningRackGeneratedCountStorage.TanningRackCountInWorld++;
                    break;
                }

                for (int l = room2.X + 2; l <= room2.X + room2.Width - 2; l++) {
                    if (flag2 = WorldGen.PlaceTile(l, num3, type, mute: true, forced: true)) {
                        break;
                    }
                }

                if (flag2) {
                    TanningRackGeneratedCountStorage.TanningRackCountInWorld++;
                    break;
                }
            }
        }
    }

    private static ushort SkinningBuffType => (ushort)ModContent.BuffType<Skinning>();

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddMapEntry(new Color(193, 125, 83), CreateMapEntryName());

        DustType = -1;
    }

    //public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override bool RightClick(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        player.AddBuff(SkinningBuffType, 18000);
        SoundStyle leatherSound = new(ResourceManager.Sounds + "Leather") {
            PitchVariance = 0.1f,
            Volume = 1.5f
        };
        SoundEngine.PlaySound(leatherSound, player.GetViableMousePosition());
        return true;
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        if (player.HasBuff(SkinningBuffType)) {
            int[] leather = [ModContent.ItemType<AnimalLeather>(), ModContent.ItemType<RoughLeather>()];
            bool flag = leather.Contains(player.GetSelectedItem().type);
            if (flag) {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.Leather;
            }

            return;
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Crafting.TanningRack>();
    }
}
