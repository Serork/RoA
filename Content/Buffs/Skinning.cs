using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Crafting;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Skinning : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Skinning");
        //Description.SetDefault("Enemies have a chance to drop rawhides, which will spoil when the effect ends");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<SkinningPlayer>().skinning = true;
}

sealed class SkinningPlayer : ModPlayer {
    public bool skinning;

    public override void PostUpdateBuffs() {
        static bool valid(Item item) {
            return !item.IsEmpty() && (item.type == (ushort)ModContent.ItemType<AnimalLeather>() || item.type == (ushort)ModContent.ItemType<RoughLeather>());
        }
        if (skinning) {
            int type = (ushort)ModContent.BuffType<Skinning>();
            if (Player.FindBuffIndex(type) != -1)
                return;
            goto reset;
        }
        else {
            if (Player.whoAmI == Main.myPlayer && !Main.mouseItem.IsEmpty() && valid(Main.mouseItem)) {
                int stack = Main.mouseItem.stack;
                Main.mouseItem.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                Main.mouseItem.stack = stack;
            }
            return;
        }
    reset:
        skinning = false;
        if (Player.chest >= 0) {
            for (int i = 0; i < Main.chest[Player.chest].item.Length; i++) {
                Item item = Main.chest[Player.chest].item[i];
                if (valid(item)) {
                    int stack = item.stack;
                    Main.chest[Player.chest].item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                    Main.chest[Player.chest].item[i].stack = stack;
                }
            }
        }
        for (int i = 0; i < Player.bank.item.Length; i++) {
            Item item = Player.bank.item[i];
            if (valid(item)) {
                int stack = item.stack;
                Player.bank.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                Player.bank.item[i].stack = stack;
            }
        }
        for (int i = 0; i < Player.bank2.item.Length; i++) {
            Item item = Player.bank2.item[i];
            if (valid(item)) {
                int stack = item.stack;
                Player.bank2.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                Player.bank2.item[i].stack = stack;
            }
        }
        for (int i = 0; i < Player.bank3.item.Length; i++) {
            Item item = Player.bank3.item[i];
            if (valid(item)) {
                int stack = item.stack;
                Player.bank3.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                Player.bank3.item[i].stack = stack;
            }
        }
        for (int i = 0; i < Player.bank4.item.Length; i++) {
            Item item = Player.bank3.item[i];
            if (valid(item)) {
                int stack = item.stack;
                Player.bank4.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                Player.bank4.item[i].stack = stack;
            }
        }
        for (int i = 0; i < Player.inventory.Length; i++) {
            Item item = Player.inventory[i];
            if (valid(item)) {
                int stack = item.stack;
                Player.inventory[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
                Player.inventory[i].stack = stack;
            }
        }
        Item trashItem = Player.trashItem;
        if (valid(trashItem)) {
            int stack = trashItem.stack;
            Player.trashItem.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
            Player.trashItem.stack = stack;
        }
        if (Player.whoAmI == Main.myPlayer && valid(Main.mouseItem)) {
            int stack = Main.mouseItem.stack;
            Main.mouseItem.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
            Main.mouseItem.stack = stack;
        }
    }

    public override void PostItemCheck() {
        Item item = Player.inventory[Player.selectedItem];
        if (Player.whoAmI == Main.myPlayer && (item.type == (ushort)ModContent.ItemType<AnimalLeather>() || item.type == (ushort)ModContent.ItemType<RoughLeather>())
            && Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == (ushort)ModContent.TileType<TanningRack>()
            && Player.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost - (float)Player.blockRange <= (float)Player.tileTargetX
            && (Player.position.X + (float)Player.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f + (float)Player.blockRange >= (float)Player.tileTargetX && Player.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost - (float)Player.blockRange <= (float)Player.tileTargetY && (Player.position.Y + (float)Player.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f + (float)Player.blockRange >= (float)Player.tileTargetY) {
            if (Player.ItemTimeIsZero
                && Player.itemAnimation > 0
                && Player.controlUseItem) {
                foreach (Item inventoryItem in Player.inventory)
                    if (inventoryItem.type == item.type) {
                        int removed = Math.Min(inventoryItem.stack, 1);
                        inventoryItem.stack -= removed;
                        if (inventoryItem.stack <= 0)
                            inventoryItem.SetDefaults();
                        break;
                    }
                Vector2 vector = Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition;
                if (Main.SmartCursorIsUsed || PlayerInput.UsingGamepad)
                    vector = Player.Center;
                int item2 = Item.NewItem(Player.GetSource_ItemUse(item), (int)vector.X, (int)vector.Y, 1, 1, ItemID.Leather, 1, noBroadcast: false, -1);
                if (Main.netMode == NetmodeID.MultiplayerClient && item2 >= 0)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item2, 1f);
                Player.ApplyItemTime(item);
                Player.SetItemAnimation(item.useAnimation);
                SoundStyle leatherSound = new(ResourceManager.Sounds + "Leather") {
                    PitchVariance = 0.5f
                };
                SoundEngine.PlaySound(leatherSound);
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new ItemAnimationPacket(Player, item.useAnimation));
                }
                //NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Player.whoAmI);
                //NetMessage.SendData(41, -1, -1, null, Player.whoAmI);
            }
        }
    }
}

sealed class SkinningNPC : GlobalNPC {
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        SkinningDropCondition dropCondition = new();
        IItemDropRule conditionalRule = new LeadingConditionRule(dropCondition);
        int npcType = npc.type;
        bool critters = NPCID.Sets.CountsAsCritter[npcType] && !NPCID.Sets.GoldCrittersCollection.Contains(npcType) &&
            !NPCID.Sets.IsDragonfly[npcType]/* && !NPCID.Sets.TownCritter[npcType]*/ && !npc.FullName.Contains("utterfly") && !npc.FullName.Contains("ragonfly");
        bool enemies = (NPCID.Sets.Zombies[npcType] || npc.DeathSound == SoundID.NPCDeath39 || npc.DeathSound == SoundID.NPCDeath1 || npc.HitSound == SoundID.NPCHit27) && !NPCID.Sets.Skeletons[npcType]
            && !npc.friendly && npc.aiStyle != 22;
        NPCsType type;
        type = critters ? NPCsType.Critters : enemies ? NPCsType.Enemies : NPCsType.None;
        if (type == NPCsType.None)
            return;
        int[] invalidTypes = [677 /*faeling*/, NPCID.SandElemental, NPCID.DungeonSpirit,
                              NPCID.Worm, NPCID.TruffleWorm, NPCID.TruffleWormDigger, NPCID.Grasshopper,
                              NPCID.Firefly, NPCID.EnchantedNightcrawler, NPCID.FairyCritterBlue, NPCID.FairyCritterGreen, NPCID.FairyCritterPink,
                              NPCID.Grubby, NPCID.LadyBug, NPCID.Lavafly, NPCID.LightningBug, NPCID.Maggot, NPCID.Snail, NPCID.GlowingSnail, NPCID.MagmaSnail, NPCID.SeaSnail,
                              NPCID.Sluggy, NPCID.Stinkbug,
                              NPCID.HellButterfly, 661,
                              ];
        if (invalidTypes.Contains(npcType)) {
            return;
        }
        int itemType = type switch {
            NPCsType.Critters => (ushort)ModContent.ItemType<AnimalLeather>(),
            NPCsType.Enemies => (ushort)ModContent.ItemType<RoughLeather>()
        };
        IItemDropRule rule = ItemDropRule.Common(itemType, chanceDenominator: 6);
        conditionalRule.OnSuccess(rule);
        npcLoot.Add(conditionalRule);
    }

    enum NPCsType {
        Critters,
        Enemies,
        None
    }
}