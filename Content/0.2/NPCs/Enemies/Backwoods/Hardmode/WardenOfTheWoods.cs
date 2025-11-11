using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.Projectiles.Enemies;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

class WardenOfTheWoods2 : WardenOfTheWoods {
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement($"Mods.RoA.Bestiary.{nameof(WardenOfTheWoods2)}")
        ]);
    }

    public override bool Alt => true;
}

class WardenOfTheWoods : ModNPC, IRequestAssets {
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement($"Mods.RoA.Bestiary.{nameof(WardenOfTheWoods)}")
        ]);
    }

    private static byte FRAMECOUNT => 9;
    public static float TARGETDISTANCE => 400f;
    public static float ATTACKTIME => 100f;
    public static float FADEOUTTIME => 50f;
    public static float BEFOREATTACKTIME => 50f;
    public static float ATTACKANIMATIONTIME => 80f;

    public static readonly Color Color = new(5, 220, 135);
    public static readonly Color AltColor = new(35, 105, 230);

    public sealed override string Texture => ResourceManager.Textures + $"NPCs/Enemies/Backwoods/Hardmode/{nameof(WardenOfTheWoods)}";

    public enum WardenOfTheWoodsRequstedTextureType : byte {
        Glow,
        Alt,
        AltGlow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [((byte)WardenOfTheWoodsRequstedTextureType.Glow, Texture + "_Glow"),
                                                              ((byte)WardenOfTheWoodsRequstedTextureType.Alt, Texture + "_Alt"),
                                                              ((byte)WardenOfTheWoodsRequstedTextureType.AltGlow, Texture + "_Alt_Glow")];

    public ref struct WardenOfTheWoodsValues(NPC npc) {
        public enum AIState : byte {
            Idle,
            HasTarget,
            Attacking,
            Count
        }

        public enum AnimationFrame : byte {
            Idle1,
            Idle2,
            Idle3,
            Idle4,
            Idle5,
            Attacking1,
            Attacking2,
            Attacking3,
            Attacking4,
            Count
        }

        public ref float InitOnSpawnValue = ref npc.localAI[0];
        public ref float FrameValue = ref npc.localAI[1];
        public ref float AttackValue = ref npc.localAI[2];
        public ref float StateValue = ref npc.ai[0];
        public ref float StateTimer = ref npc.ai[1];
        public ref float ShouldTeleportValue = ref npc.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public AIState State {
            readonly get => (AIState)StateValue;
            set => StateValue = Utils.Clamp((byte)value, (byte)AIState.Idle, (byte)AIState.Count);
        }

        public AnimationFrame Frame {
            readonly get => (AnimationFrame)FrameValue;
            set {
                byte frameToSet = Utils.Clamp((byte)value, (byte)AnimationFrame.Idle1, (byte)AnimationFrame.Count);
                FrameValue = frameToSet;
            }
        }

        public bool Attacked {
            readonly get => AttackValue == 1f;
            set => AttackValue = value.ToInt();
        }

        public bool ShouldTeleport {
            readonly get => ShouldTeleportValue == 1f;
            set => ShouldTeleportValue = value.ToInt();
        }
    }

    private Vector2 _initialPosition, _targetPosition;
    private Color? _areaColor;
    private float _timerForVisualEffects;
    private float _yOffset;
    private float _teleportOpacity;

    public virtual bool Alt { get; } = false;

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(46, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = new SoundStyle(ResourceManager.ItemSounds + "WoodBreakStrong") with { Pitch = 0.3f, Volume = 0.3f, PitchVariance = 0.2f };

        Banner = Type;
        BannerItem = Alt ? ModContent.ItemType<WardenOfTheWoods2Banner>() : ModContent.ItemType<WardenOfTheWoodsBanner>();
        ItemID.Sets.KillsToBanner[BannerItem] = 25;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_targetPosition);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _targetPosition = reader.ReadVector2();
    }

    public override void AI() {
        void init() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            if (!wardenOfTheWoodsValues.Init) {
                wardenOfTheWoodsValues.Init = true;

                _initialPosition = NPC.Center;
                //if (Helper.SinglePlayerOrServer) {
                //    Alt = Main.rand.NextBool();
                //    NPC.netUpdate = true;
                //}
                _areaColor = Alt ? AltColor : Color;
            }
            NPC.dontTakeDamage = wardenOfTheWoodsValues.State == WardenOfTheWoodsValues.AIState.Idle;
        }
        void findTarget() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            Vector2 initialPosition = _initialPosition;
            if (wardenOfTheWoodsValues.ShouldTeleport) {
                return;
            }
            bool shouldTarget() {
                bool result = false;
                foreach (Player player in Main.ActivePlayers) {
                    if (player.Distance(initialPosition) < TARGETDISTANCE) {
                        result = true;
                        break;
                    }
                }
                return result;
            }
            if (!shouldTarget()) {
                NPC.ResetTarget();
                return;
            }
            NPC.TargetClosest(false);
        }
        void goToTarget() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            bool hasTarget = NPC.HasPlayerTarget;
            if (wardenOfTheWoodsValues.ShouldTeleport) {
                return;
            }
            if (!hasTarget) {
                wardenOfTheWoodsValues.State = WardenOfTheWoodsValues.AIState.Idle;
                return;
            }
            wardenOfTheWoodsValues.State = WardenOfTheWoodsValues.AIState.HasTarget;
        }
        void setRotation() {
            NPC.rotation = NPC.velocity.X * 0.025f;
        }
        void setDirection() {
            bool hasTarget = NPC.HasPlayerTarget;
            if (hasTarget) {
                NPC.DirectTo(NPC.GetTargetPlayer().Center, reverse: true);
                return;
            }
            if (NPC.velocity.X.IsNearlyZero()) {
                return;
            }
            NPC.DirectTo(-NPC.velocity.X.GetDirection());
        }
        void handleMoveset() {
            _timerForVisualEffects += 0.02f;
            float minDistance = 10f, speed = 5f, inertia = 25f, deceleration = 0.9375f;
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            void moveTo(Vector2 destination) => NPC.SlightlyMoveTo2(destination, minDistance: minDistance, speed: speed, inertia: inertia, deceleration: deceleration);
            switch (wardenOfTheWoodsValues.State) {
                case WardenOfTheWoodsValues.AIState.Idle:
                    moveTo(_initialPosition);
                    wardenOfTheWoodsValues.Attacked = false;
                    if (NPC.velocity.IsWithinRange(1.5f)) {
                        if (wardenOfTheWoodsValues.StateTimer > -FADEOUTTIME) {
                            wardenOfTheWoodsValues.StateTimer--;
                        }
                        else {
                            _timerForVisualEffects = 16f;
                        }
                    }
                    wardenOfTheWoodsValues.ShouldTeleport = false;

                    //if (Main.rand.NextBool(1000)) SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Active") with { PitchVariance = 0.1f, Pitch = -0.5f, Volume = 0.3f, MaxInstances = 2 }, NPC.Center);
                    if (Main.rand.NextChance(1f - GetFadeOutProgress()) && Main.rand.NextBool(3)) {
                        int num730 = Dust.NewDust(NPC.position + new Vector2(10f, 32.5f + 15f * Main.rand.NextFloat()), NPC.width / 2, 8, Main.rand.NextBool() ? DustID.WoodFurniture : ModContent.DustType<WoodFurniture>(), 0, 1f, 0, Alt ? new Color(85, 90, 80) : new Color(100, 100, 80), 1f + Main.rand.NextFloatRange(0.1f));
                        Main.dust[num730].noGravity = true;
                        Main.dust[num730].velocity = new Vector2(0, Main.rand.NextFloat(6f) * Main.rand.NextFloat(0.25f, 0.9f));
                        Main.dust[num730].velocity.X += Main.dust[num730].position.DirectionTo(NPC.Center).X * Main.rand.NextFloat(0f, 0.75f);
                    }

                    break;
                case WardenOfTheWoodsValues.AIState.HasTarget:
                case WardenOfTheWoodsValues.AIState.Attacking:
                    int type = DustID.TintableDustLighted;
                    for (int i = 0; i < 1; i++) {
                        if (Main.rand.NextBool(10)) {
                            Vector2 spinningpoint = Vector2.UnitX.RotatedBy((double)Main.rand.NextFloat() * MathHelper.TwoPi);
                            Vector2 center = _initialPosition + new Vector2(NPC.direction == 1 ? 3f : -3f, 0f) + spinningpoint * (NPC.width * 4f * NPC.scale);
                            Vector2 rotationPoint = spinningpoint.RotatedBy(0.785) * NPC.direction;
                            Vector2 position = center + rotationPoint * 5f;
                            int dust = Dust.NewDust(position, 0, 0, type, newColor: _areaColor!.Value, Alpha: 200);
                            Main.dust[dust].position = position;
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f * NPC.scale;
                            Main.dust[dust].velocity = rotationPoint * NPC.scale * -2f;
                            Main.dust[dust].scale = Main.rand.NextFloat() * Main.rand.NextFloat(1f, 1.25f);
                            Main.dust[dust].scale *= NPC.scale * 1.75f;
                            //Main.dust[dust].velocity += NPC.velocity * 1.25f;
                            Main.dust[dust].position += Main.dust[dust].velocity * -5f;
                            Main.dust[dust].noLight = true;
                        }
                    }

                    if (wardenOfTheWoodsValues.StateTimer < 0f) {
                        moveTo(_initialPosition);
                        if (wardenOfTheWoodsValues.StateTimer == -0.5f) {
                            wardenOfTheWoodsValues.StateTimer = 0f;
                        }
                        wardenOfTheWoodsValues.StateTimer++;
                        return;
                    }
                    if (wardenOfTheWoodsValues.State == WardenOfTheWoodsValues.AIState.Attacking &&
                        wardenOfTheWoodsValues.StateTimer <= ATTACKTIME - ATTACKANIMATIONTIME) {
                        wardenOfTheWoodsValues.State = WardenOfTheWoodsValues.AIState.HasTarget;
                    }
                    if (wardenOfTheWoodsValues.StateTimer <= 0f) {
                        wardenOfTheWoodsValues.StateTimer = ATTACKTIME;
                        SetTargetPosition();
                        wardenOfTheWoodsValues.State = WardenOfTheWoodsValues.AIState.HasTarget;
                        wardenOfTheWoodsValues.Attacked = false;

                        if (wardenOfTheWoodsValues.ShouldTeleport) {
                            SpawnBeforeTeleportEffects();

                            wardenOfTheWoodsValues.ShouldTeleport = false;

                            Teleport();

                            _teleportOpacity = 0.5f;

                            SoundEngine.PlaySound(SoundID.Item46 with { Pitch = 0.4f, Volume = 0.75f }, NPC.Center);

                            SpawnAfterTeleportEffects();
                        }

                        if (NPC.GetTargetPlayer().Distance(_initialPosition) < TARGETDISTANCE / 2f) {
                            if (!wardenOfTheWoodsValues.ShouldTeleport) {
                                TeleportAttack();
                            }
                            wardenOfTheWoodsValues.ShouldTeleport = true;
                        }
                    }
                    else {
                        wardenOfTheWoodsValues.StateTimer--;
                        if (wardenOfTheWoodsValues.StateTimer <= ATTACKTIME - BEFOREATTACKTIME) {
                            wardenOfTheWoodsValues.State = WardenOfTheWoodsValues.AIState.Attacking;
                        }
                        moveTo(_targetPosition);
                    }
                    break;
            }
        }
        void levitate() {
            float fadeOutProgress2 = Utils.Remap(GetFadeOutProgress(), 0f, 1f, 0.2f, 1f, true);
            float offset = 10f * fadeOutProgress2;
            _yOffset = Helper.Wave(-offset, offset, 5f, NPC.whoAmI);
            NPC.Center += Vector2.UnitY * _yOffset * 0.1f;
        }
        void lightUp() {
            Lighting.AddLight(NPC.Center, _areaColor!.Value.ToVector3() * GetFadeOutProgress() * 0.75f);
        }
        void makeDusts() {
            int num67 = Main.rand.Next(4) - 2;
            num67 = (int)(num67 * GetFadeOutProgress());
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            if (wardenOfTheWoodsValues.State == WardenOfTheWoodsValues.AIState.Attacking &&
                wardenOfTheWoodsValues.StateTimer <= ATTACKTIME - ATTACKANIMATIONTIME * 0.9f) {
                num67 += 2;
            }
            for (int m = 0; m < num67; m++) {
                Color newColor2 = _areaColor!.Value;
                Vector2 position = _initialPosition;
                position = position + Vector2.UnitY * 20f + Main.rand.NextVector2Circular(TARGETDISTANCE, TARGETDISTANCE) / 3f;
                Vector2 velocity = -Vector2.UnitY * 5f * Main.rand.NextFloat(0.25f, 1f);
                WardenDust? wardenParticle = VisualEffectSystem.New<WardenDust>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(position, velocity,
                    scale: Main.rand.NextFloat(0.3f, num67 * 0.6f) * GetFadeOutProgress());
                if (wardenParticle != null) {
                    wardenParticle.CustomData = GetFadeOutProgress();
                    wardenParticle.AI0 = _timerForVisualEffects;
                    wardenParticle.Alt = Alt;
                    if (position.Distance(NPC.Center) < NPC.height * 0.75f) {
                        wardenParticle.RestInPool();
                    }
                }
            }
        }

        init();
        levitate();
        lightUp();
        makeDusts();
        findTarget();
        goToTarget();
        setRotation();
        setDirection();
        handleMoveset();
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 50.0; num828++) {
                int num730 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 0, 0, 0, Alt ? new Color(185, 190, 180) : new Color(200, 200, 180), 1f + Main.rand.NextFloatRange(0.1f));
            }

            return;
        }

        if (!Main.dedServ) {
            string name = nameof(WardenOfTheWoods);
            string alt = Alt ? "_Alt" : string.Empty;
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, (name + "5" + alt).GetGoreType(), Scale: NPC.scale);
            for (int i = 0; i < 4; i++) {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * ((float)i / 4) * NPC.height, NPC.velocity, (name + (Main.rand.Next(4) + 1) + alt).GetGoreType(), Scale: NPC.scale);
            }
        }
    }

    private void TeleportAttack() {
        if (Helper.SinglePlayerOrServer) {
            ProjectileUtils.SpawnHostileProjectile<Purification2>(new ProjectileUtils.SpawnHostileProjectileArgs(NPC, NPC.GetSource_FromAI()) {
                Damage = 50,
                KnockBack = 0f,
                Position = _initialPosition,
                AI0 = Alt.ToInt(),
                AI1 = NPC.whoAmI,
                AI2 = _timerForVisualEffects
            });
        }
    }

    private void Teleport() {
        Player target = NPC.GetTargetPlayer();
        if (Helper.SinglePlayerOrServer) {
            //Vector2 random = Main.rand.NextVector2CircularEdge(TARGETDISTANCE, TARGETDISTANCE) * 0.75f;
            //void randomize() => NPC.Center = _initialPosition = target.Center + random;
            //randomize();
            //int attempts = 100;
            //while (attempts-- > 0 && !Collision.CanHit(NPC.Center, NPC.width, NPC.height, target.Center, target.width, target.height)) {
            //    randomize();
            //}
            Point point14 = NPC.Center.ToTileCoordinates();
            Point point15 = target.Center.ToTileCoordinates();
            Vector2 chosenTile2 = Vector2.Zero;
            if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile2, point15.X, point15.Y, 20, 12, 1, solidTileCheckCentered: true, teleportInAir: true)) {
                NPC.Center = _initialPosition = chosenTile2.ToWorldCoordinates();
                SetTargetPosition();
            }
            NPC.netUpdate = true;
        }
    }

    private void SpawnAfterTeleportEffects() {
        int num67 = 30;
        num67 *= 2;
        for (int m = 0; m < num67; m++) {
            Color newColor2 = _areaColor!.Value;
            Vector2 position = NPC.Center;
            int num69 = Dust.NewDust(position, 0, 0, DustID.TintableDustLighted, 0f, 0f, 100, newColor2);
            Main.dust[num69].position = position + Vector2.UnitY * 10f + Main.rand.NextVector2Circular(NPC.width, NPC.height) * 0.5f;
            Main.dust[num69].velocity *= 0f;
            Main.dust[num69].noGravity = true;
            Main.dust[num69].velocity -= Vector2.UnitY * 5f * Main.rand.NextFloat(0.25f, 1f);
            Main.dust[num69].scale = Main.rand.NextFloat(0.1f, num67 * 0.4f) * GetFadeOutProgress() * 1.5f;
            Main.dust[num69].noLight = true;
            Main.dust[num69].noLightEmittence = true;
        }
    }

    private void SpawnBeforeTeleportEffects() {
        int num67 = 50;
        for (int m = 0; m < num67; m++) {
            Color newColor2 = _areaColor!.Value;
            Vector2 position = _initialPosition;
            position = position + Vector2.UnitX * 4f + Vector2.UnitY * 20f + Vector2.UnitX * TARGETDISTANCE / 2f * Main.rand.NextFloatDirection() + Vector2.UnitY * TARGETDISTANCE / 2f * Main.rand.NextFloatDirection();
            if (m < 20) position = _initialPosition + Vector2.UnitY.RotatedBy(Math.PI * m * 0.1f) * 100f;
            Vector2 velocity = -Vector2.UnitY * 5f * Main.rand.NextFloat(0.25f, 1f);
            if (m < 20) velocity = Vector2.Normalize(position - _initialPosition) * 5f * Main.rand.NextFloat(0.25f, 1f);
            WardenDust? wardenParticle = VisualEffectSystem.New<WardenDust>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(position, velocity,
                scale: Main.rand.NextFloat(0.5f, 2f));
            if (wardenParticle != null) {
                wardenParticle.Alt = Alt;
                wardenParticle.AI0 = _timerForVisualEffects;
            }
        }
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
        //position += Vector2.UnitY * _yOffset;

        return base.DrawHealthBar(hbPosition, ref scale, ref position);
    }

    private void Attack() {
        if (!Helper.SinglePlayerOrServer) {
            return;
        }

        ProjectileUtils.SpawnHostileProjectile<Purification>(new ProjectileUtils.SpawnHostileProjectileArgs(NPC, NPC.GetSource_FromAI()) {
            Damage = 50,
            KnockBack = 0f,
            Position = NPC.GetTargetPlayer().Center + (NPC.Center - _initialPosition) / 2f,
            AI0 = Alt.ToInt(),
            //AI1 = 1f - (float)NPC.life / NPC.lifeMax,
            AI2 = _timerForVisualEffects
        });
    }

    private void SetTargetPosition() {
        if (!Helper.SinglePlayerOrServer) {
            return;
        }

        Vector2 previousTargetPosition = _targetPosition;
        void randomize() => _targetPosition = _initialPosition + Main.rand.NextVector2Circular(TARGETDISTANCE, TARGETDISTANCE) / 4f;
        randomize();
        float checkDistance = TARGETDISTANCE / 5f;
        Vector2 checkTargetPosition = NPC.GetTargetPlayer().Center;
        while (Vector2.Distance(_targetPosition, previousTargetPosition) < checkDistance ||
               Vector2.Distance(_targetPosition, checkTargetPosition) < checkDistance) {
            randomize();
        }
        NPC.netUpdate = true;
    }

    private float GetFadeOutProgress() {
        WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
        float fadeOutProgress = -wardenOfTheWoodsValues.StateTimer / FADEOUTTIME;
        fadeOutProgress = 1f - MathUtils.Clamp01(fadeOutProgress);
        return fadeOutProgress;
    }

    public override void FindFrame(int frameHeight) {
        WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
        switch (wardenOfTheWoodsValues.State) {
            case WardenOfTheWoodsValues.AIState.Idle:
            case WardenOfTheWoodsValues.AIState.HasTarget:
                float fadeProgress = GetFadeOutProgress();
                if (fadeProgress <= 0f) {
                    break;
                }
                byte idleAnimationSpeed = (byte)(10 + 50 * (1f - fadeProgress));
                wardenOfTheWoodsValues.Frame = (WardenOfTheWoodsValues.AnimationFrame)NPC.AnimateFrame((byte)wardenOfTheWoodsValues.Frame,
                    (byte)WardenOfTheWoodsValues.AnimationFrame.Idle1, (byte)WardenOfTheWoodsValues.AnimationFrame.Idle5,
                    idleAnimationSpeed, (ushort)frameHeight);
                break;
            case WardenOfTheWoodsValues.AIState.Attacking:
                if (wardenOfTheWoodsValues.Attacked) {
                    break;
                }
                byte attackAnimationSpeed = 8;
                wardenOfTheWoodsValues.Frame = (WardenOfTheWoodsValues.AnimationFrame)NPC.AnimateFrame((byte)wardenOfTheWoodsValues.Frame,
                    (byte)WardenOfTheWoodsValues.AnimationFrame.Attacking1, (byte)WardenOfTheWoodsValues.AnimationFrame.Attacking4,
                    attackAnimationSpeed, (ushort)frameHeight);
                if (wardenOfTheWoodsValues.Frame == WardenOfTheWoodsValues.AnimationFrame.Attacking4 &&
                    !wardenOfTheWoodsValues.Attacked) {
                    Attack();
                    wardenOfTheWoodsValues.Attacked = true;
                }
                break;
        }
    }

    public override void OnKill() {
        for (int num923 = 0; num923 < 15; num923++) {
            int num730 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 0, 0, 0, Alt ? new Color(185, 190, 180) : new Color(200, 200, 180), 1f + Main.rand.NextFloatRange(0.1f));
        }
        for (int m = 0; m < 30; m++) {
            Color newColor2 = _areaColor!.Value;
            Vector2 position = NPC.Center;
            int num69 = Dust.NewDust(position, 0, 0, DustID.TintableDustLighted, 0f, 0f, 100, newColor2, Main.rand.NextFloat() + 2f);
            Main.dust[num69].position = position + Vector2.UnitY * 10f + Main.rand.NextVector2Circular(NPC.width, NPC.height) * 0.25f;
            Main.dust[num69].noGravity = true;
            Main.dust[num69].noLight = true;
            Main.dust[num69].noLightEmittence = true;
        }
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves2") with { Pitch = -0.3f, Volume = 0.7f }, NPC.Center);
        SoundEngine.PlaySound(SoundID.Item66 with { Pitch = 0.25f, Volume = 0.75f }, NPC.Center);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<WardenOfTheWoods>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Texture2D circle = ResourceManager.Circle;
        //Texture2D circle = ResourceManager.Circle3;
        Rectangle clip = circle.Bounds;
        Vector2 origin = clip.Centered();
        Vector2 position = _initialPosition;
        Color color = _areaColor ?? Color.White;
        Color color2 = Color.White;
        float fadeOutProgress = GetFadeOutProgress();
        float waveMin = MathHelper.Lerp(0.75f, 1f, 1f - fadeOutProgress), waveMax = MathHelper.Lerp(1.25f, 1f, 1f - fadeOutProgress);
        float wave = Helper.Wave(_timerForVisualEffects, waveMin, waveMax, 3f, NPC.whoAmI) * fadeOutProgress;
        float opacity = wave * fadeOutProgress * _teleportOpacity;
        color *= opacity * 0.625f;
        color2 *= opacity;
        if (_teleportOpacity < 1f) _teleportOpacity += 0.05f;
        int extra = 3;
        drawColor = Color.Lerp(drawColor, Color.Lerp(Color.Black, Alt ? Color.DarkBlue : Color.DarkGreen, 0.5f), (1f - fadeOutProgress) * 0.5f);
        WardenOfTheWoodsRequstedTextureType glowVariant = Alt ? WardenOfTheWoodsRequstedTextureType.AltGlow : WardenOfTheWoodsRequstedTextureType.Glow;
        Texture2D glowTexture = indexedTextureAssets[(byte)glowVariant].Value;
        float xOffset = 4f * NPC.spriteDirection;
        float yOffset = 0f;
        Texture2D texture = Alt ? indexedTextureAssets[(byte)WardenOfTheWoodsRequstedTextureType.Alt].Value : NPC.GetTexture();
        NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor, xOffset: xOffset, yOffset: yOffset, texture: texture);
        NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor * Utils.Remap(fadeOutProgress, 0f, 1f, 0.5f, 1f, true), texture: glowTexture, xOffset: xOffset, yOffset: yOffset);
        for (int i = 0; i < extra; i++) {
            Vector2 scale = Vector2.One * Utils.Remap(fadeOutProgress, 0f, 1f, 0.75f, 1f, true) * TARGETDISTANCE / 150f * (i != 0 ? (Utils.Remap(i, 0, extra, 0.75f, 1f) * Utils.Remap(wave, waveMin, waveMax, waveMin * 1.5f, waveMax, true)) : 1f);
            spriteBatch.DrawWithSnapshot(circle, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color * 0.8f,
                Scale = scale * _teleportOpacity * 0.45f
            }, blendState: BlendState.Additive);
            spriteBatch.DrawWithSnapshot(() => {
                NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, color2 * 0.625f * fadeOutProgress, scale: scale.X * 0.4f, texture: glowTexture, xOffset: xOffset, yOffset: yOffset);
            }, blendState: BlendState.Additive);
        }

        return false;
    }
}
