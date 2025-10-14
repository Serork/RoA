using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Projectiles.Enemies.Lothor;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed class Pipistrelle : ModNPC {
    private bool _shouldEnrage;
    private float _lightingColorValue = 1f;

    private Texture2D ItsSpriteSheet => (Texture2D)ModContent.Request<Texture2D>(Texture);
    private Texture2D GlowMask => (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, 6f),

            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 8f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.DontDoHardmodeScaling[Type] = true;
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Pipistrelle")
        ]);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
        NPC.lifeMax = (int)((double)NPC.lifeMax * 0.7 * (double)balance);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 80;
        NPC.damage = 30;
        NPC.defense = 5;
        NPC.Size = new Vector2(32, 32);
        NPC.aiStyle = -1;

        AnimationType = 82;

        NPC.HitSound = SoundID.NPCHit27;
        NPC.DeathSound = SoundID.NPCDeath39;
        NPC.noGravity = true;
        NPC.netAlways = true;
        NPC.noTileCollide = true;
        NPC.value = 0;

        NPC.rarity = 1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteEffects effects = NPC.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Vector2 position = NPC.Center - screenPos;
        Vector2 origin = NPC.frame.Size() / 2f;
        float rotation = NPC.velocity.X * 0.085f;
        float max = 0.5f;
        drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
        rotation = MathHelper.Clamp(rotation, -max, max);
        if (NPC.ai[2] != 1f) {
            Vector2 bestiaryOffset = Vector2.Zero;
            if (NPC.IsABestiaryIconDummy) {
                bestiaryOffset = Vector2.UnitX * 5f;
            }
            float progress = (Math.Abs(rotation) / MathHelper.PiOver2) * NPC.spriteDirection;
            spriteBatch.Draw(ModContent.Request<Texture2D>(ResourceManager.EnemyProjectileTextures + "Lothor/CursedAcorn").Value,
                position + origin + bestiaryOffset +
                new Vector2(-27f, 4f - (NPC.spriteDirection != 1 ? progress * -20f : 0f)) + Vector2.UnitX * -40f * progress -
                Vector2.UnitY * 12f,
                null, Color.White, rotation * 0.5f, origin / 2f, 1f, effects, 0);
        }

        void enrage(ref Color color) {
            if (_shouldEnrage) {
                color = Color.Lerp(Helper.BuffColor(color, 0.3f, 0.3f, 0.3f, 1f), color, 0.6f);
            }
        }
        enrage(ref drawColor);

        spriteBatch.Draw(ItsSpriteSheet, position, NPC.frame, drawColor, rotation, origin, NPC.scale, effects, 0f);

        NPC owner = Main.npc[(int)NPC.ai[0]];
        bool flag = !owner.active || owner.ModNPC is null || owner.ModNPC is not Lothor;

        float glowMaskOpacity = _lightingColorValue;
        Vector2 altarPosition = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        float minDistance = 300f;
        Vector2 center = NPC.Center;
        float distance = center.Distance(altarPosition + altarPosition.DirectionTo(center) * minDistance) * 2f;
        float altarOpacity = MathUtils.Clamp01(1f - distance / minDistance);
        if (center.Distance(altarPosition) < minDistance) {
            altarOpacity = 1f;
        }
        float lifeProgress = !flag ? owner.As<Lothor>().LifeProgress : 0f;
        lifeProgress = MathF.Max(lifeProgress, altarOpacity);
        glowMaskOpacity = MathF.Max(glowMaskOpacity, altarOpacity);
        float value = MathHelper.Clamp(flag ? glowMaskOpacity : Math.Max(glowMaskOpacity, lifeProgress), 0f, 1f);

        Color glowColor = Color.White * value;
        spriteBatch.Draw(GlowMask, position, NPC.frame, glowColor, rotation, origin, NPC.scale, effects, 0f);

        NPC npc = Main.npc[(int)NPC.ai[0]];
        if (!(!npc.active || npc.ModNPC is null || npc.ModNPC is not Lothor)) {
            SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
            spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            float lifeProgress2 = _shouldEnrage ? 1f : lifeProgress;
            for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                spriteBatch.Draw(GlowMask, position +
                    Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                    * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress2,
                    NPC.frame, glowColor.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress2, rotation + Main.rand.NextFloatRange(0.05f) * lifeProgress2, origin, NPC.scale, effects, 0f);
            }
            spriteBatch.Begin(snapshot, true);
        }

        return false;
    }

    private void AI_GetMyGroupIndexAndFillBlackList(out int index, out int totalIndexesInGroup) {
        index = 0;
        totalIndexesInGroup = 0;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.active && npc.type == Type) {
                if (NPC.whoAmI > i)
                    index++;

                totalIndexesInGroup++;
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 100.0; num828++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num510 = 0; num510 < 50; num510++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "PipistrelleGore2".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "PipistrelleGore1".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "PipistrelleGore1".GetGoreType());
        }
    }

    public override void AI() {
        _shouldEnrage = !Main.player[NPC.target].InModBiome<BackwoodsBiome>();
        int lothor = ModContent.NPCType<Lothor>();
        NPC owner = Main.npc[(int)NPC.ai[0]];
        bool flag3 = owner == null || owner.type != lothor || !owner.active;
        if (flag3 && NPC.localAI[3] == 0f) {
            foreach (NPC npc in Main.ActiveNPCs) {
                if (npc.type == lothor) {
                    NPC.ai[0] = npc.whoAmI;
                    owner = Main.npc[(int)NPC.ai[0]];
                    flag3 = owner == null || owner.type != lothor || !owner.active;
                    break;
                }
            }
            NPC.localAI[3] = 1f;
        }
        bool flag2 = flag3 || !owner.As<Lothor>()._shouldEnrage;
        if (flag2) {
            _shouldEnrage = false;
        }

        NPC.OffsetTheSameNPC(0.2f);

        AI_GetMyGroupIndexAndFillBlackList(out int index, out int totalIndexesInGroup);

        float overlapVelocity = 0.04f;
        for (int i = 0; i < Main.maxNPCs; i++) {
            Projectile other = Main.projectile[i];
            if (i != NPC.whoAmI && other.active && Math.Abs(NPC.position.X - other.position.X) + Math.Abs(NPC.position.Y - other.position.Y) < NPC.width / 1.5f) {
                if (NPC.position.X < other.position.X) NPC.velocity.X -= overlapVelocity;
                else NPC.velocity.X += overlapVelocity;

                if (NPC.position.Y < other.position.Y) NPC.velocity.Y -= overlapVelocity;
                else NPC.velocity.Y += overlapVelocity;
            }
        }

        bool flag = !owner.active || owner.ModNPC is null || owner.ModNPC is not Lothor;

        if (_lightingColorValue > 0f) {
            _lightingColorValue -= TimeSystem.LogicDeltaTime;
        }

        if (!Main.dedServ) {
            float value = MathHelper.Clamp(flag ? _lightingColorValue : Math.Max(_lightingColorValue, owner.As<Lothor>().LifeProgress), 0f, 1f);
            Lighting.AddLight(NPC.Top + Vector2.UnitY * NPC.height * 0.1f, new Vector3(1f, 0.2f, 0.2f) * 0.75f * value);
        }

        float lifeProgress = _shouldEnrage ? 1f : flag ? 0f : owner.As<Lothor>().LifeProgress;
        NPC.knockBackResist = 0.5f - 0.5f * lifeProgress;

        if (!flag3) {
            if (owner.life < owner.lifeMax * 0.5f) {
                NPC.defense = (int)(10 * Main.GameModeInfo.EnemyDamageMultiplier);
            }
            else if (owner.life < owner.lifeMax * 0.75f) {
                NPC.defense = (int)(8 * Main.GameModeInfo.EnemyDamageMultiplier);
            }
        }

        void playScreamSound() {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream" + (Main.rand.NextBool(2) ? 1 : 2)) { PitchVariance = 0.1f }, NPC.Center);
        }
        if (NPC.localAI[2] == 0f) {
            NPC.localAI[2] = 1f;
            Vector2 center = owner.Center;
            if (NPC.ai[3] == 3f) {
                NPC.localAI[0] = NPC.Center.X;
                NPC.localAI[1] = center.Y + 10f;
            }
            else if (NPC.ai[3] == 2f) {
                NPC.localAI[0] = NPC.Center.X;
                NPC.localAI[1] = center.Y + 20f;
            }
            else {
                NPC.localAI[0] = center.X + (int)NPC.ai[3] * 100f;
                NPC.localAI[1] = center.Y + 10f;
            }
            Vector2 positionToMove = new(NPC.localAI[0], NPC.localAI[1]);
            NPC.velocity = NPC.DirectionTo(positionToMove) * 5f;
            playScreamSound();
        }
        NPC.ai[1] += Main.rand.NextFloat();
        if (NPC.ai[1] > Main.rand.NextFloat(90f, 130f)) {
            NPC.ai[1] = 0f;
            playScreamSound();
        }
        NPC.localAI[2] += 1f / (index + 1);
        if (NPC.localAI[2] < 15f) {
            NPC.spriteDirection = NPC.direction = NPC.velocity.X.GetDirection();
            return;
        }
        NPC.TargetClosest();
        float speed = 5f + 2f * lifeProgress;
        Player player = Main.player[NPC.target];
        Vector2 destination = player.Center - new Vector2(15f, 100f);
        if (NPC.ai[2] == 0f) {
            NPC.LookAtPlayer(player);
            NPC.SlightlyMoveTo(destination, speed, 12.5f);
            if (NPC.Distance(destination) < 100f || NPC.localAI[2] > 300f) {
                int previousIndex = NPC.whoAmI - 1;
                bool flag5 = true;
                if (previousIndex >= 0 && Main.npc[previousIndex] != null && Main.npc[previousIndex].active && Main.npc[previousIndex].ModNPC is Pipistrelle && Main.npc[previousIndex].ai[2] != 1f) {
                    flag5 = false;
                }
                if (flag5) {
                    NPC.localAI[3]++;
                    if (NPC.localAI[3] > 20f) {
                        NPC.ai[2] = 1f;
                        float speed2 = 6.5f + 1.25f * lifeProgress;
                        float x = NPC.Center.X - player.Center.X;
                        float y = NPC.Center.Y - player.Center.Y;
                        float acceleration = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y) / 4f;
                        acceleration += 10f - acceleration;
                        x -= player.velocity.X * acceleration;
                        y -= player.velocity.Y * acceleration / 4f;
                        x *= 1f;
                        y *= 1f;
                        float sqrt = (float)Math.Sqrt(x * x + y * y);
                        sqrt = speed2 / sqrt;
                        NPC.velocity.X = x * sqrt;
                        NPC.velocity.Y = y * sqrt;
                        _lightingColorValue = 1f;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            int damage = (int)MathHelper.Lerp(Lothor.ACORN_DAMAGE, Lothor.ACORN_DAMAGE2, lifeProgress);
                            damage /= 2;
                            int knockBack = (int)MathHelper.Lerp(Lothor.ACORN_KNOCKBACK, Lothor.ACORN_KNOCKBACK2, lifeProgress);
                            int projectile =
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, new Vector2(-x * sqrt, -y * sqrt) * 1.5f, ModContent.ProjectileType<CursedAcorn>(),
                                damage, knockBack,
                                Main.myPlayer, ai2: player.whoAmI);
                            //NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
                        }
                    }
                }
            }
        }
        else {
            NPC.spriteDirection = NPC.direction = NPC.velocity.X.GetDirection();
            Vector2 distance = player.Center - NPC.Center;
            if (distance.Length() > 1000f) {
                NPC.alpha += 5;
                if (NPC.alpha >= 200) {
                    NPC.KillNPC();
                }
            }
            if (NPC.ai[3] == 1f)
                NPC.velocity.X += 0.5f;
            else
                NPC.velocity.X -= 0.5f;
            NPC.velocity.Y -= 0.1f;

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -speed, speed);
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -speed, speed);
        }
    }
}
