using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ScholarsArchiveTE : ModTileEntity {
    private static string ARCHIVEKEY => RoA.ModName + "spelltomesdata";

    [Flags]
    public enum ArchiveSpellTomeType : ushort {
        None = 0,
        Bookworms = 1 << 0,
        Bane = 1 << 1,
        BookofSkulls = 1 << 2,
        WaterBolt = 1 << 3,
        DemonScythe = 1 << 4,
        CrystalStorm = 1 << 5,
        CursedFlames = 1 << 6,
        GoldenShower = 1 << 7,
        MagnetSphere = 1 << 8,
        RazorbladeTyphoon = 1 << 9,
        LunarFlareBook = 1 << 10
    }

    public ArchiveSpellTomeType SpellTomes { get; private set; } = ArchiveSpellTomeType.None;

    public void InsertSpellTome(ArchiveSpellTomeType spellTome) => SpellTomes |= spellTome;

    public void DropSpellTome(ArchiveSpellTomeType spellTome) => SpellTomes &= ~spellTome;

    public bool HasSpellTome(ArchiveSpellTomeType spellTome) => SpellTomes.HasFlag(spellTome);

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        ushort tapperTileType = (ushort)ModContent.TileType<ScholarsArchive>();
        return tile.HasTile && tile.TileType == tapperTileType;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        int id = Place(i, j);
        return id;
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

    public override void SaveData(TagCompound tag) {
        tag[ARCHIVEKEY] = (short)SpellTomes;
    }

    public override void LoadData(TagCompound tag) {
        SpellTomes = (ArchiveSpellTomeType)tag.GetAsShort(ARCHIVEKEY);
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write((ushort)SpellTomes);
    }

    public override void NetReceive(BinaryReader reader) {
        SpellTomes = (ArchiveSpellTomeType)reader.ReadUInt16();
    }
}
