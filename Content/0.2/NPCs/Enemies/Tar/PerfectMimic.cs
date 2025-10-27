using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Metaballs;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Tar;

[Tracked]
sealed class PerfectMimic : ModNPC, IRequestAssets {
    private static readonly LerpColor LerpColor = new();

    public static Color LiquidColor => LerpColor.GetLerpColor([new Color(46, 34, 47)]);
    public static Color SkinColor => LerpColor.GetLerpColor([new Color(46, 34, 47), Color.Lerp(new Color(62, 53, 70), new Color(98, 85, 101), 0.5f)]);
    public static Color OutlineColor => LerpColor.GetLerpColor([/*new Color(62, 53, 70), */new Color(98, 85, 101)]);

    public enum FluidBodyPartType : byte {
        Part1,
        Part2, 
        Part3
    }

    public record struct FluidBodyPart(Vector2 Position, FluidBodyPartType Type, float Rotation, Vector2 Velocity = default);

    public enum PerfectMimicRequstedTextureType : byte {
        Part1,
        Part2,
        Part3,
        OnPlayer1,
        OnPlayer2
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture
        => [((byte)PerfectMimicRequstedTextureType.Part1, Texture + "_Part1"),
            ((byte)PerfectMimicRequstedTextureType.Part2, Texture + "_Part2"),
            ((byte)PerfectMimicRequstedTextureType.Part3, Texture + "_Part3"),
            ((byte)PerfectMimicRequstedTextureType.OnPlayer1, Texture + "_OnPlayer1"),
            ((byte)PerfectMimicRequstedTextureType.OnPlayer2, Texture + "_OnPlayer2")];

    private FluidBodyPart[] _fluidBodyParts = null!;
    private Player _playerCopy = null!;
    private Color _eyeColor, _skinColor;
    private float _minTransform, _maxTransform;
    private float _timer, _timer2;

    public ref float InitValue => ref NPC.localAI[0];
    public ref float TalkedValue => ref NPC.ai[0];
    public ref float TransformedEnoughValue => ref NPC.ai[1];
    public ref float TeleportCount => ref NPC.ai[2];
    public ref float TransformationFactor => ref NPC.scale;
    public ref float VisualTimer => ref NPC.localAI[1];
    public ref float VisualTimer2 => ref NPC.localAI[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Talked {
        get => TalkedValue == 1f;
        set => TalkedValue = value.ToInt();
    }

    public bool TransformedEnough {
        get => TransformedEnoughValue == 1f;
        set => TransformedEnoughValue = value.ToInt();
    }

    public bool CanTeleport => TeleportCount < 2;

    public override void SetStaticDefaults() {
        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        On_Player.SetTalkNPC += On_Player_SetTalkNPC;
    }

    private void On_Player_SetTalkNPC(On_Player.orig_SetTalkNPC orig, Player self, int npcIndex, bool fromNet) {
        if (npcIndex >= 0 && Main.npc[npcIndex].type == ModContent.NPCType<PerfectMimic>()) {
            return;
        }

        orig(self, npcIndex, fromNet);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(30, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;

        _playerCopy = new Player();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Vector2 basePosition = NPC.position;
        NPC.position.Y += 9f;

        NPC.position += Vector2.UnitY * NPC.gfxOffY;

        float opacity = NPC.Opacity;

        Player other = NPC.GetTargetPlayer();
        _playerCopy.direction = other.direction;
        _playerCopy.selectedItem = other.selectedItem;
        _playerCopy.extraAccessory = other.extraAccessory;
        _playerCopy.position = other.position;
        _playerCopy.velocity = other.velocity;
        _playerCopy.statLife = other.statLife;
        _playerCopy.statLifeMax = other.statLifeMax;
        _playerCopy.statLifeMax2 = other.statLifeMax2;
        _playerCopy.statMana = other.statMana;
        _playerCopy.statManaMax = other.statManaMax;
        _playerCopy.statManaMax2 = other.statManaMax2;
        _playerCopy.hideMisc = other.hideMisc;
        _playerCopy.ResetEffects();
        _playerCopy.ResetVisibleAccessories();
        _playerCopy.DisplayDollUpdate();
        _playerCopy.UpdateSocialShadow();
        _playerCopy.Center = NPC.Center;
        _playerCopy.direction = NPC.direction;
        _playerCopy.velocity = NPC.velocity;
        _playerCopy.PlayerFrame();
        _playerCopy.head = _playerCopy.body = _playerCopy.legs = -1;
        _playerCopy.socialIgnoreLight = true;
        _playerCopy.dead = true;

        _playerCopy.opacityForAnimation = opacity;
        _playerCopy.skinColor = Color.Lerp(_skinColor, SkinColor, TransformationFactor);

        Vector2 headPosition = NPC.Center - _playerCopy.Size / 2f;
        Main.PlayerRenderer.DrawPlayer(Main.Camera, _playerCopy, headPosition, 0f, Vector2.Zero, scale: opacity);

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.OnPlayer1].Value;
        Vector2 position = BodyPosition() + (NPC.FacedRight() ? new Vector2(4f, 0f) : Vector2.Zero);
        Rectangle clip = texture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = Color.White * opacity;
        Vector2 scale = Vector2.One * opacity;
        float rotation = 0f;
        SpriteEffects effects = NPC.FacedRight() ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        batch.DrawWithSnapshot(() => {
            batch.Draw(texture, position + _playerCopy.MovementOffset(), DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale,
                Rotation = rotation,
                ImageFlip = effects
            });
        });

        _playerCopy.direction = NPC.direction;
        Main.PlayerRenderer.DrawPlayerHead(Main.Camera, _playerCopy, headPosition + new Vector2(6f, 6f) - Main.screenPosition, alpha: Ease.CubeIn(opacity), scale: opacity);

        if (_playerCopy.SpeedX() < 0.1f) {
            texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.OnPlayer2].Value;
            position = ArmPosition() + (NPC.FacedRight() ? new Vector2(-14f, 0f) : Vector2.Zero);
            clip = texture.Bounds;
            origin = clip.Centered();
            color = Color.White * opacity;
            scale = Vector2.One * opacity;
            rotation = 0f;
            batch.DrawWithSnapshot(() => {
                batch.Draw(texture, position, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Scale = scale,
                    Rotation = rotation,
                    ImageFlip = effects
                });
            });
        }

        NPC.position = basePosition;

        return false;
    }

    private Vector2 BodyPosition() => _playerCopy.Center + _playerCopy.bodyPosition + new Vector2(-2f, 1f);
    private Vector2 ArmPosition() => _playerCopy.Center + _playerCopy.bodyPosition + new Vector2(7f, 10f);

    public void DrawFluidSelf() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        _playerCopy.Center = NPC.Center + Vector2.UnitY * NPC.gfxOffY;
        _playerCopy.direction = NPC.direction;
        _playerCopy.velocity = NPC.velocity;

        float opacity = NPC.Opacity;

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = NPC.GetTexture();
        Vector2 basePosition = NPC.Center + Vector2.UnitY * 5f + Vector2.UnitY * NPC.gfxOffY;
        Vector2 position = basePosition;
        Rectangle clip = texture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = Color.White * opacity;
        Vector2 scale = Vector2.One * TransformationFactor * opacity;
        float rotation = 0f;
        SpriteEffects effects = NPC.FacedRight() ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        batch.DrawWithSnapshot(() => {
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale,
                Rotation = rotation,
                ImageFlip = effects
            });
        });

        color = LiquidColor * Ease.CubeIn(opacity);
        Vector2 baseHeadPosition = _playerCopy.Center + _playerCopy.headPosition;
        Vector2 headPosition = baseHeadPosition;
        headPosition += headPosition.DirectionFrom(position) * 10f * TransformationFactor * opacity;
        batch.Line(position, Vector2.Lerp(position, headPosition, Ease.CubeIn(opacity)), thickness: 10f * TransformationFactor * opacity, color: color);

        foreach (var part in _fluidBodyParts) {
            switch (part.Type) {
                case FluidBodyPartType.Part1:
                    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part1].Value;
                    break;
                case FluidBodyPartType.Part2:
                    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part2].Value;
                    break;
                case FluidBodyPartType.Part3:
                    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part3].Value;
                    break;
            }
            position = basePosition;
            clip = texture.Bounds;
            origin = clip.Centered() + part.Position + part.Velocity;
            color = Color.White * opacity;
            rotation = part.Rotation;
            batch.DrawWithSnapshot(() => {
                batch.Draw(texture, position, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Scale = scale,
                    Rotation = rotation,
                    ImageFlip = effects
                });
            });
        }
    }

    public void DrawFluidSelf2() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }
    }

    public override bool CanChat() => true;

    public override void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) {
        position.Y += 18f + 18f * (1f - NPC.Opacity);
        position.X += 6f * NPC.direction;
        if (!NPC.FacedRight()) {
            position.X += NPC.width * 2f;
            position.X -= 12f;
        }

        spriteEffects = (-NPC.direction).ToSpriteEffects();
    }

    public override string GetChat() {
        if (!Talked && Helper.SinglePlayerOrServer) {
            Talked = true;
            NPC.netUpdate = true;
        }

        return base.GetChat();
    }

    public override void SetChatButtons(ref string button, ref string button2) {
        button = button2 = string.Empty;
    }

    public override void AI() {
        NPC.dontTakeDamage = NPC.Opacity < 0.5f;

        bool teleported = false;
        TransformationFactor = Helper.Wave(VisualTimer, _minTransform, _maxTransform, 1f, 0f);
        TransformationFactor = Ease.SineOut(MathUtils.Clamp01(TransformationFactor));

        ushort tarDustMetaball = (ushort)ModContent.DustType<TarMetaball>();

        float addFactor = 2f;
        if (Talked && !TransformedEnough) {
            VisualTimer += TimeSystem.LogicDeltaTime * addFactor;
            VisualTimer2 = VisualTimer;

            if (TransformationFactor >= _maxTransform) {
                TransformedEnough = true;
                VisualTimer2 = VisualTimer;
            }
        }
        if (TransformedEnough) {
            VisualTimer2 += TimeSystem.LogicDeltaTime * addFactor;
            float max = VisualTimer + (1f - _minTransform * 2f);
            float progress = VisualTimer2 / max;
            if (CanTeleport && VisualTimer2 > max) {
                Init = false;
                ActuallyTeleport();
                teleported = true;
            }
            if (CanTeleport && !teleported && progress >= 0.75f) {
                for (int i = 0; i < 4; i++) {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, tarDustMetaball, Alpha: 0);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity.Y -= 0.25f + 2.5f * progress * Main.rand.NextFloatDirection();
                    Main.dust[dust].velocity.X *= 2.5f * progress * Main.rand.NextFloatDirection();
                    Main.dust[dust].scale = 1.4f + Main.rand.NextFloat() * 0.6f;
                }
            }
            if (!CanTeleport) {
                Walking();
            }
            else {
                NPC.velocity.X *= 0.8f;
            }
        }

        if (!Init) {
            Init = true;

            VisualTimer = VisualTimer2 = -2f;
            TransformationFactor = 0f;

            NPC.Opacity = 0f;

            if (_maxTransform == 0f) {
                _maxTransform = 0.5f;
            }
            else {
                TeleportCount++;
                if (TeleportCount == 1) {
                    _minTransform = 0.25f;
                    _maxTransform = 0.75f;
                }
                else {
                    _minTransform = 0.5f;
                    _maxTransform = 1f;
                }
            }


            Talked = TransformedEnough = false;

            if (!teleported) {
                int partCount = 10;
                _fluidBodyParts = new FluidBodyPart[partCount];
                for (int i = 0; i < partCount; i++) {
                    Vector2 position = (i / MathHelper.TwoPi).ToRotationVector2() * 5f;
                    _fluidBodyParts[i] = new FluidBodyPart(position, Main.rand.GetRandomEnumValue<FluidBodyPartType>(), Main.rand.NextFloatRange(MathHelper.TwoPi));
                }

                RandomizePlayer();
            }
        }
        else if (NPC.IsGrounded()) {
            NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1f, 0.075f);
        }

        float maxHeadRotation = MathHelper.PiOver4 / 3f;
        ref float headRotation = ref _playerCopy.headRotation;
        headRotation = Helper.Wave(VisualTimer2, - maxHeadRotation, maxHeadRotation, 5f, 0f);
        headRotation = MathHelper.Lerp(headRotation, -0.25f * NPC.direction, 1f - TransformationFactor);
        _playerCopy.headPosition = new Vector2(0f, -24f).RotatedBy(_playerCopy.headRotation) * TransformationFactor;
        _playerCopy.headPosition.Y += 1f;
        _playerCopy.eyeColor = Color.Lerp(_eyeColor, Color.White, TransformationFactor);

        NPC.TargetClosest();

        for (int i = 0; i < _fluidBodyParts.Length; i++) {
            _fluidBodyParts[i].Rotation += TimeSystem.LogicDeltaTime * (i % 2 == 0).ToDirectionInt() * TransformationFactor * NPC.direction;
            _fluidBodyParts[i].Rotation += 0.01f * NPC.direction * TransformationFactor;
            _fluidBodyParts[i].Velocity = Helper.Wave(VisualTimer2, -1f, 1f, 5f, i).ToRotationVector2() * 5f * TransformationFactor;
        }

        if (NPC.IsGrounded()) {
            int chance = (int)(20 + 40 * (1f - TransformationFactor));
            if (Main.rand.NextBool(chance)) {
                Dust dust = Dust.NewDustPerfect(ArmPosition() + new Vector2(2f * Main.rand.NextFloatDirection(), 2f * Main.rand.NextFloatDirection()), tarDustMetaball);
                dust.alpha = (int)(255 * NPC.Opacity);
                dust.velocity.X *= 0.1f;
            }
            if (Main.rand.NextBool(chance)) {
                Dust dust = Dust.NewDustPerfect(BodyPosition() + new Vector2(6f * Main.rand.NextFloatDirection(), 4f * Main.rand.NextFloatDirection()), tarDustMetaball);
                dust.alpha = (int)(255 * NPC.Opacity);
                dust.velocity.X *= 0.1f;
            }
            if (TransformationFactor > 0.15f && Main.rand.NextBool((int)(chance + 40 * (1f - TransformationFactor)))) {
                Dust dust = Dust.NewDustPerfect(BodyPosition() - Vector2.UnitY * 14f + new Vector2(20f * Main.rand.NextFloatDirection(), 20f * Main.rand.NextFloatDirection()), tarDustMetaball);
                dust.alpha = (int)(255 * NPC.Opacity);
                dust.velocity.X *= 0.1f;
            }
        }
    }

    private void Walking() {
        NPC.aiStyle = NPC.ModNPC.AIType = -1;

        NPC npc = NPC;
        if (Main.player[npc.target].position.Y + (float)Main.player[npc.target].height == npc.position.Y + (float)npc.height)
            npc.directionY = -1;

        bool flag = false;
        bool canOpenDoor2 = false;
        bool flag6 = false;
        if (npc.velocity.X == 0f)
            flag6 = true;

        if (npc.justHit)
            flag6 = false;

        flag6 = false;

        bool targetPlayer = true;
        int num56 = 60;

        bool flag7 = false;
        bool canOpenDoor = true;

        bool flag9 = false;

        bool flag10 = true;

        if (!flag9 && flag10) {
            if (NPC.IsGrounded() && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
                flag7 = true;

            if (npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)num56 || flag7)
                npc.ai[3] += 1f;
            else if ((double)Math.Abs(npc.velocity.X) > 0.9 && npc.ai[3] > 0f)
                npc.ai[3] -= 1f;

            if (npc.ai[3] > (float)(num56 * 10))
                npc.ai[3] = 0f;

            if (npc.justHit)
                npc.ai[3] = 0f;

            if (npc.ai[3] == (float)num56)
                npc.netUpdate = true;

            if (Main.player[npc.target].Hitbox.Intersects(npc.Hitbox))
                npc.ai[3] = 0f;
        }

        bool shouldTargetPlayer = Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(npc.type, npc.position, npc);

        shouldTargetPlayer = targetPlayer;
        if (npc.ai[3] < (float)num56 && shouldTargetPlayer) {
            npc.TargetClosest();
            if (npc.directionY > 0 && Main.player[npc.target].Center.Y <= npc.Bottom.Y)
                npc.directionY = -1;
        }
        else if (!(_timer > 0f) || !Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_CanBeBusyWithAction(npc.type)) {
            bool flag12 = targetPlayer/*Main.player[npc.target].InModBiome<BackwoodsBiome>()*/;
            //if (!flag12 && (double)(npc.position.Y / 16f) < Main.worldSurface/* && npc.type != 624 && npc.type != 631*/) {
            //    npc.EncourageDespawn(10);
            //}

            if (npc.velocity.X == 0f) {
                if (NPC.IsGrounded()) {
                    _timer2 += 1f;
                    if (_timer2 >= 2f) {
                        npc.direction *= -1;
                        npc.spriteDirection = npc.direction;
                        _timer2 = 0f;
                    }
                }
            }
            else {
                _timer2 = 0f;
            }

            if (npc.direction == 0)
                npc.direction = 1;
        }

        float num87 = 1f * 4.5f;
        float num88 = 0.07f * 1.5f;
        if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
            if (NPC.IsGrounded())
                npc.velocity *= 0.7f;
        }
        else if (npc.velocity.X < num87 && npc.direction == 1) {
            npc.velocity.X += num88;
            if (npc.velocity.X > num87)
                npc.velocity.X = num87;
        }
        else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
            npc.velocity.X -= num88;
            if (npc.velocity.X < 0f - num87)
                npc.velocity.X = 0f - num87;
        }

        bool tileChecks = false;
        if (NPC.IsGrounded()) {
            int num77 = (int)(NPC.position.Y + NPC.height + 7f) / 16;
            int num189 = (int)NPC.position.X / 16;
            int num79 = (int)(NPC.position.X + NPC.width) / 16;
            for (int num80 = num189; num80 <= num79; num80++) {
                if (Main.tile[num80, num77].HasUnactuatedTile && Main.tileSolid[Main.tile[num80, num77].TileType]) {
                    tileChecks = true;
                    break;
                }
            }
        }
        float jumpModifier = 1.5f;
        if (NPC.velocity.Y >= 0f) {
            int direction = Math.Sign(NPC.velocity.X);

            Vector2 position3 = NPC.position;
            position3.X += NPC.velocity.X;
            int num82 = (int)((position3.X + NPC.width / 2 + (NPC.width / 2 + 1) * direction) / 16f);
            int num83 = (int)((position3.Y + NPC.height - 1f) / 16f);
            if (num82 * 16 < position3.X + NPC.width && num82 * 16 + 16 > position3.X && (Main.tile[num82, num83].HasUnactuatedTile && !Main.tile[num82, num83].TopSlope && !Main.tile[num82, num83 - 1].TopSlope && Main.tileSolid[Main.tile[num82, num83].TileType] && !Main.tileSolidTop[Main.tile[num82, num83].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && Main.tile[num82, num83 - 1].HasUnactuatedTile) && (!Main.tile[num82, num83 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 1].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 1].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && (!Main.tile[num82, num83 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 4].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 4].TileType])) && (!Main.tile[num82, num83 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 2].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 2].TileType]) && (!Main.tile[num82, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 3].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 3].TileType]) && (!Main.tile[num82 - direction, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82 - direction, num83 - 3].TileType])) {
                float num84 = num83 * 16;
                if (Main.tile[num82, num83].IsHalfBlock) {
                    num84 += 8f;
                }

                if (Main.tile[num82, num83 - 1].IsHalfBlock) {
                    num84 -= 8f;
                }
                if (num84 < position3.Y + NPC.height) {
                    float num85 = position3.Y + NPC.height - num84;
                    float num86 = 16.1f;
                    if (NPC.type == NPCID.BlackRecluse || NPC.type == NPCID.WallCreeper || NPC.type == NPCID.JungleCreeper || NPC.type == NPCID.BloodCrawler || NPC.type == NPCID.DesertScorpionWalk) {
                        num86 += 8f;
                    }

                    if (num85 <= num86) {
                        NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                        NPC.position.Y = num84 - NPC.height;
                        if (num85 < 9f) {
                            NPC.stepSpeed = 1f;
                        }
                        else {
                            NPC.stepSpeed = 2f;
                        }
                    }
                }
            }
        }
        if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {
            int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);

            {
                if (NPC.velocity.X < 0f && NPC.direction == -1 || NPC.velocity.X > 0f && NPC.direction == 1) {
                    void jumpIfPlayerAboveAndClose() {
                        if (NPC.IsGrounded() && Main.expertMode && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                            if (NPC.IsGrounded()) {
                                int num200 = 6;
                                if (Main.player[npc.target].Bottom.Y > npc.Top.Y - (float)(num200 * 16)) {
                                    npc.velocity.Y = -7.9f * jumpModifier;
                                }
                                else {
                                    int num201 = (int)(npc.Center.X / 16f);
                                    int num202 = (int)(npc.Bottom.Y / 16f) - 1;
                                    for (int num203 = num202; num203 > num202 - num200; num203--) {
                                        if (Main.tile[num201, num203].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[num201, num203].TileType]) {
                                            npc.velocity.Y = -7.9f * jumpModifier;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    jumpIfPlayerAboveAndClose();

                    bool JumpCheck(int tileX, int tileY) {
                        if (NPC.height >= 32 && Main.tile[tileX, tileY - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 2].TileType]) {
                            if (Main.tile[tileX, tileY - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 3].TileType]) {
                                NPC.velocity.Y = -8f * jumpModifier;
                                NPC.netUpdate = true;
                            }
                            else {
                                NPC.velocity.Y = -7f * jumpModifier;
                                NPC.netUpdate = true;
                            }
                            return true;
                        }
                        else if (Main.tile[tileX, tileY - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 1].TileType]) {
                            NPC.velocity.Y = -6f * jumpModifier;
                            NPC.netUpdate = true;
                            return true;
                        }
                        else if (NPC.position.Y + NPC.height - tileY * 16 > 20f && Main.tile[tileX, tileY].HasUnactuatedTile && !Main.tile[tileX, tileY].TopSlope && Main.tileSolid[Main.tile[tileX, tileY].TileType]) {
                            NPC.velocity.Y = -5f * jumpModifier;
                            NPC.netUpdate = true;
                            return true;
                        }
                        else if (NPC.directionY < 0 && (!Main.tile[tileX, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY + 1].TileType]) && (!Main.tile[tileX + NPC.direction, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX + NPC.direction, tileY + 1].TileType])) {
                            NPC.velocity.Y = -8f * jumpModifier;
                            NPC.velocity.X *= 1.5f;
                            NPC.netUpdate = true;
                            return true;
                        }
                        return false;
                    }
                    if (!JumpCheck(tileX, tileY)) {
                    }
                }
            }
        }
        if (npc.IsGrounded()) {
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }
    }

    private void ActuallyTeleport() {
        Player targetAsPlayer = NPC.GetTargetPlayer();
        if (Helper.SinglePlayerOrServer) {
            Point point14 = NPC.Center.ToTileCoordinates();
            Point point15 = targetAsPlayer.Center.ToTileCoordinates();
            Vector2 chosenTile2 = Vector2.Zero;
            if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile2, point15.X, point15.Y, 40, 12, 2, solidTileCheckCentered: true, teleportInAir: true)) {
                NPC.Center = chosenTile2.ToWorldCoordinates();
                while (NPC.XDistance(targetAsPlayer) < 300f) {
                    if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile2, point15.X, point15.Y, 40, 12, 2, solidTileCheckCentered: true, teleportInAir: true)) {
                        NPC.Center = chosenTile2.ToWorldCoordinates();
                    }
                }
            }
            NPC.netUpdate = true;
        }
    }

    private int[] _validClothStyles = new int[10] {
        0,
        2,
        1,
        3,
        8,
        4,
        6,
        5,
        7,
        9
    };

    private static Color ScaledHslToRgb(Vector3 hsl) => ScaledHslToRgb(hsl.X, hsl.Y, hsl.Z);
    private static Color ScaledHslToRgb(float hue, float saturation, float luminosity) => Main.hslToRgb(hue, saturation, luminosity * 0.85f + 0.15f);
    private static Vector3 GetRandomColorVector() => new Vector3(Main.rand.NextFloat(), Main.rand.NextFloat(), Main.rand.NextFloat());

    private void RandomizePlayer() {
        Player player = _playerCopy;
        player.hair = Main.rand.NextBool() ? Main.rand.Next(52) : Main.rand.Next(135, 163);
        int[] invalid = [2, 17, 136, 148, 151, 152, 153, 154, 155, 157, 158, 159];
        while (invalid.Contains(player.hair)) {
            player.hair = Main.rand.NextBool() ? Main.rand.Next(52) : Main.rand.Next(135, 163);
        }
        player.eyeColor = ScaledHslToRgb(GetRandomColorVector());
        while (player.eyeColor.R + player.eyeColor.G + player.eyeColor.B > 300) {
            player.eyeColor = ScaledHslToRgb(GetRandomColorVector());
        }
        _eyeColor = player.eyeColor;

        float num = (float)Main.rand.Next(60, 120) * 0.01f;
        if (num > 1f)
            num = 1f;

        player.skinColor.R = (byte)((float)Main.rand.Next(240, 255) * num);
        player.skinColor.G = (byte)((float)Main.rand.Next(110, 140) * num);
        player.skinColor.B = (byte)((float)Main.rand.Next(75, 110) * num);
        _skinColor = player.skinColor;
        player.hairColor = ScaledHslToRgb(GetRandomColorVector());
        player.shirtColor = ScaledHslToRgb(GetRandomColorVector());
        player.underShirtColor = ScaledHslToRgb(GetRandomColorVector());
        player.pantsColor = ScaledHslToRgb(GetRandomColorVector());
        player.shoeColor = ScaledHslToRgb(GetRandomColorVector());
        player.skinVariant = _validClothStyles[Main.rand.Next(_validClothStyles.Length)];
        switch (player.hair + 1) {
            case 5:
            case 6:
            case 7:
            case 10:
            case 12:
            case 19:
            case 22:
            case 23:
            case 26:
            case 27:
            case 30:
            case 33:
            case 34:
            case 35:
            case 37:
            case 38:
            case 39:
            case 40:
            case 41:
            case 44:
            case 45:
            case 46:
            case 47:
            case 48:
            case 49:
            case 51:
            case 56:
            case 65:
            case 66:
            case 67:
            case 68:
            case 69:
            case 70:
            case 71:
            case 72:
            case 73:
            case 74:
            case 79:
            case 80:
            case 81:
            case 82:
            case 84:
            case 85:
            case 86:
            case 87:
            case 88:
            case 90:
            case 91:
            case 92:
            case 93:
            case 95:
            case 96:
            case 98:
            case 100:
            case 102:
            case 104:
            case 107:
            case 108:
            case 113:
            case 124:
            case 126:
            case 133:
            case 134:
            case 135:
            case 144:
            case 146:
            case 147:
            case 163:
            case 165:
                player.Male = false;
                break;
            default:
                player.Male = true;
                break;
        }
    }

    public class PerfectMimicMetaballs : Metaball {
        public override MetaballDrawLayer DrawContext => MetaballDrawLayer.BeforeNPCs;

        public override Color EdgeColor => OutlineColor;

        public override bool AnythingToDraw => NPCUtils.AnyNPCs<PerfectMimic>();

        public override List<Texture2D> Layers => [ModContent.Request<Texture2D>(ResourceManager.MetaballLayerTextures + "PerfectMimic").Value];

        public override bool ShouldDrawItsContent() => true;

        public override void Update() {
            LerpColor.Update();
            LerpColor.Update();
            LerpColor.Update();
        }

        public override void BeforeDrawingTarget(SpriteBatch spriteBatch) {
        }

        public override void DrawInstances() {
            foreach (NPC perfectMimic in TrackedEntitiesSystem.GetTrackedNPC<PerfectMimic>()) {
                perfectMimic.As<PerfectMimic>().DrawFluidSelf();
            }
        }
    }

    public class PerfectMimicMetaballs_AfterEffects : Metaball {
        public override MetaballDrawLayer DrawContext => MetaballDrawLayer.AfterProjectiles;

        public override Color EdgeColor => OutlineColor;

        public override bool AnythingToDraw => NPCUtils.AnyNPCs<PerfectMimic>();

        public override List<Texture2D> Layers => [ModContent.Request<Texture2D>(ResourceManager.MetaballLayerTextures + "PerfectMimic").Value];

        public override bool ShouldDrawItsContent() => true;

        public override void Update() {
        }

        public override void BeforeDrawingTarget(SpriteBatch spriteBatch) {
        }

        public override void DrawInstances() {
            for (int i = 0; i < Main.maxDustToDraw; i++) {
                Dust dust = Main.dust[i];
                if (!dust.active)
                    continue;
                if (dust.type == ModContent.DustType<TarMetaball>()) {
                    Texture2D texture = DustLoader.GetDust(dust.type).Texture2D.Value;
                    Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, dust.frame, Color.White, dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
                }
            }
            foreach (NPC perfectMimic in TrackedEntitiesSystem.GetTrackedNPC<PerfectMimic>()) {
                perfectMimic.As<PerfectMimic>().DrawFluidSelf2();
            }
        }
    }
}
