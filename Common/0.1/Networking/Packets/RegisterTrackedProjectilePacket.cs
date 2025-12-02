using RoA.Common.Projectiles;
using RoA.Core.Utility;

using System.IO;

using Terraria;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Networking.Packets;

sealed class RegisterTrackedProjectilePacket : NetPacket {
    public RegisterTrackedProjectilePacket(Player player, int whoAmI) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(whoAmI);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int whoAmI = reader.ReadInt32();
        if (Main.projectile[whoAmI].IsModded(out ModProjectile modProjectile)) {
            TrackedEntitiesSystem.RegisterTrackedEntity(modProjectile);
        }

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new RegisterTrackedProjectilePacket(player, whoAmI), ignoreClient: sender);
        }
    }
}
