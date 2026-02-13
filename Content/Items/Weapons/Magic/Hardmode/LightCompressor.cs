using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.GlowMasks;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic.Hardmode;

[AutoloadGlowMask]
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

        Item.mana = 4;
    }

    public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
 
    }

    [Tracked]
    public class LightCompressor_Use : ModProjectile {
        private static float MAXDISTANCETOTARGETINPIXELS => 600f;
        private static float TARGETTIME => MathUtils.SecondsToFrames(0.5f);

        private static Asset<Texture2D> _lightTexture = null!;

        private record struct TargetInfo(Vector2 Position, float LaserOpacity);

        private Dictionary<ushort, TargetInfo> _targets = null!;

        public ref float SpawnValue => ref Projectile.localAI[1];

        public ref float TargetTimeValue => ref Projectile.ai[0];

        public override string Texture => ItemLoader.GetItem(ModContent.ItemType<LightCompressor>()).Texture;

        public override void Load() {
            On_Main.DrawNPCs += On_Main_DrawNPCs;
            On_Main.DrawNPCDirect += On_Main_DrawNPCDirect;
        }

        private void On_Main_DrawNPCDirect(On_Main.orig_DrawNPCDirect orig, Main self, SpriteBatch mySpriteBatch, NPC rCurrentNPC, bool behindTiles, Vector2 screenPos) {
            if (rCurrentNPC.GetCommon().LightCompressorEffectOpacity > 0f) {
                mySpriteBatch.DrawWithSnapshot(() => {
                    Effect lightCompressorShader = ShaderLoader.LightCompressor.Value;
                    float width = MathF.Max(100, rCurrentNPC.width * 2f);
                    float height = MathF.Max(100, rCurrentNPC.height * 2f);
                    Vector4 sourceRectangle = new(-width / 2f, -height / 2f, width, height);
                    Vector2 size = new(width, height);
                    float factor = rCurrentNPC.GetCommon().LightCompressorEffectOpacity * 0.75f;
                    lightCompressorShader.Parameters["uSourceRect"].SetValue(sourceRectangle);
                    lightCompressorShader.Parameters["uLegacyArmorSourceRect"].SetValue(sourceRectangle);
                    lightCompressorShader.Parameters["uImageSize0"].SetValue(size);
                    lightCompressorShader.Parameters["uTime"].SetValue(TimeSystem.TimeForVisualEffects);
                    lightCompressorShader.Parameters["uSaturation"].SetValue(factor);
                    lightCompressorShader.CurrentTechnique.Passes[0].Apply();

                    orig(self, mySpriteBatch, rCurrentNPC, behindTiles, screenPos);
                }, sortMode: SpriteSortMode.Immediate);
                return;
            }

            orig(self, mySpriteBatch, rCurrentNPC, behindTiles, screenPos);
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

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.penetrate = -1;

            //Projectile.hide = true;
        }

        //public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        //    behindNPCs.Add(index);
        //}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            foreach (var target in _targets) {
                ushort whoAmI = target.Key;
                NPC npc = Main.npc[whoAmI];
                if (targetHitbox.Contains(npc.Center.ToPoint())) {
                    return true;
                }
            }

            return false;
        }

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

            Texture2D glowTexture = ItemGlowMaskHandler.GlowMasks[ModContent.ItemType<LightCompressor>()].Texture.Value;
            Color glowColor = Color.White * 0.9f;
            int a = 255;
            foreach (var target in _targets) {
                glowColor *= 1.25f;
                a -= 50;
                a = Math.Max(100, a);
            }
            glowColor.A = (byte)a;
            glowColor = glowColor.MultiplyAlpha(0.75f);
            Main.EntitySpriteDraw(glowTexture, pos, null, glowColor, rotation, texture.Frame().Left(), Projectile.scale, effects);

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
                Vector2 endPosition = startPosition2 + normalizedVelocity * TileHelper.TileSize * 100f;
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
                    bool hasTarget = false;
                    foreach (NPC npc in Main.ActiveNPCs) {
                        if (!npc.CanBeChasedBy()) {
                            continue;
                        }
                        if (npc.Distance(position) > npc.Size.Length() * 0.375f) {
                            continue;
                        }
                        if (_targets.ContainsKey((ushort)npc.whoAmI)) {
                            continue;
                        }
                        hasTarget = true;
                        break;
                    }

                    if (hasTarget) {
                        break;
                    }
                    Vector2 velocity = startPosition2.DirectionTo(endPosition);
                    float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                    Rectangle clip = texture.Bounds;
                    float waveValue2 = Helper.Wave(0.5f, 1.25f, 20f, i * 15f + Projectile.identity);
                    clip.Height = (int)(clip.Height * waveValue2);
                    Vector2 origin = clip.Centered();
                    Color color = Color.White * 0.85f;
                    int a = 255;
                    float alpha = 0.875f;
                    bool hasTarget2 = false;
                    foreach (var target in _targets) {
                        alpha = 0.75f;
                        color *= 1.25f;
                        a -= 50;
                        a = Math.Max(100, a);
                        hasTarget2 = true;
                    }
                    color.A = (byte)a;
                    color = color.MultiplyAlpha(alpha);
                    if (hasTarget2) {
                        color *= 0.75f;
                    }
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

            int index = 0;
            void drawLightLine(TargetInfo target, bool onlyBloom = false) {
                index++;

                SpriteBatch batch = Main.spriteBatch;
                Vector2 startPosition = Projectile.Center;
                Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize();

                startPosition += normalizedVelocity.TurnLeft() * 3f * player.direction * player.gravDir;

                Vector2 startPosition2 = startPosition;
                float offsetValue = 60f;
                startPosition += normalizedVelocity * offsetValue;
                startPosition2 += normalizedVelocity * offsetValue * 1.25f;
                Vector2 endPosition = target.Position;
                Vector2 velocity = startPosition.DirectionTo(startPosition2) * 2f;
                int i = 0;
                int height = 2;
                int y = 0;
                float i3 = 1f;
                float i4 = 1f;
                Texture2D texture = _lightTexture.Value;
                float stepFactor2 = height / (float)texture.Bounds.Height;
                float lerpValue = 0.1f * stepFactor2;
                while (true) {
                    i++;

                    float extraScale = 1.25f;

                    float stepFactor = 1f;
                    i3 += stepFactor2;
                    float step = height * stepFactor /** extraScale*/;
                    float distance = Vector2.Distance(startPosition, endPosition);
                    if (distance < step * 1f) {
                        break;
                    }
                    float maxLength = MAXDISTANCETOTARGETINPIXELS;
                    float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                    Rectangle clip = texture.Bounds with { Y = y, Height = height };
                    Vector2 origin = clip.BottomCenter();
                    Color color = Color.White.MultiplyAlpha(0.75f);
                    //color *= Utils.GetLerpValue(maxLength, maxLength * 0.8f, Projectile.Distance(endPosition), true);
                    color *= target.LaserOpacity;
                    float i_check = i * stepFactor2;
                    float i2 = i_check * 10f * stepFactor;
                    float waveFactor = Ease.CubeIn(Utils.GetLerpValue(0f, 2.5f, i_check, true));
                    float scaleFactor = Utils.GetLerpValue(0f, 10f, i_check, true);
                    scaleFactor *= Ease.CubeIn(MathUtils.Clamp01(distance / (step * 3f)));
                    scaleFactor = MathF.Max(0.625f, scaleFactor);
                    Vector2 scale = new(Helper.Wave(1f - 0.5f * waveFactor, 1f + 0.5f * waveFactor, 20f, Projectile.identity * 3) * 2f * scaleFactor, 1f);
                    velocity = Vector2.Lerp(velocity, startPosition.DirectionTo(endPosition), lerpValue);

                    scale *= extraScale;

                    DrawInfo drawInfo = new() {
                        Clip = clip,
                        Origin = origin,
                        Rotation = rotation,
                        Color = color,
                        Scale = scale
                    };

                    Vector2 position = startPosition;
                    startPosition += velocity.SafeNormalize() * step;
                    float offsetValue2 = 2f * scaleFactor * waveFactor * stepFactor2 * 2f;
                    startPosition -= velocity.SafeNormalize().TurnLeft() * Helper.Wave(-1f, 1f, 10f, Projectile.identity * 3 + i2 * 0.05f + index * 3) * offsetValue2;

                    DrawInfo bloomDrawInfo = new() {
                        Clip = ResourceManager.Bloom.Bounds,
                        Origin = ResourceManager.Bloom.Bounds.Centered(),
                        Rotation = rotation,
                        Color = color.MultiplyRGB(new Color(136, 219, 227)).MultiplyAlpha(0.5f) * 0.75f * Ease.QuadOut(scaleFactor),
                        Scale = Vector2.One * scale.X * 0.15f
                    };
                    if (i3 >= 0.75f) {
                        i3 = 0f;

                        if (onlyBloom) {
                            batch.DrawWithSnapshot(ResourceManager.Bloom, position, bloomDrawInfo, blendState: BlendState.Additive);

                            void createDusts() {
                                if (!Main.gamePaused && Main.instance.IsActive) {
                                    if (Main.rand.NextBool(35)) {
                                        Dust.NewDustPerfect(position + Main.rand.NextVector2CircularEdge(10f, 10f), ModContent.DustType<LightCompressorDust>(),
                                            Main.rand.NextVector2Circular(1f, 1f) + position.DirectionTo(position + velocity), 0, Color.White, Main.rand.NextFloat(0.8f, 1.2f));
                                    }
                                }
                            }
                            createDusts();
                            for (; i4 > 0f; i4 -= 0.5f) {
                                createDusts();
                            }

                            Lighting.AddLight(position, (Color.Lerp(Color.SkyBlue, Color.Blue, 0.05f) with { A = 0 }).ToVector3() * 0.75f);
                        }
                    }

                    if (!onlyBloom) {
                        batch.Draw(texture, position, drawInfo);
                    }
                    float length = (startPosition2 - endPosition).Length();
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
                    lerpValue = Helper.Approach(lerpValue, maxLerpValue, TimeSystem.LogicDeltaTime * factor * stepFactor2);

                    y += height;
                    if (y >= texture.Bounds.Height) {
                        y = 0;
                    }
                }
            }

            foreach (var targetWhoAmI in _targets) {
                drawLightLine(targetWhoAmI.Value, true);
                index = 0;
                drawLightLine(targetWhoAmI.Value);
            }
            drawMainLightLine();
        }

        public override bool? CanDamage() => true;
        public override bool? CanCutTiles() => false;
        public override bool ShouldUpdatePosition() => false;

        public override void AI() {
            Projectile.knockBack *= 0f;

            if (SpawnValue == 0f) {
                SpawnValue = 1f;

                _targets = [];
            }

            Player player4 = Projectile.GetOwnerAsPlayer();
            if (!player4.CheckMana(player4.GetSelectedItem(), pay: false)) {
                Projectile.Kill();
            }

            if (player4.noItems || !player4.IsAliveAndFree()) {
                Projectile.Kill();
            }

            Vector2 startPosition = Projectile.Center;
            Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize();
            Vector2 endPosition = startPosition + normalizedVelocity * 600f;

            bool hasTarget = false;
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
                    if (npc.Distance(startPosition) > npc.Size.Length() * 0.375f) {
                        continue;
                    }
                    if (!hasTarget) {
                        npc.GetCommon().IsLightCompressorEffectActive = true;
                        npc.GetCommon().LightCompressorEffectOpacity = Helper.Approach(npc.GetCommon().LightCompressorEffectOpacity, 1f, 0.025f);
                    }
                    hasTarget = true;
                    if (TargetTimeValue < TARGETTIME) {
                        continue;
                    }
                    if (_targets.ContainsKey(whoAmI)) {
                        hasTarget = false;
                        continue;
                    }
                    _targets.Add(whoAmI, new TargetInfo(npc.Center + Vector2.UnitY * npc.gfxOffY, 0f));
                    TargetTimeValue = 0f;
                    break;
                }

                startPosition += velocity2 * step;
            }

            if (!hasTarget) {
                Projectile.localAI[2]++;
                if (Projectile.localAI[2] >= 20f) {
                    Player player3 = Projectile.GetOwnerAsPlayer();
                    player3.CheckMana(player3.GetSelectedItem(), pay: true);
                    Projectile.localAI[2] = 0f;
                }
            }

            if (!hasTarget) {
                if (TargetTimeValue > 0) {
                    TargetTimeValue -= 2;
                }
            }
            else {
                if (TargetTimeValue < TARGETTIME) {
                    TargetTimeValue++;
                }
            }

            float lerpValue = 0.05f;
            bool checkMana = false;
            foreach (var target in _targets) {
                ushort whoAmI = target.Key;
                NPC npc = Main.npc[whoAmI];
                void removeSlowly(float lerpValueFactor = 1f) {
                    TargetInfo targetInfo = _targets[whoAmI];
                    targetInfo.LaserOpacity = Helper.Approach(targetInfo.LaserOpacity, 0f, lerpValue * lerpValueFactor);
                    _targets[whoAmI] = targetInfo;
                    if (_targets[whoAmI].LaserOpacity <= 0f) {
                        _targets.Remove(whoAmI);
                    }
                }
                Player player2 = Projectile.GetOwnerAsPlayer();
                if (!player2.CheckMana(player2.GetSelectedItem(), pay: false)) {
                    removeSlowly();
                    continue;
                }
                if (Projectile.Distance(npc.Center) > MAXDISTANCETOTARGETINPIXELS) {
                    removeSlowly();
                    continue;
                }
                if (!npc.active || !npc.CanBeChasedBy()) {
                    removeSlowly(1.25f);
                }
                else {
                    TargetInfo targetInfo = _targets[whoAmI];
                    targetInfo.LaserOpacity = Helper.Approach(targetInfo.LaserOpacity, 1f, lerpValue);
                    targetInfo.Position = npc.Center + Vector2.UnitY * npc.gfxOffY;

                    Player player3 = Projectile.GetOwnerAsPlayer();
                    if (!checkMana) {
                        Projectile.localAI[2]++;
                        if (Projectile.localAI[2] >= 10f) {
                            player3.CheckMana(player3.GetSelectedItem(), pay: true);
                            Projectile.localAI[2] = 0f;
                        }
                    }
                    checkMana = true;

                    if (Main.rand.NextBool()) {
                        Dust.NewDustPerfect(targetInfo.Position + Main.rand.RandomPointInArea(npc.Size * 0.25f), ModContent.DustType<LightCompressorDust>(),
                            Main.rand.NextVector2Circular(1f, 1f), 0, Color.White, Main.rand.NextFloat(0.8f, 1.2f));
                    }

                    _targets[whoAmI] = targetInfo;
                    npc.GetCommon().IsLightCompressorEffectActive = true;
                    npc.GetCommon().LightCompressorEffectOpacity = Helper.Approach(npc.GetCommon().LightCompressorEffectOpacity, 1f, 0.025f);
                }
            }

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
