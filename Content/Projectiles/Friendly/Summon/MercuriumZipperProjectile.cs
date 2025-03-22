using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Summon;
using RoA.Content.Projectiles.Friendly.Melee;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class MercuriumZipper_MercuriumCenserToxicFumes : ModProjectile {
    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<MercuriumFumes>()).Texture;

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

    public override void SetDefaults() {
        int width = 24; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;

        Projectile.timeLeft = 250;
        Projectile.tileCollide = false;

        Projectile.friendly = true;

        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = 50;

        Projectile.appliesImmunityTimeOnSingleHits = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;

        Projectile.aiStyle = -1;
        Projectile.scale = 1.25f;
    }

    public override bool? CanDamage() => Projectile.Opacity >= 0.3f;

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Opacity -= 0.1f;
        return false;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            if (Projectile.owner == Main.myPlayer) {
                Projectile.ai[1] = Main.rand.NextFloat(0.75f, 1f);
                Projectile.netUpdate = true;
            }

            Projectile.localAI[0] = 1f;
        }

        if (Projectile.ai[1] != 0f) {
            Projectile.Opacity = Projectile.ai[1];
            Projectile.localAI[0] = Projectile.Opacity;
            Projectile.ai[1] = 0f;
        }

        if (++Projectile.frameCounter >= 8) {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }

        if (Projectile.owner == Main.myPlayer) {
            float distance = 30f;
            for (int findNPC = 0; findNPC < Main.npc.Length; findNPC++) {
                NPC npc = Main.npc[findNPC];
                if (npc.active && npc.life > 0 && !npc.friendly && Vector2.Distance(Projectile.Center, npc.Center) < distance) {
                    npc.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 80);
                }
            }
        }

        if (Projectile.Opacity > 0f) {
            Projectile.Opacity -= Projectile.localAI[0] * 0.025f * 0.3f;
        }
        else {
            Projectile.Kill();
        }

        Projectile.velocity *= 0.995f;
    }

    public override Color? GetAlpha(Color lightColor) => new Color(106, 140, 34, 100).MultiplyRGB(lightColor) * Projectile.Opacity * 0.7f;

    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        Vector2 drawPos = Projectile.position - Main.screenPosition + drawOrigin;
        Color color = Projectile.GetAlpha(lightColor) * 0.5f;
        for (int i = 0; i < 2; i++)
            spriteBatch.Draw(texture, drawPos + new Vector2(0, (i == 1 ? 2f : -2f) * (1f - Projectile.Opacity) * 2f).RotatedBy(Main.GlobalTimeWrappedHourly * 4f), frameRect, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}

sealed class MercuriumZipper_Effect : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    private ref float Progress => ref Projectile.ai[1];

    public override void SetDefaults() {
        Projectile.Size = Vector2.One;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => Progress != 0f;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.FinalDamage *= 4f;
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.FinalDamage *= 4f;
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
    }

    private class SummonDamageSum : ModPlayer {
        public int DamageDone;

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
            if (proj.owner != Main.myPlayer) {
                return;
            }

            NPC npc = null;
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner == proj.owner && projectile.type == ModContent.ProjectileType<MercuriumZipper_Effect>()) {
                    npc = Main.npc[(int)projectile.ai[0]];
                }
            }

            if (npc == null || !npc.active || target != npc) {
                return;
            }

            if (proj.DamageType != DamageClass.Summon ||
                ProjectileID.Sets.IsAWhip[proj.type] ||
                proj.type == ModContent.ProjectileType<MercuriumZipper_MercuriumCenserToxicFumes>()) {
                return;
            }

            DamageDone += hit.Damage;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(Projectile.localAI[2]);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        Projectile.localAI[2] = reader.ReadSingle();
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        NPC target = Main.npc[(int)Projectile.ai[0]];

        if (!(!player.frozen && !player.stoned)) {
            Projectile.Kill();

            return;
        }
        if (!target.active || player.dead) {
            Projectile.Kill();

            return;
        }

        if (Progress == 0f) {
            if (player.whoAmI == Main.myPlayer) {
                int direction = (player.MountedCenter - target.Center).X.GetDirection();
                if (Projectile.localAI[2] != direction) {
                    Projectile.localAI[2] = direction;
                    Projectile.netUpdate = true;
                }
            }
        }

        Vector2 basePosition = player.MountedCenter + Vector2.UnitY * player.gfxOffY;
        Vector2 startPosition = basePosition,
                endPosition = Projectile.position;
        Vector2 diff = (endPosition - startPosition).SafeNormalize(Vector2.Zero);
        float rotation = diff.ToRotation() - MathHelper.PiOver2;

        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            Projectile.localAI[0] = rotation;
        }

        Projectile.position = Vector2.Lerp(Projectile.position, target.Center, 0.15f);

        if (Progress == 0f && Vector2.Distance(player.Center, target.Center) > 400f) {
            Projectile.Kill();
            Dusts();
            return;
        }

        if (player.whoAmI == Main.myPlayer && player.HeldItem.type == ModContent.ItemType<MercuriumZipper>() && Main.mouseLeft && Main.mouseLeftRelease) {
            Main.mouseLeftRelease = false;
            player.controlUseItem = false;
            player.itemAnimation = player.itemTime = 10;
            Projectile.Kill();
            Dusts();
            return;
        }

        float dist = Vector2.Distance(Projectile.position, target.Center);
        dist *= (target.Center - Projectile.position).X.GetDirection();
        Projectile.localAI[0] = Utils.AngleLerp(Projectile.localAI[0], MathHelper.Pi + dist * 0.01f, 0.1f);

        ref int damageDone = ref player.GetModPlayer<SummonDamageSum>().DamageDone;
        if (++Projectile.localAI[1] > 50f && Projectile.ai[2] != -100f) {
            int damageNeeded = (int)Projectile.ai[2] * 5;
            if (damageDone >= damageNeeded) {
                Projectile.ai[2] = -100f;
                Projectile.netUpdate = true;
            }
        }
        if (Projectile.ai[2] == -100f) {
            if (Progress < 1f) {
                Progress += 0.025f;

                Vector2 spawnPosition = Vector2.Lerp(Projectile.position, basePosition, Progress);
                if (Progress < 0.85f && Main.rand.NextBool(3)) {
                    Dust obj2 = Main.dust[Dust.NewDust(spawnPosition + diff * 7.5f - Vector2.One * 4, 8, 8, ModContent.DustType<MercuriumDust2>(),
                        diff.X, diff.Y,
                        0, default, 1f + Main.rand.NextFloat(0.2f, 0.5f))];
                    obj2.velocity *= 0.5f;
                    obj2.noGravity = true;
                    obj2.fadeIn = 0.5f;
                    obj2.noLight = true;
                }

                if (Math.Round(Projectile.localAI[1]) % 2.0 == 0.0 && Progress < 0.8f && player.whoAmI == Main.myPlayer) {
                    int spawnCount = 1;
                    ushort type = (ushort)ModContent.ProjectileType<MercuriumZipper_MercuriumCenserToxicFumes>();
                    IEntitySource source = player.GetSource_FromThis();
                    int damage = (int)(Projectile.damage * 0.75f);
                    float knockback = 0f;
                    for (int i = 0; i < spawnCount; i++) {
                        Vector2 velocity = diff.RotatedByRandom(MathHelper.PiOver4 * 1.2f);
                        Projectile.NewProjectile(source, spawnPosition + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)), velocity, type,
                            damage, knockback, player.whoAmI);
                    }
                }
            }
        }
        Progress = MathHelper.Clamp(Progress, 0f, 1f);
        if (Progress >= 1f) {
            Projectile.Kill();
        }
    }

    private void Dusts() {
        int height = 8;
        Player player = Main.player[Projectile.owner];
        Vector2 basePosition = player.MountedCenter + Vector2.UnitY * player.gfxOffY;
        Vector2 startPosition = basePosition,
                endPosition = Projectile.position;
        while (true) {
            float dist = Vector2.Distance(startPosition, endPosition);
            if (dist <= height) {
                break;
            }
            Vector2 diff = (endPosition - startPosition).SafeNormalize(Vector2.Zero);
            for (int i2 = 0; i2 < 2; i2++) {
                Vector2 vector39 = startPosition - Vector2.One * 4;
                Dust obj2 = Main.dust[Dust.NewDust(vector39, 8, 8, ModContent.DustType<MercuriumDust2>(), 0f, 0f, 0, default, 1f + Main.rand.NextFloat(0.2f, 0.5f))];
                obj2.velocity *= 0.5f;
                obj2.noGravity = true;
                obj2.fadeIn = 0.5f;
                obj2.noLight = true;
            }
            Vector2 diff2 = diff * height;
            startPosition += diff2;
        }
    }

    public override void OnKill(int timeLeft) {
        ref int damageDone = ref Main.player[Projectile.owner].GetModPlayer<SummonDamageSum>().DamageDone;
        damageDone = 0;
    }

    public override bool PreDraw(ref Color lightColor) {
        Player player = Main.player[Projectile.owner];
        Vector2 basePosition = player.MountedCenter + Vector2.UnitY * player.gfxOffY;
        Vector2 startPosition = Projectile.position,
                endPosition = basePosition;
        ModProjectile zipperWhip = ProjectileLoader.GetProjectile(ModContent.ProjectileType<MercuriumZipperProjectile>());
        SpriteEffects flip = (int)Projectile.localAI[2] > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        float opacity = Utils.GetLerpValue(0f, 0.35f, 1f - Progress, true);
        int max = 0;
        int height = 8;
        float progress2 = Math.Max(0.025f, 1f - Progress);
        while (true) {
            float dist = Vector2.Distance(startPosition, endPosition);
            if (dist <= height) {
                break;
            }
            Vector2 diff = (endPosition - startPosition).SafeNormalize(Vector2.Zero);
            Vector2 diff2 = diff * height;
            startPosition += diff2;
            max++;
        }
        startPosition = Projectile.position;
        float[] values = new float[max];
        float[] values2 = new float[max];
        for (int i = 0; i < max; i++) {
            values2[i] = -20f;
        }
        int index = 0;
        while (true) {
            Texture2D texture = ModContent.Request<Texture2D>(zipperWhip.Texture + "_Segment").Value;
            int width = 16;
            float dist = Vector2.Distance(startPosition, endPosition);
            if (dist <= height) {
                break;
            }
            Vector2 diff = (endPosition - startPosition).SafeNormalize(Vector2.Zero);
            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            rotation += MathHelper.Pi;
            for (int i = 0; i < max; i++) {
                if (values2[i] == -20f) {
                    values2[i] = rotation;
                }
            }
            Color color = Lighting.GetColor(startPosition.ToTileCoordinates()) * opacity;
            Rectangle frame = new(0, 0, width, height);
            Vector2 origin = frame.Size() / 2f;
            float scale = 1f;
            float progress = Progress;
            int index2 = max - index - 1;
            float current = (float)Math.Min(max, index2 + 1) / max;
            float progress3 = progress * MathHelper.Clamp(current * 2.5f, 0f, 1f);
            values[index2] = MathHelper.Lerp(values[index2], height * max / 3f * current, progress3);
            bool flag = (1f - current) > progress;
            float value = flag ? 0f : values[index2];
            int direction = (index % 2 == 0).ToDirectionInt();
            values2[index2] = MathHelper.Lerp(values2[index2], rotation + value * 0.01f * -direction, progress * current);
            Vector2 offset = new Vector2((height / 4f + (value * (float)progress * current)) * direction, 0f).RotatedBy(rotation);
            Main.EntitySpriteDraw(texture, startPosition + offset - Main.screenPosition, frame,
                color * opacity/*(1f - (!flag ? progress3 : value))*/, 
                values2[index2], origin, scale, flip, 0);

            Vector2 diff2 = diff * height;
            startPosition += diff2;
            index++;
        }
        Vector2 diff3 = (endPosition - startPosition).SafeNormalize(Vector2.Zero);
        float rotation2 = diff3.ToRotation() - MathHelper.PiOver2;
        rotation2 += MathHelper.Pi;
        Rectangle frame2 = new(0, 0, 18, 16);
        Vector2 origin2 = frame2.Size() / 2f;
        float scale2 = 1f;
        Vector2 offset2 = new Vector2(0f, height / 2f).RotatedBy(rotation2);
        Vector2 drawPosition = Vector2.Lerp(Projectile.position, basePosition, Progress);
        Color color2 = Lighting.GetColor(drawPosition.ToTileCoordinates());
        color2 *= opacity;
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(zipperWhip.Texture + "_Slider").Value,
            drawPosition +
            offset2 -
            Main.screenPosition, frame2,
            color2, rotation2, origin2, scale2, flip, 0);

        rotation2 = Utils.AngleLerp(Projectile.localAI[0], rotation2, MathHelper.Clamp(Progress * 5f, 0f, 1f));
        frame2 = new(0, 0, 14, 22);
        origin2 = frame2.Size() / 2f;
        scale2 = 1f;
        offset2 = new Vector2(0f, height + 2).RotatedBy(rotation2);
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(zipperWhip.Texture + "_Puller").Value,
            drawPosition +
            -offset2 -
            Main.screenPosition, frame2,
            color2, rotation2, origin2, scale2, flip, 0);

        return false;
    }
}

sealed class MercuriumZipperProjectile : ModProjectile {
    private sealed class AttackCountStorage : ModPlayer {
        public byte MercuriumZipperAttackCount;

        public override void ResetEffects() {
        }
    }

    private float Timer {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void SetStaticDefaults() {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.DefaultToWhip();
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ownerHitCheck = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.WhipSettings.Segments = 17;
        Projectile.WhipSettings.RangeMultiplier = 1.1f;
    }

    public override bool? CanCutTiles() => null;

    public static void FillWhipControlPoints(Projectile proj, List<Vector2> controlPoints) {
        Projectile.GetWhipSettings(proj, out var timeToFlyOut, out var segments, out var rangeMultiplier);
        rangeMultiplier *= 0.95f;
        float num = proj.ai[0] / timeToFlyOut;
        float num2 = 0.5f;
        float num3 = 1f + num2;
        float num4 = (float)Math.PI * 10f * (1f - num * num3) * (float)(-proj.spriteDirection) / (float)segments;
        float num5 = num * num3;
        float num6 = 0f;
        if (num5 > 1f) {
            num6 = (num5 - 1f) / num2;
            num5 = MathHelper.Lerp(1f, 0f, num6);
        }

        float num7 = proj.ai[0] - 1f;
        Player player = Main.player[proj.owner];
        Item heldItem = Main.player[proj.owner].HeldItem;
        num7 = (float)(ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * num * player.whipRangeMultiplier;
        float num8 = proj.velocity.Length() * num7 * num5 * rangeMultiplier / (float)segments;
        float num9 = 1f;
        Vector2 playerArmPosition = Main.GetPlayerArmPosition(proj);
        Vector2 vector = playerArmPosition;
        float num10 = 0f - (float)Math.PI / 2f;
        Vector2 vector2 = vector;
        float num11 = 0f + (float)Math.PI / 2f + (float)Math.PI / 2f * (float)proj.spriteDirection;
        Vector2 vector3 = vector;
        float num12 = 0f + (float)Math.PI / 2f;
        controlPoints.Add(playerArmPosition);
        for (int i = 0; i < segments; i++) {
            float num13 = (float)i / (float)segments;
            float num14 = num4 * num13 * num9;
            Vector2 vector4 = vector + num10.ToRotationVector2() * num8;
            Vector2 vector5 = vector3 + num12.ToRotationVector2() * (num8 * 2f);
            Vector2 vector6 = vector2 + num11.ToRotationVector2() * (num8 * 2f);
            float num15 = 1f - num5;
            float num16 = 1f - num15 * num15;
            Vector2 value = Vector2.Lerp(vector5, vector4, num16 * 0.9f + 0.1f);
            Vector2 vector7 = Vector2.Lerp(vector6, value, num16 * 0.7f + 0.3f);
            Vector2 spinningpoint = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1f, num3);
            float num17 = num6;
            num17 *= num17;
            Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num17 * (float)proj.spriteDirection, playerArmPosition);
            controlPoints.Add(item);
            num10 += num14;
            num12 += num14;
            num11 += num14;
            vector = vector4;
            vector3 = vector5;
            vector2 = vector6;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void AI() {
        if (Projectile.ai[2] == 0f) {
            Projectile.ai[2] = 1f;

            ref byte attackCount = ref Main.player[Projectile.owner].GetModPlayer<AttackCountStorage>().MercuriumZipperAttackCount;
            if (attackCount >= 4) {
                Projectile.ai[2] = 2f;
            }
            else {
                attackCount++;
            }
        }

        float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
        // Spawn Dust along the whip path
        // This is the dust code used by Durendal. Consult the Terraria source code for even more examples, found in Projectile.AI_165_Whip.
        float swingProgress = Timer / swingTime;
        // This code limits dust to only spawn during the the actual swing.
        /*if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f && !Main.rand.NextBool(3)) {
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);
            int pointIndex = Main.rand.Next(points.Count - 10, points.Count);
            Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
            int dustType = ModContent.DustType<Dusts.ToxicFumes>();

            // After choosing a randomized dust and a whip segment to spawn from, dust is spawned.
            Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 0, Color.White);
            dust.position = points[pointIndex];
            dust.fadeIn = 0.3f;
            Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
            dust.noGravity = true;
            dust.velocity *= 0.5f;
            // This math causes these dust to spawn with a velocity perpendicular to the direction of the whip segments, giving the impression of the dust flying off like sparks.
            dust.velocity += spinningPoint.RotatedBy(Main.player[Projectile.owner].direction * ((float)Math.PI / 2f));
            dust.velocity *= 0.5f;
        }*/

        if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.8f && !Main.rand.NextBool(4)) {
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);
            int pointIndex = Main.rand.Next(points.Count - 10, points.Count);
            Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
            int dustType = ModContent.DustType<MercuriumDust2>();

            // After choosing a randomized dust and a whip segment to spawn from, dust is spawned.
            Vector2 offset = (points[pointIndex] - points[pointIndex - 1]).SafeNormalize(Vector2.Zero) * 50f;
            Dust dust = Dust.NewDustDirect(spawnArea.TopLeft() +
                offset, spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 100, Color.White);
            dust.position = points[pointIndex] + offset;
            dust.fadeIn = 0.3f;
            dust.alpha = 150;
            Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
            dust.noGravity = true;
            dust.velocity *= 0.5f;
            // This math causes these dust to spawn with a velocity perpendicular to the direction of the whip segments, giving the impression of the dust flying off like sparks.
            dust.velocity += spinningPoint.RotatedBy(Main.player[Projectile.owner].direction * ((float)Math.PI / 2f));
            dust.velocity *= 0.5f;
        }

        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;
        player.MatchItemTimeToItemAnimation();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.immortal) {
            return;
        }

        target.AddBuff(ModContent.BuffType<MercuriumZipperDebuff>(), 240);

        Player player = Main.player[Projectile.owner];
        player.MinionAttackTargetNPC = target.whoAmI;
        Projectile.damage = (int)(Projectile.damage * 0.6f);

        //target.AddBuff(ModContent.BuffType<ToxicFumes>(), 180);

        NPC npc = target;
        NPC ownerMinionAttackTargetNPC2 = Projectile.OwnerMinionAttackTargetNPC;
        if (ownerMinionAttackTargetNPC2 != null) {
            npc = ownerMinionAttackTargetNPC2;
        }

        if (Projectile.ai[2] == 2f && player.whoAmI == Main.myPlayer) {
            Projectile.NewProjectile(Projectile.GetSource_OnHit(npc), npc.Center, Vector2.Zero,
                ModContent.ProjectileType<MercuriumZipper_Effect>(), Projectile.damage, Projectile.knockBack, Projectile.owner,
                npc.whoAmI, ai2: hit.Damage);

            Main.player[Projectile.owner].GetModPlayer<AttackCountStorage>().MercuriumZipperAttackCount = 0;

            Projectile.Kill();
        }
    }

    // This method draws a line between all points of the whip, in case there's empty space between the sprites.
    private void DrawLine(List<Vector2> list) {
        Texture2D texture = TextureAssets.FishingLine.Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width / 2, 2);

        Vector2 pos = list[0];
        for (int i = 0; i < list.Count - 2; i++) {
            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            Vector2 diff2 = diff.SafeNormalize(Vector2.Zero);

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
            Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

            pos += diff;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        List<Vector2> list = new List<Vector2>();
        FillWhipControlPoints(Projectile, list);

        DrawLine(list);

        //Main.DrawWhip_WhipBland(Projectile, list);
        // The code below is for custom drawing.
        // If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
        // However, you must adhere to how they draw if you do.

        SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Vector2 pos = list[0];

        for (int i = 0; i < list.Count - 1; i++) {
            // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
            // You can change them if they don't!

            Rectangle frame = new Rectangle(14, 2, 10, 22); // The size of the Handle (measured in pixels)
            var origin = new Vector2(frame.Width / 2, 2f);
            float scale = 1;

            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;
            Vector2 diff2 = diff.SafeNormalize(Vector2.Zero);

            // These statements determine what part of the spritesheet to draw for the current segment.
            // They can also be changed to suit your sprite.
            if (i == list.Count - 2) {
                // This is the head of the whip. You need to measure the sprite to figure out these values.
                frame.X = 12;
                frame.Y = 112;
                frame.Width = 14;
                frame.Height = 26;

                // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                //Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                //float t = Timer / timeToFlyOut;
                //scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
            }
            else if (i > 10) {
                // Third segment
                frame.X = 14;
                frame.Y = 90;
                frame.Width = 10;
                frame.Height = 18;
            }
            else if (i > 5) {
                // Second Segment
                frame.X = 14;
                frame.Y = 62;
                frame.Width = 10;
                frame.Height = 18;
            }
            else if (i > 0) {
                // First Segment
                frame.X = 14;
                frame.Y = 34;
                frame.Width = 10;
                frame.Height = 18;
            }

            float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

            pos += diff;
        }

        return false;
    }
}
