using RoA.Content.Projectiles.Friendly.Melee;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class BloodshedAxeHitDustPacket : NetPacket {
    public BloodshedAxeHitDustPacket(Player player, int identity, int whoAmI) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(identity);
        Writer.Write(whoAmI);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int identity = reader.ReadInt32();
        int whoAmI = reader.ReadInt32();

        BloodshedAxesTarget.Dusts(identity, whoAmI);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new BloodshedAxeHitDustPacket(player, identity, whoAmI), ignoreClient: sender);
        }
    }
}
