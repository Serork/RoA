using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class GrimrockMimic : ModNPC {
    public override void SetStaticDefaults() {
        NPC.SetFrameCount(6);

        //NPCID.Sets.TrailingMode[Type] = 7;
        NPCID.Sets.SpecificDebuffImmunity[Type][20] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][24] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][31] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][44] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][323] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][324] = true;
    }

    public override void SetDefaults() {
        NPC.width = 24;
        NPC.height = 24;
        NPC.aiStyle = -1;
        NPC.damage = 80;
        NPC.defense = 30;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.value = 100000f;
        NPC.knockBackResist = 0.3f;
        NPC.rarity = 4;
        //NPC.coldDamage = true;
        if (Main.remixWorld && !Main.hardMode) {
            NPC.damage = 30;
            NPC.defense = 12;
            NPC.lifeMax = 300;
            NPC.value = Item.buyPrice(0, 2);
        }

        Banner = Item.NPCtoBanner(NPCID.Mimic);
        BannerItem = Item.BannerToItem(Banner);
        ItemID.Sets.KillsToBanner[BannerItem] = 50;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        //var texture = TextureAssets.Npc[Type].Value;
        //var frame = texture.Frame(verticalFrames: 6, frameY: NPC.frame.Y / NPC.frame.Height % 6);
        //int trailLength = NPCID.Sets.TrailCacheLength[NPC.type];
        //var offset = NPC.Size / 2f + new Vector2(0f, -7f);
        //var origin = frame.Size() / 2f;
        //var spriteDirection = (-NPC.spriteDirection).ToSpriteEffects();
        //for (int i = 0; i < trailLength; i++) {
        //    if (i < trailLength - 1 && (NPC.oldPos[i] - NPC.oldPos[i + 1]).Length() < 1f) {
        //        continue;
        //    }
        //    spriteBatch.Draw(texture, (NPC.oldPos[i] - screenPos + offset).Floor(), frame,
        //        Lighting.GetColor((NPC.oldPos[i] + offset).ToTileCoordinates()) * (1f - 1f / trailLength * i) * 0.4f, NPC.rotation, origin, NPC.scale, spriteDirection, 0f);
        //}
        //spriteBatch.Draw(texture, (NPC.position - screenPos + offset).Floor(), frame,
        //    drawColor, NPC.rotation, origin, NPC.scale, spriteDirection, 0f);
        return true;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        int dustId = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
        int num695 = dustId;

        if (NPC.life > 0) {
            for (int num696 = 0; (double)num696 < hit.Damage / (double)NPC.lifeMax * 50.0; num696++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, num695);
            }

            return;
        }

        for (int num697 = 0; num697 < 20; num697++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, num695);
        }

        int num698 = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y - 10f), new Vector2(hit.HitDirection, 0f), 61, NPC.scale);
        Gore gore2 = Main.gore[num698];
        gore2.velocity *= 0.3f;
        num698 = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + (float)(NPC.height / 2) - 10f), new Vector2(hit.HitDirection, 0f), 62, NPC.scale);
        gore2 = Main.gore[num698];
        gore2.velocity *= 0.3f;
        num698 = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + (float)NPC.height - 10f), new Vector2(hit.HitDirection, 0f), 63, NPC.scale);
        gore2 = Main.gore[num698];
        gore2.velocity *= 0.3f;
    }

    public override void FindFrame(int frameHeight) {
        int num = frameHeight;
        if (NPC.ai[0] == 0f) {
            NPC.frameCounter = 0.0;
            NPC.frame.Y = 0;
        }
        else {
            int num173 = 3;
            if (NPC.velocity.Y == 0f)
                NPC.frameCounter -= 1.0;
            else
                NPC.frameCounter += 1.0;

            if (NPC.frameCounter < 0.0)
                NPC.frameCounter = 0.0;

            if (NPC.frameCounter > (double)(num173 * 4))
                NPC.frameCounter = num173 * 4;

            if (NPC.frameCounter < (double)num173) {
                NPC.frame.Y = num;
            }
            else if (NPC.frameCounter < (double)(num173 * 2)) {
                NPC.frame.Y = num * 2;
            }
            else if (NPC.frameCounter < (double)(num173 * 3)) {
                NPC.frame.Y = num * 3;
            }
            else if (NPC.frameCounter < (double)(num173 * 4)) {
                NPC.frame.Y = num * 4;
            }
            else if (NPC.frameCounter < (double)(num173 * 5)) {
                NPC.frame.Y = num * 5;
            }
            else if (NPC.frameCounter < (double)(num173 * 6)) {
                NPC.frame.Y = num * 4;
            }
            else if (NPC.frameCounter < (double)(num173 * 7)) {
                NPC.frame.Y = num * 3;
            }
            else {
                NPC.frame.Y = num * 2;
                if (NPC.frameCounter >= (double)(num173 * 8))
                    NPC.frameCounter = num173;
            }
        }
        if (NPC.ai[3] == 2f || (NPC.IsABestiaryIconDummy/* && NPC.type == NPCID.Mimic*/))
            NPC.frame.Y += num * 6;
        else if (NPC.ai[3] == 3f)
            NPC.frame.Y += num * 12;
    }

    public override void AI() {
        bool flag25 = NPC.type == NPCID.PresentMimic && !Main.snowMoon;
        if (NPC.ai[3] == 0f) {
            NPC.position.X += 8f;
            if (NPC.position.Y / 16f > (float)Main.UnderworldLayer) {
                NPC.ai[3] = 3f;
            }
            else if ((double)(NPC.position.Y / 16f) > Main.worldSurface) {
                NPC.TargetClosest();
                NPC.ai[3] = 2f;
            }
            else {
                NPC.ai[3] = 1f;
            }
        }

        //if (NPC.type == NPCID.PresentMimic || NPC.type == NPCID.IceMimic)
            NPC.ai[3] = 1f;

        if (NPC.ai[0] == 0f) {
            if (!flag25)
                NPC.TargetClosest();

            if (Main.netMode == 1)
                return;

            if (NPC.velocity.X != 0f || NPC.velocity.Y < 0f || (double)NPC.velocity.Y > 0.3) {
                NPC.ai[0] = 1f;
                NPC.netUpdate = true;
                return;
            }

            Rectangle rectangle3 = new Rectangle((int)Main.player[NPC.target].position.X, (int)Main.player[NPC.target].position.Y, Main.player[NPC.target].width, Main.player[NPC.target].height);
            if (new Rectangle((int)NPC.position.X - 100, (int)NPC.position.Y - 100, NPC.width + 200, NPC.height + 200).Intersects(rectangle3) || NPC.life < NPC.lifeMax) {
                NPC.ai[0] = 1f;
                NPC.netUpdate = true;
            }
        }
        else if (NPC.velocity.Y == 0f) {
            NPC.ai[2] += 1f;
            int num348 = 20;
            if (NPC.ai[1] == 0f)
                num348 = 12;

            if (NPC.ai[2] < (float)num348) {
                NPC.velocity.X *= 0.9f;
                return;
            }

            NPC.ai[2] = 0f;
            if (!flag25)
                NPC.TargetClosest();

            if (NPC.direction == 0)
                NPC.direction = -1;

            NPC.spriteDirection = NPC.direction;
            NPC.ai[1] += 1f;
            if (NPC.ai[1] == 2f) {
                NPC.velocity.X = (float)NPC.direction * 2.5f;
                NPC.velocity.Y = -8f;
                NPC.ai[1] = 0f;
            }
            else {
                NPC.velocity.X = (float)NPC.direction * 3.5f;
                NPC.velocity.Y = -4f;
            }

            NPC.netUpdate = true;
        }
        else if (NPC.direction == 1 && NPC.velocity.X < 1f) {
            NPC.velocity.X += 0.1f;
        }
        else if (NPC.direction == -1 && NPC.velocity.X > -1f) {
            NPC.velocity.X -= 0.1f;
        }
    }
}
