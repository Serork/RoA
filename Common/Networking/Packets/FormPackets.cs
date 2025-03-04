﻿using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

using static RoA.Content.Forms.FlederForm;

namespace RoA.Common.Networking.Packets;

sealed class InsectFormPacket1 : NetPacket {
    public InsectFormPacket1(Player player, bool value) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(value);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        bool value = reader.ReadBoolean();

        var handler = player.GetModPlayer<InsectForm.InsectFormHandler>();
        handler._facedRight = value;
        handler._directionChangedFor = 1f;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new InsectFormPacket1(player, value), ignoreClient: sender);
        }
    }
}

sealed class InsectFormPacket2 : NetPacket {
    public InsectFormPacket2(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        player.GetModPlayer<InsectForm.InsectFormHandler>()._facedRight = null;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new InsectFormPacket2(player), ignoreClient: sender);
        }
    }
}

sealed class FlederFormPacket1 : NetPacket {
    public FlederFormPacket1(Player player, IDoubleTap.TapDirection direction) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write((sbyte)direction);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        IDoubleTap.TapDirection direction = (IDoubleTap.TapDirection)reader.ReadSByte();

        player.GetModPlayer<FlederFormHandler>().UseFlederDash(direction);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new FlederFormPacket1(player, direction), ignoreClient: sender);
        }
    }
}

sealed class FlederFormPacket2 : NetPacket {
    public FlederFormPacket2(Player player, bool state) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(state);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        bool state = reader.ReadBoolean();

        player.GetModPlayer<FlederFormHandler>()._holdingLmb = state;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new FlederFormPacket2(player, state), ignoreClient: sender);
        }
    }
}



sealed class FormPacket1 : NetPacket {
    public FormPacket1(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        player.GetModPlayer<WreathHandler>().Reset1();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new FormPacket1(player), ignoreClient: sender);
        }
    }
}

