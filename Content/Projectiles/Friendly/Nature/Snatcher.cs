using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Druid.Wreath;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;

using static Terraria.GameContent.Animations.Actions.Sprites;

namespace RoA.Content.Projectiles.Friendly.Nature;

// TODO: optimize (rewrite)
sealed class Snatcher : NatureProjectile {
    private const float DIST = 75f;
    private const ushort TIMELEFT = 300;

    private Vector2 _targetVector, _targetVector2, _attackVector;
    private float _rotation;
    private Vector2 _mousePos, _mousePos2;
    private float _lerpValue;
    private int _alpha = 255;
    private int _startDirection;
    private bool _shouldPlayAttackSound, _shouldPlayAttackSound2;
    private int _timeLeft;
    private bool _setScale;

    private Vector2[] _oldPositions = new Vector2[18];

    private bool IsAttacking => Projectile.ai[2] > 1f;
    private Vector2 AttackPos => new(Projectile.localAI[1], Projectile.localAI[2]);
    private float AttackFactor => IsAttacking ? Math.Min(1f, Helper.EaseInOut3(1f - ((Projectile.ai[2] - 1f) / 4f)) + _attackVector.Length() * 0.01f) : 0f;
    private bool IsAttacking2 => _attackVector.Length() > 25f;
    private bool IsAttacking3 => _attackVector.Length() > 10f;

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.WriteVector2(_mousePos);
        writer.WriteVector2(_mousePos2);
        writer.Write(Projectile.localAI[1]);
        writer.Write(Projectile.localAI[2]);
        writer.Write(_rotation);
        writer.Write(_startDirection);
        writer.Write(_timeLeft);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _mousePos = reader.ReadVector2();
        _mousePos2 = reader.ReadVector2();
        Projectile.localAI[1] = reader.ReadSingle();
        Projectile.localAI[2] = reader.ReadSingle();
        _rotation = reader.ReadSingle();
        _startDirection = reader.ReadInt32();
        _timeLeft = reader.ReadInt32();
    }

    public override void SetStaticDefaults() {

    }

    protected override void SafeSetDefaults() {
        int width = 20, height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;

        Projectile.tileCollide = false;
        Projectile.friendly = true;

        Projectile.aiStyle = -1;

        Projectile.timeLeft = TIMELEFT * 2;
        Projectile.penetrate = -1;

        ShouldChargeWreathOnDamage = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;

        Projectile.netImportant = true;

        Projectile.Opacity = 1f;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Main.player[Projectile.owner].GetWreathHandler().OnWreathReset += OnReset;
        for (int j = 0; j < _oldPositions.Length; j++) {
            _oldPositions[j] = Vector2.Zero;
        }
    }

    public override void OnKill(int timeLeft) {
        Main.player[Projectile.owner].GetWreathHandler().OnWreathReset -= OnReset;
        Dusts();
    }

    private void Dusts() {
        for (int i = 0; i < 10; i++) {
            Vector2 vector39 = GetCenter();
            Dust obj2 = Main.dust[Dust.NewDust(vector39, Projectile.width, Projectile.height, DustID.JunglePlants, 0f, 0f, 0, default, 1.15f + 0.15f * Main.rand.NextFloat())];
            obj2.noGravity = true;
            obj2.fadeIn = 0.5f;
            obj2.noLight = true;
        }
        int direction = (int)Projectile.ai[1];
        int height = 18;
        Vector2 vector = GetPos().Floor();
        Vector2 position = Projectile.Center;
        var velocity = (Projectile.rotation - (direction == 1 ? MathHelper.Pi : 0)).ToRotationVector2();
        Vector2 baseValue = Vector2.Normalize(velocity.RotatedBy(MathHelper.PiOver2 * direction));
        Vector2 value = baseValue;
        float distance = Vector2.Distance(vector, position);
        for (int i = 0; i < 250; i++) {
            if (distance < height) {
                break;
            }
            for (int i2 = 0; i2 < 5; i2++) {
                Vector2 vector39 = vector - Vector2.One * 4;
                Dust obj2 = Main.dust[Dust.NewDust(vector39, 8, 8, DustID.JunglePlants, 0f, 0f, 0, default, 1f + 0.1f * Main.rand.NextFloat())];
                obj2.velocity *= 0.5f;
                obj2.noGravity = true;
                obj2.fadeIn = 0.5f;
                obj2.noLight = true;
            }
            Vector2 to = position - Vector2.Normalize(_targetVector2) * -direction;
            distance = Vector2.Distance(vector, to);
            vector += value.SafeNormalize(Vector2.Zero) * height;
            Vector2 to2 = Helper.VelocityToPoint(vector, to, 1f).RotatedBy(-0.2f * direction * _lerpValue);
            Vector2 value3 = to2.SafeNormalize(Vector2.Zero) * 2f;
            float lerpAmount = Math.Max(0f, 0.25f - Math.Max((i - 10) * 0.01f, 0));
            value = Vector2.Lerp(value, value3, lerpAmount + _lerpValue);
        }
    }

    private Vector2 GetCenter() {
        return GetPos() - Projectile.Size / 2f + (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * Projectile.height * 0.75f;
    }

    private void OnReset() {
        Player player = Main.player[Projectile.owner];
        if (player.GetSelectedItem().ModItem is not ThornyClaws) {
            return;
        }
        if (Projectile.timeLeft > 20) {
            Projectile.timeLeft += TIMELEFT;
        }
        _timeLeft = Projectile.timeLeft;
        Projectile.netUpdate = true;
    }

    public override bool? CanCutTiles() => false;

    private bool IsValid => true/*Projectile.alpha == 0 || Projectile.Opacity <= 0f*/;

    private void SnatcherCutTiles() {
        if (!IsValid) {
            return;
        }
        if (Projectile.owner != Main.myPlayer) {
            return;
        }
        Vector2 boxPosition = GetCenter();
        int boxWidth = Projectile.width;
        int boxHeight = Projectile.height;
        int num = (int)(boxPosition.X / 16f);
        int num2 = (int)((boxPosition.X + (float)boxWidth) / 16f) + 1;
        int num3 = (int)(boxPosition.Y / 16f);
        int num4 = (int)((boxPosition.Y + (float)boxHeight) / 16f) + 1;
        if (num < 0)
            num = 0;

        if (num2 > Main.maxTilesX)
            num2 = Main.maxTilesX;

        if (num3 < 0)
            num3 = 0;

        if (num4 > Main.maxTilesY)
            num4 = Main.maxTilesY;

        bool[] tileCutIgnorance = Main.player[Projectile.owner].GetTileCutIgnorance(allowRegrowth: false, Projectile.trap);
        for (int i = num; i < num2; i++) {
            for (int j = num3; j < num4; j++) {
                if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
                    WorldGen.KillTile(i, j);
                    if (Main.netMode != 0)
                        NetMessage.SendData(17, -1, -1, null, 0, i, j);
                }
            }
        }
    }

    public override bool? CanDamage() => IsValid;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!IsAttacking) {
            return false;
        }

        return Collision.CheckAABBvAABBCollision(GetCenter() + (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * Projectile.height * 0.25f, Projectile.Size * Projectile.scale, targetHitbox.Location.ToVector2(), targetHitbox.Size());
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(20, Main.rand.Next(90, 181));
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(20, Main.rand.Next(90, 181));
    }

    private void ResetAttackState() {
        Projectile.ai[2] = 1f;
        Projectile.localAI[1] = Projectile.localAI[2] = 0f;
    }

    private bool CantAttack() {
        Player player = Main.player[Projectile.owner];
        Vector2 mousePos = Helper.GetLimitedPosition(player.GetPlayerCorePoint(), _mousePos, 200f, DIST * 0.75f);
        return GetPos().Distance(mousePos) < 40f;
    }

    public override void SafePostAI() {
        if (_timeLeft > 0) {
            _timeLeft--;
            Projectile.timeLeft = _timeLeft;
        }

        for (int num2 = _oldPositions.Length - 1; num2 > 0; num2--) {
            _oldPositions[num2] = _oldPositions[num2 - 1];
        }
        _oldPositions[0] = GetPos();

        _lerpValue = MathHelper.Lerp(_lerpValue, AttackFactor, 0.1f);
        if (Projectile.timeLeft < 20) {
        }
        else {
            _alpha -= 65;
            if (_alpha < 0)
                _alpha = 0;
        }
        if (IsAttacking && IsValid) {
            Vector2 velocity = _attackVector.SafeNormalize(Vector2.One) * _attackVector.Length() * 0.01f;
            for (int num78 = 0; num78 < 2; num78++) {
                if (Main.rand.Next(10) == 0) {
                    Dust obj = Main.dust[Dust.NewDust(GetCenter(), Projectile.width, Projectile.height, DustID.JunglePlants, -velocity.X, -velocity.Y, 0, default, 0.9f)];
                    obj.noGravity = true;
                    obj.velocity *= 2f;
                    obj.fadeIn = 1.5f;
                }
            }

            float num79 = 18f;
            for (int num80 = 0; (float)num80 < num79; num80++) {
                if (Main.rand.Next((int)num79) == 0) {
                    Vector2 vector39 = GetCenter() - _attackVector.SafeNormalize(Vector2.Zero) * ((float)num80 / num79);
                    Dust obj2 = Main.dust[Dust.NewDust(vector39, Projectile.width, Projectile.height, DustID.JunglePlants, -velocity.X, -velocity.Y, 0, default, 0.9f)];
                    obj2.noGravity = true;
                    obj2.fadeIn = 0.5f;
                    obj2.noLight = true;
                }
            }
        }
        SnatcherCutTiles();
        Player player = Main.player[Projectile.owner];
        bool flag = false;
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner == Projectile.owner && projectile.type == Type) {
                if (projectile.ai[0] != (Projectile.whoAmI + 1f)) {
                    if (projectile.ai[2] > 3f) {
                        flag = true;
                    }
                }
                else if (projectile.As<Snatcher>().CantAttack()) {
                    flag = true;
                }
                break;
            }
        }
        if (CantAttack()) {
            flag = true;
        }
        if (!flag && player.ownedProjectileCounts[Type] > 1 && Projectile.ai[0] == 1f) {
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner == Projectile.owner && projectile.type == Type &&
                    Projectile.whoAmI != projectile.whoAmI) {
                    if (projectile.ai[2] > 3f) {
                        flag = true;
                    }
                    break;
                }
            }
        }
        if (Projectile.owner == Main.myPlayer &&
            player.itemAnimation > player.itemAnimationMax - player.itemAnimationMax / 2 && !flag && !IsAttacking && !IsAttacking2) {
            Projectile.ai[2] = 5f;

            Vector2 mousePos = Helper.GetLimitedPosition(player.GetPlayerCorePoint(), _mousePos, 200f, DIST * 0.75f);
            Projectile.localAI[1] = mousePos.X;
            Projectile.localAI[2] = mousePos.Y;
            Projectile.netUpdate = true;
        }
        if (IsAttacking) {
            if (Projectile.ai[2] >= 4f && !_shouldPlayAttackSound2) {
                _shouldPlayAttackSound = true;
                _shouldPlayAttackSound2 = true;
            }
            if (_shouldPlayAttackSound) {
                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "SnatcherBite") { Volume = 0.75f }, player.GetPlayerCorePoint());
                _shouldPlayAttackSound = false;
            }

            Projectile.ai[2] -= TimeSystem.LogicDeltaTime * 5f;
            Vector2 mousePos = AttackPos;
            mousePos += Helper.VelocityToPoint(GetPos(), mousePos, (mousePos - GetPos()).Length() * 0.1f);
            Vector2 pos = mousePos - GetPos();
            Vector2 to = pos.SafeNormalize(Vector2.Zero);
            _attackVector += to + to * (pos.Length() * 0.04f);
            if (GetPos().Distance(AttackPos) < 15f) {
                ResetAttackState();
            }
        }
        else {
            _shouldPlayAttackSound2 = _shouldPlayAttackSound = false;
            _attackVector = Vector2.Lerp(_attackVector, Vector2.Zero, 0.03f);
            _attackVector *= 0.97f;
        }
    }

    private Vector2 GetPos() {
        int direction = (int)Projectile.ai[1];
        float progress = 0.5f;
        Player player = Main.player[Projectile.owner];
        Vector2 playerCenter = player.GetPlayerCorePoint();
        Vector2 drawPosition = playerCenter + Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.PiOver2 * direction)) * DIST;
        Vector2 endLocation = playerCenter + _targetVector2 + Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.PiOver2 * direction)) * Math.Max(DIST * (1f - progress), 8f);
        Vector2 result = Vector2.Lerp(drawPosition, endLocation, progress) + _attackVector;
        //if (player.Distance(result) > 200f) {
        //    ResetAttackState();
        //    _attackVector = Vector2.Zero;
        //}
        if (Projectile.Opacity > 0f) {
            Vector2 to = Vector2.Lerp(playerCenter, result, MathHelper.Clamp(result.Length() * (1f - Projectile.Opacity), 0f, 1f));
            if (player.Distance(result) > 200f) {
                to = player.GetPlayerCorePoint();
                ResetAttackState();
                _attackVector = Vector2.Zero;
            }
            return to;
        }
        return result;
    }

    private Vector2 GetLookUpPos() {
        Player player = Main.player[Projectile.owner];
        Vector2 playerCenter = player.GetPlayerCorePoint();
        int mouseDir = (_mousePos - playerCenter).X.GetDirection();
        return IsAttacking ? AttackPos : (playerCenter + Vector2.UnitX * 50f * mouseDir);
    }

    public override void AI() {
        if (Projectile.owner == Main.myPlayer && _startDirection == 0) {
            _startDirection = Main.player[Projectile.owner].direction;
            Projectile.netUpdate = true;
        }

        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead) {
            Projectile.Kill();
        }
        if (Projectile.Opacity > 0f) {
            Projectile.Opacity -= 0.1f;
        }
        Vector2 playerCenter = player.GetPlayerCorePoint();
        Projectile.direction = player.direction;
        Projectile.Center = playerCenter;
        if (Projectile.ai[0] == 0f) {
            bool flag = player.ownedProjectileCounts[Type] < 1;
            float ai1 = flag.ToDirectionInt();
            Projectile.ai[1] = ai1 * -player.direction;
            if (!flag) {
                if (Projectile.owner == Main.myPlayer) {
                    foreach (Projectile projectile in Main.ActiveProjectiles) {
                        if (projectile.owner == Projectile.owner && projectile.type == Type && projectile.whoAmI != Projectile.whoAmI) {
                            Projectile.ai[0] = projectile.whoAmI + 1f;
                            if (_startDirection * Projectile.ai[1] == projectile.As<Snatcher>()._startDirection * projectile.ai[1]) {
                                Projectile.ai[1] *= -1;
                            }
                            Projectile.netUpdate = true;
                        }
                    }
                }
            }
            else {
                Projectile.ai[0] = 1f;
            }
            Projectile.ai[2] = 0f;
        }
        if (player.ownedProjectileCounts[Type] == 1 && Projectile.ai[0] != 1f) {
            Projectile.ai[0] = 1f;
        }
        if (Projectile.owner == Main.myPlayer) {
            _mousePos = player.GetViableMousePosition();
            _mousePos2 = new(Main.mouseX - Main.screenWidth / 2f, Main.mouseY - Main.screenHeight / 2f);
            Projectile.netUpdate = true;
        }
        int direction = (int)Projectile.ai[1];
        Vector2 pos = GetPos();
        Vector2 lookUpPos = GetLookUpPos();
        float rotation = Vector2.Normalize(pos - lookUpPos).ToRotation() + MathHelper.PiOver2;
        if (Projectile.ai[2] == 0f) {
            Projectile.ai[2] = 1f;
            _targetVector = _targetVector2 = _attackVector = Vector2.Zero;
            _rotation = rotation;
        }
        _targetVector.X = _mousePos2.X;
        _targetVector.Y = _mousePos2.Y;
        float maxLength = DIST;
        if (_targetVector.Length() > maxLength) {
            _targetVector = Vector2.Normalize(_targetVector) * maxLength;
        }
        Vector2 target = _targetVector;
        Vector2 target2 = Helper.VelocityToPoint(pos, _mousePos, 1f);
        float lerp = Math.Min(0.015f + player.velocity.Length() * 0.001f, 0.1f) * 0.5f;
        if (target2.Length() > 1f && _attackVector.Length() < 1f) {
            Projectile.velocity = Helper.SmoothAngleLerp(Projectile.velocity.ToRotation(), target2.ToRotation(), lerp * 3.5f).ToRotationVector2().SafeNormalize(Vector2.Zero);
        }
        _targetVector2.X = Helper.Approach(_targetVector2.X, target.X, lerp);
        _targetVector2.Y = Helper.Approach(_targetVector2.Y, target.Y, lerp * 0.1f);
        _rotation = Utils.AngleLerp(_rotation, rotation, 0.05f - (0.04f * (_attackVector.Length() < 50f ? (_attackVector.Length() - 25f) / 25f : 1f)) + (IsAttacking ? Math.Min(1f, _lerpValue * 2f) : 0f));
        Projectile.rotation = _rotation;

        if (IsAttacking) {
            if (++Projectile.frameCounter > 4) {
                Projectile.frame++;
                if (Projectile.frame > 2) {
                    Projectile.frame = 0;
                }
                Projectile.frameCounter = 0;
            }
        }
        else {
            Projectile.frame = 0;
        }

        if (!_setScale) {
            _setScale = true;

            float scale = Projectile.GetOwnerAsPlayer().CappedMeleeOrDruidScale();
            Projectile.scale = scale;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Vector2 drawPosition = GetPos();
        int direction = (int)Projectile.ai[1];
        direction *= (drawPosition - Main.player[Projectile.owner].Center).X.GetDirection() * _startDirection;

        SpriteEffects effects = direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Vector2 mountedCenter = Main.player[Projectile.owner].GetPlayerCorePoint();
        float opacity = 1f - (_alpha / 255f);
        Color color = Color.White * opacity;
        Vector2 position = Projectile.position;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Color alpha = Projectile.GetAlpha(color);
        float num = _targetVector2.Length() + 16f;
        bool flag = num < 100f;
        int width = 42;
        Vector2 value = Vector2.Normalize(_targetVector2);
        Rectangle rectangle = new Rectangle(0, 0, width, 36);
        Vector2 value2 = new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
        float rotation = Projectile.rotation;

        Vector2 vector = drawPosition;
        position = Projectile.Center;
        var velocity = (Projectile.rotation - (direction == 1 ? MathHelper.Pi : 0)).ToRotationVector2();
        Vector2 baseValue = Vector2.Normalize(velocity.RotatedBy(MathHelper.PiOver2 * direction));
        value = baseValue;
        rectangle = new Rectangle(0, 62, width, 18);
        float distance = Vector2.Distance(vector, position);
        for (int i = 0; i < 250; i++) {
            if (distance < rectangle.Height * Projectile.scale) {
                break;
            }
            ulong randomSeed = (ulong)i;
            int useFrame = Utils.RandomInt(ref randomSeed, 10);
            if (useFrame < 3) {
                rectangle = new Rectangle(0, 40, width, 20);
            }
            else {
                rectangle = new Rectangle(0, 62, width, 18);
            }
            Vector2 lightPos = vector;
            color = Lighting.GetColor((int)lightPos.X / 16, (int)lightPos.Y / 16) * opacity;
            Main.spriteBatch.Draw(texture, vector - Main.screenPosition, new Rectangle?(rectangle), color, value.ToRotation() - MathHelper.PiOver2, new Vector2((rectangle.Width / 2), 0f), Projectile.scale, SpriteEffects.None, 0f);
            Vector2 to = position - Vector2.Normalize(_targetVector2) * -direction;
            distance = Vector2.Distance(vector, to);
            vector += value.SafeNormalize(Vector2.Zero) * rectangle.Height;
            Vector2 to2 = Helper.VelocityToPoint(vector, to, 1f).RotatedBy(-0.2f * direction * _lerpValue);
            Vector2 value3 = to2.SafeNormalize(Vector2.Zero) * 2f;
            float lerpAmount = Math.Max(0f, 0.25f - Math.Max((i - 10) * 0.01f, 0));
            value = Vector2.Lerp(value, value3, lerpAmount + _lerpValue);
        }
        Vector2 pos = GetPos() - Projectile.Size / 2f;
        color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * opacity;
        Rectangle frame = new(Projectile.frame * width, 84, width, 30);

        Vector2 origin = new(width / 2f, 0f);

        int num149 = _oldPositions.Length;
        int num147 = 0;
        int num148 = -3;
        if (IsAttacking) {
            for (int num152 = num149; (num148 > 0 && num152 < num147) || (num148 < 0 && num152 > num147); num152 += num148) {
                if (num152 >= Projectile.oldPos.Length)
                    continue;
                float num157 = num147 - num152;
                if (num148 < 0)
                    num157 = num149 - num152;
                Color color32 = color;
                color32 *= num157 / ((float)_oldPositions.Length * 1.5f);
                color32 *= _attackVector.Length() * 0.0035f;
                Vector2 vector29 = _oldPositions[num152];
                float num158 = Projectile.rotation;
                SpriteEffects effects2 = effects;

                Vector2 position3 = vector29 - Main.screenPosition;
                Main.EntitySpriteDraw(texture, position3, frame, color32, num158, origin, Projectile.scale, effects2);
            }
        }

        pos = GetPos();
        color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * opacity;
        Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, color, Projectile.rotation, origin, Projectile.scale, effects, 0);

        return false;
    }
}
