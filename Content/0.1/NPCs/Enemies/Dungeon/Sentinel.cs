using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Dungeon;

[Autoload(false)]
sealed class Sentinel : ModNPC {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Sentinel");
        Main.npcFrameCount[NPC.type] = 15;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Velocity = 1f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetDefaults() {
        NPC.width = 26;
        NPC.height = 42;
        NPC.damage = 40;
        NPC.defense = 12;
        NPC.lifeMax = 120;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.scale = 1f;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.value = Item.buyPrice(silver: 1);
        NPC.knockBackResist = 0.8f;
        NPC.aiStyle = 3;
        NPC.buffImmune[20] = true;
        NPC.buffImmune[70] = true;
        //bannerItem = mod.ItemType("Banner");
        AIType = NPCID.AngryBones;
        AnimationType = NPCID.Skeleton;

        Banner = Type;
        BannerItem = ModContent.ItemType<SentinelBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Sentinel"));
    }

    public override void PostAI() {
        Vector3 rgb3 = new Vector3(0f, 1f, 0.1f) * 0.25f;
        Lighting.AddLight(NPC.Top + new Vector2(0f, 10f), rgb3);
    }

    public override void OnKill() {
        Vector2 spawnAt = NPC.Top + new Vector2(0f, 10f);
        ushort type = (ushort)ModContent.NPCType<SentinelSpirit>();

        if (Main.rand.NextBool(2)) {
            int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X, (int)spawnAt.Y, type);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 origin = new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2;
        Vector2 offset = new Vector2(0f, -2f);
        spriteBatch.Draw(texture, NPC.Center - screenPos + offset, NPC.frame, drawColor * (1f - NPC.alpha / 255f), NPC.rotation, origin, NPC.scale, NPC.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");
        spriteBatch.Draw(texture, NPC.Center - screenPos + offset, NPC.frame, new Color(200, 200, 200, 100) * (1f - NPC.alpha / 255f), NPC.rotation, origin, NPC.scale, NPC.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
        if (Main.rand.NextBool(2))
            target.AddBuff(BuffID.BrokenArmor, 180, true);
    }

    public override void HitEffect(NPC.HitInfo hit) {
        Vector2 spawnAt = NPC.Top + new Vector2(0f, 10f);
        ushort type = (ushort)ModContent.NPCType<SentinelSpirit>();

        if (Main.rand.NextBool(2)) {
            int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X, (int)spawnAt.Y, type);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
        }

        if (NPC.life <= 0) {
            for (int num758 = 0; num758 < 20; num758++) {
                int num759 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 0f, 0f, 50, default, 1.5f);
                Dust dust = Main.dust[num759];
                dust.velocity *= 2f;
                Main.dust[num759].noGravity = true;
            }

            if (!Main.dedServ) {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "UndeadGore1".GetGoreType(), Scale: NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "UndeadGore2".GetGoreType(), Scale: NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "UndeadGore2".GetGoreType(), Scale: NPC.scale);
            }
        }

        //if (NPC.life <= 0) {
        //    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), ModContent.Find<ModGore>(nameof(RiseofAges) + "/UndeadGore1").Type);
        //    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), ModContent.Find<ModGore>(nameof(RiseofAges) + "/UndeadGore2").Type);
        //    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), ModContent.Find<ModGore>(nameof(RiseofAges) + "/UndeadGore3").Type);
        //    for (int i = 0; i < 3; i++) {
        //        int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)spawnAt.X, (int)spawnAt.Y + randomSpawnHeight, type);
        //        if (Main.netMode == NetmodeID.Server)
        //            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
        //    }
        //}
        //for (int k = 0; k < 20; k++) {
        //    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Clentaminator_Green, 2.5f * hitDirection, -2.5f, 0, default(DrawColor), 1);
        //    Main.dust[dust].noGravity = true;
        //}
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        var skeletonsDropRules = Main.ItemDropsDB.GetRulesForNPCID(NPCID.AngryBones, true);
        foreach (var skeletonDropRule in skeletonsDropRules)
            npcLoot.Add(skeletonDropRule);
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SentinelShield>(), 10));
    }

    //public override float SpawnChance(NPCSpawnInfo spawnInfo)
    //    => spawnInfo.Player.ZoneDungeon && spawnInfo.SpawnTileY > Main.rockLayer ? SpawnCondition.Dungeon.Chance * 0.025f : 0f;
}