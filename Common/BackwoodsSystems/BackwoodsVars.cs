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

namespace RoA.Common.BackwoodsSystems;

sealed class BackwoodsVars : ModSystem {
    private static float _preDownedBossTimer;
    private static bool _backwoodsAwake;

    public static int FirstTileYAtCenter { get; internal set; }
    public static int BackwoodsTileForBackground { get; internal set; }

    public static HashSet<ushort> BackwoodsTileTypes { get; } = [(ushort)ModContent.TileType<LivingElderwood>(), (ushort)ModContent.TileType<LivingElderwoodlLeaves>(), (ushort)ModContent.TileType<TreeBranch>(), (ushort)ModContent.TileType<BackwoodsGrass>(), (ushort)ModContent.TileType<BackwoodsGreenMoss>(), (ushort)ModContent.TileType<BackwoodsStone>(), TileID.Dirt];

    public override void ClearWorld() => ResetAllFlags();

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(FirstTileYAtCenter)] = FirstTileYAtCenter;
        tag[nameof(BackwoodsTileForBackground)] = BackwoodsTileForBackground;
        tag[nameof(_preDownedBossTimer)] = _preDownedBossTimer;
        tag[nameof(_backwoodsAwake)] = _backwoodsAwake;
    }

    public override void LoadWorldData(TagCompound tag) {
        FirstTileYAtCenter = tag.GetInt(nameof(FirstTileYAtCenter));
        BackwoodsTileForBackground = tag.GetInt(nameof(BackwoodsTileForBackground));
        _preDownedBossTimer = tag.GetFloat(nameof(_preDownedBossTimer));
        _backwoodsAwake = tag.GetBool(nameof(_backwoodsAwake));
    }

    private static void ResetAllFlags() {
        FirstTileYAtCenter = BackwoodsTileForBackground = 0;
        _preDownedBossTimer = 0f;
        _backwoodsAwake = false;
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(FirstTileYAtCenter);
        writer.Write(BackwoodsTileForBackground);
        writer.Write(_preDownedBossTimer);
        writer.Write(_backwoodsAwake);
    }

    public override void NetReceive(BinaryReader reader) {
        FirstTileYAtCenter = reader.ReadInt32();
        BackwoodsTileForBackground = reader.ReadInt32();
        _preDownedBossTimer = reader.ReadSingle();
        _backwoodsAwake = reader.ReadBoolean();
    }

    public override void PostUpdateNPCs() {
        if (!_backwoodsAwake) {
            if (NPC.downedBoss2) {
                _backwoodsAwake = true;
            }

            return;
        }

        if (_preDownedBossTimer == -1f || Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }
        _preDownedBossTimer += TimeSystem.LogicDeltaTime;
        if (_preDownedBossTimer >= 5f) {
            _preDownedBossTimer = -1f;

            Color color = Color.LightGreen;
            string text1 = Language.GetText("Mods.RoA.World.BackwoodsFree").ToString();
            string text = text1.AddSpace() + Language.GetText("Mods.RoA.World.WorldEvil" + (WorldGen.crimson ? "1" : "2")).ToString().AddSpace() + Language.GetText("Mods.RoA.World.BackwoodsFreeLast");
            text += "...";
            Helper.NewMessage(text, color);

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
    }
}
