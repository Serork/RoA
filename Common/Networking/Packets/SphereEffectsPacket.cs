using Microsoft.Xna.Framework;

using RoA.Content.Items.Special;
using RoA.Core.Utility;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class SphereEffectsPacket : NetPacket {
    public SphereEffectsPacket(Vector2 position, Color color) {
        Writer.WriteVector2(position);
        Writer.WriteRGBA(color);
    }

    public override void Read(BinaryReader reader, int sender) {
        Vector2 position = reader.ReadVector2();
        Color color = reader.ReadRGBA();
        SphereHandler.MakeEffects(null, color, position, true);
    }
}
