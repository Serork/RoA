using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Bloodly : NatureProjectile, IRequestAssets {
    private static byte FRAMECOUNT => 6;
    private static byte FRAMECOUNTER => 4;
    private static ushort TIMELEFT => 180;
    private static float MOVELENGTHMIN => 300f;
    private static float MOVELENGTHMAX => 500f;
    private static float SINEOFFSET => 2f;

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [(0, Texture + "_Glow")];

    public ref struct BloodlyValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float ReversedValue = ref projectile.localAI[1];

        public ref float SineYOffset = ref projectile.ai[0];
        public ref float GotPositionX = ref projectile.ai[1];
        public ref float GoPositionY = ref projectile.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public bool Reversed {
            readonly get => ReversedValue == 1f;
            set => ReversedValue = value.ToInt();
        }

        public Vector2 GoToPosition {
            readonly get => new(GotPositionX, GoPositionY);
            set {
                GotPositionX = value.X;
                GoPositionY = value.Y;
            }
        }

        public void SetGoToPosition(bool onSpawn = false) {
            if (projectile.IsOwnerLocal()) {
                int velocityDirection = onSpawn ? 1 : Reversed.ToDirectionInt();
                GoToPosition = projectile.GetOwnerAsPlayer().Top + projectile.velocity.SafeNormalize() * Main.rand.NextFloat(MOVELENGTHMIN, MOVELENGTHMAX) * velocityDirection;

                projectile.netUpdate = true;
            }
        }
    }

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(40);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
    }

    public override void AI() {
        void init() {
            BloodlyValues bloodlyValues = new(Projectile);
            if (!bloodlyValues.Init) {
                bloodlyValues.Init = true;

                bloodlyValues.SineYOffset = Main.rand.NextFloatRange(MathHelper.TwoPi) * 10f;

                bloodlyValues.SetGoToPosition(true);
            }
        }
        void animate() {
            Projectile.spriteDirection = Projectile.direction;
            Projectile.Animate(FRAMECOUNTER);
        }
        void handleMovement() {
            BloodlyValues bloodlyValues = new(Projectile);
            float speed = 7.5f, inertia = 20f;
            Projectile.SlightlyMoveTo(bloodlyValues.GoToPosition, speed, inertia);
            Projectile.position += Vector2.UnitY.RotatedBy(Projectile.velocity.ToRotation()) * Projectile.direction * MathF.Sin(bloodlyValues.SineYOffset++ * 0.1f) * SINEOFFSET;
        }
        void resetGoToPosition() {
            BloodlyValues bloodlyValues = new(Projectile);
            Vector2 goToPosition = bloodlyValues.GoToPosition;
            bool flew = Projectile.Distance(goToPosition) < Projectile.width * 2;
            if (flew) {
                bloodlyValues.Reversed = !bloodlyValues.Reversed;
                if (bloodlyValues.Reversed) {
                    bloodlyValues.GoToPosition = Projectile.GetOwnerAsPlayer().Top;
                }
                else {
                    bloodlyValues.SetGoToPosition();
                }
            }
        }
        void moveFromOthers() {
            Projectile.OffsetTheSameProjectile(0.1f);
        }

        init();
        animate();
        handleMovement();
        resetGoToPosition();
        moveFromOthers();
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 15; i++) {
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.X * 0.2f, 100, default(Color), 1.25f + Main.rand.NextFloatRange(0.25f));
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnIchorStreams();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        SpawnIchorStreams();
    }

    private void SpawnIchorStreams() {
        float damageMult = 0.5f;
        for (int i = 0; i < 3; i++) {
            ProjectileUtils.SpawnPlayerOwnedProjectile<IchorStream>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                Position = Projectile.Bottom - Vector2.UnitY * Projectile.height * 0.15f + Main.rand.NextVector2Circular(Projectile.width * 0.4f, Projectile.height * 0.4f),
                Velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f,
                Damage = (int)(Projectile.damage * damageMult),
                KnockBack = Projectile.knockBack * damageMult
            });
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SpawnIchorStreams();
        Projectile.Kill();

        return base.OnTileCollide(oldVelocity);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 20;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool PreDraw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Bloodly>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Projectile.QuickDrawAnimated(lightColor);
        Projectile.QuickDrawAnimated(Color.White, texture: indexedTextureAssets[0].Value);

        return false;
    }
}
