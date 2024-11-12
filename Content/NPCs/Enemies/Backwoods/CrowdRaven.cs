using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class CrowdRaven : ModNPC {
	public override void SetStaticDefaults() {
		//base.SetStaticDefaults();

		// DisplayName.SetDefault("Summoned Raven");
		Main.npcFrameCount[Type] = 5;

		//NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
		//	CustomTexturePath = nameof(RiseofAges) + "/Assets/Textures/Bestiary/SummonedRaven",
		//	Position = new Vector2(0f, -16f),
		//	Velocity = 0f,
		//	Frame = 1
		//};
		//NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
	}

	public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 26;

        NPC.friendly = false;

        NPC.aiStyle = -1;

        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 15;

        NPC.npcSlots = 0.4f;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void AI() {
        NPC.noGravity = true;
        if (NPC.localAI[0] == 0f) {
            NPC.localAI[0] = 1f;
            NPC.direction = Main.rand.NextFromList(-1, 1);
        }
        if (NPC.ai[1] == 1f) {
            if (NPC.Opacity <= 0f) {
                NPC.KillNPC();
            }
            else {
                NPC.Opacity -= TimeSystem.LogicDeltaTime;
            }
        }
        if (NPC.ai[3] == 0f) {
            NPC.ai[3] = 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                for (int i = 0; i < Main.rand.Next(3, 6); i++) {
                    int x = (int)(NPC.Center.X + Main.rand.NextFloatRange(50f));
                    int y = (int)NPC.Center.Y;
                    bool flag = false;
                    while (WorldGen.SolidTile(x / 16, y / 16)) {
                        y -= 16;
                        if (Math.Abs(NPC.Center.Y - y) > 100f) {
                            flag = true;
                            break;
                        }
                        if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(x / 16, y / 16).TileType]) {
                            break;
                        }
                    }
                    if (flag) {
                        continue;
                    }
                    int npcSlot = NPC.NewNPC(NPC.GetSource_FromAI(), x, y, Type, ai3: 2f);
                    Main.npc[npcSlot].netUpdate = true;
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcSlot);
                }
            }
        }
        if (NPC.ai[0] != 0f) {
            NPC.noTileCollide = true;
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height)) {
                NPC.ai[1] = 1f;
            }
        }
        if (NPC.ai[0] == 0f) {
            NPC.noGravity = false;
            NPC.TargetClosest();
            if (NPC.ai[2] > 0f) {
                if (NPC.localAI[1] < NPC.ai[2]) {
                    NPC.localAI[1]++;
                }
                else {
                    NPC.ai[0] = 1f;
                    NPC.velocity.Y -= 6f;
                    NPC.direction = Main.rand.NextFromList(-1, 1);
                    if (NPC.ai[3] == 1f && BackwoodsFogHandler.IsFogActive) {
                        Ravencaller.SummonItself(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y);
                    }
                    NPC.netUpdate = true;
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                Rectangle rectangle2 = new((int)Main.player[NPC.target].position.X, (int)Main.player[NPC.target].position.Y, Main.player[NPC.target].width, Main.player[NPC.target].height);
                if (new Rectangle((int)NPC.position.X - 250, (int)NPC.position.Y - 250, NPC.width + 500, NPC.height + 500).Intersects(rectangle2) || NPC.life < NPC.lifeMax) {
                    foreach (NPC npc in Main.ActiveNPCs) {
                        if (npc.whoAmI != NPC.whoAmI && npc.type == Type && NPC.Distance(npc.Center) < 100f && npc.ai[2] == 0f) {
                            npc.ai[2] = Main.rand.NextFloat(15f, 30f);
                            npc.netUpdate = true;
                        }
                    }
                    NPC.ai[0] = 1f;
                    NPC.velocity.Y -= 6f;
                    NPC.direction = Main.rand.NextFromList(-1, 1);
                    if (NPC.ai[3] == 1f && BackwoodsFogHandler.IsFogActive) {
                        Ravencaller.SummonItself(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y);
                    }
                    NPC.netUpdate = true;
                }
            }
        }
        else if (NPC.ai[0] == 2f) {
            NPC.velocity.X *= 0.98f;
            if (NPC.velocity.Y == 0f) {
                NPC.ai[0] = 0f;
                NPC.velocity.X = 0f;
            }

            NPC.velocity.Y += 0.05f;
            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y = 2f;
        }
        else if (!Main.player[NPC.target].dead) {
            float num343 = 3f;
            num343 = 4f;

            //if (NPC.collideX) {
            //    NPC.direction *= -1;
            //    velocity.X = oldVelocity.X * -0.5f;
            //    if (direction == -1 && velocity.X > 0f && velocity.X < num343 - 1f)
            //        velocity.X = num343 - 1f;

            //    if (direction == 1 && velocity.X < 0f && velocity.X > 0f - num343 + 1f)
            //        velocity.X = 0f - num343 + 1f;
            //}

            //if (collideY) {
            //    velocity.Y = oldVelocity.Y * -0.5f;
            //    if (velocity.Y > 0f && velocity.Y < 1f)
            //        velocity.Y = 1f;

            //    if (velocity.Y < 0f && velocity.Y > -1f)
            //        velocity.Y = -1f;
            //}

            if (NPC.direction == -1 && NPC.velocity.X > 0f - num343) {
                NPC.velocity.X -= 0.1f;
                if (NPC.velocity.X > num343)
                    NPC.velocity.X -= 0.1f;
                else if (NPC.velocity.X > 0f)
                    NPC.velocity.X -= 0.05f;

                if (NPC.velocity.X < 0f - num343)
                    NPC.velocity.X = 0f - num343;
            }
            else if (NPC.direction == 1 && NPC.velocity.X < num343) {
                NPC.velocity.X += 0.1f;
                if (NPC.velocity.X < 0f - num343)
                    NPC.velocity.X += 0.1f;
                else if (NPC.velocity.X < 0f)
                    NPC.velocity.X += 0.05f;

                if (NPC.velocity.X > num343)
                    NPC.velocity.X = num343;
            }

            int num344 = (int)((NPC.position.X + (float)(NPC.width / 2)) / 16f) + NPC.direction;
            int num345 = (int)((NPC.position.Y + (float)NPC.height) / 16f);
            bool flag23 = true;
            int num346 = 15;
            bool flag24 = false;
            for (int num347 = num345; num347 < num345 + num346; num347++) {
                if (!WorldGen.InWorld(num344, num347))
                    continue;

                //if (Main.tile[num344, num347] == null)
                //    Main.tile[num344, num347] = new Tile();

                if ((Main.tile[num344, num347].HasUnactuatedTile && Main.tileSolid[Main.tile[num344, num347].TileType]) || Main.tile[num344, num347].LiquidAmount > 0) {
                    if (num347 < num345 + 5)
                        flag24 = true;

                    flag23 = false;
                    break;
                }
            }

            if (flag23)
                NPC.velocity.Y += 0.05f;
            else
                NPC.velocity.Y -= 0.1f;

            if (flag24)
                NPC.velocity.Y -= 0.2f;

            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y = 2f;

            if (NPC.velocity.Y < -4f)
                NPC.velocity.Y = -4f;
        }
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = -NPC.direction;
        NPC.rotation = NPC.velocity.X * 0.1f;
        if (NPC.velocity.X == 0f && NPC.velocity.Y == 0f) {
            NPC.frame.Y = 0;
            NPC.frameCounter = 0.0;
            return;
        }

        int num83 = Main.npcFrameCount[NPC.type] - 1;
        NPC.frameCounter += 1.0;
        if (NPC.frameCounter >= 4.0) {
            NPC.frame.Y += frameHeight;
            NPC.frameCounter = 0.0;
        }

        if (NPC.frame.Y >= frameHeight * num83)
            NPC.frame.Y = frameHeight;
    }
}