using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ObjectData;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;
using RoA.Common;

using System;
using System.Collections.Generic;

namespace RoA.Content.Tiles.Plants;

sealed class MiracleMintTE : ModTileEntity {
    public float Counting { get; private set; }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        return Place(i, j);
    }

    public override void Update() {
        if (Main.netMode != NetmodeID.Server) {
            Counting += (float)Math.Round(TimeSystem.LogicDeltaTime / 3f + Main.rand.NextFloatRange(0.015f), 2);

            if (Counting >= 1f) {
                Counting = 0f;
            }
        }
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<MiracleMint>());
}

sealed class MiracleMint : Plant1x {
    protected override void SafeStaticDefaults() {
        Main.tileLighted[Type] = true;

        AddMapEntry(new Color(102, 243, 205), CreateMapEntryName());

        HitSound = SoundID.Grass;

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MiracleMintTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (IsGrown(i, j)) {
            yield return new Item(ModContent.ItemType<Items.Materials.MiracleMint>());
        }
    }

    protected override int[] AnchorValidTiles => [ModContent.TileType<BackwoodsGrass>()];

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<MiracleMintTE>().Place(i, j);
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<MiracleMintTE>().Kill(i, j);

    public override void NumDust(int i, int j, bool fail, ref int num) => num = IsGrown(i, j) ? Main.rand.Next(3, 6) : IsGrowing(i, j) ? Main.rand.Next(2, 5) : Main.rand.Next(1, 3);

    public override bool CreateDust(int i, int j, ref int type) {
        if (IsGrown(i, j)) {
            type = DustID.BlueTorch;
        }
        else {
            type = DustID.Grass;
        }

        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        float counting = TileHelper.GetTE<MiracleMintTE>(i, j).Counting;
        float factor = Math.Max(0.1f, (double)counting < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)counting) : 1f);
        float lightValue = (factor > 0.5f ? 1f - factor : factor) + 0.5f;
        if (IsGrown(i, j)) {
            r = 0.3f * lightValue;
            g = 0.6f * lightValue;
            b = 1.2f * lightValue;
        }
    }
}
