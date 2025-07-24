using Microsoft.Xna.Framework;

using RoA.Content.Items.Food;
using RoA.Content.Items.Placeable.Miscellaneous;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Tiles.Solid.Backwoods;

using System.Reflection;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.Items;

sealed class ExtraItemsOnShakingTrees : ILoadable {
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "KillTile_GetItemDrops")]
    public extern static void WorldGen_KillTile_GetItemDrops(WorldGen self, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops = false);

    private static EntitySource_ShakeTree GetProjectileSource_ShakeTree(int x, int y) => new EntitySource_ShakeTree(x, y);
    private static EntitySource_ShakeTree GetItemSource_ShakeTree(int x, int y) => new EntitySource_ShakeTree(x, y);

    public void Load(Mod mod) {
        On_WorldGen.ShakeTree += On_WorldGen_ShakeTree;
    }

    private void On_WorldGen_ShakeTree(On_WorldGen.orig_ShakeTree orig, int i, int j) {
        WorldGen.GetTreeBottom(i, j, out var x, out var y);
        int num = y;
        TreeTypes treeType = WorldGen.GetTreeType(Main.tile[x, y].TileType);
        bool flag = Main.tile[x, y].TileType == ModContent.TileType<BackwoodsGrass>();
        if (!flag) {
            orig(i, j);
            return;
        }
        UnifiedRandom genRand = WorldGen.genRand;
        FieldInfo numTreeShakesReflect = typeof(WorldGen).GetField("numTreeShakes", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
        int numTreeShakes = (int)numTreeShakesReflect.GetValue(null);
        int maxTreeShakes = (int)typeof(WorldGen).GetField("maxTreeShakes", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(null);
        int[] treeShakeX = (int[])typeof(WorldGen).GetField("treeShakeX", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(null);
        int[] treeShakeY = (int[])typeof(WorldGen).GetField("treeShakeY", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(null);
        if (numTreeShakes == maxTreeShakes)
            return;

        if (treeType == TreeTypes.None && !flag)
            return;

        for (int k = 0; k < numTreeShakes; k++) {
            if (treeShakeX[k] == x && treeShakeY[k] == y)
                return;
        }

        treeShakeX[numTreeShakes] = x;
        treeShakeY[numTreeShakes] = y;
        numTreeShakesReflect.SetValue(null, ++numTreeShakes);
        y--;
        while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType]) {
            y--;
        }

        y++;
        if (!WorldGen.IsTileALeafyTreeTop(x, y) || Collision.SolidTiles(x - 2, x + 2, y - 2, y + 2))
            return;

        bool createLeaves = true;
        if (TileLoader.GlobalShakeTree(x, y, treeType)) { }
        else if (!PlantLoader.ShakeTree(x, y, Main.tile[x, num].TileType, ref createLeaves)) {

        }
        else if (Main.getGoodWorld && genRand.Next(17) == 0) {
            Projectile.NewProjectile(GetProjectileSource_ShakeTree(x, y), x * 16, y * 16, (float)Main.rand.Next(-100, 101) * 0.002f, 0f, 28, 0, 0f, Main.myPlayer, 16f, 16f);
        }
        else if (NPC.downedBoss2) {
            if (genRand.Next(300) == 0 && flag) {
                Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, ModContent.ItemType<LivingPrimordialWand>());
            }
            else if (genRand.Next(300) == 0 && flag) {
                Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, ModContent.ItemType<LivingPrimordialWand2>());
            }
        }
        else if (genRand.Next(300) == 0 && treeType == TreeTypes.Forest) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 832);
        }
        else if (genRand.Next(300) == 0 && treeType == TreeTypes.Forest) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 933);
        }
        else if (genRand.Next(200) == 0 && treeType == TreeTypes.Jungle) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 3360);
        }
        else if (genRand.Next(200) == 0 && treeType == TreeTypes.Jungle) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 3361);
        }
        else if (genRand.Next(1000) == 0 && treeType == TreeTypes.Forest) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 4366);
        }
        else if (genRand.Next(7) == 0 && (treeType == TreeTypes.Forest || treeType == TreeTypes.Snow || treeType == TreeTypes.Hallowed || treeType == TreeTypes.Ash)) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 27, genRand.Next(1, 3));
        }
        else if (genRand.Next(8) == 0 && treeType == TreeTypes.Mushroom) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 194, genRand.Next(1, 2));
        }
        else if (genRand.Next(35) == 0 && Main.halloween) {
            Item.NewItem(GetItemSource_ShakeTree(x, y), x * 16, y * 16, 16, 16, 1809, genRand.Next(1, 3));
        }
        else if (genRand.Next(12) == 0) {
            int dropItem = 0;
            WorldGen_KillTile_GetItemDrops(null, i, j, Main.tile[i, j], out dropItem, out var _, out var _, out var _);
            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, dropItem, genRand.Next(1, 4));
        }
        else if (genRand.Next(20) == 0) {
            int type = 71;
            int num2 = genRand.Next(50, 100);
            if (genRand.Next(30) == 0) {
                type = 73;
                num2 = 1;
                if (genRand.Next(5) == 0)
                    num2++;

                if (genRand.Next(10) == 0)
                    num2++;
            }
            else if (genRand.Next(10) == 0) {
                type = 72;
                num2 = genRand.Next(1, 21);
                if (genRand.Next(3) == 0)
                    num2 += genRand.Next(1, 21);

                if (genRand.Next(4) == 0)
                    num2 += genRand.Next(1, 21);
            }

            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, type, num2);
        }
        else if (genRand.Next(15) == 0 && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) {
            int type2;
            switch (genRand.Next(5)) {
                case 0:
                    type2 = 74;
                    break;
                case 1:
                    type2 = 297;
                    break;
                case 2:
                    type2 = 298;
                    break;
                case 3:
                    type2 = 299;
                    break;
                default:
                    type2 = 538;
                    break;
            }

            if (Player.GetClosestRollLuck(x, y, NPC.goldCritterChance) == 0f)
                type2 = ((genRand.Next(2) != 0) ? 539 : 442);

            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, type2);
        }
        else if (genRand.Next(50) == 0 && treeType == TreeTypes.Hallowed && !Main.dayTime) {
            int type3 = Main.rand.NextFromList(new short[3] {
                583,
                584,
                585
            });

            if (Main.tenthAnniversaryWorld && Main.rand.Next(4) != 0)
                type3 = 583;

            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, type3);
        }
        else if (genRand.Next(50) == 0 && treeType == TreeTypes.Forest && !Main.dayTime) {
            NPC obj = Main.npc[NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 611)];
            obj.velocity.Y = 1f;
            obj.netUpdate = true;
        }
        else if (genRand.Next(50) == 0 && treeType == TreeTypes.Jungle && Main.dayTime) {
            NPC obj2 = Main.npc[NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, Main.rand.NextFromList(new short[5] {
                671,
                672,
                673,
                674,
                675
            }))];

            obj2.velocity.Y = 1f;
            obj2.netUpdate = true;
        }
        else if (genRand.Next(40) == 0 && treeType == TreeTypes.Forest && !Main.dayTime && Main.halloween) {
            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 301);
        }
        else if (genRand.Next(50) == 0 && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) {
            for (int l = 0; l < 5; l++) {
                Point point = new Point(x + Main.rand.Next(-2, 2), y - 1 + Main.rand.Next(-2, 2));
                int type4 = ((Player.GetClosestRollLuck(x, y, NPC.goldCritterChance) != 0f) ? Main.rand.NextFromList(new short[3] {
                    74,
                    297,
                    298
                }) : 442);

                NPC obj3 = Main.npc[NPC.NewNPC(new EntitySource_ShakeTree(x, y), point.X * 16, point.Y * 16, type4)];
                obj3.velocity = Main.rand.NextVector2CircularEdge(3f, 3f);
                obj3.netUpdate = true;
            }
        }
        else if (genRand.Next(40) == 0 && treeType == TreeTypes.Jungle) {
            for (int m = 0; m < 5; m++) {
                Point point2 = new Point(x + Main.rand.Next(-2, 2), y - 1 + Main.rand.Next(-2, 2));
                NPC obj4 = Main.npc[NPC.NewNPC(new EntitySource_ShakeTree(x, y), point2.X * 16, point2.Y * 16, Main.rand.NextFromList(new short[2] {
                    210,
                    211
                }))];

                obj4.ai[1] = 65f;
                obj4.netUpdate = true;
            }
        }
        else if (genRand.Next(20) == 0 && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) {
            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 603);
        }
        else if (genRand.Next(30) == 0 && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) {
            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16 + 8, (y - 1) * 16, -22);
        }
        else if (genRand.Next(15) == 0 && flag) {
            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16 + 8, (y + 1) * 16, !NPC.downedBoss2 ? ModContent.NPCType<BabyFleder>() : ModContent.NPCType<Fleder>());
        }
        else if (genRand.Next(30) == 0 && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) {
            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16 + 8, (y - 1) * 16, -11);
        }
        else if (genRand.Next(30) == 0 && treeType == TreeTypes.Jungle && !Main.dayTime) {
            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 51);
        }
        else if (genRand.Next(40) == 0 && treeType == TreeTypes.Jungle) {
            Projectile.NewProjectile(GetProjectileSource_ShakeTree(x, y), x * 16 + 8, (y - 1) * 16, 0f, 0f, 655, 0, 0f, Main.myPlayer);
        }
        else if (genRand.Next(20) == 0 && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed) && !Main.raining && !NPC.TooWindyForButterflies && Main.dayTime) {
            int type5 = 356;
            if (Player.GetClosestRollLuck(x, y, NPC.goldCritterChance) == 0f)
                type5 = 444;

            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, type5);
        }
        else if (genRand.Next(20) == 0 && treeType == TreeTypes.Ash && y > Main.maxTilesY - 250) {
            int type6;
            switch (genRand.Next(3)) {
                case 0:
                    type6 = 654;
                    break;
                case 1:
                    type6 = 653;
                    break;
                default:
                    type6 = 655;
                    break;
            }

            NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, type6);
        }
        else if (Main.remixWorld && genRand.Next(20) == 0 && treeType == TreeTypes.Ash && y > Main.maxTilesY - 250) {
            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, 965, genRand.Next(20, 41));
        }
        else if (genRand.Next(12) == 0 && treeType == TreeTypes.Forest && !flag) {
            int type7;
            switch (genRand.Next(5)) {
                case 0:
                    type7 = 4009;
                    break;
                case 1:
                    type7 = 4293;
                    break;
                case 2:
                    type7 = 4282;
                    break;
                case 3:
                    type7 = 4290;
                    break;
                default:
                    type7 = 4291;
                    break;
            }

            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, type7);
        }
        else if (genRand.Next(6) == 0 && flag) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? ModContent.ItemType<Pistachio>() : ModContent.ItemType<Almond>(), source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && treeType == TreeTypes.Snow) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 4295 : 4286, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && treeType == TreeTypes.Jungle) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 4292 : 4294, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 4287 : 4283, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 4289 : 4284, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && (treeType == TreeTypes.Hallowed || treeType == TreeTypes.PalmHallowed)) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 4288 : 4297, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 4285 : 4296, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }
        else if (genRand.Next(12) == 0 && treeType == TreeTypes.Ash) {
            Item.NewItem(Type: (genRand.Next(2) != 0) ? 5278 : 5277, source: WorldGen.GetItemSource_FromTreeShake(x, y), X: x * 16, Y: y * 16, Width: 16, Height: 16);
        }

        if (!createLeaves)
            return;

        int treeHeight = 0;
        int treeFrame = 0;
        int passStyle = 0;
        WorldGen.GetTreeLeaf(x, Main.tile[x, y], Main.tile[x, num], ref treeHeight, out treeFrame, out passStyle);

        /*
		if (passStyle != -1) {
		*/
        if (passStyle > 0) {
            if (Main.netMode == 2)
                NetMessage.SendData(112, -1, -1, null, 1, x, y, 1f, passStyle);

            if (Main.netMode == 0)
                WorldGen.TreeGrowFX(x, y, 1, passStyle, hitTree: true);
        }
    }

    public void Unload() { }
}
