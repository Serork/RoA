using Microsoft.Xna.Framework;

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
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 150;
        NPC.damage = 12;
        NPC.defense = 6;
        NPC.knockBackResist = 0.05f;
        NPC.Size = new Vector2(32, 32);
        NPC.aiStyle = -1;
        AnimationType = 82;
        NPC.HitSound = SoundID.NPCHit27;
        NPC.DeathSound = SoundID.NPCDeath39;
        NPC.noGravity = true;
        NPC.netAlways = true;
        NPC.noTileCollide = true;
        NPC.value = Item.buyPrice();

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void AI() {
        NPC owner = Main.npc[(int)NPC.ai[0]];
        if (NPC.localAI[2] == 0f) {
            NPC.localAI[2] = 1f;
            NPC.localAI[0] = owner.Center.X + (int)NPC.ai[3] * 100f;
            NPC.localAI[1] = owner.Center.Y + 10f;
            Vector2 positionToMove = new(NPC.localAI[0], NPC.localAI[1]);
            NPC.velocity = NPC.DirectionTo(positionToMove) * 5f;
        }
        NPC.ai[1] += Main.rand.NextFloat();
        if (NPC.ai[1] > Main.rand.NextFloat(90f, 130f)) {
            NPC.ai[1] = 0f;
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream" + (Main.rand.NextBool(2) ? 1 : 2)), NPC.Center);
        }
        NPC.localAI[2]++;
        if (NPC.localAI[2] < 15f) {
            NPC.spriteDirection = NPC.direction = NPC.velocity.X.GetDirection();
            return;
        }
        NPC.TargetClosest();
        Player player = Main.player[NPC.target];
        Vector2 destination = player.position - new Vector2(10f, 50f);
        if (NPC.ai[2] == 0f) {
            NPC.LookAtPlayer(player);
            NPC.SlightlyMoveTo(destination, 5f, 12.5f);
            if (NPC.Distance(destination) < 35f) {
                NPC.localAI[3]++;
                if (NPC.localAI[3] > 20f) {
                    NPC.ai[2] = 1f;
                    float speed2 = 6.5f;
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
                            NPC.damage, 0, Main.myPlayer);
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

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5f, 5f);
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -5f, 5f);
        }
        NPC.rotation = NPC.velocity.X * 0.085f;
        NPC.rotation = MathHelper.Clamp(NPC.rotation, -0.2f, 0.2f);
    }
}
