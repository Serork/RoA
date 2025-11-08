using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public static ushort ATTACKTIME => 40;

    public bool ApplyDeerSkullSetBonus;
    public bool HasViableTargetNearby;

    public float HornsOpacity;
    public float HornsBorderOpacity, HornsBorderOpacity2;
    public NPC? HornsTarget;
    public ushort AttackTime;

    public partial void DeerSkullResetEffects() {
        HasViableTargetNearby = false;
    }

    public partial void DeerSkullPostUpdateEquips() {
        float lerpValue = 0.1f;
        if (!ApplyDeerSkullSetBonus) {
            HornsOpacity = Helper.Approach(HornsOpacity, 0f, lerpValue);
            return;
        }

        Vector2 checkPosition = Player.Center;
        int checkDistance = (int)TileHelper.TileSize * 15;
        NPC? target = NPCUtils.FindClosestNPC(checkPosition, checkDistance, false, false);
        HornsTarget = target;

        HornsBorderOpacity = Helper.Approach(HornsBorderOpacity, 0f, lerpValue);
        HornsBorderOpacity2 = Helper.Approach(HornsBorderOpacity2, 0f, lerpValue);

        if (target is null) {
            HornsOpacity = Helper.Approach(HornsOpacity, 0f, lerpValue);
            return;
        }

        if (Player.HasProjectile<HornsLightning>()) {
            HornsOpacity = Helper.Approach(HornsOpacity, 1f, lerpValue * 2.5f);
        }

        void spawnLightning() {
            //HornsOpacity = 1f;

            if (!Player.IsLocal()) {
                return;
            }

            int damage = 50;
            float knockBack = 1f;
            Vector2 targetPosition = HornsTarget!.Center;
            ProjectileUtils.SpawnPlayerOwnedProjectile<HornsLightning>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc("hornsattack")) {
                Damage = damage,
                KnockBack = knockBack,
                AI0 = targetPosition.X,
                AI1 = targetPosition.Y
            });
        }
        if (AttackTime > ATTACKTIME / 3) {
            if (AttackTime % (int)(ATTACKTIME / 4) == 0) {
                spawnLightning();
                HornsBorderOpacity = 2f;
                HornsBorderOpacity2 = 1f;
            }
        }
        if (AttackTime++ > ATTACKTIME) {
            AttackTime = 0;
        }
    }
}
