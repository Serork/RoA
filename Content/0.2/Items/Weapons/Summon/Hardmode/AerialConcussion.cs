﻿using Microsoft.Xna.Framework;

using RoA.Common.Items;
using RoA.Content.Dusts;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon.Hardmode;

sealed class AerialConcussion : WhipBase {
    protected override int TagDamage => 10;
    protected override int SegmentCount => 15;
    protected override float RangeMultiplier => 1.5f;
    protected override Rectangle TailClip => new(14, 0, 14, 24);
    protected override Rectangle Body1Clip => new(14, 34, 14, 18);
    protected override Rectangle Body2Clip => new(14, 62, 14, 18);
    protected override Rectangle Body3Clip => new(14, 90, 14, 18);
    protected override Rectangle TipClip => new(12, 112, 20, 26);

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 34);
    }

    protected override bool Flip() => true;

    protected override float Scale(Player player, int index, float attackProgress, float lengthProgress) => index == 0 ? 1f : MathHelper.Lerp(0.5f, MathHelper.Lerp(1f, 1.5f, lengthProgress), Utils.GetLerpValue(0.1f, 0.7f, attackProgress, true) * Utils.GetLerpValue(0.9f, 0.7f, attackProgress, true));

    protected override void OnUse(Player player, float swingProgress, List<Vector2> points) {
        float t6 = swingProgress;
        float num8 = Utils.GetLerpValue(0.1f, 0.7f, t6, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, t6, clamped: true);
        if (num8 > 0.2f && Main.rand.NextFloat() < num8 * 0.9f) {
            int pointIndex = Main.rand.Next(points.Count - 5, points.Count);
            Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(15f, 15f));
            int dustType = ModContent.DustType<Dusts.AerialConcussion>();

            // After choosing a randomized dust and a whip segment to spawn from, dust is spawned.
            Vector2 offset = (points[pointIndex] - points[pointIndex - 1]).SafeNormalize(Vector2.Zero) * 70f;
            Dust dust = Dust.NewDustDirect(spawnArea.TopLeft() +
                offset, spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, Main.rand.Next(25, 75), Color.Lerp(default, Color.White, Main.rand.NextFloat()));
            dust.position = points[pointIndex] + offset;
            dust.fadeIn = -5f;
            Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
            dust.scale *= 1.25f;
            dust.velocity *= 0.5f;
            // This math causes these dust to spawn with a velocity perpendicular to the direction of the whip segments, giving the impression of the dust flying off like sparks.
            dust.velocity += spinningPoint.RotatedBy(player.direction * ((float)Math.PI / 2f));
            dust.velocity *= 0.375f;
        }
    }
}