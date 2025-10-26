using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Metaballs;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

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
    private Color _eyeColor;

    public ref float InitValue => ref NPC.localAI[0];
    public ref float TalkedValue => ref NPC.ai[0];
    public ref float TransformationFactor => ref NPC.scale;
    public ref float VisualTimer => ref NPC.localAI[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Talked {
        get => TalkedValue == 1f;
        set => TalkedValue = value.ToInt();
    }

    public override void SetStaticDefaults() {
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
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

        Vector2 headPosition = NPC.Center - _playerCopy.Size / 2f;
        Main.PlayerRenderer.DrawPlayer(Main.Camera, _playerCopy, headPosition, 0f, Vector2.Zero);

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.OnPlayer1].Value;
        Vector2 position = BodyPosition() + (NPC.FacedRight() ? new Vector2(4f, 0f) : Vector2.Zero);
        Rectangle clip = texture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = Color.White;
        Vector2 scale = Vector2.One;
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

        _playerCopy.direction = NPC.direction;
        Main.PlayerRenderer.DrawPlayerHead(Main.Camera, _playerCopy, headPosition + new Vector2(6f, 4f) - Main.screenPosition);

        texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.OnPlayer2].Value;
        position = ArmPosition() + (NPC.FacedRight() ? new Vector2(-18f, 0f) : Vector2.Zero);
        clip = texture.Bounds;
        origin = clip.Centered();
        color = Color.White;
        scale = Vector2.One;
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

        NPC.position = basePosition;

        return false;
    }

    private Vector2 BodyPosition() => _playerCopy.Center + _playerCopy.bodyPosition + new Vector2(-2f, 1f);
    private Vector2 ArmPosition() => _playerCopy.Center + _playerCopy.bodyPosition + new Vector2(9f, 10f);

    public void DrawFluidSelf() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        _playerCopy.Center = NPC.Center;
        _playerCopy.direction = NPC.direction;
        _playerCopy.velocity = NPC.velocity;

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = NPC.GetTexture();
        Vector2 basePosition = NPC.Center + Vector2.UnitY * 5f;
        Vector2 position = basePosition;
        Rectangle clip = texture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = Color.White;
        Vector2 scale = Vector2.One * TransformationFactor;
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

        color = LiquidColor;
        Vector2 baseHeadPosition = _playerCopy.Center + _playerCopy.headPosition;
        Vector2 headPosition = baseHeadPosition;
        headPosition += headPosition.DirectionFrom(position) * 10f;
        batch.Line(position, headPosition, thickness: 10f, color: color);

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
            color = Color.White;
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
        position.Y += 18f;
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
        if (!Init) {
            Init = true;

            VisualTimer = -2f;
            TransformationFactor = 0f;

            int partCount = 10;
            _fluidBodyParts = new FluidBodyPart[partCount];
            for (int i = 0; i < partCount; i++) {
                Vector2 position = (i / MathHelper.TwoPi).ToRotationVector2() * 5f;
                _fluidBodyParts[i] = new FluidBodyPart(position, Main.rand.GetRandomEnumValue<FluidBodyPartType>(), Main.rand.NextFloatRange(MathHelper.TwoPi));
            }

            RandomizePlayer();
        }

        TransformationFactor = VisualTimer < 2f ? Helper.Wave(VisualTimer, 0f, 2f, 1f, 0f) : 2f;
        TransformationFactor = Ease.SineOut(MathUtils.Clamp01(TransformationFactor));

        if (Talked) {
            VisualTimer += TimeSystem.LogicDeltaTime * 1.5f;
        }

        float maxHeadRotation = MathHelper.PiOver4 / 3f;
        ref float headRotation = ref _playerCopy.headRotation;
        headRotation = Helper.Wave(VisualTimer, - maxHeadRotation, maxHeadRotation, 5f, 0f);
        headRotation = MathHelper.Lerp(headRotation, -0.1f * NPC.direction, 1f - TransformationFactor);
        _playerCopy.headPosition = new Vector2(0f, -20f).RotatedBy(_playerCopy.headRotation) * TransformationFactor;
        _playerCopy.headPosition.Y += 1f;
        _playerCopy.eyeColor = Color.Lerp(_eyeColor, Color.White, TransformationFactor);

        NPC.TargetClosest();

        NPC.velocity.X *= 0.8f;

        for (int i = 0; i < _fluidBodyParts.Length; i++) {
            _fluidBodyParts[i].Rotation += TimeSystem.LogicDeltaTime * (i % 2 == 0).ToDirectionInt() * TransformationFactor;
            _fluidBodyParts[i].Velocity = Helper.Wave(VisualTimer, -1f, 1f, 5f, i).ToRotationVector2() * 5f * TransformationFactor;
        }

        ushort tarDustMetaball = (ushort)ModContent.DustType<TarMetaball>();
        int chance = (int)(20 + 40 * (1f - TransformationFactor));
        if (Main.rand.NextBool(chance)) {
            Dust.NewDustPerfect(ArmPosition() + new Vector2(2f * Main.rand.NextFloatDirection(), 2f * Main.rand.NextFloatDirection()), tarDustMetaball);
        }
        if (Main.rand.NextBool(chance)) {
            Dust.NewDustPerfect(BodyPosition() + new Vector2(6f * Main.rand.NextFloatDirection(), 4f * Main.rand.NextFloatDirection()), tarDustMetaball);
        }
        if (TransformationFactor > 0.15f && Main.rand.NextBool((int)(chance + 40 * (1f - TransformationFactor)))) {
            Dust.NewDustPerfect(BodyPosition() - Vector2.UnitY * 14f + new Vector2(20f * Main.rand.NextFloatDirection(), 20f * Main.rand.NextFloatDirection()), tarDustMetaball);
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
