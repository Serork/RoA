using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Forms;

sealed class ChlorophyteSlug : BaseForm {
    private static ushort FRAMECOUNT => 6;

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 1f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1f);

    public override Vector2 SetWreathOffset(Player player) => new(0f, 0f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 0f);

    protected override void SafeSetDefaults() {
        MountData.totalFrames = FRAMECOUNT;
        MountData.fallDamage = 0f;

        MountData.xOffset = 0;
        MountData.yOffset = 0;

        MountData.playerHeadOffset = 0;
    }

    public override bool ShouldSpawnFloorDust(Player player) => false;

    protected override void SafePostUpdate(Player player) {
        player.GetFormHandler().UsePlayerSpeed = false;
        player.GetFormHandler().UsePlayerHorizontals = false;

        Vector2 direction = Vector2.Zero;

        if (player.controlUp || player.controlJump) {
            direction.Y = -1f;
        }
        if (player.controlDown) {
            direction.Y = 1f;
        }
        if ((player.controlUp || player.controlJump) && player.controlDown) {
            direction.Y = 0f;
        }

        if (player.controlLeft) {
            direction.X = -1f;
        }
        if (player.controlRight) {
            direction.X = 1f;
        }
        if (player.controlLeft && player.controlRight) {
            direction.X = 0f;
        }

        player.velocity = Vector2.Lerp(player.velocity, direction.SafeNormalize() * 5f, direction == Vector2.Zero ? 0.2f : 0.1f);
        player.fullRotation = player.velocity.ToRotation() + MathHelper.PiOver2;

        if (player.velocity.X < 0f)
            player.direction = -1;
        else if (player.velocity.X > 0f)
            player.direction = 1;

        player.gravity = 0f;

        player.fullRotationOrigin = player.getRect().Centered();
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        frame = 0;
        frameCounter = 0f;

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        skipDust = true;
    }
}
