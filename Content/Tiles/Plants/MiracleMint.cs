using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Content.Dusts;
using RoA.Content.Items.Placeable.Seeds;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Plants;

sealed class MiracleMint : PlantBase, TileHooks.IGrowPlantRandom {
    void TileHooks.IGrowPlantRandom.OnGlobalRandomUpdate(int i, int j) {
        if (WorldGen.gen) {
            return;
        }

        TryPlacePlant2(i, j, Type, 0, onPlaced: (position) => {
            //int x = position.X, y = position.Y;
            //ModContent.GetInstance<MiracleMintTE>().Place(x, y);
            //if (Main.netMode != NetmodeID.SinglePlayer) {
            //    MultiplayerSystem.SendPacket(new PlaceMiracleMintTEPacket(x, y));
            //}
        }, validTiles: [ModContent.TileType<BackwoodsGrass>()]);
    }

    protected override void SafeSetStaticDefaults() {
        Main.tileLighted[Type] = true;

        AddMapEntry(new Color(102, 243, 205), CreateMapEntryName());

        HitSound = SoundID.Grass;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.MiracleMint>();
    }

    protected override bool CanBloom() => Main.GetMoonPhase() == Terraria.Enums.MoonPhase.Full;

    protected override int PlantDrop => DropItem;

    protected override int SeedsDrop => (ushort)ModContent.ItemType<MiracleMintSeeds>();

    //protected override void PreAddNewTile() {
    //    TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MiracleMintTE>().Hook_AfterPlacement, -1, 0, false);
    //}

    protected override int[] AnchorValidTiles => [ModContent.TileType<BackwoodsGrass>()];

    //public override void PlaceInWorld(int i, int j, Item item) {
    //    ModContent.GetInstance<MiracleMintTE>().Place(i, j);
    //    if (Main.netMode != NetmodeID.SinglePlayer) {
    //        MultiplayerSystem.SendPacket(new PlaceMiracleMintTEPacket(i, j));
    //    }
    //}

    //public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
    //    if (Main.netMode != NetmodeID.Server) {
    //        if (!fail) {
    //            ModContent.GetInstance<MiracleMintTE>().Kill(i, j);
    //            if (Main.netMode != NetmodeID.SinglePlayer) {
    //                MultiplayerSystem.SendPacket(new RemoveMiracleTileEntityOnServerPacket(i, j));
    //            }
    //        }
    //    }
    //}

    public override bool CreateDust(int i, int j, ref int type) {
        if (IsGrown(i, j) && Main.rand.NextBool(3)) {
            type = ModContent.DustType<MiracleMintDust>();
        }
        else {
            type = ModContent.DustType<Dusts.Backwoods.Grass>();
        }

        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        //MiracleMintTE tileEntity = TileHelper.GetTE<MiracleMintTE>(i, j);
        //if (tileEntity == null) {
        //    return;
        //}
        ulong seed = (ulong)(i * j);
        float random = Utils.RandomFloat(ref seed);
        float counting = Helper.Repeat(AltarHandler.MiracleMintCounting + ((float)i / Main.maxTilesX * (j / Main.maxTilesY)) * random + random * i + random * j, 1f);
        float factor = Math.Max(0.1f, (double)counting < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)counting) : 1f);
        float lightValue = (factor > 0.5f ? 1f - factor : factor) + 0.5f;
        lightValue *= 0.85f;
        if (IsGrown(i, j)) {
            r = 0.3f * lightValue;
            g = 0.6f * lightValue;
            b = 1.2f * lightValue;

            if (Main.rand.NextBool(50)) {
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, ModContent.DustType<MiracleMintDust>());
            }
        }
    }
}
