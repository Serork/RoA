﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class SerpentChain : ModProjectile_NoTextureLoad, IRequestAssets {
    public sealed class SerpentChain_ReduceTargetNPCImmunityFrames : GlobalProjectile {
        public static float IMMUNITYFRAMEMODIFIER => 0.5f;

        private static int _previousImmune, _previousLocalImmune;
        private static uint _previousStaticImmune;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.DamageType == DamageClass.Summon || entity.minion || entity.DamageType == DamageClass.MagicSummonHybrid;

        public override bool? CanHitNPC(Projectile projectile, NPC target) {
            Player owner = projectile.GetOwnerAsPlayer();
            int ownerWhoAmI = projectile.owner;
            if (!owner.HasProjectile<SerpentChain>()) {
                return base.CanHitNPC(projectile, target);
            }
            Projectile serpent = TrackedEntitiesSystem.GetTrackedProjectile<SerpentChain>(checkProjectile => checkProjectile.owner != ownerWhoAmI).ToList()[0];
            if (serpent.ai[0] == 1f || serpent.ai[2] < 0.25f) {
                return base.CanHitNPC(projectile, target);
            }

            int i = target.whoAmI;
            if (i != owner.MinionAttackTargetNPC) {
                return base.CanHitNPC(projectile, target);
            }

            bool? result = base.CanHitNPC(projectile, target);
            if (target.immune[ownerWhoAmI] == 0) {
                _previousImmune = target.immune[ownerWhoAmI];
            }
            int type = projectile.type;
            if (Projectile.perIDStaticNPCImmunity[type][i] == Main.GameUpdateCount) {
                _previousStaticImmune = Projectile.perIDStaticNPCImmunity[type][i] + (uint)projectile.idStaticNPCHitCooldown;
            }
            if (projectile.localNPCImmunity[i] == 0) {
                _previousLocalImmune = projectile.localNPCHitCooldown;
            }

            return base.CanHitNPC(projectile, target);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
            Player owner = projectile.GetOwnerAsPlayer();
            int ownerWhoAmI = projectile.owner;
            if (!owner.HasProjectile<SerpentChain>()) {
                return;
            }
            Projectile serpent = TrackedEntitiesSystem.GetTrackedProjectile<SerpentChain>(checkProjectile => checkProjectile.owner != ownerWhoAmI).ToList()[0];
            if (serpent.ai[0] == 1f || serpent.ai[2] < 0.25f) {
                return;
            }

            int i = target.whoAmI;
            if (i != owner.MinionAttackTargetNPC) {
                return;
            }

            int type = projectile.type;
            if (target.immune[ownerWhoAmI] == _previousImmune) {
                target.immune[ownerWhoAmI] = (int)(target.immune[ownerWhoAmI] * IMMUNITYFRAMEMODIFIER);
            }
            int staticCooldown = projectile.idStaticNPCHitCooldown;
            if (Projectile.perIDStaticNPCImmunity[type][i] == _previousStaticImmune) {
                Projectile.perIDStaticNPCImmunity[type][i] = Main.GameUpdateCount + (uint)(projectile.idStaticNPCHitCooldown * IMMUNITYFRAMEMODIFIER);
            }
            if (projectile.localNPCImmunity[i] == _previousLocalImmune) {
                projectile.localNPCImmunity[i] = (int)(projectile.localNPCImmunity[i] * IMMUNITYFRAMEMODIFIER);
            }
        }
    }

    public enum SerpentChainRequstedTextureType : byte {
        Head,
        Body
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)SerpentChainRequstedTextureType.Head, ResourceManager.FriendlyMiscProjectiles + "SerpentChain2"),
         ((byte)SerpentChainRequstedTextureType.Body, ResourceManager.FriendlyMiscProjectiles + "SerpentChain1")];

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.friendly = true;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        Projectile.localAI[0]++;

        Player owner = Projectile.GetOwnerAsPlayer();
        Vector2 center = owner.Center;
        float minDistance = 20f;
        float speed = 10f;
        float maxDistance = 16f * 20;
        if (!owner.HasMinionAttackTargetNPC) {
            Projectile.SlightlyMoveTo2(center, speed);
            if (Projectile.Distance(center) < minDistance) {
                Projectile.Kill();
            }
            return;
        }
        NPC target = Main.npc[owner.MinionAttackTargetNPC];
        Vector2 moveTo = target.Center;
        float distance = target.Distance(center);
        if (distance > maxDistance) {
            moveTo = owner.Center;
            if (Projectile.Distance(moveTo) < minDistance) {
                Projectile.Kill();
            }
            Projectile.ai[1] = 1f;
        }
        else {
            Projectile.ai[1] = 0f;
        }
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.ai[2] = 1f - MathUtils.Clamp01(target.Distance(Projectile.Center) / 100f);
        Projectile.SlightlyMoveTo2(moveTo, speed, deceleration: 0.97f - Projectile.localAI[1] * 0.17f);
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<SerpentChain>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Player owner = Projectile.GetOwnerAsPlayer();
        Vector2 startPosition = Utils.Floor(owner.Center) + Vector2.UnitY * owner.gfxOffY,
                endPosition = Projectile.Center;
        float opacity = Projectile.Opacity;
        Asset<Texture2D> chainTexture = indexedTextureAssets[(byte)SerpentChainRequstedTextureType.Body],
                         headTexture = indexedTextureAssets[(byte)SerpentChainRequstedTextureType.Head];
        for (int i = 0; i < 200; i++) {
            Asset<Texture2D> texture = chainTexture;
            bool start = i == 0;
            if (start) {
                texture = headTexture;
            }
            Vector2 dif = startPosition - endPosition;
            float height = texture.Height() * (start ? 0.5f : 0.8f);
            Vector2 chainVelocity = Vector2.Normalize(dif) * height;
            Vector2 chainOrigin = texture.Size() / 2f;
            float minDistance = height * 3f;
            Vector2 shakeVector = Vector2.Normalize(chainVelocity).RotatedBy(MathHelper.PiOver2);
            float intensity = 1f ;
            Vector2 offset = Vector2.Zero;
            float rotation = chainVelocity.ToRotation() - MathHelper.PiOver2;
            float chainDistance = Vector2.Distance(endPosition, startPosition);
            if (intensity > 0f && chainDistance > 8f) {
                float targetMultiplier = 1f;
                if (owner.HasMinionAttackTargetNPC) {
                    NPC target = Main.npc[owner.MinionAttackTargetNPC];
                    targetMultiplier = MathUtils.Clamp01(target.Distance(endPosition) / 100f);
                }
                intensity *= targetMultiplier;
                float shakeMultiplier = Math.Min((chainDistance - 8f) / 200f, 1f);
                float shakeValue = MathF.Sin(Projectile.localAI[0] * 10f + i * 0.3f) * intensity;
                offset += shakeVector * shakeValue * 12.5f * shakeMultiplier;
                if (!start) {
                    rotation -= shakeValue * 0.2f;
                }
            }
            Main.EntitySpriteDraw(texture.Value, endPosition - Main.screenPosition + offset, null, Lighting.GetColor(endPosition.ToTileCoordinates()) * opacity, rotation, chainOrigin, 1f, SpriteEffects.None);
            endPosition += chainVelocity;
            if (Vector2.DistanceSquared(endPosition, startPosition) < minDistance) {
                break;
            }
        }
    }

    public override void OnKill(int timeLeft) {
        base.OnKill(timeLeft);
    }
}
