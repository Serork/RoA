using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
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
        NPCID.Sets.AttackTime[Type] = 0;
        NPCID.Sets.AttackAverageChance[Type] = 0;

        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Velocity = 1f,
            Direction = 1
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        NPCProfile = new Profiles.StackedNPCProfile(
            new Profiles.DefaultNPCProfile(Texture, -1)
        );
    }

    public override bool UsesPartyHat() => false;

    public override void SetDefaults() {
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
    }

    public override void FindFrame(int frameHeight) {
        int num236 = 10;
        if (TownNPCProfiles.Instance.GetProfile(NPC, out var profile)) {
            Asset<Texture2D> textureNPCShouldUse = profile.GetTextureNPCShouldUse(NPC);
            int num = 0;
            if (textureNPCShouldUse.IsLoaded) {
                num = textureNPCShouldUse.Height() / Main.npcFrameCount[Type];
                NPC.frame.Width = textureNPCShouldUse.Width();
                NPC.frame.Height = num;
            }

            if (NPC.velocity.Y == 0f) {
                if (NPC.direction == 1)
                    NPC.spriteDirection = 1;

                if (NPC.direction == -1)
                    NPC.spriteDirection = -1;

                int num237 = Main.npcFrameCount[Type] - NPCID.Sets.AttackFrameCount[Type];
                if (NPC.ai[0] == 23f) {

                }
                else if (NPC.ai[0] >= 20f && NPC.ai[0] <= 22f) {

                }
                else if (NPC.ai[0] == 2f) {

                }
                else if (NPC.velocity.X == 0f) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }
                else {
                    int num287 = 6;

                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 2f;
                    NPC.frameCounter += 1.0;

                    int num288 = num * 2;

                    if (NPC.frame.Y < num288)
                        NPC.frame.Y = num288;

                    if (NPC.frameCounter > (double)num287) {
                        NPC.frame.Y += num;
                        NPC.frameCounter = 0.0;
                    }

                    if (NPC.frame.Y / num >= Main.npcFrameCount[Type] - num236)
                        NPC.frame.Y = num288;
                }

                return;
            }
           
            NPC.frameCounter = 0.0;
            NPC.frame.Y = num;
        }
    }

    public override bool NeedSaving() => true;

    public override bool CheckActive() => false;

    public override bool CanChat() => true;

    public override ITownNPCProfile TownNPCProfile() {
        return NPCProfile;
    }

    public override string GetChat() {
        WeightedRandom<string> chat = new();
        string key = $"Mods.RoA.NPC.Quotes.{nameof(Hunter)}.Quote";
        for (int i = 1; i < 4; i++) {
            chat.Add(Language.GetTextValue(key + i.ToString()));
        }
        return chat; 
    }
}
