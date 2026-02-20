using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.BackwoodsSystems;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Buffs;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class GrimDefender : ModNPC {
    private static Asset<Texture2D> _bodyTexture = null!;

    private const float ATTACKTIME = 145f;

    private Vector2 _tempPosition, _extraVelocity, _extraVelocity2;
    private bool _spearAttack;
    private float _rotation;
    private bool _isAngry;
    private float _angryTimer;
    private byte _hitCount;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 8;

        NPCID.Sets.TrailingMode[NPC.type] = 7;
        NPCID.Sets.TrailCacheLength[NPC.type] = 7;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, 20f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 0f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        //NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        //NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;
        //NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
        //NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;

        if (Main.dedServ) {
            return;
        }

        _bodyTexture = ModContent.Request<Texture2D>(Texture + "_Body");
    }

    public override void SetDefaults() {
        NPC.lifeMax = 50;
        NPC.damage = 30;
        NPC.defense = 10;

        int width = 22; int height = 28;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 1.25f;

        NPC.dontTakeDamage = true;

        //NPC.npcSlots = 0.8f;

        NPC.value = Item.buyPrice(0, 0, 2, 0);

        NPC.HitSound = SoundID.NPCHit7;
        NPC.DeathSound = SoundID.NPCDeath6;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<GrimDefenderBanner>();
    }

    private class RecognizeHit : ModPlayer {
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (target.ModNPC != null && target.ModNPC.SpawnModBiomes.Contains(ModContent.GetInstance<BackwoodsBiome>().Type)) {
                foreach (NPC npc in Main.ActiveNPCs) {
                    if (npc.type == ModContent.NPCType<GrimDefender>()) {
                        if (Player.whoAmI != npc.target) {
                            return;
                        }

                        npc.As<GrimDefender>().MakeAngry();

                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new RecognizeHitPacket(Player, npc.whoAmI));
                        }
                    }
                }
            }
        }
    }

    internal void MakeAngry() {
        _isAngry = true;
        _angryTimer = 0f;
        NPC.netUpdate = true;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
        position = new(NPC.position.X + 22 / 2, NPC.position.Y + NPC.gfxOffY);
        position += new Vector2(22, 28) / 2f;
        position.X -= 11;
        position.Y -= 9;
        if (Main.HealthBarDrawSettings == 1) {
            position.Y += 28 + 10f + Main.NPCAddHeight(NPC);
        }
        else if (Main.HealthBarDrawSettings == 2) {
            position.Y -= 24f + Main.NPCAddHeight(NPC) / 2f;
        }

        return true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.GrimDefender")
        ]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = _bodyTexture.Value;
        Vector2 offset = new Vector2(22, 28) / 2f;
        Vector2 position = NPC.position - screenPos + offset;
        Vector2 origin = NPC.frame.Size() / 2f;
        SpriteEffects effects = NPC.spriteDirection != -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
        if (NPC.IsABestiaryIconDummy) {
            Vector2 drawPosition = position + new Vector2(36f, -32f);
            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects);

            texture = TextureAssets.Npc[Type].Value;
            Rectangle sourceRectangle2 = NPC.frame;
            sourceRectangle2.Width = 72;
            sourceRectangle2.X = 0;
            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle2, new Color(255, 255, 255, 0) * 0.8f, NPC.rotation, origin, NPC.scale, effects);
            return false;
        }
        float attackCd = ATTACKTIME;
        float num = attackCd * 0.6f;
        float progress = NPC.ai[1] > num ? Ease.CubeInOut(1f - (NPC.ai[1] - num) / (attackCd - num)) : 1f;
        bool flag = NPC.localAI[2] == 1f;
        if (NPC.ai[0] > 1f) {
            float num170 = NPC.localAI[3];
            if (num170 > 1f || flag) {
                num170 = 1f;
            }
            int max = NPCID.Sets.TrailCacheLength[Type] - 1;
            float mult = 1f / max;
            Color trailColor = drawColor * num170 * (!flag ? Utils.GetLerpValue(0f, 5f, NPC.velocity.Length(), true) : 1f);
            if (flag) {
                trailColor *= 1f - NPC.localAI[1] / 12f;
            }
            for (int i = 0; i < max; i++) {
                Main.EntitySpriteDraw(texture, NPC.oldPos[i] + offset - Main.screenPosition, NPC.frame, trailColor * (mult * (max - i)), NPC.oldRot[i], origin, NPC.scale, effects);
            }
        }
        float opacity = 1f - progress;
        opacity = Math.Min(opacity, 0.8f);
        for (int i = 0; i < 4; i++) {
            Main.EntitySpriteDraw(texture,
                                  position + (i * MathHelper.PiOver2 + progress * MathHelper.PiOver2 * NPC.direction).ToRotationVector2() * (progress * 12f) * NPC.scale,
                                  new Rectangle(NPC.frame.X, Math.Max(NPC.frame.Y, NPC.frame.Height * (_spearAttack ? 6 : 2)), NPC.frame.Width, NPC.frame.Height),
                                  drawColor * opacity * 0.8f,
                                  NPC.rotation,
                                  origin,
                                  NPC.scale,
                                  effects);
        }
        Main.EntitySpriteDraw(texture, position, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects);
        texture = TextureAssets.Npc[Type].Value;
        Color color = new Color(255, 255, 255, 0) * 0.8f;
        Rectangle sourceRectangle = NPC.frame;
        sourceRectangle.X = _isAngry ? 0 : 72;
        for (int i = 0; i < NPCID.Sets.TrailCacheLength[Type]; i++) {
            float mult = 1f / NPCID.Sets.TrailCacheLength[Type];
            Main.EntitySpriteDraw(texture, NPC.oldPos[i] + offset - Main.screenPosition, sourceRectangle, color * 0.9f * (mult * (NPCID.Sets.TrailCacheLength[Type] - i)), NPC.oldRot[i], origin, NPC.scale, effects);
        }
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, NPC.rotation, origin, NPC.scale, effects);

        return false;
    }

    public override void FindFrame(int frameHeight) {
        if (NPC.IsABestiaryIconDummy) {
            ++NPC.frameCounter;

            if (NPC.frameCounter < 40.0) {
                NPC.frame.Y = frameHeight * 3;
            }
            else {
                NPC.frame.Y = frameHeight * 7;
            }

            if (NPC.frameCounter > 80.0) {
                NPC.frameCounter = 0;
            }

            return;
        }

        NPC.frame.Width = 72;

        if (NPC.ai[0] == 0f) {
            float num = ATTACKTIME;
            if (NPC.ai[1] <= num * 0.7f) {
                if (++NPC.frameCounter > 3 && NPC.localAI[0] > 0) {
                    NPC.frameCounter = 0.0;
                    NPC.localAI[0]--;
                }
            }
            else if (++NPC.frameCounter > 6 && NPC.localAI[0] < 3) {
                NPC.frameCounter = 0.0;
                NPC.localAI[0]++;
            }
            NPC.frame.Y = frameHeight * ((int)NPC.localAI[0] + (_spearAttack ? 4 : 0));
        }
        else if (NPC.ai[0] >= 1f) {
            NPC.frame.Y = frameHeight * (_spearAttack ? 7 : 3);
        }

        NPC.spriteDirection = NPC.direction;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.ai[0] > 1f && !_spearAttack;

    public override void HitEffect(NPC.HitInfo hit) {
        void reset() {
            NPC.ai[0] = 0f;
            NPC.ai[1] = 60f - NPC.ai[1];
            NPC.localAI[1] = 0f;
            NPC.localAI[2] = 0f;
            //NPC.localAI[3] = 0f;
            NPC.dontTakeDamage = true;
            SpawnHitGores();
            NPC.netUpdate = true;
        }
        if (!_spearAttack) {
            if (_hitCount <= 0) {
                _hitCount = 2;
                reset();
            }
            else {
                _hitCount--;
                NPC.netUpdate = true;
            }
            return;
        }
        reset();
    }

    public override void AI() {
        NPC.localAI[3] = MathHelper.Lerp(NPC.localAI[3], NPC.velocity.Length() / 6.5f, 0.325f);

        NPC.ShowNameOnHover = _isAngry;

        NPC.wet = false;
        NPC.noTileCollide = NPC.noGravity = true;

        Lighting.AddLight(NPC.Center, (NPC.ai[0] == 0f && NPC.ai[1] <= ATTACKTIME * 0.7f ? (_isAngry ? new Color(255, 120, 120) : new Color(153, 244, 114)) * 0.5f : new Color(148, 1, 26)).ToVector3() * 0.75f);

        //DrawColor value3 = DrawColor.LimeGreen;
        //DrawColor value4 = DrawColor.LightSeaGreen;
        //int num17 = 4;
        //if (_isAngry) {
        //    value3 = new(255, 147, 147, 200);
        //}
        //Vector2 dustCenter = NPC.position + new Vector2(22, 28) / 2f;
        //if (!_isAngry && NPC.velocity.Length() > 1.5f && (int)Main.timeForVisualEffects % 2 == 0) {
        //    Dust dust = Dust.NewDustDirect(dustCenter - new Vector2(num17), num17 + 4, num17 + 4, DustID.FireworksRGB, 0f, 0f, 200, DrawColor.Lerp(value3, value3, Main.rand.NextFloat()), 0.65f);
        //    dust.velocity *= 0f;
        //    dust.velocity += NPC.velocity * 0.3f;
        //    dust.noGravity = true;
        //    dust.noLight = true;
        //}

        bool flag = true;
        Vector2 diff = Main.player[NPC.target].GetPlayerCorePoint() - NPC.Center;
        diff.Normalize();
        void directedRotation(Vector2 destination) {
            flag = false;
            float rotation = Helper.VelocityAngle(destination.SafeNormalize(Vector2.Zero)) - MathHelper.PiOver2;
            _rotation = Utils.AngleLerp(_rotation, rotation, Math.Abs(rotation) * 0.075f + 0.05f);
            NPC.rotation = _rotation;
            if (NPC.spriteDirection == -1) {
                NPC.rotation -= MathHelper.Pi;
            }
        }
        Vector2 toHead = Main.player[NPC.target].GetPlayerCorePoint() - Vector2.UnitY * 18f - NPC.Center;
        float attackCd = ATTACKTIME;

        Vector2 center = new(NPC.position.X + 22 / 2, NPC.position.Y + 28 / 2 + NPC.gfxOffY);
        center.X -= 11;
        center.Y -= 9;

        if (NPC.ai[0] > 1f) {
            NPC.knockBackResist = 0f;

            float num = 60f;
            Vector2 desiredVelocity = new(NPC.ai[2], NPC.ai[3]);

            if (NPC.velocity.Length() > 1.5f && NPC.localAI[2] != 1f) {
                for (int i = 0; i < 4; i++) {
                    if (Main.rand.NextBool(4)) {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<WoodTrash>());
                        Main.dust[dust].velocity = -NPC.velocity * 0.3f * Main.rand.NextFloat();
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].scale = Main.rand.NextFloat(0.9f, 3f) * 0.38f * (1f + 0.2f * Main.rand.NextFloat());
                        Main.dust[dust].fadeIn = Main.rand.NextFloat(0.1f, 0.5f);
                    }
                }
            }

            if (!_spearAttack) {
                NPC.ai[1]++;
                bool flag2 = NPC.ai[1] <= num * 0.75f;
                Vector2 extraVelocity = (Utils.MoveTowards(NPC.Center, Main.player[NPC.target].GetPlayerCorePoint(), NPC.ai[0]) - NPC.Center) * MathHelper.Clamp(NPC.ai[0] - 2f, 0f, 0.2f) * 0.4f;
                if (flag2) {
                    directedRotation(desiredVelocity + extraVelocity);
                }
                //NormalMovement(false);
                float value2 = Math.Abs(NPC.ai[2] * NPC.ai[3]);
                _extraVelocity *= 0.97f;
                NPC.ai[0] = MathHelper.Lerp(NPC.ai[0] - 2f, (float)Math.Sqrt(value2), 0.0045f) + 2f;
                NPC.ai[2] *= 0.98f;
                NPC.ai[3] *= 0.98f;
                NPC.velocity = Vector2.SmoothStep(NPC.velocity, desiredVelocity, (NPC.ai[0] - 2f) * 0.5f);
                NPC.velocity += extraVelocity;
                //NPC.velocity *= 0.95f;
            }
            else {
                diff = _tempPosition /*- Vector2.UnitY * 20f*/ - center;
                diff.Normalize();

                //NPC.Center = TempPosition + NPC.velocity;
                ////NPC.velocity = Utils.MoveTowards(NPC.Center, Main.player[NPC.target].Center, NPC.ai[2]) - NPC.Center;

                //int width = 60; int height = width;
                //NPC.Size = new Vector2(width, height);

                if (NPC.velocity.Length() < 8f) {
                    float progress = NPC.ai[1] / num;
                    float num2 = 0.4f;
                    if (Vector2.Distance(center, _tempPosition) <= 45f && NPC.localAI[2] != 1f) {
                        NPC.localAI[2] = 1f;
                        NPC.velocity *= 0.5f;
                    }

                    bool flag3 = NPC.localAI[2] == 1f;
                    float speed = 1.5f;
                    if (!flag3) {
                        NPC.ai[2]++;
                        //if (NPC.ai[2] <= num * 0.4f) {
                        _tempPosition = Main.player[NPC.target].GetPlayerCorePoint();
                        //}
                        progress = NPC.ai[2] / num;
                        _extraVelocity2 = diff * speed * progress;
                    }
                    else {
                        if (NPC.localAI[1] < num * 0.2f) {
                            progress = NPC.ai[2] / num;
                            NPC.localAI[1]++;
                            _extraVelocity2 = -diff * speed * 0.5f * (progress - num2);
                            NPC.ai[2]--;
                        }
                        NPC.ai[1]++;
                    }
                    NPC.velocity += _extraVelocity2 * 0.75f;
                    _extraVelocity2 = Vector2.Zero;
                    num *= 0.85f;
                }

                NormalMovement(true, _tempPosition);
                ApplyExtraVelocity1();
                directedRotation(_tempPosition /*- Vector2.UnitY * 20f*/ - center);
            }

            if (NPC.ai[1] >= num/* || NPC.justHit*/) {
                NPC.ai[0] = 0f;
                NPC.ai[1] = /*NPC.justHit ? (60f - NPC.ai[1]) : */0f;
                NPC.localAI[1] = 0f;
                NPC.localAI[2] = 0f;
                //NPC.localAI[3] = 0f;
                NPC.netUpdate = true;
            }
        }
        else if (NPC.ai[0] == 1f && NPC.ai[1] >= attackCd) {
            _extraVelocity = _extraVelocity2 = Vector2.Zero;
            flag = false;
            NPC.ai[1] = 0f;
            NPC.ai[0] = 2f;
            if (!_spearAttack) {
                Vector2 desiredVelocity = diff * 8f;
                NPC.ai[2] = desiredVelocity.X;
                NPC.ai[3] = desiredVelocity.Y;
            }
            else {
                NPC.ai[2] = 0f;
            }
            NPC.dontTakeDamage = false;
            NPC.netUpdate = true;
        }
        else {
            int width = 22; int height = 28;
            NPC.Size = new Vector2(width, height);

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
                NPC.TargetClosest();
            }

            NPC.knockBackResist = 0.9f;
            bool flag3 = ((!Main.player[NPC.target].InModBiome<BackwoodsBiome>() || NPC.Center.Y / 16 <= BackwoodsVars.FirstTileYAtCenter + 35) && !_isAngry) || Main.player[NPC.target].dead || (!_isAngry && _angryTimer >= 20f);
            if (flag3) {
                float maxSpeed = 3.5f;
                if (NPC.velocity.Y < -maxSpeed) {
                    NPC.velocity.Y = -maxSpeed;
                }
                float speedY = 0.075f * 0.3f;
                NPC.velocity.Y += speedY + speedY / 2f * Main.rand.NextFloat();
                float speedX = NPC.direction * 0.1f * 0.45f;
                NPC.velocity.X += speedX + speedX / 2f * Main.rand.NextFloat();
                if (NPC.velocity.X < -maxSpeed) {
                    NPC.velocity.X = -maxSpeed;
                }
                if (NPC.velocity.X > maxSpeed) {
                    NPC.velocity.X = maxSpeed;
                }
            }
            else {
                bool flag2 = NPC.ai[1] <= attackCd * 0.5f;
                NPC.dontTakeDamage = !_isAngry || NPC.ai[1] <= attackCd * 0.7f;
                if (!_isAngry) {
                    if (_angryTimer < 20f) {
                        _angryTimer += TimeSystem.LogicDeltaTime;
                    }
                }
                if (NPC.ai[1] < attackCd) {
                    if ((!_isAngry && flag2) || (_isAngry && NPC.Distance(Main.player[NPC.target].GetPlayerCorePoint()) <= 240f)) {
                        NPC.ai[1]++;
                    }

                    NormalMovement();

                    _tempPosition = Main.player[NPC.target].GetPlayerCorePoint();
                }

                if (_spearAttack) {
                    _tempPosition = Main.player[NPC.target].GetPlayerCorePoint();
                }

                ApplyExtraVelocity1();

                diff = !_spearAttack ? toHead : (_tempPosition - center);
                directedRotation(diff);

                if (flag2) {
                    NPC.TargetClosest();
                    _extraVelocity *= 0.97f;
                    NPC.velocity *= 0.97f;
                }
                else if (_isAngry && NPC.localAI[2] == 0f) {
                    if (Main.rand.NextBool()) {
                        _spearAttack = !_spearAttack;
                        foreach (Projectile projectile in Main.ActiveProjectiles) {
                            if (projectile.ai[0] == NPC.whoAmI) {
                                projectile.Kill();
                            }
                        }
                    }
                    if (!_spearAttack && _hitCount != 2) {
                        _hitCount = 2;
                    }
                    if (_spearAttack) {
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GrimDefenderSpearAttack>(),
                                60 / 3, 4f, Main.myPlayer, NPC.whoAmI);
                        }
                        //TempPosition = Main.player[NPC.target].Center;
                        //_extraVelocity = Vector2.Zero;
                    }
                    NPC.localAI[2] = 1f;
                }

                if (NPC.ai[1] >= attackCd && _isAngry) {
                    NPC.ai[0] = 1f;
                    NPC.localAI[2] = 0f;
                }
            }
        }

        if (flag) {
            NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.X * 0.1f, 0.1f);
        }
    }

    private void SpawnHitGores() {
        for (int i = 0; i < 10; i++) {
            int dust = Dust.NewDust(new Vector2(NPC.Center.X, NPC.Center.Y), 10, 10, ModContent.DustType<WoodTrash>(), 0, 0, 0, default, 0.4f + Main.rand.NextFloat(0, 1f));
            Main.dust[dust].velocity *= 0.3f;
        }

        if (!Main.dedServ) {
            int count = Main.rand.Next(3);
            for (int i = count - count / 2; i < 2 + count * 2; i++) {
                Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, Vector2.Zero, ModContent.Find<ModGore>(RoA.ModName + "/VileSpikeGore").Type, 1f);
            }
        }
    }

    private void ApplyExtraVelocity1() => NPC.velocity += _extraVelocity * 0.075f;

    private void NormalMovement(bool applyExtraVelocity = true, Vector2? center = null) {
        center ??= Main.player[NPC.target].GetPlayerCorePoint();
        Vector2 playerCenter = center.Value;
        float dist = NPC.Distance(playerCenter);
        float minDist = 150f;
        float inertia = 22f;
        float speed = 5f;
        if (dist <= minDist * 1.25f) {
            Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, NPC.Center - (playerCenter - NPC.Center).SafeNormalize(Vector2.Zero) * minDist, inertia * 0.5f, speed * 0.5f, minDist);
        }
        if (dist > minDist) {
            Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, playerCenter, inertia * 0.75f, speed * 0.75f, minDist);
        }
        if (applyExtraVelocity) {
            float num269 = Math.Abs(NPC.position.X + (float)(NPC.width / 2) - center.Value.X);
            float num270 = center.Value.Y - (float)(NPC.height);
            if (num269 > 50f)
                num270 -= 50f;

            if (NPC.position.Y < num270) {
                _extraVelocity.Y += 0.03f;
                if (_extraVelocity.Y < 0f)
                    _extraVelocity.Y += 0.01f;
            }
            else {
                _extraVelocity.Y -= 0.03f;
                if (_extraVelocity.Y > 0f)
                    _extraVelocity.Y -= 0.01f;
            }

            if (_extraVelocity.Y < -1.35f)
                _extraVelocity.Y = -1.35f;

            if (_extraVelocity.Y > 1.35f)
                _extraVelocity.Y = 1.35f;
        }
    }

    private class GrimDefenderSpearAttack : ModProjectile {
        public override string Texture => ResourceManager.EmptyTexture;

        public override bool PreDraw(ref Color lightColor) => false;

        public override void SetDefaults() {
            int width = 20; int height = 20;
            Projectile.Size = new Vector2(width, height);

            Projectile.friendly = false;
            Projectile.hostile = true;

            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;

            Projectile.tileCollide = false;

            Projectile.alpha = byte.MaxValue;
        }

        public override void AI() {
            Projectile.timeLeft = 2;

            NPC parent = Main.npc[(int)Projectile.ai[0]];
            if (!parent.active) {
                Projectile.Kill();
                return;
            }

            Projectile.Center = parent.Center + parent.velocity.SafeNormalize(Vector2.Zero) * 40f;
        }

        public override bool? CanDamage() => Main.npc[(int)Projectile.ai[0]].velocity.Length() > 1.5f;

        public override void OnHitPlayer(Player target, Player.HurtInfo info) {
            target.AddBuff(ModContent.BuffType<Hemorrhage>(), 180);
            Projectile.Kill();
        }
    }


    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_tempPosition);
        writer.WriteVector2(_extraVelocity);
        writer.WriteVector2(_extraVelocity2);
        writer.Write(_spearAttack);
        writer.Write(_isAngry);
        writer.Write(_angryTimer);
        writer.Write(_hitCount);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _tempPosition = reader.ReadVector2();
        _extraVelocity = reader.ReadVector2();
        _extraVelocity2 = reader.ReadVector2();
        _spearAttack = reader.ReadBoolean();
        _isAngry = reader.ReadBoolean();
        _angryTimer = reader.ReadSingle();
        _hitCount = reader.ReadByte();
    }
}
