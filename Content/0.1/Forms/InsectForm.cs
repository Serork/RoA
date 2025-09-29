using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Druid.Forms;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

abstract class InsectForm : BaseForm {
    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 2.2f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 0.8f);

    private float _maxRotation;

    public override SoundStyle? HurtSound => SoundID.NPCHit31;

    public override Vector2 SetWreathOffset(Player player) => new(0f, 12f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, -6f);

    protected virtual float InsectDustScale { get; } = 1f;
    protected virtual ushort InsectProjectileType { get; } = (ushort)ModContent.ProjectileType<CorruptionInsect>();

    protected sealed override void SafeSetDefaults() {
        MountData.fallDamage = 0;
        MountData.runSpeed = 5f;
        MountData.dashSpeed = 5f;
        MountData.flightTimeMax = 100;
        MountData.acceleration = 0.2f;
        MountData.totalFrames = 6;
        MountData.yOffset = 2;

        MountData.yOffset = 6;
        MountData.playerHeadOffset = -12;

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults2() { }

    protected override void SafePostUpdate(Player player) {
        float rotation = player.velocity.X * (IsInAir(player) ? 0.2f : 0.15f);
        rotation *= 0.5f;
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        float maxRotation = 0.2f;
        bool? variable = player.GetFormHandler()._facedRight;
        if (variable.HasValue) {
            maxRotation = 0.1f;
            _maxRotation = MathHelper.Lerp(_maxRotation, maxRotation, 0.1f);
        }
        else {
            _maxRotation = maxRotation;
        }
        //fullRotation = MathHelper.Clamp(fullRotation, -_maxRotation, _maxRotation);
        player.fullRotation = fullRotation;
        if (!IsInAir(player)) {
            player.velocity.X *= 0.925f;
        }
        Player.jumpHeight = 50;
        Player.jumpSpeed = 4f;
        player.velocity.Y = Math.Min(5f, player.velocity.Y);
        player.fullRotationOrigin = new Vector2(player.width / 2f, player.height / 2f);

        SpecialAttackHandler(player);
    }

    protected override void GetSpriteEffects(Player player, ref SpriteEffects spriteEffects) {
        bool? variable = player.GetFormHandler()._facedRight;
        if (!variable.HasValue) {
            return;
        }
        spriteEffects = variable.Value ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    }

    private void SpecialAttackHandler(Player player) {
        if (player.whoAmI != Main.myPlayer || Main.mouseText)
            return;

        ref bool? facedRight = ref player.GetFormHandler()._facedRight;
        ref int insectTimer = ref player.GetFormHandler()._insectTimer;
        string context = "insectformattack";
        IEntitySource source = player.GetSource_Misc(context);
        if (player.velocity.Y == 0f && player.velocity.X == 0f) {
            if (!player.wet) {
                insectTimer++;
                if (insectTimer >= 90) {
                    BaseFormDataStorage.ChangeAttackCharge1(player, 1f);
                    //player.GetWreathHandler().ResetGryphonStats(true, 0.25f);
                    SoundEngine.PlaySound(SoundID.NPCHit32, player.Center);
                    if (Main.netMode == NetmodeID.MultiplayerClient) {
                        MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 11, player.Center));
                    }
                    for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++) {
                        int insectDamage = 20;
                        float insectKnockback = 3f;
                        int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(insectDamage);
                        insectKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(insectKnockback);
                        Vector2 spread = new Vector2(0, Main.rand.Next(-5, -2)).RotatedByRandom(MathHelper.ToRadians(90));
                        Projectile.NewProjectile(source, player.Center + Main.rand.RandomPointInArea(player.width / 2f, player.height / 2f) / 2f, new Vector2(spread.X, spread.Y), InsectProjectileType, damage, insectKnockback, player.whoAmI);
                    }
                    insectTimer = 30;
                }
            }
        }
        else
            insectTimer = 0;
        if (!Main.mouseLeft || !player.controlUseItem) {
            if (player.GetFormHandler()._directionChangedFor <= 0f) {
                facedRight = null;
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new InsectFormPacket2(player));
                }
            }
            return;
        }
        player.GetFormHandler()._directionChangedFor = 1f;
        var value = (Main.MouseWorld.X > player.position.X ? 1 : -1) == 1;
        if (facedRight != value) {
            facedRight = value;
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new InsectFormPacket1(player, facedRight.Value));
            }
        }
        ref int shootCounter = ref player.GetFormHandler()._shootCounter;
        if (!Main.mouseText) {
            if (Main.mouseLeft) {
                shootCounter++;
                insectTimer = 0;
            }
            if (Main.mouseLeftRelease)
                shootCounter = 0;
        }
        if (shootCounter % 10 == 5 && shootCounter > 0) {
            BaseFormDataStorage.ChangeAttackCharge1(player, 1.25f);
            //player.GetWreathHandler().ResetGryphonStats(true, 0.25f);

            SoundEngine.PlaySound(SoundID.Item17, player.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 12, player.Center));
            }

            Vector2 playerPos, velocity;
            if (facedRight.Value) {
                playerPos = new Vector2(player.Center.X + player.width / 2f - 8, player.position.Y + 14);
                velocity = Helper.VelocityToPoint(playerPos, Main.rand.RandomPointInArea(new Vector2(playerPos.X + 60, playerPos.Y + 30), new Vector2(playerPos.X + 60, playerPos.Y + 30)), 4);
            }
            else {
                playerPos = new Vector2(player.Center.X - player.width / 2f + 2, player.position.Y + 14);
                velocity = Helper.VelocityToPoint(playerPos, Main.rand.RandomPointInArea(new Vector2(playerPos.X - 60, playerPos.Y + 30), new Vector2(playerPos.X - 60, playerPos.Y + 30)), 4);
            }
            velocity.Y += player.velocity.Y * 0.4f;
            playerPos += player.velocity * 4f;

            //int shootDamage = modPlayer.DruidFormDamage(7, 25);
            float shootKnockback = 1.5f;
            shootKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(shootKnockback);

            int type = ModContent.ProjectileType<ToxicStream>();
            int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(54);
            Projectile.NewProjectile(source, playerPos, velocity + new Vector2(0f, 0f), type, damage, shootKnockback, player.whoAmI, 0f, 0f, 2f);
            if (shootCounter >= 80) {
                shootKnockback = 2.5f;
                shootKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(shootKnockback);
                Projectile.NewProjectile(source, playerPos, velocity + new Vector2(0f, 1f), type, damage, shootKnockback, player.whoAmI, 0f, 0f, 2f);
                Projectile.NewProjectile(source, playerPos, velocity + new Vector2(0f, -1f), type, damage, shootKnockback, player.whoAmI, 0f, 0f);
                shootCounter = -35;
            }
        }
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 5;
        float flightFrameFrequency = 4f, walkingFrameFrequiency = 20f;
        if (IsInAir(player)) {
            frameCounter += MathF.Max(3f, Math.Abs(player.velocity.Y)) * (player.velocity.Y < 0f ? 0.5f : 0.25f);
            while (frameCounter > flightFrameFrequency) {
                frameCounter -= flightFrameFrequency;
                frame++;
            }
            if (frame > maxFrame) {
                frame = 0;
            }
        }
        else if (player.velocity.X != 0f) {
            frameCounter += Math.Abs(player.velocity.X) * 1.5f;
            while (frameCounter > walkingFrameFrequiency) {
                frameCounter -= walkingFrameFrequiency;
                frame++;
            }
            if (frame > maxFrame) {
                frame = 0;
            }
        }
        else {
            frameCounter = 0f;
            frame = 0;
        }

        return false;
    }

    protected sealed override void SafeSetMount(Player player, ref bool skipDust) {
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 16; i++) {
            Vector2 position = center + new Vector2(0, -6) + new Vector2(30, 0).RotatedBy(i * Math.PI * 2 / 16f) - new Vector2(8f, 4f);
            int dust = Dust.NewDust(position, 0, 0, MountData.spawnDust, 0, 0, 0, default(Color), InsectDustScale);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1.25f;
            Main.dust[dust].velocity += Helper.VelocityToPoint(position, player.Center, 2f);
        }
        SoundEngine.PlaySound(SoundID.Zombie46 with { Pitch = -0.35f, Volume = 0.6f }, player.Center);
        skipDust = true;
    }

    protected sealed override void SafeDismountMount(Player player, ref bool skipDust) {
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 16; i++) {
            Vector2 position = center + new Vector2(2, -6) + new Vector2(20, 0).RotatedBy(i * Math.PI * 2f / 16f) - new Vector2(0f, -12f);
            Vector2 direction = (center - position) * 0.8f;
            int dust = Dust.NewDust(position + direction, 0, 0, MountData.spawnDust, direction.X, direction.Y, 0, default(Color), InsectDustScale * 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.3f;
        }
        skipDust = true;
    }
}