using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;

using Terraria;

namespace RoA.Content.Forms;

sealed class HallowedGryphon : BaseForm {
    protected override Color GlowColor(Player player, Color drawColor, float progress) => WreathHandler.GetArmorGlowColor1(player, drawColor, progress);

    public override ushort HitboxWidth => (ushort)(Player.defaultWidth * 2.5f);
    public override ushort HitboxHeight => (ushort)(Player.defaultHeight * 1f);

    public override Vector2 WreathOffset => new(0f, 0f);
    public override Vector2 WreathOffset2 => new(0f, 0f);

    protected override void SafeSetDefaults() {
        MountData.fallDamage = 0.1f;
        MountData.flightTimeMax = 0;
        MountData.fatigueMax = 0;
        MountData.jumpHeight = 15;
        MountData.jumpSpeed = 6f;
        MountData.totalFrames = 1;
        MountData.constantJump = false;
        MountData.usesHover = false;

        //MountData.yOffset = 3;
        //MountData.playerHeadOffset = -14;
    }

    protected override void SafePostUpdate(Player player) {
        player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed = true;

        MountData.yOffset = 2;
        MountData.playerHeadOffset = -14;
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
