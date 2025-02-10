using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.NPCs.Friendly;

sealed class Hunter : ModNPC {
    private static Profiles.StackedNPCProfile NPCProfile;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 26;

        NPCID.Sets.ExtraFramesCount[Type] = 9; 
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.DangerDetectRange[Type] = 700; 
        NPCID.Sets.PrettySafe[Type] = 300;
        //NPCID.Sets.AttackType[Type] = 1; 
        NPCID.Sets.AttackTime[Type] = 0;
        NPCID.Sets.AttackAverageChance[Type] = 0;
        //NPCID.Sets.HatOffsetY[Type] = 4;
        //NPCID.Sets.ShimmerTownTransform[NPC.type] = true;

        NPCID.Sets.ActsLikeTownNPC[Type] = true;

        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        NPCID.Sets.SpawnsWithCustomName[Type] = true;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Velocity = 1f,
            Direction = 1
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        //NPCProfile = new Profiles.StackedNPCProfile(
        //    new Profiles.DefaultNPCProfile(Texture, -1),
        //    new Profiles.DefaultNPCProfile(Texture + "_Shimmer", -1)
        //);

        NPCProfile = new Profiles.StackedNPCProfile(
            new Profiles.DefaultNPCProfile(Texture, -1)
        );
    }

    public override bool UsesPartyHat() => false;

    public override void SetDefaults() {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;

        AnimationType = NPCID.Guide;
    }

    public override bool CanChat() => true;

    //public override List<string> SetNPCNameList() {
    //    return new List<string> {
    //            "Blocky Bones",
    //            "Someone's Ribcage",
    //            "Underground Blockster",
    //            "Darkness"
    //        };
    //}

    public override ITownNPCProfile TownNPCProfile() {
        return NPCProfile;
    }

    public override string GetChat() {
        WeightedRandom<string> chat = new WeightedRandom<string>();

        chat.Add("1");
        return chat; 
    }
}
