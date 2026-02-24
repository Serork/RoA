using RoA.Common;
using RoA.Core.Utility;

//using System;

//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace RoA.Content.Tiles.Plants;

//sealed class MiracleMintTE : ModTileEntity {
//    public float Counting { get; private set; }

//    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
//        if (Main.netMode == NetmodeID.MultiplayerClient) {
//            //NetMessage.SendTileSquare(Main.myPlayer, i, j);
//            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

//            return -1;
//        }

//        return Place(i, j);
//    }

//    public override void Update() {
//        Counting += (float)Math.Round(TimeSystem.LogicDeltaTime / 3f + Main.rand.NextFloatRange(0.015f), 2);

//        if (Counting >= 1f) {
//            Counting = 0f;
//        }
//    }

//    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Positions.X, Positions.Y, 0f, 0, 0, 0);

//    public override bool IsTileValidForEntity(int x, int y) => true;/*WorldGenHelper.GetTileSafely(x, y).ActiveTile(ModContent.TileType<Beacon>());*/
//}