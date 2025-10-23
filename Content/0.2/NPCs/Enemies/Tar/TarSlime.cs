using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Content.Items.LiquidsSpecific;
using RoA.Content.Projectiles.LiquidsSpecific;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Tar;

sealed class TarSlime : ModNPC {
    public override void SetStaticDefaults() {
        NPC.SetFrameCount(2);
    }

    public override void SetDefaults() {
        NPC.width = 24;
        NPC.height = 18;
        NPC.aiStyle = -1;
        NPC.damage = 5;
        NPC.defense = 5;
        NPC.lifeMax = 300;
        NPC.knockBackResist *= 1.4f;
        //NPC.rarity = 2;
        NPC.scale = 1f;
        NPC.value = Item.buyPrice(0, 15);
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void AI() {
        if (Helper.SinglePlayerOrServer) {
            if (NPC.lavaWet || NPC.onFire || NPC.onFire2 || NPC.onFire3 || NPC.onFrostBurn || NPC.onFrostBurn2 || NPC.shadowFlame) {
                Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<TarExplosion>(), 100, 0f, Main.myPlayer);
                NPC.KillNPC();
                return;
            }
        }

        bool flag = false;
        if (!Main.dayTime || NPC.life != NPC.lifeMax || (double)NPC.position.Y > Main.worldSurface * 16.0 || Main.slimeRain)
            flag = true;

        //if (type == 667)
            flag = true;

        if (NPC.ai[2] > 1f)
            NPC.ai[2] -= 1f;

        if (NPC.wet) {
            if (NPC.collideY)
                NPC.velocity.Y = -2f;

            if (NPC.velocity.Y < 0f && NPC.ai[3] == NPC.position.X) {
                NPC.direction *= -1;
                NPC.ai[2] = 200f;
            }

            if (NPC.velocity.Y > 0f)
                NPC.ai[3] = NPC.position.X;

            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y *= 0.9f;

            NPC.velocity.Y -= 0.5f;
            if (NPC.velocity.Y < -4f)
                NPC.velocity.Y = -4f;

            if (NPC.ai[2] == 1f && flag)
                NPC.TargetClosest();
        }

        NPC.aiAction = 0;
        if (NPC.ai[2] == 0f) {
            NPC.ai[0] = -100f;
            NPC.ai[2] = 1f;
            NPC.TargetClosest();
        }

        NPC.GravityMultiplier *= 0.75f;
        if (NPC.velocity.Y > 0f) {
            NPC.GravityMultiplier *= 3f;
        }

        if (MathF.Abs(NPC.velocity.Y) > NPC.gravity) {
            NPC.localAI[0] = 1f;
        }

        if (NPC.velocity.Y == 0f) {
            if (NPC.collideY && NPC.oldVelocity.Y != 0f) {
                if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height)) {
                    NPC.position.X -= NPC.velocity.X + (float)NPC.direction;
                }
            }

            if (NPC.ai[3] == NPC.position.X) {
                NPC.direction *= -1;
                NPC.ai[2] = 200f;
            }

            if (NPC.localAI[0] != 0f) {
                NPC.localAI[0] = 0f;

                float num5 = NPC.oldVelocity.X;
                if (num5 > 6f)
                    num5 = 6f;

                if (num5 < -6f)
                    num5 = -6f;
                for (int i = 0; i < (int)MathF.Abs(num5) * 4; i++) {
                    int num6 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + (float)NPC.height - 2f), NPC.width, 6, ModContent.DustType<Dusts.SolidifiedTar>(), 0f, 0f, 50, default);
                    Main.dust[num6].noGravity = true;
                    Main.dust[num6].velocity.X *= 1.2f;
                    Main.dust[num6].velocity.Y *= 0.8f;
                    Main.dust[num6].velocity.Y -= NPC.oldVelocity.Y / 2f;
                    Main.dust[num6].velocity *= 0.8f;
                    Main.dust[num6].scale += (float)Main.rand.Next(3) * 0.1f;
                    Main.dust[num6].velocity.X = (Main.dust[num6].position.X - (NPC.position.X + (float)(NPC.width / 2))) * 0.2f;
                    if (Main.dust[num6].velocity.Y > 0f)
                        Main.dust[num6].velocity.Y *= -1f;

                    Main.dust[num6].velocity.X += num5 * 0.3f;
                }
            }

            NPC.ai[3] = 0f;
            if (NPC.velocity.Y == 0f) {
                NPC.velocity.X *= 0.8f;
            }
            if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                NPC.velocity.X = 0f;

            if (flag)
                NPC.ai[0] += 1f;

            NPC.ai[0] += 1.5f;

            float num33 = -1000f;

            num33 = -600f;

            int num34 = 0;
            if (NPC.ai[0] >= 0f)
                num34 = 1;

            if (NPC.ai[0] >= num33 && NPC.ai[0] <= num33 * 0.5f)
                num34 = 2;

            if (NPC.ai[0] >= num33 * 2f && NPC.ai[0] <= num33 * 1.5f)
                num34 = 3;

            if (num34 > 0) {
                NPC.netUpdate = true;
                if (flag && NPC.ai[2] == 1f)
                    NPC.TargetClosest();

                if (num34 == 3) {
                    NPC.velocity.Y = -8f;

                    NPC.velocity.X += 3 * NPC.direction;

                    NPC.ai[0] = -200f;
                    NPC.ai[3] = NPC.position.X;
                }
                else {
                    NPC.velocity.Y = -6f;
                    NPC.velocity.X += 2 * NPC.direction;

                    NPC.ai[0] = -120f;
                    if (num34 == 1)
                        NPC.ai[0] += num33;
                    else
                        NPC.ai[0] += num33 * 2f;
                }
            }
            else if (NPC.ai[0] >= -30f) {
                NPC.aiAction = 1;
            }
        }
        else if (NPC.target < 255 && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f))) {
            if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                NPC.position.X -= 1.4f * (float)NPC.direction;

            if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.position.X -= NPC.velocity.X + (float)NPC.direction;

            if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.01) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.01))
                NPC.velocity.X += 0.2f * (float)NPC.direction;
            else
                NPC.velocity.X *= 0.93f;
        }
    }

    public override void OnKill() {

    }

    public override void HitEffect(NPC.HitInfo hit) {
        int num30 = 7;
        float num31 = 1.1f;
        int num32 = ModContent.DustType<Dusts.SolidifiedTar>();
        Color newColor6 = default(Color);
        if (NPC.life <= 0) {
            num31 = 1.5f;
            num30 = 40;
        }
        else {
        }

        for (int num37 = 0; num37 < num30; num37++) {
            int num38 = Dust.NewDust(NPC.position, NPC.width, NPC.height, num32, 2 * hit.HitDirection, -1f, 50, newColor6, num31);
            if (Main.rand.Next(3) != 0)
                Main.dust[num38].noGravity = true;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        spriteBatch.DrawWithSnapshot(() => {
            TarDyeArmorShaderData.Color1 = Vector3.Lerp(new Vector3(62 / 255f + 0.3f, 53 / 255f + 0.3f, 70 / 255f + 0.3f), Color.White.ToVector3(), 0.5f);
            TarDyeArmorShaderData.Color2 = Vector3.Lerp(new Vector3(98 / 255f + 0.3f, 85 / 255f + 0.3f, 101 / 255f + 0.3f), Color.White.ToVector3(), 0.5f);
            GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<TarDye>()), NPC, NPC.QuickDrawAsDrawData(spriteBatch, screenPos, drawColor * 1f));
            NPC.QuickDraw(spriteBatch, screenPos, drawColor * 1f);
        }, sortMode: SpriteSortMode.Immediate);

        NPC.QuickDraw(spriteBatch, screenPos, drawColor * 0.25f * 1f);

        return false;
    }

    public override void FindFrame(int frameHeight) {
        int num2 = 0;
        if (NPC.aiAction == 0)
            num2 = ((NPC.velocity.Y < 0f) ? 2 : ((NPC.velocity.Y > 0f) ? 3 : ((NPC.velocity.X != 0f) ? 1 : 0)));
        else if (NPC.aiAction == 1)
            num2 = 4;

        NPC.frameCounter += 1.0;
        if (num2 > 0)
            NPC.frameCounter += 1.0;
        if (num2 == 4)
            NPC.frameCounter += 1.0;
        if (NPC.frameCounter >= 8.0) {
            NPC.frame.Y += frameHeight;
            NPC.frameCounter = 0.0;
        }
        if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
            NPC.frame.Y = 0;
    }
}
