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
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Tar;

[Tracked]
sealed class PerfectMimic : ModNPC, IRequestAssets {
    private static readonly LerpColor LerpColor = new();

    public static Color OutlineColor => LerpColor.GetLerpColor([/*new Color(62, 53, 70), */new Color(98, 85, 101)]);

    public enum FluidBodyPartType : byte {
        Part1,
        Part2, 
        Part3
    }

    public record struct FluidBodyPart(Vector2 Position, FluidBodyPartType Type, float Rotation);

    private FluidBodyPart[] _fluidBodyParts = null!;

    public ref float InitValue => ref NPC.localAI[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public enum PerfectMimicRequstedTextureType : byte {
        Part1,
        Part2,
        Part3
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture
        => [((byte)PerfectMimicRequstedTextureType.Part1, Texture + "_Part1"),
            ((byte)PerfectMimicRequstedTextureType.Part2, Texture + "_Part2"),
            ((byte)PerfectMimicRequstedTextureType.Part3, Texture + "_Part3")];

    public override void SetDefaults() {
        NPC.SetSizeValues(30, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        return false;
    }

    public void DrawFluidSelf() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = NPC.GetTexture();
        Vector2 position = NPC.Center;
        Rectangle clip = texture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = Color.White;
        Vector2 scale = Vector2.One * NPC.scale;
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
            origin = clip.Centered() + part.Position;
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
        NPC.velocity.X *= 0.8f;

        NPC.scale = 1f;

        if (!Init) {
            Init = true;

            int partCount = 10;
            _fluidBodyParts = new FluidBodyPart[partCount];
            for (int i = 0; i < partCount; i++) {
                Vector2 position = (i / MathHelper.TwoPi).ToRotationVector2() * 10f;
                _fluidBodyParts[i] = new FluidBodyPart(position, Main.rand.GetRandomEnumValue<FluidBodyPartType>(), Main.rand.NextFloatRange(MathHelper.TwoPi));
            }
        }

        for (int i = 0; i < _fluidBodyParts.Length; i++) {
            _fluidBodyParts[i].Rotation += TimeSystem.LogicDeltaTime * (i % 2 == 0).ToDirectionInt();
            _fluidBodyParts[i].Position = Helper.Wave(-1f, 1f, 5f, i).ToRotationVector2() * 5f;
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
