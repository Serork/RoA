using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Generic;
using RoA.Common.NPCs;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Summon;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Items;

abstract class WhipBase : ModItem {
    public delegate float ScaleDelegate(Player player, int index, float attackProgress, float lengthProgress);
    public delegate void OnUseDelegate(Player player, float swingProgress, List<Vector2> points);
    public delegate void OnHitDelegate(Player player, NPC target);

    public static WhipBase_TagDamageDebuff? Debuff { get; private set; }
    public static WhipBase_Projectile? Whip { get; private set; }

    protected abstract int TagDamage { get; }
    protected abstract float DamagePenalty { get; }
    protected abstract int SegmentCount { get; }
    protected abstract float RangeMultiplier { get; }
    protected virtual Rectangle TailClip { get; }
    protected virtual Rectangle Body1Clip { get; }
    protected virtual Rectangle Body2Clip { get; }
    protected virtual Rectangle Body3Clip { get; }
    protected virtual Rectangle TipClip { get; }

    public override void Load() {
        Debuff = new WhipBase_TagDamageDebuff(this, TagDamage);
        Mod.AddContent(Debuff);
        Whip = new WhipBase_Projectile(new WhipBase_ProjectileArgs(this, Debuff, SegmentCount, RangeMultiplier, DamagePenalty, string.Empty, TailClip, Body1Clip, Body2Clip, Body3Clip, TipClip, Flip(), DrawLine(), Scale, OnUse, OnHit));
        Mod.AddContent(Whip);
    }

    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(Debuff!.TagDamage);

    public override void SetDefaults() {
        Item.DefaultToWhip(Whip!.Type, 21, 2, 4);

        Item.useStyle = ItemUseStyleID.HiddenAnimation;

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 40, 0);

        SafeSetDefaults();
    }

    protected virtual void SafeSetDefaults() { }

    public override bool MeleePrefix() => true;

    protected virtual bool Flip() => false;

    protected virtual bool DrawLine() => true;

    protected virtual float Scale(Player player,int index, float attackProgress, float lengthProgress) => 1f;

    protected virtual void OnUse(Player player, float swingProgress, List<Vector2> points) { }

    protected virtual void OnHit(Player player, NPC target) { }
}

sealed class WhipBase_TagDamageDebuff(WhipBase attachedWhip, int tagDamage, string nameSuffix = "") : InstancedBuff($"{attachedWhip.Name}{nameSuffix}", ResourceManager.BuffTextures + "BuffTemplate") {
    public int TagDamage { get; init; } = tagDamage;

    [CloneByReference]
    public WhipBase AttachedWhip { get; init; } = attachedWhip;

    public override LocalizedText DisplayName => this.GetLocalization("DisplayName");
    public override LocalizedText Description => this.GetLocalization("Description");

    public override void SetStaticDefaults() {
        BuffID.Sets.IsATagBuff[Type] = true;
    }
}

readonly record struct WhipBase_ProjectileArgs(WhipBase AttachedWhip, WhipBase_TagDamageDebuff TagDebuff, int SegmentCount, float RangeMultiplier, float DamagePenalty, string NameSuffix = "",
                                               Rectangle? TailClip = null,
                                               Rectangle? Body1Clip = null,
                                               Rectangle? Body2Clip = null,
                                               Rectangle? Body3Clip = null,
                                               Rectangle? TipClip = null,
                                               bool Flip = false,
                                               bool DrawLine = true,
                                               WhipBase.ScaleDelegate? ScaleFunction = null,
                                               WhipBase.OnUseDelegate? OnUseFunction = null,
                                               WhipBase.OnHitDelegate? OnHitFunction = null);

sealed class WhipBase_Projectile(WhipBase_ProjectileArgs args) : InstancedProjectile($"{args.AttachedWhip.Name}{args.NameSuffix}", ResourceManager.SummonProjectileTextures + $"{args.AttachedWhip.Name}Whip") {
    [CloneByReference]
    private readonly WhipBase.OnHitDelegate? _onHitFunction = args.OnHitFunction;

    [CloneByReference]
    public WhipBase_TagDamageDebuff TagDebuff { get; init; } = args.TagDebuff;

    public override void Load() {
        NPCCommonHandler.ModifyHitByProjectileEvent += RoANPC_ModifyHitByProjectileEvent;
    }

    private void RoANPC_ModifyHitByProjectileEvent(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        if (projectile.ModProjectile is WhipBase_Projectile whip && !npc.immortal && npc.lifeMax > 5) {
            whip._onHitFunction?.Invoke(projectile.GetOwnerAsPlayer(), npc);
        }

        if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated) {
            return;
        }

        if (TryGetActiveWhip(projectile.GetOwnerAsPlayer(), out Projectile heldProjectile, out WhipBase_Projectile heldWhip)) {
            WhipBase_TagDamageDebuff whipBuff = heldWhip.TagDebuff;
            if (npc.HasBuff(whipBuff.Type)) {
                float tagDamageMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
                modifiers.FlatBonusDamage += whipBuff.TagDamage * tagDamageMultiplier;
            }
        }
    }

    public static bool TryGetActiveWhip(Player player, out Projectile projectile, out WhipBase_Projectile whip) {
        projectile = null;
        whip = null;

        if (player.heldProj >= 0 && player.heldProj < Main.maxProjectiles) {
            projectile = Main.projectile[player.heldProj];
            if (projectile.ModProjectile is not WhipBase_Projectile whipBase) {
                return false;
            }
            whip = whipBase;
            return true;
        }

        for (int i = 0; i < Main.maxProjectiles; i++) {
            projectile = Main.projectile[i];
            if (projectile.owner == player.whoAmI && projectile.ModProjectile is WhipBase_Projectile whipBase) {
                player.heldProj = projectile.whoAmI;
                whip = whipBase;
                return true;
            }
        }

        return false;
    }

    public float Timer {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void SetStaticDefaults() {
        ProjectileID.Sets.IsAWhip[Type] = true;
        //ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ownerHitCheck = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.WhipSettings.Segments = 19;
        Projectile.WhipSettings.RangeMultiplier = args.RangeMultiplier;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

        Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
        // Vanilla uses Vector2.Dot(Projectile.velocity, Vector2.UnitX) here. Dot Product returns the difference between two vectors, 0 meaning they are perpendicular.
        // However, the use of UnitX basically turns it into a more complicated way of checking if the projectile's velocity is above or equal to zero on the X axis.
        Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

        Timer++;

        float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
        if (Timer >= swingTime || owner.itemAnimation <= 0) {
            Projectile.Kill();
            return;
        }

        owner.heldProj = Projectile.whoAmI;
        if (Timer == swingTime / 2) {
            // Plays a whipcrack sound at the tip of the whip.
            List<Vector2> points = Projectile.WhipPointsForCollision;
            FillWhipControlPoints(Projectile, points);
            SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
        }

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

        Player player = Main.player[Projectile.owner];
        if (args.OnUseFunction != null) {
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);
            args.OnUseFunction(player, swingProgress, points);
        }

        player.heldProj = Projectile.whoAmI;
        player.MatchItemTimeToItemAnimation();

        if ((double)player.itemAnimation < (double)player.itemAnimationMax * 0.333)
            player.bodyFrame.Y = player.bodyFrame.Height * 3;
        else if ((double)player.itemAnimation < (double)player.itemAnimationMax * 0.666)
            player.bodyFrame.Y = player.bodyFrame.Height * 2;
        else
            player.bodyFrame.Y = player.bodyFrame.Height;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.immortal) {
            return;
        }

        target.AddBuff(args.TagDebuff.Type, 240);

        Player player = Main.player[Projectile.owner];
        player.MinionAttackTargetNPC = target.whoAmI;
        Projectile.damage = (int)(Projectile.damage * args.DamagePenalty);
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

        if (args.DrawLine) {
            DrawLine(list);
        }

        //Main.DrawWhip_WhipBland(Projectile, list);
        // The code below is for custom drawing.
        // If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
        // However, you must adhere to how they draw if you do.

        SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        if (args.Flip) {
            flip = (flip == SpriteEffects.None) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Vector2 pos = list[0];

        float count = list.Count - 1;
        for (int i = 0; i < count; i++) {
            float progress = (float)i / count;

            // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
            // You can change them if they don't!

            Rectangle frame; // The size of the Handle (measured in pixels)
            if (args.TailClip != null) {
                frame = args.TailClip.Value;
            }
            else {
                frame = new Rectangle(14, 2, 10, 22);
            }
            float scale = 1;

            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;
            Vector2 diff2 = diff.SafeNormalize(Vector2.Zero);

            // These statements determine what part of the spritesheet to draw for the current segment.
            // They can also be changed to suit your sprite.

            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
            float t = Timer / timeToFlyOut;
            if (args.ScaleFunction != null) {
                scale = args.ScaleFunction(Main.player[Projectile.owner], i, t, progress);
            }

            int segmentVariant = i == 0 ? 0 : GetSegmentVariant(i);
            if (i == list.Count - 2) {
                if (args.TipClip != null) {
                    frame = args.TipClip.Value;
                }
                else {
                    // This is the head of the whip. You need to measure the sprite to figure out these values.
                    frame.X = 12;
                    frame.Y = 112;
                    frame.Width = 14;
                    frame.Height = 26;
                }

                // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
            }
            else if (segmentVariant == 3) {
                if (args.Body3Clip != null) {
                    frame = args.Body3Clip.Value;
                }
                else {
                    // Third segment
                    frame.X = 14;
                    frame.Y = 90;
                    frame.Width = 10;
                    frame.Height = 18;
                }
            }
            else if (segmentVariant == 2) {
                if (args.Body2Clip != null) {
                    frame = args.Body2Clip.Value;
                }
                else {
                    // Second Segment
                    frame.X = 14;
                    frame.Y = 62;
                    frame.Width = 10;
                    frame.Height = 18;
                }
            }
            else if (segmentVariant == 1) {
                if (args.Body1Clip != null) {
                    frame = args.Body1Clip.Value;
                }
                else {
                    // First Segment
                    frame.X = 14;
                    frame.Y = 34;
                    frame.Width = 10;
                    frame.Height = 18;
                }
            }

            Vector2 origin = frame.Size() / 2f;
            origin.Y = 2f;

            float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct _rotation.
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

            if (i == 0) {
                pos += diff * 0.25f;
            }

            pos += diff;
        }

        return false;
    }

    private int GetSegmentVariant(int segment) => 1 + segment % 3;

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
        Vector2 playerArmPosition = Main.GetPlayerArmPosition(proj) - new Vector2(0f, player.gfxOffY);
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
}
