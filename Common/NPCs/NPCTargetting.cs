using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class NPCTargetting : GlobalNPC {
    private readonly static Vector2[] _before = new Vector2[256];

    public override bool PreAI(NPC npc) {
        if (!npc.friendly) {
            foreach (Player player in Main.ActivePlayers) {
                RavencallersCloak.RavencallerPlayer data = player.GetModPlayer<RavencallersCloak.RavencallerPlayer>();
                if (data.AvailableForNPCs) {
                    _before[player.whoAmI] = player.Center;
                    RavencallersCloak.RavencallerPlayer.OldPositionInfo[] playerOldPositions = data.OldPositionInfos;
                    RavencallersCloak.RavencallerPlayer.OldPositionInfo lastPositionInfo = playerOldPositions[^1];
                    Vector2 position = lastPositionInfo.Position;
                    if (position != Vector2.Zero) {
                        player.Center = lastPositionInfo.Position;
                    }
                }
            }
            foreach (Projectile starfruit in TrackedEntitiesSystem.GetTrackedProjectile<Starfruit>()) {
                Vector2 checkPosition = starfruit.As<Starfruit>().NPCTargetPosition;
                if (npc.Distance(checkPosition) > TileHelper.TileSize * Starfruit.NPCTARGETTINGTILECOUNTDISTANCE) {
                    continue;
                }
                Player player = starfruit.GetOwnerAsPlayer();
                _before[player.whoAmI] = player.Center;
                player.Center = checkPosition;
                break;
            }
        }

        return base.PreAI(npc);
    }

    public override void PostAI(NPC npc) {
        if (!npc.friendly) {
            foreach (Player player in Main.ActivePlayers) {
                RavencallersCloak.RavencallerPlayer data = player.GetModPlayer<RavencallersCloak.RavencallerPlayer>();
                if (data.AvailableForNPCs) {
                    player.Center = _before[player.whoAmI];
                }
            }
            foreach (Projectile starfruit in TrackedEntitiesSystem.GetTrackedProjectile<Starfruit>()) {
                Vector2 checkPosition = starfruit.As<Starfruit>().NPCTargetPosition;
                if (npc.Distance(checkPosition) > TileHelper.TileSize * Starfruit.NPCTARGETTINGTILECOUNTDISTANCE) {
                    continue;
                }
                Player player = starfruit.GetOwnerAsPlayer();
                player.Center = _before[player.whoAmI];
                break;
            }
        }
    }
}
