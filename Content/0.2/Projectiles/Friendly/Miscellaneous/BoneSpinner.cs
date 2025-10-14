using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class BoneSpinner : ModProjectile {
    public sealed class BoneSpinner_MakeAmmoHoming : GlobalProjectile {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Damage_GetHitbox")]
        public extern static Rectangle Projectile_Damage_GetHitbox(Projectile self);

        private float _target;
        private float _init;

        public bool IsEffectActive;

        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.DamageType == DamageClass.Ranged;

        public override void PostAI(Projectile projectile) {
            if (!IsEffectActive) {
                return;
            }

            //if (type == 207 && alpha < 170) {
            //    for (int num182 = 0; num182 < 10; num182++) {
            //        float x2 = position.X - velocity.X / 10f * (float)num182;
            //        float y2 = position.Y - velocity.Y / 10f * (float)num182;
            //        int num183 = Dust.NewDust(new Vector2(x2, y2), 1, 1, 75);
            //        Main.dust[num183].alpha = alpha;
            //        Main.dust[num183].position.X = x2;
            //        Main.dust[num183].position.Y = y2;
            //        Main.dust[num183].velocity *= 0f;
            //        Main.dust[num183].noGravity = true;
            //    }
            //}

            float num184 = (float)Math.Sqrt(projectile.velocity.X * projectile.velocity.X + projectile.velocity.Y * projectile.velocity.Y);
            float num185 = _init;
            if (num185 == 0f) {
                _init = num184;
                num185 = num184;

                for (int num615 = 0; num615 < 3; num615++) {
                    if (Main.rand.NextBool(3)) {
                        continue;
                    }
                    Vector2 direction = projectile.velocity.RotateRandom(MathHelper.PiOver4 / 2f);
                    int num616 = Dust.NewDust(projectile.Center + Main.rand.RandomPointInArea(36) / 3f, 0, 0, DustID.Bone, direction.X, direction.Y, 0, 
                        default, 1f + 0.1f * Main.rand.NextFloatDirection());
                    Main.dust[num616].noGravity = Main.rand.NextBool();
                    Dust dust2 = Main.dust[num616];
                    if (!Main.dust[num616].noGravity) {
                        dust2.scale *= 1.25f;
                    }
                    dust2 = Main.dust[num616];
                    dust2.velocity *= 0.5f;
                }
            }

            float num186 = projectile.position.X;
            float num187 = projectile.position.Y;
            float num188 = 300f;
            bool flag5 = false;
            int num189 = 0;
            if (_target == 0f) {
                for (int num190 = 0; num190 < 200; num190++) {
                    if (Main.npc[num190].CanBeChasedBy(this) && (_target == 0f || _target == (float)(num190 + 1))) {
                        float num191 = Main.npc[num190].position.X + (float)(Main.npc[num190].width / 2);
                        float num192 = Main.npc[num190].position.Y + (float)(Main.npc[num190].height / 2);
                        float num193 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num191) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num192);
                        if (num193 < num188 && Collision.CanHit(new Vector2(projectile.position.X + (float)(projectile.width / 2), projectile.position.Y + (float)(projectile.height / 2)), 1, 1, Main.npc[num190].position, Main.npc[num190].width, Main.npc[num190].height)) {
                            num188 = num193;
                            num186 = num191;
                            num187 = num192;
                            flag5 = true;
                            num189 = num190;
                        }
                    }
                }

                if (flag5)
                    _target = num189 + 1;

                flag5 = false;
            }

            if (_target > 0f) {
                int num194 = (int)(_target - 1f);
                if (Main.npc[num194].active && Main.npc[num194].CanBeChasedBy(this, ignoreDontTakeDamage: true) && !Main.npc[num194].dontTakeDamage) {
                    float num195 = Main.npc[num194].position.X + (float)(Main.npc[num194].width / 2);
                    float num196 = Main.npc[num194].position.Y + (float)(Main.npc[num194].height / 2);
                    if (Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num195) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num196) < 1000f) {
                        flag5 = true;
                        num186 = Main.npc[num194].position.X + (float)(Main.npc[num194].width / 2);
                        num187 = Main.npc[num194].position.Y + (float)(Main.npc[num194].height / 2);
                    }
                }
                else {
                    _target = 0f;
                }
            }

            if (!projectile.friendly)
                flag5 = false;

            if (flag5) {
                float num197 = num185;
                Vector2 vector26 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                float num198 = num186 - vector26.X;
                float num199 = num187 - vector26.Y;
                float num200 = (float)Math.Sqrt(num198 * num198 + num199 * num199);
                num200 = num197 / num200;
                num198 *= num200;
                num199 *= num200;
                int num201 = 8;
                //if (type == 837)
                //    num201 = 32;

                projectile.velocity.X = (projectile.velocity.X * (float)(num201 - 1) + num198) / (float)num201;
                projectile.velocity.Y = (projectile.velocity.Y * (float)(num201 - 1) + num199) / (float)num201;
            }
        }
    }

    private static byte FRAMECOUNT => 3;

    public ref float State => ref Projectile.ai[1];

    public float RotationSlow => MathF.Max(0.25f, MathUtils.Clamp01((Projectile.penetrate + 1) / (float)Projectile.maxPenetrate * 3));

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(FRAMECOUNT);
        Projectile.SetTrail(3, 5);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(36);

        Projectile.aiStyle = -1;
        Projectile.friendly = true;

        Projectile.tileCollide = true;

        Projectile.penetrate = 10;

        Projectile.timeLeft = 600;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 24;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override void AI() {
        ref float initValue = ref Projectile.localAI[0];
        if (initValue == 0f) {
            initValue = 1f;

            if (Projectile.IsOwnerLocal()) {
                Projectile.velocity.Y = -MathF.Abs(Projectile.velocity.Length());
                Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4);

                Projectile.velocity.X *= Main.rand.NextFloatDirection();

                Projectile.netUpdate = true;
            }
        }

        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner != Projectile.owner) {
                continue;
            }

            if (projectile.DamageType != DamageClass.Ranged) {
                continue;
            }

            var handler = projectile.GetGlobalProjectile<BoneSpinner_MakeAmmoHoming>();
            if (handler.IsEffectActive) {
                continue;
            }

            Rectangle rectangle = BoneSpinner_MakeAmmoHoming.Projectile_Damage_GetHitbox(projectile);
            if (projectile.Colliding(rectangle, Projectile.getRect())) {
                handler.IsEffectActive = true;

                if (Projectile.penetrate-- <= 0) {
                    Projectile.Kill();
                }

                if (!Collision.SolidCollision(Projectile.position + Vector2.One * 3f, Projectile.width - 6, Projectile.height - 6)) {
                    Projectile.position += projectile.velocity.SafeNormalize() * 2f;
                }

                if (projectile.IsOwnerLocal()) {
                    projectile.velocity = -projectile.velocity.RotatedByRandom(MathHelper.PiOver2);
                    projectile.netUpdate = true;
                }

                continue;
            }
        }

        Projectile.OffsetTheSameProjectile(0.1f);

        Projectile.velocity.X *= 0.95f;
        Projectile.velocity.Y *= 0.915f;

        ref float rotationTime = ref Projectile.ai[0];
        float neededTime = 30f;
        float neededTimeToSwapState = neededTime * 1.1f;
        Projectile.rotation += MathF.Min(neededTime, rotationTime) / neededTime * 0.5f * Projectile.direction * RotationSlow;
        if (rotationTime >= neededTime) {
            if (rotationTime >= neededTimeToSwapState) {
                State = 1f;
            }
            else {
                rotationTime++;
            }
        }
        else {
            rotationTime++;
        }
    }

    public override void PostAI() {
        if (State == 0f) {
            return;
        }

        ushort frameTime = 4;
        Projectile.frame = (int)(Projectile.localAI[1] / frameTime + 1);
        if (Projectile.frame >= Projectile.GetFrameCount()) {
            Projectile.frame = 1;
        }
        Projectile.localAI[1] += RotationSlow;
        if (Projectile.localAI[1] >= frameTime * 2) {
            Projectile.frameCounter = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawShadowTrails(lightColor, 0.5f, 1, Projectile.rotation);

        Projectile.QuickDrawAnimated(lightColor);

        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 16; i++) {
            Vector2 direction = Vector2.One.RotateRandom(MathHelper.TwoPi) * 2.5f;
            int num616 = Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(Projectile.width) / 3f, 0, 0, 
                DustID.Bone, direction.X, direction.X, 0,
                default, 1f + 0.1f * Main.rand.NextFloatDirection());
            Main.dust[num616].noGravity = Main.rand.NextBool();
            Dust dust2 = Main.dust[num616];
            if (!Main.dust[num616].noGravity) {
                dust2.scale *= 1.25f;
            }
            dust2 = Main.dust[num616];
            dust2.velocity *= 0.5f;
        }
    }
}
