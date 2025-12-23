using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Mono.Cecil;

using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class HornsLightning : FormProjectile_NoTextureLoad {
    private readonly List<Vector2> _nodes = [];

    private Vector2 _startPosition;
    private bool _onEnemy, _skySpawn;

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

    public bool EnemySpawn => Seed == 1f;
    public bool SkySpawn => Seed == 2f;

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(80);

        Projectile.friendly = true;
        Projectile.hostile = false;

        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        bool inDruidicForm = owner.GetFormHandler().IsInADruidicForm;
        if (!Init) {
            if (owner.IsLocal() && !EnemySpawn && !SkySpawn) {
                if (!inDruidicForm) {
                    _startPosition = Vector2.UnitX * 25f * Main.rand.NextFloat(0.5f, 1f) * Main.rand.NextFloatDirection() + Main.rand.RandomPointInArea(20f);
                }
                else {
                    _startPosition = Main.rand.RandomPointInArea(owner.width / 2, owner.height / 2);
                }
                Projectile.netUpdate = true;
            }
            if (EnemySpawn) {
                _onEnemy = true;
            }
            if (SkySpawn) {
                _skySpawn = true;
            }
        }

        if (_skySpawn) {

        }
        else if (!_onEnemy) {
            if (inDruidicForm) {
                Projectile.Center = owner.GetPlayerCorePoint() + _startPosition;
            }
            else {
                Projectile.Center = owner.GetPlayerCorePoint() - new Vector2(0f, 26f) + _startPosition;
            }
        }

        if (!Init) {
            Init = true;

            if (_skySpawn) {
                int k = 10;
                Vector2 velocity = Projectile.Center.DirectionTo(TargetPosition) * 10f;
                for (int i = 0; i < k; i++) {
                    int x = (int)((double)Projectile.Center.X);
                    int y = (int)((double)Projectile.Center.Y);
                    Vector2 vector3 = (new Vector2((float)Projectile.width / 2f, Projectile.height) * 0.25f).RotatedBy((float)(i - (k / 2 - 1)) * ((float)Math.PI * 2f) / (float)k) + new Vector2((float)x, (float)y);
                    Vector2 vector2 = -(vector3 - new Vector2((float)x, (float)y));
                    int dust = Dust.NewDust(vector3 - velocity * 3f + vector2 * 2f * Main.rand.NextFloat() - new Vector2(1f, 2f), 0, 0, 226, vector2.X * 2f, vector2.Y * 2f, 0, default(Color), 3.15f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = -Vector2.Normalize(vector2) * Main.rand.NextFloat(1.5f, 3f) * Main.rand.NextFloat() + velocity * 0.75f * Main.rand.NextFloat(0.5f, 1f);
                    Main.dust[dust].scale *= 0.5f;
                }
            }

            if (owner.IsLocal()) {
                Seed = Main.rand.Next(100);

                Vector2 start = Projectile.Center;
                Vector2 end = TargetPosition;

                CreateLightning(start, end, jaggedness: 0.1f, segments: 10);
                Projectile.netUpdate = true;
            }

            Scale = _skySpawn ? 10f : 5f;
        }

        Scale = MathHelper.Lerp(Scale, 0f, _skySpawn ? 0.3f : 0.15f);

        if (_skySpawn && Scale <= 2.25f) {
            if (Projectile.timeLeft > 60) {
                Projectile.timeLeft = 60;
                for (int num770 = 0; num770 < 6; num770++) {
                    float num771 = 0f + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                    float num772 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                    Vector2 vector89 = new Vector2((float)Math.Cos(num771) * num772, (float)Math.Sin(num771) * num772);
                    int num773 = Dust.NewDust(TargetPosition, 0, 0, 226, vector89.X, vector89.Y);
                    Main.dust[num773].noGravity = true;
                    Main.dust[num773].scale = 1.2f;

                    if (Main.rand.Next(3) == 0) {
                        Vector2 vector90 = Vector2.One.RotatedBy(MathHelper.TwoPi) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
                        int num774 = Dust.NewDust(TargetPosition + vector90 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust2 = Main.dust[num774];
                        dust2.velocity *= 0.5f;
                        Main.dust[num774].velocity.Y = 0f - Math.Abs(Main.dust[num774].velocity.Y);
                    }
                }
            }
        }

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

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff<DeerSkullElectrified>(20);

        Vector2 velocity = Projectile.Center.DirectionTo(target.Center) * 10f;
        Vector2 hitPoint = _nodes[^2] + velocity.SafeNormalize(Vector2.UnitX) * 2f;
        Vector2 normal = (-velocity).SafeNormalize(Vector2.UnitX);
        Vector2 spinningpoint = Vector2.Reflect(velocity, normal);
        float scale = 2.5f - Vector2.Distance(target.Center, Projectile.position) * 0.01f;
        scale = MathHelper.Clamp(scale, 0.75f, 1.15f);
        scale *= 0.95f;
        for (int i = 0; i < 4; i++) {
            int num156 = ModContent.DustType<Electric>();
            Dust dust = Dust.NewDustPerfect(_nodes[^1], num156, spinningpoint.RotatedBy((float)Math.PI / 4f * Main.rand.NextFloatDirection()) * 0.6f * Main.rand.NextFloat() * 0.5f, 100, default, 0.5f + 0.3f * Main.rand.NextFloat());
            dust.scale *= scale;
            dust.noGravity = true;
            dust.fadeIn = dust.scale / 2 + 0.1f;
            Dust dust2 = Dust.CloneDust(dust);
            dust2.color = Color.White;
        }

        Projectile.damage /= 2;
        Projectile.knockBack *= 0.75f;

        Player player = Projectile.GetOwnerAsPlayer();
        if (!player.IsLocal()) {
            return;
        }
        NPC? target2 = NPCUtils.FindClosestNPC(target.Center, PlayerCommon.DEERSKULLATTACKDISTANCE / 2, false, filter: (npc) => npc.HasBuff<DeerSkullElectrified>());
        if (target2 is not null) {
            ProjectileUtils.SpawnPlayerOwnedProjectile<HornsLightning>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("hornsattack")) {
                Position = target.Center,
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack,
                AI0 = target2.Center.X,
                AI1 = target2.Center.Y,
                AI2 = 1f
            });
        }
    }

    public override bool? CanHitNPC(NPC target) => !target.friendly && !target.HasBuff<DeerSkullElectrified>();

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        //target.AddBuff<DeerSkullElectrified>(20);
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
        var random = Main.rand;
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
            Texture2D text = ResourceManager.Bloom;
            Vector2 start = _nodes[index];
            ulong seed = (ulong)(index + Projectile.whoAmI);
            Color borderColor = new Color(49, 183, 184);
            while (start.Distance(_nodes[index + 1]) > 10f) {
                float size = 0.5f + Utils.RandomInt(ref seed, 100) / 100f * 0.5f;
                borderColor = new Color(49, 183, 184).MultiplyAlpha(0.75f + 0.25f * Utils.RandomInt(ref seed, 100) / 100f);
                Main.spriteBatch.Draw(text, start - Main.screenPosition, null, borderColor * 0.375f, 0f, text.Size() / 2f, size * 0.25f * scaleOpacity, 0, 0);
                start += start.DirectionTo(_nodes[index + 1]) * 5f;
            }

            if (Scale > 1f && index > 1 && Main.rand.NextChance(0.025)) {
                Dust dust = Dust.NewDustPerfect(_nodes[index], ModContent.DustType<Electric>(), Utils.SafeNormalize(_nodes[index].DirectionTo(_nodes[index + 1]), Vector2.Zero) * 7.5f * Main.rand.NextFloat(0.25f, 1f), 0, Color.White);
                dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                dust.scale *= 0.4f + Main.rand.NextFloatRange(0.15f);
                dust.scale *= 0.925f;
                dust.fadeIn = dust.scale + 0.1f;
                dust.velocity *= 0.25f;
                dust.noGravity = true;
            }

            DrawLightning(Main.spriteBatch, (uint)Seed, _nodes[index], _nodes[index + 1], Scale, 0f, Color.White * scaleOpacity, borderColor.ModifyRGB(0.9f) * 0.9f * scaleOpacity);
        }
        for (int index = 0; index < _nodes.Count - 1; index++) {
            float scaleOpacity = Utils.GetLerpValue(0.375f, 1f, Scale, true);
            Texture2D text = ResourceManager.Bloom;
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
