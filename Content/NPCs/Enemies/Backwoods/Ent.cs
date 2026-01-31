using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Ent : RoANPC {
    private int ParentNPCIndex => (int)StateTimer;
    private NPC Parent => Main.npc[ParentNPCIndex];

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 18;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            SpriteDirection = -1,
            Position = new Vector2(10f, 36f),
            PortraitPositionXOverride = 12f,
            PortraitPositionYOverride = -1f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Ent")
        ]);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 450;
        NPC.damage = 35;
        NPC.defense = 6;
        NPC.knockBackResist = 0f;

        int width = 80; int height = 100;
        NPC.Size = new Vector2(width, height);

        NPC.netAlways = true;
        NPC.dontCountMe = true;
        NPC.noTileCollide = true;

        NPC.npcSlots = 1.5f;

        NPC.aiStyle = -1;

        NPC.rarity = 1;

        NPC.HitSound = new SoundStyle(ResourceManager.NPCSounds + "EntHit3") { Pitch = 0f, Volume = 0.75f };
        NPC.DeathSound = SoundID.NPCDeath27;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<EntBanner>();
        ItemID.Sets.KillsToBanner[BannerItem] = 25;
    }

    public override void Load() {
        On_NPC.NPCLoot_DropItems += On_NPC_NPCLoot_DropItems;
        On_NPC.NPCLoot_DropMoney += On_NPC_NPCLoot_DropMoney;
    }

    private void On_NPC_NPCLoot_DropMoney(On_NPC.orig_NPCLoot_DropMoney orig, NPC self, Player closestPlayer) {
        if (self.type == ModContent.NPCType<Ent>() && self.SpawnedFromStatue) {
            return;
        }

        orig(self, closestPlayer);
    }

    private void On_NPC_NPCLoot_DropItems(On_NPC.orig_NPCLoot_DropItems orig, NPC self, Player closestPlayer) {
        if (self.type == ModContent.NPCType<Ent>() && self.SpawnedFromStatue) {
            return;
        }

        orig(self, closestPlayer);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (NPC.IsABestiaryIconDummy) {
            NPC.spriteDirection = NPC.direction = 1;
        }
        else {
            NPCUtils.QuickDraw(NPC, spriteBatch, screenPos, drawColor, NPC.frame, effect: (NPC.spriteDirection * -1).ToSpriteEffects(), yOffset: 2f, xOffset: 10f * NPC.spriteDirection);

            return false;
        }

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NaturesHeart>()));

    public override void HitEffect(NPC.HitInfo hit) {
        int dustType = ModContent.DustType<WoodTrash>();
        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 50.0; num828++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num510 = 0; num510 < 30; num510++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, 2.5f * hit.HitDirection, -2.5f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y), NPC.velocity, "EntGore2".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 10f), NPC.velocity, "EntGore1".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, (Main.rand.NextBool() ? "EntGore1" : "EntGore3").GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 30f), NPC.velocity, "EntGore3".GetGoreType());
        }
    }

    public override void AI() {
        if (ParentNPCIndex <= 0) {
            NPC.KillNPC();
            return;
        }

        NPC npc = Parent;
        if (npc == null || !npc.active) {
            NPC.KillNPC();
        }

        npc.value = NPC.value;

        NPC.realLife = NPC.whoAmI;

        NPC.lifeMax = npc.lifeMax;

        NPC.defense = npc.defense;
        NPC.Center = npc.Center + npc.gfxOffY * Vector2.UnitY - Vector2.UnitY * 32f;
        NPC.velocity = npc.velocity;

        NPC.netUpdate = true;
    }

    public override void OnKill() => Parent.KillNPC();

    public override void FindFrame(int frameHeight) {
        if (NPC.IsABestiaryIconDummy) {
            if (++NPC.frameCounter >= 6.0) {
                NPC.frameCounter = 0.0;
                CurrentFrame++;
                if (CurrentFrame >= 13 || CurrentFrame < 3) {
                    CurrentFrame = 3;
                }
                ChangeFrame(((int)CurrentFrame, frameHeight));
            }
        }

        if (ParentNPCIndex <= 0) {
            return;
        }

        NPC npc = Parent;
        if (npc == null || !npc.active) {
            return;
        }

        NPC.direction = npc.direction;
        NPC.spriteDirection = -NPC.direction;

        ModNPC modNPC = npc.ModNPC;
        if (modNPC != null && modNPC is RoANPC roaNPC) {
            ChangeFrame(((int)roaNPC.CurrentFrame, frameHeight));
        }
    }

    //public override void HitEffect (NPC.HitInfo hit) {
    //	if (Main.netMode == NetmodeID.Server) {
    //		return;
    //	}
    //	if (NPC.life <= 0) {
    //		for (int i = 0; i < Main.rand.Next(3); i++) {
    //			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.Find<ModGore>(nameof(RiseofAges) + "/EntGore1").Type, 1f);
    //		}
    //		for (int i = 0; i < 3; i++) {
    //			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.Find<ModGore>(nameof(RiseofAges) + "/EntGore" + (i + 1).ToString()).Type, 1f);
    //		}
    //		for (int i = 0; i < Main.rand.Next(5, 16); i++) {
    //			Item.NewItem(NPC.GetSource_Loot(), (int) NPC.position.X, (int) NPC.position.Y, NPC.width, NPC.height, ModContent.ItemType<Elderwood>());
    //		}
    //	} else {
    //		if (Main.rand.NextBool(4))
    //			Item.NewItem(NPC.GetSource_Loot(), (int) NPC.position.X, (int) NPC.position.Y, NPC.width, NPC.height, ModContent.ItemType<Elderwood>());
    //	}
    //}
}