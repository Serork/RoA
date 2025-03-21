using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Buffs;
using RoA.Content.Items.Equipables.Armor.Summon;
using RoA.Core;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Mounts;

sealed class BoneHarpyMount : ModMount {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void Load() {
        On_PlayerDrawLayers.DrawPlayer_02_MountBehindPlayer += On_PlayerDrawLayers_DrawPlayer_02_MountBehindPlayer;
        On_PlayerDrawLayers.DrawPlayer_23_MountFront += On_PlayerDrawLayers_DrawPlayer_23_MountFront;
        On_Player.HorizontalMovement += On_Player_HorizontalMovement;
    }

    private void On_Player_HorizontalMovement(On_Player.orig_HorizontalMovement orig, Player self) {
        if (self.mount.Active && self.mount.Type == ModContent.MountType<BoneHarpyMount>()) {
            return;
        }

        orig(self);
    }

    private void On_PlayerDrawLayers_DrawPlayer_23_MountFront(On_PlayerDrawLayers.orig_DrawPlayer_23_MountFront orig, ref PlayerDrawSet drawinfo) {
        if (drawinfo.drawPlayer.mount.Active && drawinfo.drawPlayer.mount.Type == ModContent.MountType<BoneHarpyMount>()) {
            return;
        }

        orig(ref drawinfo);
    }

    private void On_PlayerDrawLayers_DrawPlayer_02_MountBehindPlayer(On_PlayerDrawLayers.orig_DrawPlayer_02_MountBehindPlayer orig, ref PlayerDrawSet drawinfo) {
        if (drawinfo.drawPlayer.mount.Active && drawinfo.drawPlayer.mount.Type == ModContent.MountType<BoneHarpyMount>()) {
            return;
        }

        orig(ref drawinfo);
    }

    public override void SetStaticDefaults() {
        MountData.buff = ModContent.BuffType<BoneHarpyMountBuff>();
    }

    public override void SetMount(Player player, ref bool skipDust) {
        //if (_flyTime <= 0f) {
        //    _flyTime = FLYTIME;
        //}

        skipDust = true;
    }

    public override void Dismount(Player player, ref bool skipDust) {
        player.GetModPlayer<WorshipperBonehelm.BoneHarpyOptions>().JumpOffHarpy();

        skipDust = true;
    }

    public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        => false;

    public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
        mountedPlayer.legFrame.Y = mountedPlayer.legFrame.Height * 5;

        return base.UpdateFrame(mountedPlayer, state, velocity);
    }

    public override void UpdateEffects(Player player) {
        float num = 3.5f;
        float num2 = 0.1f;
        WorshipperBonehelm.BoneHarpyOptions handler = player.GetModPlayer<WorshipperBonehelm.BoneHarpyOptions>();
        bool up = (player.controlUp || player.controlJump) && handler.FlyTime > 0f;
        float value = up ? 0f : 0.3f;
        if (handler.FlyTime > 0f) {
            /*if (up) */
            {
                handler.FlyTime -= 1f;
            }
        }
        else {
            handler.JumpOffHarpy();
        }
        //else if (((player.velocity.Y == 0f || player.sliding) && player.releaseJump) || (player.autoJump && player.justJumped)) {
        //    _flyTime = FLYTIME;
        //}
        if (player.gravity > value) {
            player.gravity = value;
        }
        if (up) {
            if (player.velocity.Y > 0f)
                player.velocity.Y *= 0.9f;

            player.velocity.Y -= num2;
            if (player.velocity.Y < 0f - num)
                player.velocity.Y = 0f - num;
        }
        else if (player.controlDown) {
            if (player.velocity.Y < 0f)
                player.velocity.Y *= 0.9f;

            player.velocity.Y += num2;
            if (player.velocity.Y > num)
                player.velocity.Y = num;
        }
        else if (player.velocity.Y < 0f - num2 || player.velocity.Y > num2) {
            player.velocity.Y *= 0.9f;
        }
        else {
            player.velocity.Y = 0f;
        }

        if (player.controlLeft) {
            if (player.velocity.X > 0f)
                player.velocity.X *= 0.9f;

            player.velocity.X -= num2;
            if (player.velocity.X < 0f - num)
                player.velocity.X = 0f - num;
        }
        else if (player.controlRight) {
            if (player.velocity.X < 0f)
                player.velocity.X *= 0.9f;

            player.velocity.X += num2;
            if (player.velocity.X > num)
                player.velocity.X = num;
        }
        else if (player.velocity.X < 0f - num2 || player.velocity.X > num2) {
            player.velocity.X *= 0.9f;
        }
        else {
            player.velocity.X = 0f;
        }
    }
}
