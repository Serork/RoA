using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public static float HORNETDASHTIME => 17.5f;
    public static float HORNETDASHLENGTH => 200f;
    public static float HORNETDASHVELOCITYDECELERATIONFACTOR => 0.975f;

    public bool DoingHornetDash => DashTime > 0f;
    public float HornetDashProgress => 1f - DashTime / HORNETDASHTIME;

    public void HandleHornetDash() {
        if (!DoingHornetDash) {
            return;
        }

        Player.shimmering = true;
        Player.shimmerTransparency = 0f;

        float lerpValue = 1f;
        DashTime = Helper.Approach(DashTime, 0f, lerpValue);
        if (Collision.SolidCollision(Player.position + SavedVelocity.SafeNormalize() * 10f, Player.width, Player.height)) {
            DashTime = 0;
        }
        Player.eocDash = (int)DashTime;
        if (DashTime == 0f) {
            Player.shimmering = false;
            Player.velocity += SavedVelocity * 0.025f;
            return;
        }

        Player.itemAnimation = 1;
        Player.controlJump = true;

        Player.armorEffectDrawShadowEOCShield = true;

        Player.velocity *= 1E-5F;

        SavedVelocity *= HORNETDASHVELOCITYDECELERATIONFACTOR;
        Player.position = SavedPosition + SavedVelocity * HornetDashProgress;

        Player.gravity = 0f;
    }

    public void OnHornetDash(IDoubleTap.TapDirection direction) {
        if (!Player.HasSetBonusFrom<HornetSkull>()) {
            return;
        }

        if (!Player.IsAliveAndFree()) {
            return;
        }

        if (direction != IDoubleTap.TapDirection.Down &&
            direction != IDoubleTap.TapDirection.Top) {
            return;
        }

        if (DoingHornetDash) {
            return;
        }

        if (Player.mount.Active) {
            Player.mount.Dismount(Player);
        }

        DashTime = HORNETDASHTIME;
        SavedPosition = Player.position;
        SavedVelocity = new Vector2(Player.direction, 1f * (direction == IDoubleTap.TapDirection.Down).ToDirectionInt()) * HORNETDASHLENGTH;
    }

    private void On_Player_DryCollision(On_Player.orig_DryCollision orig, Player self, bool fallThrough, bool ignorePlats) {
        orig(self, fallThrough, ignorePlats);
    }
}
