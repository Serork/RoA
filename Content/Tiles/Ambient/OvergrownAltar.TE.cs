using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.WorldEvents;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
//using Terraria.Audio;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace RoA.Content.Tiles.Ambient;

//sealed class OvergrownAltarTE : ModTileEntity {
//    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
//        if (Main.netMode == NetmodeID.MultiplayerClient) {
//            NetMessage.SendTileSquare(Main.myPlayer, i, j);
//            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

//            return -1;
//        }

//        return Place(i, j);
//    }

//    public override void Update() {

//        //if (Main.netMode == NetmodeID.Server) {
//        //    NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
//        //}
//    }

//    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

//    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<OvergrownAltar>());
//}