using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Buffs;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class EnduranceCloud : ModProjectile {
    public static ushort TIMELEFT => 600;
    public static float TIMETOSPAWNANOTHER => TIMELEFT * 0.1f;
    public static float TIMETOREPLACE => TIMELEFT * 0.95f;

    private static byte FRAMECOUNT => 3;

    public bool ShouldBeRenderedOverPlayer {
        get => Projectile.ai[0] != 0f;
        set => Projectile.ai[0] = value.ToInt();
    }

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

    public override void SetDefaults() {
        Projectile.SetSizeValues(46, 20);

        Projectile.friendly = true;
        Projectile.damage = 0;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;
        Projectile.manualDirectionChange = true;

        Projectile.hide = true;

        Projectile.tileCollide = false;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        ref float opacityFactor = ref Projectile.localAI[0];
        opacityFactor++;

        ref float offsetX = ref Projectile.ai[1]; 
        ref float offsetY = ref Projectile.ai[2];
        Player owner = Projectile.GetOwnerAsPlayer();
        if (Projectile.IsOwnerLocal() && offsetX == 0f) {
            bool shouldReRandom = true;
            int attempts = 100;
            while (shouldReRandom && attempts-- > 0) {
                shouldReRandom = false;
                bool facedRight = owner.FacedRight();
                Vector2 randomOffset = Main.rand.NextVector2(facedRight ? owner.width * 1.5f : owner.width * 0.25f, owner.height * 0.25f, facedRight ? owner.width * 2.25f : owner.width * 1f, owner.height * 1.75f);
                offsetX = randomOffset.X;
                offsetY = randomOffset.Y;
                Vector2 playerCenter = owner.MountedCenter;
                playerCenter = Utils.Floor(playerCenter) + Vector2.UnitY * owner.gfxOffY;
                Projectile.Center = playerCenter + new Vector2(offsetX, offsetY) - owner.Size;
                foreach (Projectile trackedCloud in TrackedEntitiesSystem.GetTrackedProjectile<EnduranceCloud>(checkProjectile => checkProjectile.owner != Projectile.owner || checkProjectile.whoAmI == Projectile.whoAmI)) {
                    float distance = trackedCloud.Distance(Projectile.Center);
                    if (distance < 30f) {
                        shouldReRandom = true;
                        break;
                    }
                }
            }

            Projectile.velocity *= 0f;

            if (Main.rand.NextBool()) {
                ShouldBeRenderedOverPlayer = Projectile.whoAmI % 2 == 0;
            }
            else {
                ShouldBeRenderedOverPlayer = Main.rand.NextBool();
            }

            Projectile.netUpdate = true;
        }

        ref float checkForBuffTimer = ref Projectile.localAI[1];
        const float CHECKFORBUFFSTIME = 10f;
        float opacityFluffMin = TIMELEFT - 30f;
        if (!owner.HasBuff<EnduranceCloud1>() && !owner.HasBuff<EnduranceCloud2>() && !owner.HasBuff<EnduranceCloud3>()) {
            checkForBuffTimer++;
            if (checkForBuffTimer > CHECKFORBUFFSTIME && opacityFactor < opacityFluffMin) {
                opacityFactor = opacityFluffMin;
            }
        }
        else {
            checkForBuffTimer = 0f;
        }
        Projectile.Opacity = 0.65f * Utils.GetLerpValue(0f, 30f, opacityFactor, true) * Utils.GetLerpValue(TIMELEFT, opacityFluffMin, opacityFactor, true);

        Vector2 playerCenter2 = owner.MountedCenter;
        playerCenter2 = Utils.Floor(playerCenter2) + Vector2.UnitY * owner.gfxOffY;
        Projectile.Center = playerCenter2 + new Vector2(offsetX, offsetY) - owner.Size;
        Projectile.Center = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity, 0.05f);
        Projectile.velocity.X -= 1f * owner.direction;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (ShouldBeRenderedOverPlayer) {
            overPlayers.Add(index);
        }
        else {
            behindProjectiles.Add(index);
        }
    }
}
