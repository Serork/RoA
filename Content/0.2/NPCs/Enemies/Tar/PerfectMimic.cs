using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Metaballs;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

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
        Part3
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture
        => [((byte)PerfectMimicRequstedTextureType.Part1, Texture + "_Part1"),
            ((byte)PerfectMimicRequstedTextureType.Part2, Texture + "_Part2"),
            ((byte)PerfectMimicRequstedTextureType.Part3, Texture + "_Part3")];

    private FluidBodyPart[] _fluidBodyParts = null!;
    private Player _playerCopy = null!;
    private Color _eyeColor;

    public ref float InitValue => ref NPC.localAI[0];
    public ref float TransformationFactor => ref NPC.scale;
    public ref float VisualTimer => ref NPC.localAI[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }


    public override void SetDefaults() {
        NPC.SetSizeValues(30, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;

        _playerCopy = new Player();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
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

        Main.PlayerRenderer.DrawPlayer(Main.Camera, _playerCopy, NPC.Center - _playerCopy.Size / 2f, 0f, Vector2.Zero);

        return false;
    }

    public void DrawFluidSelf() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        _playerCopy.Center = NPC.Center;
        _playerCopy.direction = NPC.direction;
        _playerCopy.velocity = NPC.velocity;

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = NPC.GetTexture();
        Vector2 position = NPC.Center;
        Rectangle clip = texture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = LiquidColor;
        Vector2 scale = Vector2.One * TransformationFactor;
        float rotation = 0f;
        batch.DrawWithSnapshot(() => {
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale,
                Rotation = rotation
            });
        });

        Vector2 center = NPC.Center;
        Vector2 headPosition = _playerCopy.Center + _playerCopy.headPosition;
        headPosition += headPosition.DirectionFrom(center) * 10f;
        batch.Line(center, headPosition, thickness: 10f, color: color);

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
            position = NPC.Center;
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
                    Rotation = rotation
                });
            });
        }
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

        VisualTimer += TimeSystem.LogicDeltaTime;

        float maxHeadRotation = MathHelper.PiOver4 / 3f;
        ref float headRotation = ref _playerCopy.headRotation;
        headRotation = Helper.Wave(VisualTimer, - maxHeadRotation, maxHeadRotation, 5f, 0f);
        headRotation = MathHelper.Lerp(headRotation, 0f, 1f - TransformationFactor);
        _playerCopy.headPosition = new Vector2(0f, -20f).RotatedBy(_playerCopy.headRotation) * TransformationFactor;
        _playerCopy.eyeColor = Color.Lerp(_eyeColor, Color.White, TransformationFactor);

        NPC.TargetClosest(false);

        NPC.velocity.X *= 0.8f;

        TransformationFactor = Helper.Wave(VisualTimer, 0f, 1f, 1f, 0f);

        for (int i = 0; i < _fluidBodyParts.Length; i++) {
            _fluidBodyParts[i].Rotation += TimeSystem.LogicDeltaTime * (i % 2 == 0).ToDirectionInt();
            _fluidBodyParts[i].Velocity = Helper.Wave(VisualTimer, -1f, 1f, 5f, i).ToRotationVector2() * 5f;
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
        Main.Hairstyles.UpdateUnlocks();
        int index = Main.rand.Next(Main.Hairstyles.AvailableHairstyles.Count);
        player.hair = Main.Hairstyles.AvailableHairstyles[index];
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
}
