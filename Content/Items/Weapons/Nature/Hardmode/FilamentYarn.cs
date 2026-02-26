using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Items.Weapons.Magic.Hardmode;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class FilamentYarn : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(46, 48);
        Item.SetWeaponValues(100, 5f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, autoReuse: false, showItemOnUse: false);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<FilamentYarn_Use>());
        Item.SetShopValues(ItemRarityColor.StrongRed10, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 200);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);

        Item.channel = true;
    }

    public sealed class FilamentYarn_Use : ModProjectile {
        private static float MAXLENGTH => 200f;
        private static ushort LINECOUNT => 5;

        public override string Texture => ItemLoader.GetItem(ModContent.ItemType<FilamentYarn>()).Texture;

        private (Vector2, Vector2?)[] _connectPoints = null!;
        private Vector2 _mousePosition;

        public ref float InitValue => ref Projectile.localAI[0];
        public ref float Cooldown => ref Projectile.ai[1];
        public ref float CurrentLength => ref Projectile.ai[0];

        public bool Init {
            get => InitValue != 0f;
            set => InitValue = value.ToInt();
        }

        public override bool? CanDamage() => true;
        public override bool? CanCutTiles() => false;
        public override bool ShouldUpdatePosition() => false;

        public override void SetDefaults() {
            Projectile.SetSizeValues(10);
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.penetrate = -1;

            //Projectile.hide = true;
        }

        private void DrawConnectedLines() {
            SpriteBatch batch = Main.spriteBatch;
            Player owner = Projectile.GetOwnerAsPlayer();
            int index = 0;
            foreach ((Vector2, Vector2?) connectedPoints in _connectPoints) {
                index++;
                if (index > LINECOUNT + 1) {
                    continue;
                }
                Vector2 from = connectedPoints.Item1;
                if (from == Vector2.Zero) {
                    continue;
                }
                Vector2 to = connectedPoints.Item2 ?? _mousePosition;
                float distance = from.Distance(to);
                float length = MAXLENGTH;
                float currentLength = MathF.Max(0f, MathF.Min(length, length - distance * 1f));
                float sineX = TimeSystem.TimeForVisualEffects * 0f,
                      sineY = TimeSystem.TimeForVisualEffects * 0f;
                SimpleCurve curve = new(from, to, Vector2.Zero);
                curve.Control = (curve.Begin + curve.End) / 2f + new Vector2(0f, currentLength) + new Vector2(MathF.Sin(sineX), MathF.Sin(sineY)) * 20f;
                Vector2 start = curve.Begin;
                int count = 16;
                for (int i = 1; i <= count; i++) {
                    Vector2 point = curve.GetPoint(i / (float)count);
                    batch.Line(start, point, Color.White);
                    start = point;
                }
            }
        }

        private void AddPoint(Vector2 position) {
            ref float currentIndex = ref Projectile.ai[2];
            if (_connectPoints.Length < LINECOUNT + 2) {
                Array.Resize(ref _connectPoints, _connectPoints.Length + 1);
            }
            if (currentIndex > LINECOUNT) {
                return;
            }
            _connectPoints[(int)currentIndex].Item2 = position;
            _connectPoints[(int)(++currentIndex)] = (position, null);
        }

        public override void AI() {
            Player player4 = Projectile.GetOwnerAsPlayer();

            if (player4.IsLocal()) {
                Vector2 mousePosition = player4.GetViableMousePosition();
                _mousePosition = Vector2.Lerp(_mousePosition, mousePosition, 0.25f);
                if (Init) {
                    if (Projectile.ai[2] > 0) {
                        Vector2 last = _connectPoints[(int)Projectile.ai[2] - 1].Item2!.Value;
                        if (_mousePosition.Distance(last) > MAXLENGTH) {
                            _mousePosition = last + last.DirectionTo(mousePosition) * MAXLENGTH;
                        }
                    }
                }
            }

            if (!Init) {
                Init = true;

                if (player4.IsLocal()) {
                    _mousePosition = Vector2.Lerp(_mousePosition, player4.GetViableMousePosition(), 1f);
                }

                _connectPoints = new (Vector2, Vector2?)[1];
                AddPoint(_mousePosition);
            }
            else {
                void addPoint() {
                    AddPoint(_mousePosition);
                }
                if (Main.mouseLeft && Main.mouseLeftRelease) {
                    addPoint();
                }

                if (Cooldown <= 0f) {
                    foreach ((Vector2, Vector2?) connectedPoints in _connectPoints) {
                        if (connectedPoints.Item2 is not null) {
                            continue;
                        }
                        Vector2 from = connectedPoints.Item1;
                        Vector2 to = _mousePosition;
                        float distance = from.Distance(to);
                        float length = MAXLENGTH;
                        CurrentLength = distance;
                        if (CurrentLength >= length) {
                            addPoint();
                            break;
                        }
                    }
                }
                else {
                    Cooldown = Helper.Approach(Cooldown, 0f, 1f);
                }
            }

            if (player4.noItems || !player4.IsAliveAndFree()) {
                Projectile.Kill();
            }

            Player player = Main.player[Projectile.owner];

            int owner = Projectile.owner;
            ref Vector2 velocity = ref Projectile.velocity;
            float scale = Projectile.scale;
            Vector2 vector21 = Main.player[owner].GetPlayerCorePoint();
            if (Projectile.ai[2] < LINECOUNT + 1) {
                Projectile.timeLeft = 30;
                if (Main.myPlayer == owner) {
                    float num = (float)Math.PI / 2f;
                    Vector2 vector = vector21;
                    int num2 = 2;
                    float num3 = 0f;
                    float num8 = 1f * Projectile.scale;
                    Vector2 vector3 = vector;
                    Vector2 value = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector3;
                    if (player.gravDir == -1f)
                        value.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector3.Y;


                    Vector2 vector4 = Vector2.Normalize(value);
                    if (float.IsNaN(vector4.X) || float.IsNaN(vector4.Y))
                        vector4 = -Vector2.UnitY;

                    vector4 = Vector2.Normalize(Vector2.Lerp(vector4, Vector2.Normalize(Projectile.velocity), 0f));
                    vector4 *= num8;
                    if (vector4.X != Projectile.velocity.X || vector4.Y != Projectile.velocity.Y)
                        Projectile.netUpdate = true;

                    Projectile.velocity = vector4;
                }
            }
            else {

            }

            if (velocity.X > 0f)
                Main.player[owner].ChangeDir(1);
            else if (velocity.X < 0f)
                Main.player[owner].ChangeDir(-1);

            Projectile.spriteDirection = Projectile.direction;
            Main.player[owner].ChangeDir(Projectile.direction);
            Main.player[owner].heldProj = Projectile.whoAmI;
            Main.player[owner].SetDummyItemTime(2);
            Projectile.position.X = vector21.X - (float)(Projectile.width / 2);
            Projectile.position.Y = vector21.Y - (float)(Projectile.height / 2);
            Projectile.rotation = (float)(Math.Atan2(velocity.Y, velocity.X) + 1.5700000524520874) - MathHelper.PiOver2;
            if (Main.player[owner].direction == 1)
                Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);
            else
                Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);

            Projectile.Center = Utils.Floor(Projectile.Center);
            //Projectile.position -= velocity.SafeNormalize() * 14f;
            //Projectile.position -= velocity.TurnLeft().SafeNormalize() * 2f * -player.direction * player.gravDir;
        }

        public override bool PreDraw(ref Color lightColor) {
            var texture = Projectile.GetTexture();

            Player player = Projectile.GetOwnerAsPlayer();

            var pos = Projectile.Center - Main.screenPosition;
            var effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically : Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            if (player.gravDir < 0) {
                effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            }

            float rotation = player.itemRotation + 0.785f * (float)player.direction;

            Vector2 origin = new Vector2(0f, texture.Height);
            if (player.gravDir == -1f) {
                if (player.direction == -1) {
                    rotation += MathHelper.Pi + MathHelper.PiOver2;
                    origin = new Vector2(0f, texture.Height);
                }
                else {
                    rotation -= 1.57f;
                    origin = Vector2.Zero;
                }
            }
            else if (player.direction == -1) {
                rotation += MathHelper.Pi;
                origin = Vector2.Zero;
            }

            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, effects);

            DrawConnectedLines();

            return false;
        }
    }
}
