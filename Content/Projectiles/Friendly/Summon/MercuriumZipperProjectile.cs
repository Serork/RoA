using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Buffs;
using RoA.Core.Utility;

using System;
using System.Collections;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class MercuriumZipperProjectile : ModProjectile {
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
        rangeMultiplier *= 0.8f;
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
        float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
        // Spawn Dust along the whip path
        // This is the dust code used by Durendal. Consult the Terraria source code for even more examples, found in Projectile.AI_165_Whip.
        float swingProgress = Timer / swingTime;
        // This code limits dust to only spawn during the the actual swing.
        if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f && !Main.rand.NextBool(3)) {
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
        }

        float num = Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true);
        if (num > 0.7f && num < 0.85f && !Main.rand.NextBool(4)) {
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);
            int pointIndex = Main.rand.Next(points.Count - 10, points.Count);
            Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
            int dustType = DustID.Poisoned;

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
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<MercuriumZipperDebuff>(), 240);

        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        Projectile.damage = (int)(Projectile.damage * 0.6f);

        target.AddBuff(ModContent.BuffType<ToxicFumes>(), 180);
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
