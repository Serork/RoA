using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.BackwoodsSystems;
using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;
using RoA.Core.Utility;

using System;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Friendly;

sealed class Hunter : ModNPC {
    private const int MAXQUOTES = 5;
    private int _currentQuote;

    private static Profiles.StackedNPCProfile NPCProfile;

    private Vector2 _extraVelocity;

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
            //Velocity = 1f,
            //Direction = 1
            Hide = true
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
        NPC.homeless = true;
    }

    public override void OnKill() {
        HunterSpawnSystem.HunterWasKilled = true;
        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Weapons.Magic.FireLighter>()));
    }

    public override bool PreAI() {
        int type = ModContent.NPCType<Hunter2>();
        var bestiaryEntry = Main.BestiaryDB.FindEntryByNPCID(type);
        if (!(bestiaryEntry == null || bestiaryEntry.Info == null)) {
            if (IsNpcOnscreen(NPC.Center) &&
                bestiaryEntry.UIInfoProvider.GetEntryUICollectionInfo().UnlockState != Terraria.GameContent.Bestiary.BestiaryEntryUnlockState.CanShowDropsWithDropRates_4) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, type);
                    if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                        NetMessage.SendData(MessageID.SyncNPC, number: npc);
                    }
                }
            }
        }

        if (HunterSpawnSystem.ShouldDespawnHunter && !IsNpcOnscreen(NPC.Center)) {
            NPC.active = false;
            NPC.netSkip = -1;
            NPC.life = 0;
            return false;
        }

        return true;
    }

    private static bool IsNpcOnscreen(Vector2 center) {
        int w = NPC.sWidth + NPC.safeRangeX * 2;
        int h = NPC.sHeight + NPC.safeRangeY * 2;
        Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
        foreach (Player player in Main.ActivePlayers) {
            if (player.getRect().Intersects(npcScreenRect)) {
                return true;
            }
        }
        return false;
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

                    NPC.frameCounter += Math.Abs(NPC.velocity.X + _extraVelocity.X) * 1f;
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

    public override void AI() {
        NPC.homeTileX = NPC.homeTileY = -1;

        if (NPC.ai[0] == 0f) {
            NPC.ai[1] -= 4f;
            NPC.ai[1] = Math.Max(0f, NPC.ai[1]);
            _extraVelocity.X = 0f;
        }
        else if (NPC.ai[0] == 1f) {
            NPC.ai[1] += 0.5f;
            NPC.ai[1] = Math.Max(0f, NPC.ai[1]);

            bool flag3 = false;
            if (NPC.ai[0] != 24f) {
                for (int j = 0; j < 255; j++) {
                    if (Main.player[j].active && Main.player[j].talkNPC == NPC.whoAmI) {
                        flag3 = true;
                    }
                }
            }
            bool flag13 = false;
            float num8 = 200f;
            if (NPCID.Sets.DangerDetectRange[Type] != -1)
                num8 = NPCID.Sets.DangerDetectRange[Type];
            float num9 = -1f;
            float num10 = -1f;
            int num11 = 0;
            if (Main.netMode != 1 && !flag3) {
                for (int m = 0; m < 200; m++) {
                    if (!Main.npc[m].active || Main.npc[m].friendly || Main.npc[m].damage <= 0 || !(Main.npc[m].Distance(NPC.Center) < num8) || (!Main.npc[m].noTileCollide && !Collision.CanHit(NPC.Center, 0, 0, Main.npc[m].Center, 0, 0)))
                        continue;

                    if (!NPCLoader.CanHitNPC(Main.npc[m], NPC))
                        continue;

                    flag13 = true;
                    float num14 = Main.npc[m].Center.X - NPC.Center.X;
                    if (num14 < 0f && (num9 == -1f || num14 > num9)) {
                        num9 = num14;
                    }

                    if (num14 > 0f && (num10 == -1f || num14 < num10)) {
                        num10 = num14;
                    }
                }

                if (flag13) {
                    num11 = ((num9 == -1f) ? 1 : ((num10 != -1f) ? (num10 < 0f - num9).ToDirectionInt() : (-1)));
                    float num15 = 0f;
                    if (num9 != -1f)
                        num15 = 0f - num9;

                    if (num15 == 0f || (num10 < num15 && num10 > 0f))
                        num15 = num10;

                    if (NPC.ai[0] == 8f) {

                    }
                    else if (NPC.ai[0] != 10f && NPC.ai[0] != 12f && NPC.ai[0] != 13f && NPC.ai[0] != 14f && NPC.ai[0] != 15f) {
                        if (NPCID.Sets.PrettySafe[Type] != -1 && (float)NPCID.Sets.PrettySafe[Type] < num15) {
                            flag13 = false;
                        }
                    }
                }
            }

            float num17 = 0.5f;
            float num18 = 0.02f;

            bool flag17 = Collision.DrownCollision(NPC.position, NPC.width, NPC.height, 1f, includeSlopes: true);

            if (NPC.friendly && (flag13 || flag17)) {
                num17 = 1f;
                float num19 = 1f - (float)NPC.life / (float)NPC.lifeMax;
                num17 += num19 * 0.9f;
                num18 = 0.04f;
            }

            if (_extraVelocity.X < 0f - num17 || _extraVelocity.X > num17) {
                if (NPC.velocity.Y == 0f)
                    _extraVelocity *= 0.8f;
            }
            else if (_extraVelocity.X < num17 && NPC.direction == 1) {
                _extraVelocity.X += num18;
                if (_extraVelocity.X > num17)
                    _extraVelocity.X = num17;
            }
            else if (_extraVelocity.X > 0f - num17 && NPC.direction == -1) {
                _extraVelocity.X -= num18;
                if (_extraVelocity.X < -num17)
                    _extraVelocity.X = -num17;
            }

            NPC.position += _extraVelocity;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_extraVelocity);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _extraVelocity = reader.ReadVector2();
    }

    public override bool NeedSaving() => true;

    public override bool CheckActive() => false;

    public override bool CanChat() => true;

    public override ITownNPCProfile TownNPCProfile() => NPCProfile;

    public override void SetChatButtons(ref string button, ref string button2) => button = Language.GetTextValue($"Mods.RoA.NPC.Quotes.{nameof(Hunter)}.Button1");

    public override bool CanGoToStatue(bool toKingStatue) => false;

    public override void Load() => On_NPC.GetChat += On_NPC_GetChat;

    private string On_NPC_GetChat(On_NPC.orig_GetChat orig, NPC self) {
        if (self.type == ModContent.NPCType<Hunter>()) {
            return self.As<Hunter>().GetQuote();
        }

        return orig(self);
    }

    private string GetQuote() {
        _currentQuote++;
        if (_currentQuote > MAXQUOTES - 1) {
            _currentQuote = 0;
        }
        return Language.GetTextValue($"Mods.RoA.NPC.Quotes.{nameof(Hunter)}.Quote{_currentQuote + 1}");
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
        if (firstButton) {
            Main.npcChatText = GetQuote();
        }
    }
}
