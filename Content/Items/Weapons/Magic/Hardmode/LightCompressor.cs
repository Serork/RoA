using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic.Hardmode;

sealed class LightCompressor : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(64, 26);
        Item.DefaultToRangedWeapon(ModContent.ProjectileType<LightCompressor_Use>(), AmmoID.None, 10, 5f);
        Item.knockBack = 6.5f;
        Item.UseSound = null;
        Item.damage = 14;
        Item.value = Item.buyPrice(0, 35);
        Item.rare = 3;

        Item.DamageType = DamageClass.Magic;

        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.channel = true;
    }

    [Tracked]
    public class LightCompressor_Use : ModProjectile {
        private static float MAXDISTANCETOTARGETINPIXELS => 600f;

        private static Asset<Texture2D> _lightTexture = null!;

        private List<ushort> _targets = null!;

        public ref float SpawnValue => ref Projectile.localAI[1];

        public override string Texture => ItemLoader.GetItem(ModContent.ItemType<LightCompressor>()).Texture;

        public override void Load() {
            On_Main.DrawNPCs += On_Main_DrawNPCs;
        }

        private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
            Projectile? projectile = TrackedEntitiesSystem.GetSingleTrackedProjectile<LightCompressor_Use>();
            projectile?.As<LightCompressor_Use>().DrawLightLines();

            orig(self, behindTiles);
        }

        public override void SetStaticDefaults() {
            if (Main.dedServ) {
                return;
            }

            _lightTexture = ModContent.Request<Texture2D>(Texture + "_Light");
        }

        public override void SetDefaults() {
            Projectile.SetSizeValues(10);
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            //Projectile.hide = true;
        }

        //public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        //    behindNPCs.Add(index);
        //}

        public override bool PreDraw(ref Color lightColor) {
            if (SpawnValue == 0f) {
                return false;
            }

            var texture = Projectile.GetTexture();

            Player player = Projectile.GetOwnerAsPlayer();

            var pos = Projectile.Center - Main.screenPosition;
            var effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically : Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            if (player.gravDir < 0) {
                effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            }

            float rotation = Projectile.rotation;
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor), rotation, texture.Frame().Left(), Projectile.scale, effects);
      
            return false;
        }

        public void DrawLightLines() {
            Player player = Projectile.GetOwnerAsPlayer();
            

            void drawMainLightLine() {
                SpriteBatch batch = Main.spriteBatch;
                Vector2 baseStartPosition = Projectile.Center;
                Vector2 startPosition = baseStartPosition;
                Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize();

                float waveValue = TimeSystem.TimeForVisualEffects * 4f % 3f;
                startPosition += normalizedVelocity * 20f * waveValue;

                startPosition += normalizedVelocity.TurnLeft() * 3f * player.direction * player.gravDir;

                float offsetValue = 60f;
                startPosition += normalizedVelocity * offsetValue;
                Vector2 startPosition2 = startPosition - normalizedVelocity * 46f;
                Vector2 endPosition = startPosition2 + normalizedVelocity * 1200f;
                int i = 0;
                while (true) {
                    i++;
                    Texture2D texture = _lightTexture.Value;
                    float step = texture.Height;
                    float distance = Vector2.Distance(startPosition2, endPosition),
                          distance2 = Vector2.Distance(startPosition2, baseStartPosition);
                    if (distance < step * 1f) {
                        break;
                    }
                    Vector2 position = startPosition2;
                    if (WorldGenHelper.SolidTileNoPlatform(position.ToTileCoordinates())) {
                        break;
                    }
                    Vector2 velocity = startPosition2.DirectionTo(endPosition);
                    float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                    Rectangle clip = texture.Bounds;
                    float waveValue2 = Helper.Wave(0.5f, 1.25f, 20f, i * 15f + Projectile.whoAmI);
                    clip.Height = (int)(clip.Height * waveValue2);
                    Vector2 origin = clip.Centered();
                    Color color = Color.White.MultiplyAlpha(0.75f);
                    Vector2 scale = new(1f, 0.5f);
                    DrawInfo drawInfo = new() {
                        Clip = clip,
                        Origin = origin,
                        Rotation = rotation,
                        Color = color,
                        Scale = scale
                    };
                    if (distance2 > step * 1f) {
                        batch.Draw(texture, position, drawInfo);
                    }
                    startPosition2 += velocity.SafeNormalize() * step;
                }
            }

            void drawLightLine(Vector2 targetPosition) {
                SpriteBatch batch = Main.spriteBatch;
                Vector2 startPosition = Projectile.Center;
                Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize();

                startPosition += normalizedVelocity.TurnLeft() * 3f * player.direction * player.gravDir;

                Vector2 startPosition2 = startPosition;
                float offsetValue = 60f;
                startPosition += normalizedVelocity * offsetValue;
                startPosition2 += normalizedVelocity * offsetValue * 1.25f;
                Vector2 endPosition = targetPosition;
                Vector2 velocity = startPosition.DirectionTo(startPosition2) * 10f;
                float lerpValue = 0.1f;
                while (true) {
                    Texture2D texture = _lightTexture.Value;
                    float step = texture.Height;
                    float distance = Vector2.Distance(startPosition, endPosition);
                    if (distance < step * 1f) {
                        break;
                    }
                    float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                    Rectangle clip = texture.Bounds;
                    Vector2 origin = clip.Centered();
                    Color color = Color.White.MultiplyAlpha(0.75f);
                    DrawInfo drawInfo = new() {
                        Clip = clip,
                        Origin = origin,
                        Rotation = rotation,
                        Color = color
                    };
                    Vector2 position = startPosition;
                    batch.Draw(texture, position, drawInfo);
                    startPosition += velocity.SafeNormalize() * step;
                    velocity = Vector2.Lerp(velocity, startPosition.DirectionTo(endPosition), lerpValue);
                    float length = (startPosition2 - endPosition).Length();
                    float maxLength = MAXDISTANCETOTARGETINPIXELS;
                    if (length > maxLength) {
                        break;
                    }
                    float factor = 1f;
                    factor -= length / maxLength;
                    factor = MathF.Max(0.01f, factor);
                    float minDistance = maxLength / 2f;
                    if (distance < minDistance) {
                        float distanceFactor = 1f - distance / minDistance;
                        distanceFactor *= 1f;
                        factor += distanceFactor;
                    }
                    float maxLerpValue = factor * 0.25f;
                    lerpValue = Helper.Approach(lerpValue, maxLerpValue, TimeSystem.LogicDeltaTime * factor);
                }
            }

            drawMainLightLine();
            foreach (ushort targetWhoAmI in _targets) {
                NPC npc = Main.npc[targetWhoAmI];
                Vector2 targetPosition = npc.Center + Vector2.UnitY * npc.gfxOffY;
                drawLightLine(targetPosition);
            }
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;
        public override bool ShouldUpdatePosition() => false;

        public override void AI() {
            if (SpawnValue == 0f) {
                SpawnValue = 1f;

                _targets = [];
            }

            Vector2 startPosition = Projectile.Center;
            Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize();
            Vector2 endPosition = startPosition + normalizedVelocity * 600f;
            while (true) {
                float distance = Vector2.Distance(startPosition, endPosition);
                float step = 12f;
                if (distance < step * 1f) {
                    break;
                }

                Vector2 velocity2 = startPosition.DirectionTo(endPosition);

                foreach (NPC npc in Main.ActiveNPCs) {
                    if (!npc.CanBeChasedBy()) {
                        continue;
                    }
                    ushort whoAmI = (ushort)npc.whoAmI;
                    if (npc.Distance(startPosition) > step) {
                        continue;
                    }
                    if (_targets.Contains(whoAmI)) {
                        continue;
                    }
                    _targets.Add(whoAmI);
                }

                startPosition += velocity2 * step;
            }

            //foreach (NPC npc in Main.ActiveNPCs) {
            //    ushort whoAmI = (ushort)npc.whoAmI;
            //    if (Projectile.Distance(npc.Center) > MAXDISTANCETOTARGETINPIXELS) {
            //        continue;
            //    }
            //    if (_targets.Contains(whoAmI)) {
            //        continue;
            //    }
            //    _targets.Add(whoAmI);
            //}
            //for (int i = 0; i < _targets.Count; i++) {
            //    ushort whoAmI = _targets[i];
            //    NPC npc = Main.npc[whoAmI];
            //    if (Projectile.Distance(npc.Center) > MAXDISTANCETOTARGETINPIXELS) {
            //        _targets.Remove(whoAmI);
            //    }
            //    if (!npc.active) {
            //        _targets.Remove(whoAmI);
            //    }
            //}

            Player player = Main.player[Projectile.owner];

            int owner = Projectile.owner;
            ref Vector2 velocity = ref Projectile.velocity;
            float scale = Projectile.scale;
            Vector2 vector21 = Main.player[owner].GetPlayerCorePoint();
            if (Main.myPlayer == owner) {
                if (Main.player[owner].channel) {
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

                    vector4 = Vector2.Normalize(Vector2.Lerp(vector4, Vector2.Normalize(Projectile.velocity), MathF.Min(0.95f, 0.75f + _targets.Count * 0.1f)));
                    vector4 *= num8;
                    if (vector4.X != Projectile.velocity.X || vector4.Y != Projectile.velocity.Y)
                        Projectile.netUpdate = true;

                    Projectile.velocity = vector4;
                }
                else {
                    Projectile.Kill();
                }
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
            Projectile.position -= velocity.SafeNormalize() * 14f;
            Projectile.position -= velocity.TurnLeft().SafeNormalize() * 2f * -player.direction * player.gravDir;
        }
    }
}
