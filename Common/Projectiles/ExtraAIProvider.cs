using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.Projectiles;

sealed class ExtraAIProvider : GlobalProjectile {
    public float[] ExtraAI = null!;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.ModProjectile is IRequestExtraAIValue;

    public override void SetDefaults(Projectile entity) {
        ExtraAI = new float[((IRequestExtraAIValue)entity.ModProjectile).NeededAmountOfAI];
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
        for (int i = 0; i < ExtraAI.Length; i++) {
            binaryWriter.Write(ExtraAI[i]);
        }
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
        for (int i = 0; i < ExtraAI.Length; i++) {
            ExtraAI[i] = binaryReader.ReadSingle();
        }
    }
}

static class ExtraAIProviderExtensions {
    public static ref float[] GetExtraAI(this Projectile projectile) => ref projectile.GetGlobalProjectile<ExtraAIProvider>().ExtraAI;
}