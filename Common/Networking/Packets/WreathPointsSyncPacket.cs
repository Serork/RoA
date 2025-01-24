using RoA.Common.Druid.Wreath;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class WreathPointsSyncPacket : NetPacket {
    public WreathPointsSyncPacket(byte who, ushort resource) {
        Writer.Write(who);
        Writer.Write(resource);
    }

    public override void Read(BinaryReader reader, int sender) {
        byte who = reader.ReadByte();
        ushort resource = reader.ReadUInt16();
        WreathHandler handler = Main.player[who].GetModPlayer<WreathHandler>();
        handler.ReceivePlayerSync(resource);
        if (Main.netMode == NetmodeID.Server) {
            handler.SyncPlayer(-1, sender, false);
        }
    }
}

sealed class WreathPointsSyncPacket2 : NetPacket {
    public WreathPointsSyncPacket2(Player player, ushort resource) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(resource);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        ushort resource = reader.ReadUInt16();
        WreathHandler handler = player.GetModPlayer<WreathHandler>();
        handler.ReceivePlayerSync(resource);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new WreathPointsSyncPacket2(player, resource), ignoreClient: sender);
        }
    }
}
