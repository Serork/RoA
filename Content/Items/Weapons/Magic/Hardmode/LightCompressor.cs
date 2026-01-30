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
        private static Asset<Texture2D> _lightTexture = null!;

        private List<int> _targets = null!;

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
            void drawLightLine(Vector2 targetPosition) {
                SpriteBatch batch = Main.spriteBatch;

                Vector2 startPosition = Projectile.Center,
                        startPosition2 = startPosition;
                Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize();
                float offsetValue = 60f;
                startPosition += normalizedVelocity * offsetValue;
                startPosition2 += normalizedVelocity * offsetValue * 1.25f;
                Vector2 endPosition = targetPosition;

                Vector2 velocity = startPosition.DirectionTo(startPosition2) * 10f;

                float lerpValue = 0.1f;

                while (true) {
                    Texture2D texture = _lightTexture.Value;
                    float step = texture.Height;
                    if (Vector2.Distance(startPosition, endPosition) < step) {
                        break;
                    }

                    float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                    Rectangle clip = texture.Bounds;
                    Vector2 origin = clip.Centered();
                    DrawInfo drawInfo = new() {
                        Clip = clip,
                        Origin = origin,
                        Rotation = rotation
                    };
                    Vector2 position = startPosition;
                    batch.Draw(texture, position, drawInfo);

                    startPosition += velocity.SafeNormalize() * step;

                    velocity = Vector2.Lerp(velocity, startPosition.DirectionTo(endPosition), lerpValue);

                    float length = (startPosition2 - endPosition).Length();
                    float maxLength = 600f;
                    if (length > maxLength) {
                        break;
                    }
                    float factor = 1f;
                    factor -= length / maxLength;
                    factor = MathF.Max(0.01f, factor);
                    float maxLerpValue = factor * 10f;
                    lerpValue = Helper.Approach(lerpValue, maxLerpValue, TimeSystem.LogicDeltaTime * factor);
                }
            }

            foreach (byte targetWhoAmI in _targets) {
                Vector2 targetPosition = Main.npc[targetWhoAmI].Center;
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

                foreach (NPC npc in Main.ActiveNPCs) {
                    _targets.Add(npc.whoAmI);
                }
            }

            Player player = Main.player[Projectile.owner];

            int owner = Projectile.owner;
            ref Vector2 velocity = ref Projectile.velocity;
            float scale = Projectile.scale;
            Vector2 vector21 = Main.player[owner].GetPlayerCorePoint();
            if (Main.myPlayer == owner) {
                if (Main.player[owner].channel) {
                    float num178 = Main.player[owner].inventory[Main.player[owner].selectedItem].shootSpeed * scale;
                    Vector2 vector22 = vector21;
                    float num179 = (float)Main.mouseX + Main.screenPosition.X - vector22.X;
                    float num180 = (float)Main.mouseY + Main.screenPosition.Y - vector22.Y;
                    if (Main.player[owner].gravDir == -1f)
                        num180 = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector22.Y;

                    float num181 = (float)Math.Sqrt(num179 * num179 + num180 * num180);
                    num181 = (float)Math.Sqrt(num179 * num179 + num180 * num180);
                    num181 = num178 / num181;
                    num179 *= num181;
                    num180 *= num181;
                    if (num179 != velocity.X || num180 != velocity.Y)
                        Projectile.netUpdate = true;

                    velocity.X = num179;
                    velocity.Y = num180;
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
