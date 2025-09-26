using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Core.Graphics.Data;

using Terraria;

namespace RoA.Content.Forms;

sealed class HallowEnt : BaseForm {
    protected override Color GlowColor(Player player, Color drawColor, float progress) => WreathHandler.GetArmorGlowColor1(player, drawColor, progress);

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 2.5f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1.85f);

    public override Vector2 SetWreathOffset(Player player) => new(0f, -36f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 18f);

    protected override void SafeSetDefaults() {
        MountData.fallDamage = 0.1f;
        MountData.flightTimeMax = 0;
        MountData.fatigueMax = 0;
        MountData.jumpHeight = 15;
        MountData.jumpSpeed = 6f;
        MountData.totalFrames = 1;
        MountData.constantJump = false;
        MountData.usesHover = false;
        MountData.xOffset = -6;
        MountData.yOffset = 3;
        MountData.playerHeadOffset = -14;

    }

    protected override void SafePostUpdate(Player player) {
        player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed = true;
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        frameCounter = 0;
        frame = 0;

        return true;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        skipDust = true;
    }
}
