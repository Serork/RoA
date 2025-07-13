using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

interface IUseCustomImmunityFrames { }

sealed class CustomImmunityFramesHandler : GlobalProjectile {
    public ushort[][]? ImmunityFramesPerNPC;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.IsModded(out ModProjectile modProjectile) && modProjectile is IUseCustomImmunityFrames;

    public static void Initialize(Projectile projectile, ushort size) {
        CustomImmunityFramesHandler handler = projectile.GetGlobalProjectile<CustomImmunityFramesHandler>();
        handler.ImmunityFramesPerNPC = new ushort[size][];
        for (int i = 0; i < handler.ImmunityFramesPerNPC.Length; i++) {
            handler.ImmunityFramesPerNPC[i] = new ushort[Main.npc.Length];
        }
    }

    public static ref ushort GetImmuneTime(Projectile projectile, byte index, int npcId) => ref projectile.GetGlobalProjectile<CustomImmunityFramesHandler>().ImmunityFramesPerNPC![index][npcId];
}
