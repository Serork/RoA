using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Content.Projectiles.Enemies;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class WardenOfTheWoods : ModNPC, IRequestAssets {
    private static byte FRAMECOUNT => 9;
    private static float TARGETDISTANCE => 400f;
    private static float ATTACKTIME => 100f;
    private static float FADEOUTTIME => 50f;
    private static float BEFOREATTACKTIME => 50f;
    private static float ATTACKANIMATIONTIME => 80f;

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
    }

    private Vector2 _initialPosition, _targetPosition;
    private Color? _areaColor;
    private float _timerForVisualEffects;
    private float _yOffset;
    private bool _alt;

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(46, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
        NPC.noGravity = true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_alt);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _alt = reader.ReadBoolean();
    }

    public override void AI() {
        void init() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            if (!wardenOfTheWoodsValues.Init) {
                wardenOfTheWoodsValues.Init = true;

                _initialPosition = NPC.Center;
                _areaColor = _alt ? new Color(112, 187, 219) : new Color(5, 220, 135);

                if (Helper.SinglePlayerOrServer) {
                    _alt = Main.rand.NextBool();
                    NPC.netUpdate = true;
                }
            }
            NPC.dontTakeDamage = wardenOfTheWoodsValues.State == WardenOfTheWoodsValues.AIState.Idle;
        }
        void findTarget() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            Vector2 initialPosition = _initialPosition;
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
                    break;
                case WardenOfTheWoodsValues.AIState.HasTarget:
                case WardenOfTheWoodsValues.AIState.Attacking:
                    if (wardenOfTheWoodsValues.StateTimer < 0f) {
                        moveTo(_initialPosition);
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
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new WardenOfTheWoodsValues(NPC);
            if (wardenOfTheWoodsValues.State == WardenOfTheWoodsValues.AIState.Attacking &&
                wardenOfTheWoodsValues.StateTimer <= ATTACKTIME - ATTACKANIMATIONTIME * 0.9f) {
                num67 += 2;
            }
            for (int m = 0; m < num67; m++) {
                Color newColor2 = _areaColor!.Value;
                Vector2 position = _initialPosition;
                position = position + Vector2.UnitY * 20f + Main.rand.NextVector2Circular(TARGETDISTANCE, TARGETDISTANCE) / 3f;
                Vector2 velocity = -Vector2.UnitY * 5f * Main.rand.NextFloat(0.25f, 1f);
                WardenDust? leafParticle = VisualEffectSystem.New<WardenDust>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(position, velocity,
                    scale: Main.rand.NextFloat(0.3f, num67 * 0.6f) * GetFadeOutProgress());
                if (leafParticle != null) {
                    leafParticle.CustomData = GetFadeOutProgress();
                    leafParticle.AI0 = _timerForVisualEffects;
                    if (position.Distance(NPC.Center) < NPC.height * 0.75f) {
                        leafParticle.RestInPool();
                    }
                }
            }
        }

        init();
        findTarget();
        goToTarget();
        setRotation();
        setDirection();
        handleMoveset();
        levitate();
        lightUp();
        makeDusts();
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
        //position += Vector2.UnitY * _yOffset;

        return base.DrawHealthBar(hbPosition, ref scale, ref position);
    }

    private void Attack() {
        if (!Helper.SinglePlayerOrServer) {
            return;
        }

        ProjectileUtils.SpawnHostileProjectile<WardenPurification>(new ProjectileUtils.SpawnHostileProjectileArgs(NPC, NPC.GetSource_FromAI()) {
            Damage = 50,
            KnockBack = 0f,
            Position = NPC.GetTargetPlayer().Center + (NPC.Center - _initialPosition) / 2f,
            AI0 = _alt.ToInt(),
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

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<WardenOfTheWoods>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Texture2D circle = ResourceManager.Circle;
        Rectangle clip = circle.Bounds;
        Vector2 origin = clip.Centered();
        Vector2 position = _initialPosition;
        Color color = _areaColor ?? Color.White;
        Color color2 = Color.White;
        float fadeOutProgress = GetFadeOutProgress();
        float waveMin = MathHelper.Lerp(0.75f, 1f, 1f - fadeOutProgress), waveMax = MathHelper.Lerp(1.25f, 1f, 1f - fadeOutProgress);
        float wave = Helper.Wave(_timerForVisualEffects, waveMin, waveMax, 3f, NPC.whoAmI) * fadeOutProgress;
        float opacity = wave * fadeOutProgress;
        color *= opacity * 0.625f;
        color2 *= opacity;
        int extra = 3;
        drawColor = Color.Lerp(drawColor, Color.Lerp(Color.Black, Color.DarkGreen, 0.5f), (1f - fadeOutProgress) * 0.5f);
        WardenOfTheWoodsRequstedTextureType glowVariant = _alt ? WardenOfTheWoodsRequstedTextureType.AltGlow : WardenOfTheWoodsRequstedTextureType.Glow;
        Texture2D glowTexture = indexedTextureAssets[(byte)glowVariant].Value;
        float xOffset = 4f * NPC.spriteDirection;
        float yOffset = 0f;
        Texture2D texture = _alt ? indexedTextureAssets[(byte)WardenOfTheWoodsRequstedTextureType.Alt].Value : NPC.GetTexture();
        NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor, xOffset: xOffset, yOffset: yOffset, texture: texture);
        NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor * Utils.Remap(fadeOutProgress, 0f, 1f, 0.5f, 1f, true), texture: glowTexture, xOffset: xOffset, yOffset: yOffset);
        for (int i = 0; i < extra; i++) {
            Vector2 scale = Vector2.One * Utils.Remap(fadeOutProgress, 0f, 1f, 0.75f, 1f, true) * TARGETDISTANCE / 150f * (i != 0 ? (Utils.Remap(i, 0, extra, 0.75f, 1f) * Utils.Remap(wave, waveMin, waveMax, waveMin * 1.5f, waveMax, true)) : 1f);
            spriteBatch.DrawWithSnapshot(circle, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale
            }, blendState: BlendState.Additive);
            spriteBatch.DrawWithSnapshot(() => {
                NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, color2 * 0.625f * fadeOutProgress, scale: scale.X * 0.4f, texture: glowTexture, xOffset: xOffset, yOffset: yOffset);
            }, blendState: BlendState.Additive);
        }

        return false;
    }
}
