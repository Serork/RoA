using RoA.Common.CustomSkyAmbience;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class CustomAmbiencePacket : NetPacket {
    public CustomAmbiencePacket(Player player, CustomSkyEntityType type, int seed) {
        Writer.TryWriteSenderPlayer(player);
        int value = seed;
        Writer.Write(value);
        Writer.Write((byte)type);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int seed = reader.ReadInt32();
        CustomSkyEntityType type = (CustomSkyEntityType)reader.ReadByte();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new CustomAmbiencePacket2(player, type, seed));
        }
    }
}

sealed class CustomAmbiencePacket2 : NetPacket {
    public CustomAmbiencePacket2(Player player, CustomSkyEntityType type, int seed) {
        Writer.TryWriteSenderPlayer(player);
        int value = seed;
        Writer.Write(value);
        Writer.Write((byte)type);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int seed = reader.ReadInt32();
        CustomSkyEntityType type = (CustomSkyEntityType)reader.ReadByte();

        if (!Main.dedServ) {
            Main.QueueMainThreadAction(delegate {
                ((CustomAmbientSky)SkyManager.Instance["CustomAmbience"]).Spawn(player, type, seed);
            });
        }
    }
}
