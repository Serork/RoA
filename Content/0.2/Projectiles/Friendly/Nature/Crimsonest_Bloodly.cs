using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class Bloodly : NatureProjectile, IRequestAssets {
    private enum ExtraBloodlyTextureType : byte {
        Glow,
        Cocoon
    }

    public static byte AMOUNTNEEDFORATTACK => 3;
    private static byte FRAMECOUNT => 6;
    private static byte COCOONFRAMECOUNT => 4;
    private static byte FRAMECOUNTER => 4;
    private static byte COCOONFRAMECOUNTER => 6;
    private static float COCOONTIMELEFTMODIFIER => 0.2f;
    private static float MOVELENGTHMIN => 400f;
    private static float MOVELENGTHMAX => 500f;
    private static float SPEED => 12.5f;
    private static float SINEOFFSET => 2f;
    private static ushort HITTIMERCHECK => 10;
    private static float ONHITSLOWMODIFIER => 0.625f;

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [((byte)ExtraBloodlyTextureType.Glow, Texture + "_Glow"), ((byte)ExtraBloodlyTextureType.Cocoon, Texture + "_Cocoon")];

    public ref struct BloodlyValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float ReversedValue = ref projectile.localAI[1];
        public ref float ScaleValue = ref projectile.localAI[2];

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

    private ushort _hitTimer;
    private bool _directedLeft;
    private float _cooconAngle;
    private int _index;

    private float AttackTime => Projectile.GetOwnerAsPlayer().itemTimeMax * 5f;
    private bool InCocoon => Projectile.timeLeft > LastCocoonTime && _index != -1;
    private ushort LastCocoonTime => (ushort)(AttackTime - AttackTime * COCOONTIMELEFTMODIFIER);

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(40);

        Projectile.aiStyle = -1;

        Projectile.timeLeft = 3600;

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.tileCollide = false;
    }

    public override bool? CanDamage() => !InCocoon;
    public override bool? CanCutTiles() => !InCocoon;
    public override bool ShouldUpdatePosition() => !InCocoon;

    public override void AI() {
        void setPosition() {
            Player owner = Projectile.GetOwnerAsPlayer();
            _cooconAngle = (float)_index / 3 * MathHelper.PiOver4 * -owner.direction;
            if (_index == 2) {
                _cooconAngle = -(float)1 / 3 * MathHelper.PiOver4 * -owner.direction;
            }
            Projectile.Center = owner.MountedCenter - Vector2.UnitY * owner.height / 2;
            Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * owner.gfxOffY + (Vector2.UnitY * -20f + (Vector2.UnitY * (_index == 1 ? 30f : _index == 2 ? 35f : 25f)).RotatedBy(_cooconAngle * owner.gravDir)) * owner.gravDir;
        }
        void init() {
            BloodlyValues bloodlyValues = new(Projectile);
            if (!bloodlyValues.Init) {
                bloodlyValues.Init = true;

                bloodlyValues.SineYOffset = Main.rand.NextFloatRange(MathHelper.TwoPi) * 10f;

                //AI_GetMyGroupIndexAndFillBlackList(out int index, out _);
                Player owner = Projectile.GetOwnerAsPlayer();
                _index = owner.GetModPlayer<Crimsonest_AttackEncounter>().AttackCount;
                if (_index > AMOUNTNEEDFORATTACK - 1) {
                    _index = 0;
                }
                setPosition();
                foreach (Projectile otherCocoons in TrackedEntitiesSystem.GetTrackedProjectile<Bloodly>(checkProjectile => checkProjectile.owner != Projectile.owner || checkProjectile.whoAmI == Projectile.whoAmI || !checkProjectile.As<Bloodly>().InCocoon)) {
                    float dist = MathF.Abs(otherCocoons.Center.X - Projectile.Center.X);
                    if (dist < 3) {
                        _index = -_index;
                        setPosition();
                    }
                }

                Projectile.timeLeft = (int)AttackTime;

                if (Projectile.IsOwnerLocal()) {
                    _directedLeft = Main.rand.NextBool();

                    //_cooconAngle = Main.rand.NextFloatRange(MathHelper.PiOver4 * 0.75f);

                    Projectile.netUpdate = true;
                }

                bloodlyValues.SetGoToPosition(true);
            }
        }
        void animate() {
            Projectile.manualDirectionChange = InCocoon;
            Projectile.spriteDirection = Projectile.direction;
            if (!InCocoon) {
                Projectile.Animate(FRAMECOUNTER);
            }
            else {
                Projectile.Animate(COCOONFRAMECOUNTER, COCOONFRAMECOUNT);
            }
        }
        void handleMovement() {
            BloodlyValues bloodlyValues = new(Projectile);
            float speed = SPEED, inertia = 20f;
            Projectile.rotation = Projectile.velocity.X * 0.025f;
            Projectile.SlightlyMoveTo(bloodlyValues.GoToPosition, speed, inertia);
            Projectile.position += Vector2.UnitY.RotatedBy(Projectile.velocity.ToRotation()) * Projectile.direction * MathF.Sin(bloodlyValues.SineYOffset++ * 0.1f) * SINEOFFSET;
        }
        void playSound() {
            if(Main.rand.NextBool(150)) {
                SoundEngine.PlaySound(SoundID.Zombie43 with { Volume = 0.25f, Pitch = 0.4f, MaxInstances = 3 }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Zombie44 with { Volume = 0.45f, Pitch = -0.4f, MaxInstances = 3 }, Projectile.Center);
            }
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
        void slowOnHit() {
            if (_hitTimer > 0f) {
                _hitTimer--;
                Projectile.timeLeft--;
                Projectile.velocity *= ONHITSLOWMODIFIER;
            }
        }
        void handleCocoon() {
            BloodlyValues bloodlyValues = new(Projectile);
            Player owner = Projectile.GetOwnerAsPlayer();
            setPosition();
            if (owner.gravDir < 0) {
                Projectile.position.Y += 42f;
            }
            Projectile.direction = _directedLeft.ToDirectionInt();
            float minus = _cooconAngle;
            if (owner.gravDir < 0) {
                minus *= -1f;
                Projectile.position.X += 16f * minus;
            }
            Projectile.rotation = MathHelper.TwoPi - minus + owner.fullRotation;
            int timeLeft = Projectile.timeLeft;
            float revealTime = AttackTime - AttackTime * COCOONTIMELEFTMODIFIER / 2;
            bloodlyValues.ScaleValue = Utils.GetLerpValue(AttackTime, revealTime, timeLeft, true);
            bool holdingItem = owner.IsHolding<Crimsonest>();
            if (Projectile.timeLeft == LastCocoonTime + 1) {
                _index = -1;
                if (Projectile.IsOwnerLocal()) {
                    Projectile.velocity = Projectile.Center.DirectionTo(owner.GetWorldMousePosition()) * Projectile.velocity.Length();
                    bloodlyValues.SetGoToPosition(true);
                    Projectile.netUpdate = true;
                }
                if (!Main.dedServ) {
                    for (int i = 0; i < 18; i++) {
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.X * 0.2f, 100, default(Color), 1.25f + Main.rand.NextFloatRange(0.25f));
                    }
                    int goreCount = 2;
                    for (int i = 0; i < goreCount; i++) {
                        int currentIndex = i + 1;
                        Vector2 gorePosition = Projectile.Top/* + Main.rand.RandomPointInArea(Projectile.width, Projectile.height) / 2f*/;
                        int gore = Gore.NewGore(Projectile.GetSource_Misc("crimsonest"),
                            gorePosition,
                            Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f, ModContent.Find<ModGore>(RoA.ModName + $"/CrimsonestGore{1 + Main.rand.NextBool().ToInt()}").Type, 1f);
                        Main.gore[gore].velocity *= 1f;
                    }
                    SoundEngine.PlaySound(SoundID.NPCDeath35 with { Volume = 0.75f, PitchVariance = 0.1f, Pitch = 0.2f }, Projectile.Center);
                }
            }
            bool canReveal = true;
            foreach (Projectile otherCocoons in TrackedEntitiesSystem.GetTrackedProjectile<Bloodly>(checkProjectile => checkProjectile.owner != Projectile.owner || checkProjectile.whoAmI == Projectile.whoAmI)) {
                if (otherCocoons.As<Bloodly>().InCocoon && otherCocoons.timeLeft > revealTime) {
                    canReveal = false;
                    break;
                }
            }
            if (timeLeft <= revealTime && (!canReveal || !owner.GetModPlayer<Crimsonest_AttackEncounter>().CanReveal)) {
                Projectile.timeLeft = (int)revealTime;
            }
            if (Projectile.timeLeft < revealTime) {
                int baseTime = ItemLoader.GetItem(ModContent.ItemType<Crimsonest>()).Item.useTime * 5;
                Projectile.scale += (baseTime + (baseTime - AttackTime)) / 40000f;
                Projectile.scale *= 1.015f;
            }
            else {
                if (!holdingItem) {
                    for (int i = 0; i < 18; i++) {
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.X * 0.2f, 100, default(Color), 1.25f + Main.rand.NextFloatRange(0.25f));
                    }
                    Projectile.active = false;
                }
            }
        }

        init();
        animate();
        if (!InCocoon) {
            Projectile.scale = 1f;

            handleMovement();
            playSound();
            resetGoToPosition();
            moveFromOthers();
            slowOnHit();
        }
        else {
            handleCocoon();
        }
    }

    private void AI_GetMyGroupIndexAndFillBlackList(out int index, out int totalIndexesInGroup) {
        index = 0;
        totalIndexesInGroup = 0;
        for (int i = 0; i < 1000; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == Projectile.owner && projectile.type == Projectile.type) {
                if (projectile.As<Bloodly>().InCocoon) {
                    if (Projectile.whoAmI > i)
                        index++;

                    totalIndexesInGroup++;
                }
            }
        }
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.Write(_hitTimer);
        writer.Write(_cooconAngle);
        writer.Write(_directedLeft);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _hitTimer = reader.ReadUInt16();
        _cooconAngle = reader.ReadSingle();
        _directedLeft = reader.ReadBoolean();
    }

    public override void OnKill(int timeLeft) {
        SpawnIchorStreams();
        for (int i = 0; i < 18; i++) {
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.X * 0.2f, 100, default(Color), 1.25f + Main.rand.NextFloatRange(0.25f));
        }
        if (!Main.dedServ) {
            int goreCount = Main.rand.NextBool().ToInt() + 1;
            for (int i = 0; i < goreCount; i++) {
                int currentIndex = i + 1;
                Vector2 gorePosition = Projectile.Center + Main.rand.RandomPointInArea(Projectile.width, Projectile.height) / 2f;
                int gore = Gore.NewGore(Projectile.GetSource_Misc("crimsonest"),
                    gorePosition + Projectile.velocity,
                    Vector2.One.RotatedBy(currentIndex * MathHelper.TwoPi / goreCount) * 2f, ModContent.Find<ModGore>(RoA.ModName + $"/CrimsonestGore{3 + Main.rand.Next(3)}").Type, 1f);
                Main.gore[gore].velocity = Projectile.velocity;
                Main.gore[gore].velocity *= 0.5f;
            }
        }
        SoundEngine.PlaySound(SoundID.NPCDeath11 with { Volume = 0.75f, Pitch = 0.2f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.5f, PitchVariance = 0.1f, Pitch = -0.3f }, Projectile.Center);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        HitEnemyForSlow();
        //SpawnIchorStreams();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        HitEnemyForSlow();
        //SpawnIchorStreams();
    }

    private void HitEnemyForSlow() {
        _hitTimer = (ushort)Projectile.localNPCHitCooldown;
        Projectile.netUpdate = true;
    }

    private void SpawnIchorStreams() {
        Vector2 getBlisterPosition() => Projectile.Bottom - Vector2.UnitY * Projectile.height * 0.15f + Main.rand.NextVector2Circular(Projectile.width * 0.4f, Projectile.height * 0.4f);
        for (int i = 0; i < 3; i++) {
            Dust.NewDustPerfect(getBlisterPosition(), DustID.Ichor);
        }
        float damageMult = 1f;
        for (int i = 0; i < 3; i++) {
            ProjectileUtils.SpawnPlayerOwnedProjectile<IchorStream>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                Position = getBlisterPosition(),
                Velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f,
                Damage = (int)(Projectile.damage * damageMult),
                KnockBack = Projectile.knockBack * damageMult
            });
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Bloodly>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        BloodlyValues bloodlyValues = new(Projectile);
        lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
        if (InCocoon) {
            Vector2 position = Projectile.position;
            Player player = Projectile.GetOwnerAsPlayer();
            Projectile.position += player.MovementOffset();
            SpriteEffects effects = player.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float rotation = 0f;
            if (player.gravDir < 0) {
                effects = player.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                effects |= SpriteEffects.FlipVertically;
            }
            Projectile.QuickDrawAnimated(lightColor, exRot: rotation, texture: indexedTextureAssets[(byte)ExtraBloodlyTextureType.Cocoon].Value, maxFrames: COCOONFRAMECOUNT, 
                scale: new Vector2(MathUtils.Clamp01(bloodlyValues.ScaleValue * 1.75f), MathUtils.Clamp01(bloodlyValues.ScaleValue * 2f)) * Projectile.scale, 
                originScale: new Vector2(1f, player.gravDir < 0 ? 0.625f : 1.375f), spriteEffects: effects);
            Projectile.position = position;
        }
        else {
            Projectile.QuickDrawAnimated(lightColor, scale: Vector2.One);
            Projectile.QuickDrawAnimated(Color.White, texture: indexedTextureAssets[(byte)ExtraBloodlyTextureType.Glow].Value, scale: Vector2.One);
        }

        return false;
    }
}
