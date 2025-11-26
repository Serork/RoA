using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Players;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

[Tracked]
sealed class CorruptorBone : ModProjectile {
    private static byte FRAMECOUNT => 5;
    private static ushort TIMELEFT => 300;
    private static byte BONECOUNTTOSPAWNCORRUPTOR => 5;

    public ref struct CorruptorBoneValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float MouseXValue = ref projectile.ai[0];
        public ref float MouseYValue = ref projectile.ai[1];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public Vector2 MousePosition {
            readonly get => new(MouseXValue, MouseYValue);
            set {
                MouseXValue = value.X;
                MouseYValue = value.Y;
            }
        }
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(FRAMECOUNT);
        Projectile.SetTrail(0, 2);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(14);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.penetrate = -1;
    }

    public override void AI() {
        ushort type = (ushort)ModContent.DustType<Dusts.Corruptor2>();
        if (Projectile.velocity.Length() > 2.5f && Main.rand.NextBool(32)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, type);
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(1f, 1.5f) * 0.8f;
        }

        Projectile.Opacity = Utils.GetLerpValue(0, 7, Projectile.timeLeft, true) * Utils.GetLerpValue(TIMELEFT, TIMELEFT - 1, Projectile.timeLeft, true);

        void init() {
            CorruptorBoneValues corruptorBoneValues = new(Projectile);
            if (!corruptorBoneValues.Init) {
                corruptorBoneValues.Init = true;

                Projectile.frame = Main.rand.Next(FRAMECOUNT);

                //Projectile.damage /= 5;
                //Projectile.knockBack /= 5;

                float startSpeed = 7.5f;
                Projectile.velocity = Projectile.velocity.SafeNormalize() * startSpeed;
                if (Projectile.IsOwnerLocal()) {
                    Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4);

                    corruptorBoneValues.MousePosition = Projectile.GetOwnerAsPlayer().GetWorldMousePosition();

                    Projectile.netUpdate = true;
                }
            }
        }
        void rotate() {
            Projectile.rotation += Projectile.velocity.X * 0.05f;
        }
        void shiftToMousePosition() {
            CorruptorBoneValues corruptorBoneValues = new(Projectile);

            ref Vector2 velocity = ref Projectile.velocity;
            velocity = Vector2.Lerp(velocity, Projectile.Center.DirectionTo(corruptorBoneValues.MousePosition) * velocity.Length(), 0.1f);
        }
        void offsetSame() {
            Projectile.OffsetTheSameProjectile(0.2f);
        }

        init();
        rotate();
        shiftToMousePosition();
        offsetSame();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        ushort type = (ushort)ModContent.DustType<Dusts.Corruptor2>();
        for (int i = 0; i < 2; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, type);
            dust.noGravity = true;
        }

        return base.OnTileCollide(oldVelocity);
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawShadowTrails(lightColor * Projectile.Opacity, 0.5f, 1, Projectile.rotation);
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, 0f);

        return false;
    }

    private class ConnectBones : ModSystem {
        public override void PreUpdateProjectiles() {
            IEnumerable<Projectile> corruptorBoneProjectiles = TrackedEntitiesSystem.GetTrackedProjectile<CorruptorBone>(checkForType: false);
            float maxAttractionDistance = 200f;
            float fuseDistance = 20f;
            maxAttractionDistance *= maxAttractionDistance;
            fuseDistance *= fuseDistance;
            float attractionSpeed = 0.05f;
            int boneCountNeeded = BONECOUNTTOSPAWNCORRUPTOR - 1;
            float minVelocityNeeded = 1f;
            foreach (Projectile corruptorBoneProjectile in corruptorBoneProjectiles) {
                foreach (Projectile checkCorruptorBoneProjectile in corruptorBoneProjectiles) {
                    if (checkCorruptorBoneProjectile == corruptorBoneProjectile || checkCorruptorBoneProjectile.owner != corruptorBoneProjectile.owner || !corruptorBoneProjectile.active || !checkCorruptorBoneProjectile.active) {
                        continue;
                    }
                    float distance = corruptorBoneProjectile.DistanceSQ(checkCorruptorBoneProjectile.Center);
                    if (distance < fuseDistance) {
                        if (corruptorBoneProjectile.velocity.Length() <= minVelocityNeeded) {
                            int countNearby = 0;
                            HashSet<Projectile> countedBoneProjectiles = [];
                            foreach (Projectile countCorruptorBoneProjectile in corruptorBoneProjectiles) {
                                if (countCorruptorBoneProjectile == corruptorBoneProjectile || countCorruptorBoneProjectile.owner != corruptorBoneProjectile.owner || !corruptorBoneProjectile.active || !countCorruptorBoneProjectile.active) {
                                    continue;
                                }
                                if (countCorruptorBoneProjectile.velocity.Length() <= minVelocityNeeded) {
                                    if (corruptorBoneProjectile.DistanceSQ(countCorruptorBoneProjectile.Center) < fuseDistance) {
                                        countNearby++;
                                        countedBoneProjectiles.Add(countCorruptorBoneProjectile);
                                    }
                                }
                            }
                            if (countNearby >= boneCountNeeded) {
                                corruptorBoneProjectile.Kill();
                                foreach (Projectile corruptorBoneProjectileToKill in countedBoneProjectiles) {
                                    corruptorBoneProjectileToKill.Kill();
                                }
                                Player owner = corruptorBoneProjectile.GetOwnerAsPlayer();
                                if (owner.IsLocal()) {
                                    float spawnSpeed = 5f;
                                    ProjectileUtils.SpawnPlayerOwnedProjectile<Corruptor>(new ProjectileUtils.SpawnProjectileArgs(owner, owner.GetSource_ReleaseEntity()) {
                                        Position = corruptorBoneProjectile.Center,
                                        Velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * spawnSpeed,
                                        Damage = corruptorBoneProjectile.damage * 5,
                                        KnockBack = corruptorBoneProjectile.knockBack * 5
                                    });
                                    SoundEngine.PlaySound(SoundID.NPCDeath25 with { Volume = 0.9f, Pitch = 1f, MaxInstances = 3 }, corruptorBoneProjectile.Center);
                                }
                            }
                        }
                    }
                    else if (distance < maxAttractionDistance) {
                        Vector2 velocity = corruptorBoneProjectile.DirectionTo(checkCorruptorBoneProjectile.Center) * (1f - distance / maxAttractionDistance) * attractionSpeed;
                        corruptorBoneProjectile.velocity += velocity;
                    }
                }
            }
        }
    }
}
