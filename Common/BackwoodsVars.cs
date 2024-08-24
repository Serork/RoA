using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Utilities;

using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common;

sealed class BackwoodsVars : ModSystem {
    private static float _preDownedBoss2Timer;
    private static bool _backwoodsAwake, _backwoodsAwake2;

    public static int FirstTileYAtCenter { get; internal set; }
    public static int BackwoodsTileForBackground { get; internal set; }

    public static IReadOnlyList<ushort> BackwoodsSurfaceTileTypes { get; } = [(ushort)ModContent.TileType<TreeBranch>(), (ushort)ModContent.TileType<BackwoodsGrass>(), (ushort)ModContent.TileType<BackwoodsGreenMoss>(), (ushort)ModContent.TileType<BackwoodsStone>()];

    public override void ClearWorld() => ResetAllFlags();

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(FirstTileYAtCenter)] = FirstTileYAtCenter;
        tag[nameof(BackwoodsTileForBackground)] = BackwoodsTileForBackground;
        tag[nameof(_preDownedBoss2Timer)] = _preDownedBoss2Timer;
    }

    public override void LoadWorldData(TagCompound tag) {
        FirstTileYAtCenter = tag.GetInt(nameof(FirstTileYAtCenter));
        BackwoodsTileForBackground = tag.GetInt(nameof(BackwoodsTileForBackground));
        _preDownedBoss2Timer = tag.GetFloat(nameof(_preDownedBoss2Timer));
    }

    private static void ResetAllFlags() {
        FirstTileYAtCenter = BackwoodsTileForBackground = 0;
        _preDownedBoss2Timer = 0f;
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(FirstTileYAtCenter);
        writer.Write(BackwoodsTileForBackground);
        writer.Write(_preDownedBoss2Timer);
    }

    public override void NetReceive(BinaryReader reader) {
        FirstTileYAtCenter = reader.ReadInt32();
        BackwoodsTileForBackground = reader.ReadInt32();
        _preDownedBoss2Timer = reader.ReadSingle();
    }

    public override void PostUpdateNPCs() {
        if (!_backwoodsAwake) {
            if (NPC.downedBoss2) {
                _backwoodsAwake = true;
            }

            return;
        }

        if (_preDownedBoss2Timer == -1f) {
            return;
        }
        _preDownedBoss2Timer += TimeSystem.LogicDeltaTime;
        if (_preDownedBoss2Timer >= 5f) {
            _preDownedBoss2Timer = -1f;

            Color color = Color.LightGreen;
            string text1 = Language.GetText("Mods.RoA.World.BackwoodsFree").ToString();
            string text = text1.AddSpace() + Language.GetText("Mods.RoA.World.WorldEvil" + (WorldGen.crimson ? "1" : "2")).ToString().AddSpace() + Language.GetText("Mods.RoA.World.BackwoodsFreeLast");
            text += "...";
            Helper.NewMessage(text, color);
        }
    }
}
