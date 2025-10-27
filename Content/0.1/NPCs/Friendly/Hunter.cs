using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Emotes;
using RoA.Content.Items.Weapons.Magic;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.NPCs.Friendly;

sealed class Hunter : ModNPC {
    // dont forget hjson
    private const byte MAXQUOTES = 17;
    private const byte MAXNOTENOUGHTRADEQUOTES = 3;
    private const byte MAXTRADEQUOTES = 4;
    private const byte MAXFIRELIGHTERQUOTES = 1;
    private const byte TRADEAMOUNTTODROPFIRELIGHTER = 10;

    private const byte LEATHERAMOOUNTNEEDED = 15;
    private const byte GOLDAMOUNTTODROP = 3;

    private const byte MAXNAMES = 9;

    private int _currentQuote, _currentNotEnoughTradeQuote, _currentTradeQuote, _currentFireLighterQuote;

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

        NPCID.Sets.ActsLikeTownNPC[Type] = true;

        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        NPCID.Sets.SpawnsWithCustomName[Type] = true;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Velocity = 1f,
            Direction = -1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<HunterEmote>();

        NPCProfile = new Profiles.StackedNPCProfile(
            new Profiles.DefaultNPCProfile(Texture, -1)
        );

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
    }

    public override bool UsesPartyHat() => false;

    public override bool? CanFallThroughPlatforms() => true;

    public override List<string> SetNPCNameList() {
        string getName(byte index) => Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.Name{index}");
        List<string> names = [];
        for (int i = 0; i < MAXNAMES; i++) {
            names.Add(getName((byte)(i + 1)));
        }
        return names;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Hunter")
        ]);
    }

    public override void SetDefaults() {
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 25;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.homeless = true;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
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

    private void UnlockBestiaryEntry() {
        int type = ModContent.NPCType<Hunter2>();
        var bestiaryEntry = Main.BestiaryDB.FindEntryByNPCID(type);
        if (!(bestiaryEntry == null || bestiaryEntry.Info == null)) {
            if (/*IsNpcOnscreen(NPC.Center) &&*/
                bestiaryEntry.UIInfoProvider.GetEntryUICollectionInfo().UnlockState == Terraria.GameContent.Bestiary.BestiaryEntryUnlockState.NotKnownAtAll_0) {
                //if (Main.netMode == NetmodeID.SinglePlayer) {
                //    NPC.NewNPC(null, (int)NPC.Center.X, (int)NPC.Center.Y, type); 
                //}
                //else {
                //    MultiplayerSystem.SendPacket(new SpawnHunter2Packet(NPC.Center));
                //}
            }
        }
    }

    public override bool PreAI() {
        //UnlockBestiaryEntry();

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
        if (Main.dedServ) {
            return;
        }

        int num236 = 10;
        if (TownNPCProfiles.Instance.GetProfile(NPC, out var profile)) {
            Asset<Texture2D> textureNPCShouldUse = profile.GetTextureNPCShouldUse(NPC);
            int num = 0;
            if (textureNPCShouldUse.IsLoaded) {
                num = textureNPCShouldUse.Height() / Main.npcFrameCount[Type];
                NPC.frame.Width = textureNPCShouldUse.Width();
                NPC.frame.Height = num;
            }

            if (NPC.IsGrounded()) {
                if (NPC.direction == 1)
                    NPC.spriteDirection = 1;

                if (NPC.direction == -1)
                    NPC.spriteDirection = -1;

                int num237 = Main.npcFrameCount[Type] - NPCID.Sets.AttackFrameCount[Type];
                if (NPC.ai[0] == -10f) {
                    ref double frameCounter = ref NPC.frameCounter;
                    frameCounter += 1.5;
                    int num268 = 0;
                    int num269 = 0;

                    if (frameCounter >= 10)
                        num269 = 19;
                    else {
                        num269 = 0;
                    }
                    if (frameCounter >= 20) {
                        frameCounter = 0;
                        num269 = 0;
                    }

                    NPC.frame.Y = num * num269;
                    if (frameCounter >= 420.0)
                        frameCounter = 0.0;
                }
                else if (NPC.ai[0] == -20f) {
                    ref double frameCounter = ref NPC.frameCounter;
                    frameCounter += 1.0;

                    int num268 = 0;
                    if (frameCounter >= 8)
                        num268 = 16;
                    else {
                        num268 = 0;
                    }
                    if (frameCounter >= 16)
                        num268 = 17;
                    if (frameCounter >= 52)
                        num268 = 16;
                    if (frameCounter >= 60)
                        num268 = 0;
                    NPC.frame.Y = num * num268;
                }
                else if (NPC.ai[0] == 23f) {

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

        bool flag0 = false;
        foreach (Player player in Main.ActivePlayers) {
            if (player.talkNPC == NPC.whoAmI) {
                if (player.GetModPlayer<DropHunterRewardHandler>().JustTraded) {
                    NPC.ai[0] = -20f;
                    NPC.frameCounter = 0;
                    NPC.netUpdate = true;
                }
                else {
                    NPC.ai[0] = -5f;
                    NPC.netUpdate = true;
                }
                flag0 = true;
            }
        }

        if (!flag0 && NPC.ai[0] == -5f) {
            NPC.ai[0] = -10f;
            NPC.ai[1] = 100f;

            NPC.frameCounter = 0;

            int emoteType = ModContent.EmoteBubbleType<BackwoodsEmote>();
            if (Main.rand.NextChance(0.75)) {
                if (Main.rand.NextBool(5) || (BackwoodsFogHandler.IsFogActive && Main.rand.NextBool(3))) {
                    emoteType = ModContent.EmoteBubbleType<BackwoodsFogEmote>();
                }
                else if (Main.rand.NextBool(5)) {
                    emoteType = EmoteID.EmoteFear;
                }
                else if (Main.rand.NextBool(8)) {
                    emoteType = ModContent.EmoteBubbleType<HunterEmote>();
                }
                else if (Main.rand.NextBool(6)) {
                    emoteType = ModContent.EmoteBubbleType<LothorEmote>();
                }
            }
            EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(NPC), 100);
        }

        if (NPC.ai[0] == -10f) {
            if (NPC.ai[1] > 0f) {
                NPC.ai[1]--;

                if (NPC.IsGrounded()) {
                    NPC.velocity.X *= 0.8f;
                }

                NPC.direction = NPC.spriteDirection = (Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)].Center.X - NPC.Center.X).GetDirection();
            }
            else {
                NPC.ai[0] = 0f;
            }
        }

        if (NPC.ai[0] == -20f) {
            if (NPC.ai[1] > 0f) {
                NPC.ai[1] -= 2f;

                if (NPC.IsGrounded()) {
                    NPC.velocity.X *= 0.8f;
                }

                NPC.direction = NPC.spriteDirection = (Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)].Center.X - NPC.Center.X).GetDirection();
            }
            else {
                NPC.ai[0] = 0f;
            }
        }

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
                if (NPC.IsGrounded())
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

    public override void SetChatButtons(ref string button, ref string button2) {
        button = Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.Button1");
        int variant = 1;
        //if (HasPlayerEnoughLeatherInInventory())
        {
            variant = 2;
        }
        button2 = Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.Button2_{variant}");
    }

    public override bool CanGoToStatue(bool toKingStatue) => false;

    public override void Load() => On_NPC.GetChat += On_NPC_GetChat;

    private string On_NPC_GetChat(On_NPC.orig_GetChat orig, NPC self) {
        if (self.type == ModContent.NPCType<Hunter>()) {
            UnlockBestiaryEntry();

            return self.As<Hunter>().GetRandomQuote();
        }

        return orig(self);
    }

    private string GetRandomQuote() {
        int currentTradeQuoteIndex = _currentQuote;
        while (_currentQuote == currentTradeQuoteIndex) {
            _currentQuote = Main.rand.Next(MAXQUOTES);
        }
        return Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.Quote{_currentQuote + 1}");
    }

    private string GetQuote() {
        _currentQuote++;
        if (_currentQuote > MAXQUOTES - 1) {
            _currentQuote = 0;
        }
        return Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.Quote{_currentQuote + 1}");
    }

    private string GetNotEnoughTradeQuote() {
        int currentTradeQuoteIndex = _currentNotEnoughTradeQuote;
        if (MAXNOTENOUGHTRADEQUOTES > 1) {
            while (_currentNotEnoughTradeQuote == currentTradeQuoteIndex) {
                _currentNotEnoughTradeQuote = Main.rand.Next(MAXNOTENOUGHTRADEQUOTES);
            }
        }
        return Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.NotEnoughTradeQuote{_currentNotEnoughTradeQuote + 1}");
    }

    private string GetTradeQuote() {
        int currentTradeQuoteIndex = _currentTradeQuote;
        if (MAXTRADEQUOTES > 1) {
            while (_currentTradeQuote == currentTradeQuoteIndex) {
                _currentTradeQuote = Main.rand.Next(MAXTRADEQUOTES);
            }
        }
        return Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.TradeQuote{_currentTradeQuote + 1}");
    }

    private string GetDropFireLighterQuote() {
        int currentFireLighterQuote = _currentFireLighterQuote;
        if (MAXFIRELIGHTERQUOTES > 1) {
            while (_currentFireLighterQuote == currentFireLighterQuote) {
                _currentFireLighterQuote = Main.rand.Next(MAXFIRELIGHTERQUOTES);
            }
        }
        return Language.GetTextValue($"Mods.RoA.NPCs.Town.{nameof(Hunter)}.DropFireLighterQuote{_currentFireLighterQuote + 1}");
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
        if (firstButton) {
            Main.npcChatText = GetQuote();
        }
        else {
            if (ConsumePlayerLeatherAndDropCoins()) {
                return;
            }
            Main.npcChatText = GetNotEnoughTradeQuote();
        }
    }

    internal sealed class DropHunterRewardHandler : ModPlayer {
        public int TradeCount { get; private set; }
        public bool JustTraded { get; private set; }

        public override void SaveData(TagCompound tag) {
            tag[RoA.ModName + nameof(TradeCount)] = TradeCount;
        }

        public override void LoadData(TagCompound tag) {
            TradeCount = tag.GetInt(RoA.ModName + nameof(TradeCount));
        }

        internal void Trade1(bool state) {
            JustTraded = state;
        }

        public void GetHunterReward(NPC hunter, bool shouldGiveFireLighter) {
            int num = ItemID.GoldCoin;
            if (shouldGiveFireLighter) {
                num = ModContent.ItemType<FireLighter>();
            }
            Trade1(true);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new TradedWithHunterPacket1(Player, true));
            }
            Item item = new Item();
            item.SetDefaults(num);
            item.stack = shouldGiveFireLighter ? 1 : GOLDAMOUNTTODROP;
            item.position = Player.Center;
            Item item2 = Player.GetItem(Player.whoAmI, item, GetItemSettings.NPCEntityToPlayerInventorySettings);
            if (item2.stack > 0) {
                int number = Item.NewItem(new EntitySource_Gift(hunter), (int)Player.position.X, (int)Player.position.Y, Player.width, Player.height, item2.type, item2.stack, noBroadcast: false, 0, noGrabDelay: true);
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
                }
            }
            TradeCount++;
        }

        public override void PostUpdate() {
            if (JustTraded && Player.talkNPC == -1) {
                Trade1(false);
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new TradedWithHunterPacket1(Player, false));
                }
            }
        }
    }

    private bool HasPlayerEnoughLeatherInInventory() => Main.LocalPlayer.CountItem(ItemID.Leather) >= LEATHERAMOOUNTNEEDED;
    private bool ShouldGiveFireLighter() => Main.LocalPlayer.GetModPlayer<DropHunterRewardHandler>().TradeCount == TRADEAMOUNTTODROPFIRELIGHTER;

    private bool ConsumePlayerLeatherAndDropCoins() {
        if (!HasPlayerEnoughLeatherInInventory()) {
            return false;
        }

        Player player = Main.LocalPlayer;
        if (ShouldGiveFireLighter()) {
            Main.npcChatText = GetDropFireLighterQuote();
        }
        else {
            Main.npcChatText = GetTradeQuote();
        }
        Main.npcChatCornerItem = 0;

        int num = 0;
        int num2 = 58;
        int num3 = 1;
        int type = ItemID.Leather;
        for (int i = num; i != num2; i += num3) {
            if (player.inventory[i].stack > 0 && player.inventory[i].type == type) {
                player.inventory[i].stack -= LEATHERAMOOUNTNEEDED;
                if (player.inventory[i].stack <= 0) {
                    player.inventory[i].SetDefaults();
                }
            }
        }

        SoundEngine.PlaySound(SoundID.Chat);
        player.GetModPlayer<DropHunterRewardHandler>().GetHunterReward(Main.npc[player.talkNPC], ShouldGiveFireLighter());

        return true;
    }
}
