using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer, IDoubleTap {
    public static ushort DEERSKULLATTACKTIME => 40;
    public static ushort DEERSKULLATTACKDISTANCE => (ushort)(TileHelper.TileSize * 15);

    public bool ApplyDeerSkullSetBonus;

    public float DeerSkullHornsOpacity;
    public float DeerSkullHornsBorderOpacity, DeerSkullHornsBorderOpacity2;
    public NPC? DeerSkullHornsTarget;
    public ushort DeerSkullAttackTime;

    public partial void DeerSkullResetEffects() { }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.GetCommon().ApplyDeerSkullSetBonus && direction == Helper.CurrentDoubleTapDirectionForSetBonuses) {
            player.GetWreathHandler().SlowlyFill2();
        }
    }

    public partial void DeerSkullPostUpdateEquips() {
        float lerpValue = 0.1f;

        if (!ApplyDeerSkullSetBonus || !Player.GetWreathHandler().ChargedBySlowFill) {
            DeerSkullHornsOpacity = Helper.Approach(DeerSkullHornsOpacity, 0f, lerpValue);
            return;
        }

        Vector2 checkPosition = Player.Center;
        int checkDistance = DEERSKULLATTACKDISTANCE;
        NPC? target = NPCUtils.FindClosestNPC(checkPosition, checkDistance, false);
        DeerSkullHornsTarget = target;

        DeerSkullHornsBorderOpacity = Helper.Approach(DeerSkullHornsBorderOpacity, 0f, lerpValue);
        DeerSkullHornsBorderOpacity2 = Helper.Approach(DeerSkullHornsBorderOpacity2, 0f, lerpValue);

        if (target is null) {
            DeerSkullHornsOpacity = Helper.Approach(DeerSkullHornsOpacity, 0f, lerpValue);
            return;
        }

        if (Player.HasProjectile<HornsLightning>()) {
            DeerSkullHornsOpacity = Helper.Approach(DeerSkullHornsOpacity, 1f, lerpValue * 2.5f);
        }

        void spawnLightning() {
            //DeerSkullHornsOpacity = 1f;

            if (!Player.IsLocal()) {
                return;
            }

            int damage = 50;
            float knockBack = 1f;
            Vector2 targetPosition = DeerSkullHornsTarget!.Center;
            ProjectileUtils.SpawnPlayerOwnedProjectile<HornsLightning>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc("hornsattack")) {
                Damage = damage,
                KnockBack = knockBack,
                AI0 = targetPosition.X,
                AI1 = targetPosition.Y
            });
        }
        if (DeerSkullAttackTime > DEERSKULLATTACKTIME / 3) {
            if (DeerSkullAttackTime % (int)(DEERSKULLATTACKTIME / 4) == 0) {
                spawnLightning();
                DeerSkullHornsBorderOpacity = 2f;
                DeerSkullHornsBorderOpacity2 = 1f;
            }
        }
        if (DeerSkullAttackTime++ > DEERSKULLATTACKTIME) {
            DeerSkullAttackTime = 0;
        }
    }
}
