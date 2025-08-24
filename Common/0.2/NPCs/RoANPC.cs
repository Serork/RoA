using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class RoANPC : GlobalNPC {
    public delegate void ModifyHitByProjectileDelegate(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers);
    public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        ModifyHitByProjectileEvent?.Invoke(npc, projectile, ref modifiers);
    }
}
