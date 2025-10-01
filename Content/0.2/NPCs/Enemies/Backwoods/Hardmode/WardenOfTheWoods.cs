using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

using static RoA.Content.NPCs.Enemies.Backwoods.Hardmode.WardenOfTheWoods;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class WardenOfTheWoods : ModNPC, IRequestAssets {
    private static byte FRAMECOUNT => 9;
    private static float TARGETDISTANCE => 400f;
    private static float ATTACKTIME => 100f;
    private static float FADEOUTTIME => 50f;
    private static float BEFOREATTACKTIME => 50f;
    private static float ATTACKANIMATIONTIME => 80f;

    public enum WardenOfTheWoodsRequstedTextureType : byte {
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [((byte)WardenOfTheWoodsRequstedTextureType.Glow, Texture + "_Glow")];

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

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(46, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
        NPC.noGravity = true;
    }

    public override void AI() {
        void init() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            if (!wardenOfTheWoodsValues.Init) {
                wardenOfTheWoodsValues.Init = true;

                _initialPosition = NPC.Center;
                _areaColor = new Color(5, 220, 135);
            }
        }
        void findTarget() {
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            Vector2 initialPosition = _initialPosition;
            Vector2 center = NPC.Center;
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
            NPC.Center = initialPosition;
            NPC.TargetClosest(false);
            NPC.Center = center;
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
            float minDistance = 10f, speed = 5f, inertia = 25f, deceleration = 0.9375f;
            WardenOfTheWoodsValues wardenOfTheWoodsValues = new(NPC);
            void moveTo(Vector2 destination) => NPC.SlightlyMoveTo2(destination, minDistance: minDistance, speed: speed, inertia: inertia, deceleration: deceleration);
            switch (wardenOfTheWoodsValues.State) {
                case WardenOfTheWoodsValues.AIState.Idle:
                    moveTo(_initialPosition);
                    wardenOfTheWoodsValues.Attacked = false;
                    if (NPC.velocity.IsWithinRange(1f)) {
                        if (wardenOfTheWoodsValues.StateTimer > -FADEOUTTIME) {
                            wardenOfTheWoodsValues.StateTimer--;
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

        init();
        findTarget();
        goToTarget();
        setRotation();
        setDirection();
        handleMoveset();
    }

    private void SetTargetPosition() {
        if (!Helper.SinglePlayerOrServer) {
            return;
        }

        Vector2 previousTargetPosition = _targetPosition;
        void randomize() => _targetPosition = _initialPosition + Main.rand.NextVector2Circular(TARGETDISTANCE, TARGETDISTANCE) / 4f;
        randomize();
        while (Vector2.Distance(_targetPosition, previousTargetPosition) < TARGETDISTANCE / 5f) {
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
                if (wardenOfTheWoodsValues.Frame == WardenOfTheWoodsValues.AnimationFrame.Attacking4) {
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
        float waveMin = 0.75f, waveMax = 1.25f;
        float wave = Helper.Wave(waveMin, waveMax, 3f, NPC.whoAmI);
        color *= wave;
        color *= 0.625f;
        float fadeOutProgress = GetFadeOutProgress();
        color *= fadeOutProgress;
        int extra = 3;
        drawColor = Color.Lerp(drawColor, Color.Lerp(Color.Black, Color.DarkGreen, 0.5f), (1f - fadeOutProgress) * 0.5f);
        Texture2D glowTexture = indexedTextureAssets[(byte)WardenOfTheWoodsRequstedTextureType.Glow].Value;
        float xOffset = 4f * NPC.spriteDirection;
        NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor, xOffset: xOffset);
        NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor * Utils.Remap(fadeOutProgress, 0f, 1f, 0.5f, 1f, true), texture: glowTexture, xOffset: xOffset);
        for (int i = 0; i < extra; i++) {
            Vector2 scale = Vector2.One * TARGETDISTANCE / 150f * (i != 0 ? (Utils.Remap(i, 0, extra, 0.75f, 1f) * Utils.Remap(wave, waveMin, waveMax, waveMin * 1.5f, waveMax, true)) : 1f);
            spriteBatch.DrawWithSnapshot(circle, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale
            }, blendState: BlendState.Additive);
            spriteBatch.DrawWithSnapshot(() => {
                NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor * 0.5f * fadeOutProgress, scale: scale.X * 0.4f, texture: glowTexture, xOffset: xOffset);
            }, blendState: BlendState.Additive);
        }

        return false;
    }
}
