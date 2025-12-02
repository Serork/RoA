using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class PhoenixDashPacket : NetPacket {
    public PhoenixDashPacket(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        var plr = player.GetFormHandler();
        plr.Prepared = false;
        plr.Dashed2 = plr.Dashed = true;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PhoenixDashPacket(player), ignoreClient: sender);
        }
    }
}

sealed class FlederDashPacket : NetPacket {
    public FlederDashPacket(Player player, IDoubleTap.TapDirection direction) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write((sbyte)direction);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        IDoubleTap.TapDirection direction = (IDoubleTap.TapDirection)reader.ReadSByte();

        player.GetFormHandler().UseFlederDash(direction, true);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new FlederDashPacket(player, direction), ignoreClient: sender);
        }
    }
}

sealed class ResetFormPacket : NetPacket {
    public ResetFormPacket(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        player.GetWreathHandler().Reset1();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new ResetFormPacket(player), ignoreClient: sender);
        }
    }
}

