using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class EvilLeaf : NatureProjectile {
    private const int TIMELEFT = 400;

    private Vector2 _twigPosition;
    private float _ai5, _ai4;
    private float _angle, _angle2;
    private Vector2 _to;
    private int _index;
    private bool _crimson;
    private bool _init;
    private Projectile _parent = null;

    private float[] _oldRotations = new float[18];

    public override string Texture => ResourceManager.FriendlyProjectileTextures + $"Nature/{nameof(EvilLeaf)}";

    public override void SetStaticDefaults() => Projectile.SetTrail(2, 18);

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;
        Projectile.penetrate = 1;
        Projectile.hide = true;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(10, 10);
    }

    internal void SetUpTwigPosition(Vector2 position) {
        _twigPosition = position;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_to);
        writer.Write(_crimson);
        writer.Write(_angle);
        writer.Write(_index);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _to = reader.ReadVector2();
        _crimson = reader.ReadBoolean();
        _angle = reader.ReadSingle();
        _index = reader.ReadInt32();
    }

    public override bool ShouldUpdatePosition() => false;

    //public override bool? CanDamage() => Projectile.Opacity >= 0.35f;

    private void SetPosition(Projectile parent, Vector2? scale = null) {
        Vector2 parentScale = scale != null ? Vector2.One : new(parent.ai[0], parent.ai[1]);
        Vector2 position = parent.position;
        Vector2 myPosition = _twigPosition * parentScale - Vector2.One * 2f - (Projectile.ai[0] == 1f ? new Vector2(10f, 0f) : Vector2.Zero);
        Projectile.position = position + myPosition.RotatedBy(parent.rotation);
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        int num176 = (int)(MathHelper.Pi * 20f);
        _parent ??= Main.projectile.FirstOrDefault(x => x.identity == (int)Projectile.ai[1]);
        Projectile.direction = (int)Projectile.ai[0];
        Projectile parent = _parent;
        if (Projectile.timeLeft > TIMELEFT - 30 * (_index + 1) - 10 && Projectile.ai[2] != -100f) {
            Vector2 parentScale = new(parent.ai[0], parent.ai[1]);
            if (Projectile.localAI[0] == 0f) {
                if (Projectile.owner == Main.myPlayer && Projectile.ai[2] != 0f) {
                    _index = (int)Projectile.ai[2];
                    Projectile.netUpdate = true;
                }
                Projectile.ai[2] = 1f;
                Projectile.localAI[2] = 1.25f;
                Projectile.localAI[1] = Projectile.localAI[2];
                _ai4 = -1f;
            }
            if (Projectile.owner == Main.myPlayer) {
                SetPosition(parent);
                Projectile.netUpdate = true;
            }
            float scaleY = parentScale.Y * Projectile.ai[2];
            float rotation = parent.rotation + MathHelper.Pi * 0.875f * Projectile.direction * (1f - scaleY);
            if (Projectile.owner == Main.myPlayer) {
                if (!_init) {
                    _init = true;
                    Projectile.localAI[0] = 1f;
                    Projectile.rotation = rotation;
                    for (int j = 0; j < _oldRotations.Length; j++) {
                        _oldRotations[j] = Projectile.rotation;
                    }
                    Projectile.velocity = Vector2.Zero;
                    _crimson = parent.ai[2] == 1f;
                    float num175 = ((float)Math.PI + (float)Math.PI * 2f * Main.rand.NextFloat() * 1.5f) * ((float)(-(int)Projectile.ai[0]));
                    float num177 = num175 / (float)num176;
                    if (Math.Abs(num177) >= 0.17f)
                        num177 *= 0.7f;
                    _angle = num177;
                    Projectile.netUpdate = true;
                }
            }
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rotation, 0.2f);
            if (scaleY < 0.5f) {
            }
            else {
                float angle = Math.Sign(_ai5) == Math.Sign(_ai4) ? 4f : 3f;
                if (Math.Abs(_ai5) < 0.5f) {
                    angle *= 0.5f;
                }
                if (Math.Abs(_ai5) < 0.25f) {
                    angle *= 0.25f;
                }
                _ai4 += (float)-Math.Sign(_ai5) * angle;
                _ai4 *= 0.95f;
                _ai5 += _ai4 * TimeSystem.LogicDeltaTime;
                Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], 1f + _ai5, Projectile.localAI[1] * 0.175f);
                Projectile.localAI[1] *= 0.995f;
            }
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.localAI[2], Projectile.ai[2] * 0.215f);
        }
        else {
            updateOldPos();
            if (Projectile.ai[2] != -100f && Projectile.velocity == Vector2.Zero) {
                if (Projectile.owner == Main.myPlayer) {
                    _to = player.GetViableMousePosition();
                    Projectile.netUpdate = true;
                }
                _ai4 = 0f;
                _index = 0;
                Projectile.ai[1] = Projectile.timeLeft;
                Projectile.velocity = Helper.VelocityToPoint(Projectile.position, _to, 0.1f);
                for (int i = 0; i < 3; i++) {
                    if (Main.rand.NextBool(2)) {
                        Vector2 dustVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat() * ((float)Math.PI * 2f) * 0.25f) * (Main.rand.NextFloat() * 3f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, _crimson ? DustID.CrimsonPlants : DustID.CorruptPlants);
                        dust.alpha = 150;
                        dust.velocity += dustVelocity;
                        dust.velocity *= 0.6f + Main.rand.NextFloatRange(0.1f);
                        dust.noGravity = true;
                    }
                }
                float num177 = _angle / (float)num176;
                float num178 = 16f;
                Vector2 vector57 = Vector2.UnitX * num178;
                Vector2 v7 = vector57;
                Vector2 vector55 = _to;
                Vector2 vector56 = Projectile.Center + new Vector2(Main.rand.NextFloatDirection() * (float)Projectile.width / 2f, Projectile.height / 2);
                Vector2 v6 = vector55 - vector56;
                float num179 = v6.Length();
                int num180 = 0;
                while (v7.Length() < num179 && num180 < num176) {
                    num180++;
                    v7 += vector57;
                    vector57 = vector57.RotatedBy(num177);
                }
                Projectile.ai[2] = -100f;
                _angle2 = num180;
            }
            else {
                if (Main.rand.NextBool(11)) {
                    Dust dust2 = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, _crimson ? DustID.CrimsonPlants : DustID.CorruptPlants, 0f, 0f, 150, default(Color), 1f);
                    dust2.noGravity = true;
                    dust2.velocity = Projectile.velocity * 0.5f;
                }
            }
            _ai4 = MathHelper.SmoothStep(_ai4, 1f, 0.1f);
            num176 = (int)(MathHelper.Pi * 20f * _ai4);
            float angle = _angle;
            float angle2 = _angle2;
            Projectile.velocity = Projectile.velocity.RotatedBy(angle);
            float fromValue = num176 - Projectile.timeLeft;
            float fromMax = angle2 + num176 / 3;
            float num4 = Utils.Remap(fromValue, angle2, fromMax, 0f, 1f) * Utils.Remap(fromValue, angle2, angle2 + num176, 1f, 0f);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * (4f + 12f * (1f - num4) * 0.1f);
            updateOldPos();
            Projectile.Opacity = Utils.GetLerpValue(Projectile.timeLeft, 0f, 10f, true);
            Projectile.velocity *= Math.Min(1f, Utils.Remap(_ai4, 0f, 1f, 0.1f, 1f) * 2f);
            Vector2 velocity = Projectile.velocity/* * (int)Projectile.ai[0]*/;
            Projectile.rotation = Helper.SmoothAngleLerp(Projectile.rotation, Helper.VelocityAngle(velocity) - MathHelper.PiOver2 * Projectile.ai[0], MathHelper.Min(1f, 1f * _ai4));
            Projectile.position += velocity;
            Projectile.position += Helper.VelocityToPoint(Projectile.position, _to, 1f);
        }
        //else if (Projectile.owner != Main.myPlayer) {
        //    updateOldPos();
        //}
        //else {
        //    updateOldPos();
        //    Projectile.ai[1] = -20f;
        //    Projectile.ai[2] = Projectile.timeLeft;
        //    Projectile.netUpdate = true;
        //}
        void updateOldPos() {
            for (int num2 = _oldRotations.Length - 1; num2 > 0; num2--) {
                _oldRotations[num2] = _oldRotations[num2 - 1];
            }
            _oldRotations[0] = Projectile.rotation;
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (_to == Vector2.Zero) {
            behindNPCsAndTiles.Add(index);
            return;
        }
        Projectile.hide = false;
        behindNPCsAndTiles.Remove(index);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 5; i++) {
            int ind4 = Dust.NewDust(Projectile.Center - Vector2.One * 4, 8, 8, _crimson ? (ushort)ModContent.DustType<EvilStaff1>() : (ushort)ModContent.DustType<EvilStaff2>(), 0f, 0f, 0, Color.Lerp(default, Color.Black, 0.25f), 1f);
            Main.dust[ind4].velocity *= 0.95f;
            Main.dust[ind4].noGravity = true;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        for (int i = 0; i < 5; i++) {
            int ind4 = Dust.NewDust(Projectile.Center - Vector2.One * 4, 8, 8, _crimson ? (ushort)ModContent.DustType<EvilStaff1>() : (ushort)ModContent.DustType<EvilStaff2>(), 0f, 0f, 0, Color.Lerp(default, Color.Black, 0.25f), 1f);
            Main.dust[ind4].velocity *= 0.95f;
            Main.dust[ind4].noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        SpriteEffects spriteEffects = (SpriteEffects)((int)Projectile.ai[0] != 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new(_crimson ? 14 : 0, 0, 14, 14);
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Vector2 origin = Projectile.ai[0] == 1 ? new Vector2(2, 12) : new Vector2(12, 12);
        if (_to != Vector2.Zero) {
            int num149 = Projectile.oldPos.Length;
            int num147 = 0;
            int num148 = -3;
            for (int num152 = num149; (num148 > 0 && num152 < num147) || (num148 < 0 && num152 > num147); num152 += num148) {
                if (num152 >= Projectile.oldPos.Length)
                    continue;
                float num157 = num147 - num152;
                if (num148 < 0)
                    num157 = num149 - num152;
                Color color32 = color;
                Vector2 vector29 = Projectile.oldPos[num152] + Projectile.Size / 2f;
                color32 *= Lighting.GetColor((vector29).ToTileCoordinates()).ToVector3().Length() / 1.74f;
                color32 *= 1.15f;
                color32 *= num157 / ((float)Projectile.oldPos.Length * 3f);
                Vector2 position3 = vector29 - Main.screenPosition;
                Main.EntitySpriteDraw(texture, position3, sourceRectangle, color32, _oldRotations[num152], origin, Projectile.scale, spriteEffects);
            }
        }
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        return false;
    }
}
