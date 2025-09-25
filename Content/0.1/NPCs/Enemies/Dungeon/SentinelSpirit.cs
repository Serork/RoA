using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Dungeon;

[Autoload(false)]
public class SentinelSpirit : ModNPC {
    public override Color? GetAlpha(Color drawColor) => new Color(200, 200, 200, 100) * (1f - NPC.alpha / 255f);

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Sentinel Spirit");
        Main.npcFrameCount[NPC.type] = 2;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(2f, -16f),
            PortraitPositionXOverride = 2f,
            PortraitPositionYOverride = -35f,
            Velocity = 1f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 10;
        NPC.width = 20;
        NPC.height = 16;
        NPC.damage = 25;
        NPC.defense = 1;
        NPC.aiStyle = 2;
        AnimationType = NPCID.DemonEye;
        NPC.value = 0;
        NPC.knockBackResist = 0.1f;
        NPC.npcSlots = 0.1f;
        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.noTileCollide = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.SentinelSpirit"));
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
        => NPC.lifeMax = 15;

    public override void OnSpawn(IEntitySource source)
        => NPC.velocity += new Vector2(Main.rand.Next(-10, 15), Main.rand.Next(-3, 3)) * 0.1f;

    public override void AI() {
        if (Main.rand.NextBool(4)) {
            int dust = Dust.NewDust(NPC.position - Vector2.UnitY * 2f, NPC.width, NPC.height, DustID.Clentaminator_Green, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f, 100, default, 0.8f);
            Main.dust[dust].noGravity = true;
        }
        Vector3 rgb3 = new Vector3(0f, 1f, 0.1f) * 0.35f;
        Lighting.AddLight(NPC.Center + NPC.velocity, rgb3);
    }

    public override void HitEffect(NPC.HitInfo hit) {
        for (int num351 = 0; num351 < 20; num351++) {
            int num352 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Clentaminator_Green, 2.5f * hit.HitDirection, -2.5f, 0, default, Main.rand.NextFloat(1.1f, 1.3f));
            Dust dust = Main.dust[num352];
            dust.velocity *= 2f;
            Main.dust[num352].noGravity = true;
            Main.dust[num352].scale = 1.4f;
        }

        SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
        //if (NPC.life <= 0) {
        //    for (int k = 0; k < 20; k++) {
        //        int _dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Clentaminator_Green, 2.5f * hit.HitDirection, -2.5f, 0, default(DrawColor), 1.2f);
        //        Main.dust[_dust].noGravity = true;
        //    }
        //}
    }
}