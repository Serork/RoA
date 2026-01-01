using Microsoft.Xna.Framework;

using RoA.Content.Items.Weapons.Magic;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ScholarsArchiveTE : ModTileEntity {
    private static string ARCHIVEKEY => RoA.ModName + "spelltomesdata";

    public readonly static int[] SpellTomeTypes = [
        ModContent.ItemType<Bookworms>(),
        ModContent.ItemType<Bane>(),
        ItemID.BookofSkulls,
        ItemID.WaterBolt,
        ItemID.DemonScythe,
        ItemID.CrystalStorm,
        ItemID.CursedFlames,
        ItemID.GoldenShower,
        ItemID.MagnetSphere,
        ItemID.RazorbladeTyphoon,
        ItemID.LunarFlareBook
    ];

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

    public static Color GetColorPerSpellTome(int itemType) {
        if (itemType == ModContent.ItemType<Bookworms>()) {
            return new(172, 145, 147);
        }
        if (itemType == ModContent.ItemType<Bane>()) {
            return new(214, 149, 255);
        }
        if (itemType == ItemID.BookofSkulls) {
            return new(217, 203, 174);
        }
        if (itemType == ItemID.WaterBolt) {
            return new(115, 176, 247);
        }
        if (itemType == ItemID.DemonScythe) {
            return new(209, 123, 224);
        }
        if (itemType == ItemID.CrystalStorm) {
            return new(157, 170, 204);
        }
        if (itemType == ItemID.CursedFlames) {
            return new(238, 252, 148);
        }
        if (itemType == ItemID.GoldenShower) {
            return new(255, 251, 166);
        }
        if (itemType == ItemID.MagnetSphere) {
            return new(2, 254, 201);
        }
        if (itemType == ItemID.RazorbladeTyphoon) {
            return new(48, 248, 171);
        }
        if (itemType == ItemID.LunarFlareBook) {
            return new(185, 255, 254);
        }
        return Color.White;
    }

    public static bool IsSpellTome(int itemType) => SpellTomeTypes.Contains(itemType);

    public static int[] GetSpellTomeItemTypes(ScholarsArchiveTE scholarsArchiveTE) {
        HashSet<int> spellTomeItemTypes = [];
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.Bookworms)) {
            spellTomeItemTypes.Add(ModContent.ItemType<Bookworms>());
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.Bane)) {
            spellTomeItemTypes.Add(ModContent.ItemType<Bane>());
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.BookofSkulls)) {
            spellTomeItemTypes.Add(ItemID.BookofSkulls);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.WaterBolt)) {
            spellTomeItemTypes.Add(ItemID.WaterBolt);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.DemonScythe)) {
            spellTomeItemTypes.Add(ItemID.DemonScythe);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.CrystalStorm)) {
            spellTomeItemTypes.Add(ItemID.CrystalStorm);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.CursedFlames)) {
            spellTomeItemTypes.Add(ItemID.CursedFlames);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.GoldenShower)) {
            spellTomeItemTypes.Add(ItemID.GoldenShower);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.MagnetSphere)) {
            spellTomeItemTypes.Add(ItemID.MagnetSphere);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.RazorbladeTyphoon)) {
            spellTomeItemTypes.Add(ItemID.RazorbladeTyphoon);
        }
        if (scholarsArchiveTE.HasSpellTome(ArchiveSpellTomeType.LunarFlareBook)) {
            spellTomeItemTypes.Add(ItemID.LunarFlareBook);
        }
        return [.. spellTomeItemTypes];
    }

    public static int GetSpellTomeItemType(ArchiveSpellTomeType spellTome) {
        int result = -1;
        switch (spellTome) {
            case ArchiveSpellTomeType.Bookworms:
                result = ModContent.ItemType<Bookworms>();
                break;
            case ArchiveSpellTomeType.Bane:
                result = ModContent.ItemType<Bane>();
                break;
            case ArchiveSpellTomeType.BookofSkulls:
                result = ItemID.BookofSkulls;
                break;
            case ArchiveSpellTomeType.WaterBolt:
                result = ItemID.WaterBolt;
                break;
            case ArchiveSpellTomeType.DemonScythe:
                result = ItemID.DemonScythe;
                break;
            case ArchiveSpellTomeType.CrystalStorm:
                result = ItemID.CrystalStorm;
                break;
            case ArchiveSpellTomeType.CursedFlames:
                result = ItemID.CursedFlames;
                break;
            case ArchiveSpellTomeType.GoldenShower:
                result = ItemID.GoldenShower;
                break;
            case ArchiveSpellTomeType.MagnetSphere:
                result = ItemID.MagnetSphere;
                break;
            case ArchiveSpellTomeType.RazorbladeTyphoon:
                result = ItemID.RazorbladeTyphoon;
                break;
            case ArchiveSpellTomeType.LunarFlareBook:
                result = ItemID.LunarFlareBook;
                break;
        }
        return result;
    }

    public ArchiveSpellTomeType SpellTomes { get; private set; } = ArchiveSpellTomeType.None;

    public void InsertSpellTome(ArchiveSpellTomeType spellTome) => SpellTomes |= spellTome;

    public void DropSpellTome(ArchiveSpellTomeType spellTome) => SpellTomes &= ~spellTome;

    public bool HasSpellTome(ArchiveSpellTomeType spellTome) => SpellTomes.HasFlag(spellTome);

    public bool HasAnySpellTome() => (ushort)SpellTomes > 0;

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
