using RoA.Common.Druid.Forms;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

using static RoA.Common.Druid.Forms.BaseForm;

namespace RoA.Common.Networking.Packets;

sealed class BaseFormPacket1 : NetPacket {
    public BaseFormPacket1(Player player, float value) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(value);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        float value = reader.ReadSingle();
        BaseForm.BaseFormDataStorage.ChangeAttackCharge1(player, value, false);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new BaseFormPacket1(player, value), ignoreClient: sender);
        }
    }
}

sealed class BaseFormPacket2 : NetPacket {
    public BaseFormPacket2(Player player, float value) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(value);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        float value = reader.ReadSingle();
        player.GetModPlayer<BaseFormDataStorage>()._attackCharge2 = value;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new BaseFormPacket2(player, value), ignoreClient: sender);
        }
    }
}

