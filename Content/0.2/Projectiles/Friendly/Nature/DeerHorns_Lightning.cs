using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class HornsLightning : FormProjectile_NoTextureLoad {
    private readonly List<Vector2> _nodes = [];

    private Vector2 _startPosition;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float Scale => ref Projectile.localAI[1];

    public ref float TargetX => ref Projectile.ai[0];
    public ref float TargetY => ref Projectile.ai[1];
    public ref float Seed => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public Vector2 TargetPosition => new(TargetX, TargetY);

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(80);

        Projectile.friendly = true;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        if (!Init) {
            if (owner.IsLocal()) {
                _startPosition = Vector2.UnitX * 45 * Main.rand.NextFloat(0.5f, 1f) * Main.rand.NextFloatDirection() + Main.rand.RandomPointInArea(20f);
                Projectile.netUpdate = true;
            }
        }

        Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter);
        Projectile.Center = Utils.Floor(Projectile.Center) - new Vector2(0f, 20f) + _startPosition;

        if (!Init) {
            Init = true;

            if (owner.IsLocal()) {
                Seed = Main.rand.Next(100);

                Vector2 start = Projectile.Center;
                Vector2 end = TargetPosition;

                CreateLightning(start, end, jaggedness: 0.1f, segments: 10);
                Projectile.netUpdate = true;
            }

            Scale = 5f;
        }

        Scale = MathHelper.Lerp(Scale, 0f, 0.15f);
        if (Scale <= 0.1f) {
            Projectile.Kill();
            return;
        }

        for (int index = 0; index < _nodes.Count - 1; index++) {
            float scaleOpacity = Utils.GetLerpValue(0.375f, 1f, Scale, true);
            Vector2 start = _nodes[index];
            Color borderColor = new Color(49, 183, 184);
            while (start.Distance(_nodes[index + 1]) > 10f) {
                Lighting.AddLight(start, borderColor.ToVector3() * 0.5f * scaleOpacity);
                start += start.DirectionTo(_nodes[index + 1]) * 5f;
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (Scale <= 1f) {
            return false;
        }

        foreach (Vector2 nodePosition in _nodes) {
            float scaleOpacity = Utils.GetLerpValue(0.375f, 1f, Scale, true);
            if (GeometryUtils.CenteredSquare(nodePosition, (int)(16 * scaleOpacity)).Intersects(targetHitbox)) {
                return true;
            }
        }

        return false;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_startPosition);

        writer.Write((ushort)_nodes.Count);
        foreach (Vector2 vector2 in _nodes) {
            writer.WriteVector2(vector2);
        }
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _startPosition = reader.ReadVector2();
        ushort count = reader.ReadUInt16();
        _nodes.Clear();
        for (int i = 0; i < count; i++) {
            Vector2 nodePosition = reader.ReadVector2();
            _nodes.Add(nodePosition);
        }
    }

    public void CreateLightning(Vector2 start, Vector2 end, float jaggedness = 0.15f, int segments = 8, float maxOffset = 50f) {
        _nodes.Clear();
        UnifiedRandom random = Main.rand;
        _nodes.Add(start);
        for (int i = 1; i < segments; i++) {
            float t = (float)i / segments;
            Vector2 point = Vector2.Lerp(start, end, t);
            float maxOffset2 = Math.Min(Vector2.Distance(start, end) * jaggedness, maxOffset);
            point += new Vector2(
                random.NextFloat(-maxOffset2, maxOffset2),
                random.NextFloat(-maxOffset2, maxOffset2)
            );
            _nodes.Add(point);
        }
        _nodes.Add(end);
    }

    protected override void Draw(ref Color lightColor) {
        if (_nodes.Count <= 0) {
            return;
        }
        for (int index = 0; index < _nodes.Count - 1; index++) {
            float scaleOpacity = Utils.GetLerpValue(0.375f, 1f, Scale, true);
            Texture2D text = ModContent.Request<Texture2D>(ResourceManager.Textures + "Bloom0").Value;
            Vector2 start = _nodes[index];
            ulong seed = (ulong)(index + Projectile.whoAmI);
            Color borderColor = new Color(49, 183, 184);
            while (start.Distance(_nodes[index + 1]) > 10f) {
                float size = 0.5f + Utils.RandomInt(ref seed, 100) / 100f * 0.5f;
                borderColor = new Color(49, 183, 184).MultiplyAlpha(0.75f + 0.25f * Utils.RandomInt(ref seed, 100) / 100f);
                Main.spriteBatch.Draw(text, start - Main.screenPosition, null, borderColor * 0.375f, 0f, text.Size() / 2f, size * 0.25f * scaleOpacity, 0, 0);
                start += start.DirectionTo(_nodes[index + 1]) * 5f;
            }
            DrawLightning(Main.spriteBatch, (uint)Seed, _nodes[index], _nodes[index + 1], Scale, 0f, Color.White * scaleOpacity, borderColor.ModifyRGB(0.9f) * 0.9f * scaleOpacity);
        }
        for (int index = 0; index < _nodes.Count - 1; index++) {
            float scaleOpacity = Utils.GetLerpValue(0.375f, 1f, Scale, true);
            Texture2D text = ModContent.Request<Texture2D>(ResourceManager.Textures + "Bloom0").Value;
            Vector2 start = _nodes[index];
            Color borderColor = new Color(49, 183, 184);
            while (start.Distance(_nodes[index + 1]) > 10f) {
                Main.spriteBatch.Draw(text, start - Main.screenPosition, null, borderColor * 0.1f, 0f, text.Size() / 2f, 1f * 0.25f * scaleOpacity, 0, 0);
                start += start.DirectionTo(_nodes[index + 1]) * 5f;
            }
        }
    }

    private void DrawLightning(SpriteBatch batch, uint seed, Vector2 a, Vector2 b, float size, float gap, Color color, Color borderColor) {
        batch.DrawWithSnapshot(() => {
            seed += (uint)(a.GetHashCode() + b.GetHashCode());
            float num1 = (b - a).Length();
            Vector2 vec = (b - a) / num1;
            Vector2 vector2_1 = vec.TurnRight();
            Vector2 start = a;
            int num2 = 1;
            double num3 = (double)MathUtils.PseudoRandRange(ref seed, 0.0f, MathHelper.TwoPi);
            float num4 = 0.0f;
            List<(Vector2, Vector2)> drawLines = [];
            List<(Vector2, Vector2)> drawLines2 = [];
            gap = MathUtils.PseudoRandRange(ref seed, 0f, 0.75f);
            do {
                bool shouldDrawBorder = true;
                num4 += MathUtils.PseudoRandRange(ref seed, 10f, 14f);
                Vector2 vector2_2 = a + vec * num4;
                Vector2 vector2_3 = (double)num4 >= (double)num1 ? b : vector2_2 + (float)num2 * vector2_1 * MathUtils.PseudoRandRange(ref seed, 0.0f, 6f);
                Vector2 vector2_4 = vector2_3;
                if ((double)gap > 0.0) {
                    vector2_4 = start + (vector2_3 - start) * (1f - gap);
                    drawLines.Add((start, vector2_3 + vec));
                    if (shouldDrawBorder) {
                        for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
                            Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 1f * size * 0.5f;
                            if (MathUtils.PseudoRandRange(ref seed, 0f, 1f) < 0.9f) {
                                batch.Line(start + vector2, Vector2.Lerp(vector2_3 + vec, start, 0f) + vector2, borderColor, size * 0.5f);
                            }
                        }
                    }
                }
                drawLines2.Add((start, vector2_4 + vec));
                if (shouldDrawBorder) {
                    for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
                        Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 1f * size * 0.5f;
                        if (MathUtils.PseudoRandRange(ref seed, 0f, 1f) < 0.9f) {
                            batch.Line(start + vector2, Vector2.Lerp(vector2_4 + vec, start, 0f) + vector2, borderColor, size);
                        }
                    }
                }
                start = vector2_3;
                num2 = -num2;
            }
            while ((double)num4 < (double)num1);

            foreach (var drawLine in drawLines) {
                batch.Line(drawLine.Item1, drawLine.Item2, color, size * 0.5f);
            }
            foreach (var drawLine in drawLines2) {
                batch.Line(drawLine.Item1, drawLine.Item2, color, size);
            }

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);

        //if (shouldDrawBorder) {
        //    for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
        //        Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * size;
        //        batch.Line(start + vector2, vector2_3 + vec + vector2, borderColor, size * 0.5f);
        //    }
        //}
    }
}
