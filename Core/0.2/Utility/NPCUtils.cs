using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Core.Utility;

static class NPCUtils {
    public static NPC? FindClosestNPC(Vector2 checkPosition, int checkDistance, bool checkForCollisions = true) {
        NPC? target = null;
        int neededDistance = checkDistance;
        foreach (NPC checkNPC in Main.ActiveNPCs) {
            if (!checkNPC.CanBeChasedBy()) {
                continue;
            }
            float distance = (checkPosition - checkNPC.Center).Length();
            if (distance < neededDistance && (!checkForCollisions || Collision.CanHitLine(checkPosition, 1, 1, checkNPC.Center, 1, 1))) {
                target = checkNPC;
            }
        }
        return target;
    }
}
