using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Projectiles.Enemies.Lothor;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed class Pipistrelle : ModNPC {
    private Texture2D ItsSpriteSheet => (Texture2D)ModContent.Request<Texture2D>(Texture);
    private Texture2D GlowMask => (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 150;
        NPC.damage = 12;
        NPC.defense = 6;
        NPC.Size = new Vector2(32, 32);
        NPC.aiStyle = -1;
        AnimationType = 82;
        NPC.HitSound = SoundID.NPCHit27;
        NPC.DeathSound = SoundID.NPCDeath39;
        NPC.noGravity = true;
        NPC.netAlways = true;
        NPC.noTileCollide = true;
        NPC.value = 0;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteEffects effects = NPC.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Vector2 position = NPC.Center - screenPos;
        Vector2 origin = NPC.frame.Size() / 2f;
        if (NPC.ai[2] != 1f) {
            float progress = (Math.Abs(NPC.rotation) / MathHelper.PiOver2) * NPC.spriteDirection;
            spriteBatch.Draw(ModContent.Request<Texture2D>(ResourceManager.EnemyProjectileTextures + "Lothor/CursedAcorn").Value, 
                position + new Vector2(-4f, 8f - (NPC.spriteDirection != 1 ? progress * -20f : 0f)) + Vector2.UnitX * -14f * progress, null, Color.White, NPC.rotation * 0.5f, origin / 2f, NPC.scale, effects, 0);
        }

        spriteBatch.Draw(ItsSpriteSheet, NPC.position - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
        spriteBatch.Draw(GlowMask, NPC.position - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);

        NPC npc = Main.npc[(int)NPC.ai[0]];
        if (!(!npc.active || npc.ModNPC is null || npc.ModNPC is not Lothor)) {
            spriteBatch.BeginBlendState(BlendState.Additive);
            float lifeProgress = npc.As<Lothor>().LifeProgress;
            for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                spriteBatch.Draw(GlowMask, NPC.position - screenPos +
                    Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                    * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress,
                    NPC.frame, Color.White.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress, NPC.rotation + Main.rand.NextFloatRange(0.05f), origin, NPC.scale, effects, 0f);
            }
            spriteBatch.EndBlendState();
        }

        return false;
    }

    public override void AI() {
        NPC.OffsetTheSameNPC(0.2f);

        Lighting.AddLight(NPC.Top + Vector2.UnitY * NPC.height * 0.1f, new Vector3(1f, 0.2f, 0.2f) * 0.75f);

        NPC owner = Main.npc[(int)NPC.ai[0]];
        float lifeProgress = !owner.active || owner.ModNPC is null || owner.ModNPC is not Lothor ? 0f : owner.As<Lothor>().LifeProgress;
        NPC.knockBackResist = 0.1f - 0.1f * lifeProgress;
        void playScreamSound() => SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream" + (Main.rand.NextBool(2) ? 1 : 2)), NPC.Center);
        if (NPC.localAI[2] == 0f) {
            NPC.localAI[2] = 1f;
            NPC.localAI[0] = owner.Center.X + (int)NPC.ai[3] * 100f;
            NPC.localAI[1] = owner.Center.Y + 10f;
            Vector2 positionToMove = new(NPC.localAI[0], NPC.localAI[1]);
            NPC.velocity = NPC.DirectionTo(positionToMove) * 5f;
            playScreamSound();
        }
        NPC.ai[1] += Main.rand.NextFloat();
        if (NPC.ai[1] > Main.rand.NextFloat(90f, 130f)) {
            NPC.ai[1] = 0f;
            playScreamSound();
        }
        NPC.localAI[2]++;
        if (NPC.localAI[2] < 15f) {
            NPC.spriteDirection = NPC.direction = NPC.velocity.X.GetDirection();
            return;
        }
        NPC.TargetClosest();
        float speed = 5f + 2f * lifeProgress;
        Player player = Main.player[NPC.target];
        Vector2 destination = player.Center - new Vector2(10f, 50f);
        if (NPC.ai[2] == 0f) {
            NPC.LookAtPlayer(player);
            NPC.SlightlyMoveTo(destination, speed, 12.5f);
            if (NPC.Distance(destination) < 35f || NPC.localAI[2] > 300f) {
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
                    x *= 0f;
                    y *= 1f + Main.rand.NextFloat(-10f, 11f) * 0.01f;
                    float sqrt = (float)Math.Sqrt(x * x + y * y);
                    sqrt = speed2 / sqrt;
                    NPC.velocity.X = x * sqrt;
                    NPC.velocity.Y = y * sqrt;
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        int projectile = 
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(-x * sqrt, -y * sqrt), ModContent.ProjectileType<CursedAcorn>(),
                            NPC.damage, 0, Main.myPlayer, ai2: player.whoAmI);
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
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
        NPC.rotation = NPC.velocity.X * 0.085f;
        NPC.rotation = MathHelper.Clamp(NPC.rotation, -0.2f, 0.2f);
    }
}
