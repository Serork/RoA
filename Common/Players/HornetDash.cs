using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public static float HORNETDASHTIME => 15f;
    public static float HORNETDASHLENGTH => 150f;

    public bool ApplyHornetSkullSetBonus;

    public bool CanDoHornetDash {
        get => !Dashed;
        set => Dashed = !value;
    }
    public bool DoingHornetDash => DashTime > 0f;
    public bool IsHornetDashInCooldown => DashTime < 0f;
    public float HornetDashProgress => 1f - DashTime / HORNETDASHTIME;

    public void HandleHornetDash() {
        if (!CanDoHornetDash && !BaseForm.IsInAir(Player)) {
            CanDoHornetDash = true;
        }

        float lerpValue = 1f;
        if (!DoingHornetDash) {
            if (IsHornetDashInCooldown) {
                DashTime = Helper.Approach(DashTime, 0f, lerpValue);
            }
            return;
        }

        Player.shimmering = true;
        Player.shimmerTransparency = 0f;

        DashTime = Helper.Approach(DashTime, 0f, lerpValue);
        bool hit = false;
        Vector2 checkVelocity = SavedVelocity * 0.025f;
        Vector2 checkPosition = Player.position + SavedVelocity.SafeNormalize() * Player.width / 2f;
        float f = SavedVelocity.ToRotation();
        float collisionPoint2 = 0f;
        float num10 = 70f;
        if (DashTime < HORNETDASHTIME * 0.75f) {
            foreach (NPC nPC in Main.ActiveNPCs) {
                if (nPC.dontTakeDamage || nPC.friendly || (nPC.aiStyle == 112 && !(nPC.ai[2] <= 1f)) || !Player.CanNPCBeHitByPlayerOrPlayerProjectile(nPC))
                    continue;

                Rectangle rect = nPC.getRect();
                if (Collision.CheckAABBvLineCollision(rect.TopLeft(), rect.Size(), Player.Center, Player.Center + f.ToRotationVector2() * num10, 20f, ref collisionPoint2) &&
                    (nPC.noTileCollide || Player.CanHit(nPC))) {
                    hit = true;
                    DashTime = 0f;
                    DoBackflip();
                    break;
                }
            }
        }
        bool inBlocks = false;
        if (Collision.SolidCollision(checkPosition, Player.width, Player.height)) {
            DashTime = 0;
            inBlocks = true;
        }
        Player.eocDash = (int)DashTime;
        if (DashTime == 0f) {
            Projectile hornetSpear = TrackedEntitiesSystem.GetSingleTrackedProjectile<HornetSpear>(checkProjectile => checkProjectile.owner != Player.whoAmI);
            hornetSpear.ai[1] = 1f;
            hornetSpear.netUpdate = true;

            Player.shimmering = false;
            Vector2 extraVelocity = SavedVelocity * 0.025f;
            Player.velocity += extraVelocity;
            if (hit) {
                Player.velocity.Y = -MathF.Abs(Player.velocity.Y);
                float speed = 4f;
                Player.velocity.Y -= speed * 1.2f * SavedVelocity.Y.GetDirection();
                Player.velocity.X = -Player.direction * speed;
                Player.velocity.X = -Player.velocity.X;
                Player.GiveImmuneTimeForCollisionAttack(4);
            }
            if (inBlocks) {
                Player.velocity *= 0.5f;
                Player.DryCollision(false, true);
            }

            if (!hit) {
                DashTime = -HORNETDASHTIME;
            }
            return;
        }

        Player.controlUseItem = false;
        LockHorizontalMovement = true;

        Player.armorEffectDrawShadowEOCShield = true;

        Player.velocity *= 0.01f;

        //SavedVelocity *= HORNETDASHVELOCITYDECELERATIONFACTOR;
        Player.position = SavedPosition + SavedVelocity * HornetDashProgress;

        Player.gravity = 0f;
    }

    public void DoHornetDash(IDoubleTap.TapDirection direction) {
        if (!Player.GetCommon().ApplyHornetSkullSetBonus || Player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        if (!Player.IsAliveAndFree()) {
            return;
        }

        if (Player.velocity.Y == 0f || Player.ItemAnimationActive) {
            return;
        }

        if (direction != IDoubleTap.TapDirection.Left && direction != IDoubleTap.TapDirection.Right) {
            return;
        }

        bool dashUp = Player.controlJump;
        if (DoingHornetDash || IsHornetDashInCooldown || (dashUp && !CanDoHornetDash)) {
            return;
        }

        if (Player.mount.Active) {
            Player.mount.Dismount(Player);
        }

        DashTime = HORNETDASHTIME;
        SavedPosition = Player.position;
        SavedVelocity = new Vector2(Player.direction, 1f * (!dashUp).ToDirectionInt()) * HORNETDASHLENGTH;
        if (dashUp) {
            CanDoHornetDash = false;
        }

        ProjectileUtils.SpawnPlayerOwnedProjectile<HornetSpear>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc("hornetspear")) {
            Position = Player.Center,
            Velocity = SavedVelocity.SafeNormalize(),
            AI0 = DashTime,
            Damage = 50
        });
    }

    private void On_Player_DryCollision(On_Player.orig_DryCollision orig, Player self, bool fallThrough, bool ignorePlats) {
        orig(self, fallThrough, ignorePlats);
    }
}
