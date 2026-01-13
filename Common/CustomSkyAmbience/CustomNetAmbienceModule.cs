using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Net;

namespace RoA.Common.CustomSkyAmbience;

sealed class CustomNetAmbienceModule : NetModule {
    public static NetPacket SerializeSkyEntitySpawn(Player player, CustomSkyEntityType type) {
        int value = Main.rand.Next();
        NetPacket result = CreatePacket<CustomNetAmbienceModule>(6);
        result.Writer.Write((byte)player.whoAmI);
        result.Writer.Write(value);
        result.Writer.Write((byte)type);
        return result;
    }

    public override bool Deserialize(BinaryReader reader, int userId) {
        if (Main.dedServ)
            return false;

        byte playerId = reader.ReadByte();
        int seed = reader.ReadInt32();
        CustomSkyEntityType type = (CustomSkyEntityType)reader.ReadByte();
        if (Main.remixWorld)
            return true;

        Main.QueueMainThreadAction(delegate {
            ((CustomAmbientSky)SkyManager.Instance["CustomAmbience"]).Spawn(Main.player[playerId], type, seed);
        });

        return true;
    }
}
