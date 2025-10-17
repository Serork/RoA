using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Initializers;
using Terraria.ModLoader;
using Terraria.Net;

namespace RoA.Common.CustomSkyAmbience;

sealed class CustomNetAmbienceModule : NetModule, ILoadable {
    public void Load(Mod mod) {
        On_NetworkInitializer.Load += On_NetworkInitializer_Load;
    }

    private void On_NetworkInitializer_Load(On_NetworkInitializer.orig_Load orig) {
        orig();
        NetManager.Instance.Register<CustomNetAmbienceModule>();
    }

    public void Unload() {
    }

    public static NetPacket SerializeSkyEntitySpawn(Player player, CustomSkyEntityType type) {
        int value = Main.rand.Next();
        NetPacket result = CreatePacket<CustomNetAmbienceModule>(6);
        result.Writer.Write((byte)player.whoAmI);
        result.Writer.Write(value);
        result.Writer.Write((byte)type);
        return result;
    }

    public override bool Deserialize(BinaryReader reader, int userId) {
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
