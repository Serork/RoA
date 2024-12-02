using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;

using System;

using Terraria;

namespace RoA.Content.Forms;

sealed class FlederForm : BaseForm {
    protected override void SafeSetDefaults() {
        MountData.totalFrames = 8;
        MountData.spawnDust = 59;
        MountData.heightBoost = -14;
        MountData.fallDamage = 0f;
        MountData.dashSpeed = 2f;
        MountData.runSpeed = 2f;
        MountData.flightTimeMax = 60;
        MountData.fatigueMax = 40;
        MountData.jumpHeight = 1;
        MountData.acceleration = 1f;
        MountData.jumpSpeed = 4f;
        MountData.blockExtraJumps = false;
        MountData.constantJump = false;
        MountData.usesHover = false;

        if (!Main.dedServ) {
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    public override void SetMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center + new Vector2(30f, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }
    public override void Dismount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center - new Vector2(30f, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }
}