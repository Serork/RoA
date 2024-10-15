using RoA.Content.Buffs;
using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class CondorPacket : NetPacket {
    public CondorPacket(Player player, float wingsTime, bool noFallDmg, int wings, int wingsLogic, int wingTimeMax) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(wingsTime);
        Writer.Write(noFallDmg);
        Writer.Write(wings);
        Writer.Write(wingsLogic);
        Writer.Write(wingTimeMax);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        float wingsTime = reader.ReadSingle();
        bool noFallDmg = reader.ReadBoolean();
        int wings = reader.ReadInt32();
        int wingsLogic = reader.ReadInt32();
        int wingTimeMax = reader.ReadInt32();

        player.GetModPlayer<RodOfTheCondor.CondorWingsHandler>().ReceivePacket(wingsTime, noFallDmg, wings, wingsLogic, wingTimeMax);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new CondorPacket(player, wingsTime, noFallDmg, wings, wingsLogic, wingTimeMax), ignoreClient: sender);
        }
    }
}
