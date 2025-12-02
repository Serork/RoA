using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class IceBlockDamagePacket : NetPacket {
    public IceBlockDamagePacket(Player player, int whoAmI, byte index) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(whoAmI);
        Writer.Write(index);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int whoAmI = reader.ReadInt32();
        byte index = reader.ReadByte();
        Main.projectile[whoAmI].As<IceBlock>().Damage(index);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new IceBlockDamagePacket(player, whoAmI, index), ignoreClient: sender);
        }
    }
}
