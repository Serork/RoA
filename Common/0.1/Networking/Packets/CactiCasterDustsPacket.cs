using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Networking.Packets;

sealed class CactiCasterDustsPacket : NetPacket {
    public CactiCasterDustsPacket(Player player, Vector2 corePosition) {
        Writer.TryWriteSenderPlayer(player);
        Writer.WriteVector2(corePosition);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        Vector2 corePosition = reader.ReadVector2();

        if (!Main.dedServ) {
            for (int i = 0; i < 15; i++) {
                int dust = Dust.NewDust(corePosition, 4, 4, ModContent.DustType<CactiCasterDust>(), Main.rand.Next(-50, 51) * 0.05f, Main.rand.Next(-50, 51) * 0.05f, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
            }
        }

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new CactiCasterDustsPacket(player, corePosition), ignoreClient: sender);
        }
    }
}
