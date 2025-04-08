using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.CustomConditions;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Crafting;
using RoA.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Buffs;

sealed class Skinning : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Skinning");
        //Description.SetDefault("Enemies have a chance to drop rawhides, which will spoil when the effect ends");
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<SkinningPlayer>().skinning = true;
}

sealed class SpoilLeatherHandler : GlobalItem {
    public override bool InstancePerEntity => true;

    public ulong StartSpoilingTime;

    public ulong NeedToSpoilTime => 1800;

    public override void SaveData(Item item, TagCompound tag) {
        if (!IsValidToHandle(item)) {
            return;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        tag[nameof(handler.StartSpoilingTime)] = handler.StartSpoilingTime;
    }

    public override void LoadData(Item item, TagCompound tag) {
        if (!IsValidToHandle(item)) {
            return;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        handler.StartSpoilingTime = tag.Get<ulong>(nameof(handler.StartSpoilingTime));
    }

    public static bool IsValidToHandle(Item item) => item.ModItem is AnimalLeather;

    public override void NetSend(Item item, BinaryWriter writer) {
        if (!IsValidToHandle(item)) {
            return;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        writer.Write(handler.StartSpoilingTime);    
    }

    public override void NetReceive(Item item, BinaryReader reader) {
        if (!IsValidToHandle(item)) {
            return;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        handler.StartSpoilingTime = reader.ReadUInt64();
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!IsValidToHandle(item)) {
            return;
        }

        float num = item.velocity.X * 0.2f;
        if (item.shimmered)
            num = 0f;

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "Expiry").Value;
        int height = texture.Height / 5;
        int frames = 5;
        int usedFrame = (int)(((ulong)Main.time - handler.StartSpoilingTime) / (float)handler.NeedToSpoilTime * frames);
        //usedFrame = (int)MathHelper.Clamp(usedFrame, 0, frames - 1);
        spriteBatch.Draw(texture, position + frame.Size().RotatedBy(num) * 0.2f * item.scale, 
            new Rectangle(0, height * usedFrame, texture.Width, height),
            drawColor, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!IsValidToHandle(item)) {
            return;
        }
        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        if (handler.StartSpoilingTime == 0) {
            return;
        }
        ulong ticks = handler.NeedToSpoilTime - ((ulong)Main.time - handler.StartSpoilingTime);
        int minutes = (int)(ticks / 3600);
        minutes += 1;
        string text = Language.GetText($"Mods.RoA.ExpireLeather{(minutes <= 1 ? 2 : 1)}").WithFormatArgs(minutes).Value;
        tooltips.Add(new TooltipLine(Mod, "LeatherExpireTooltip", text));
    }

    private static void SpoilLeather(ref Item item) {
        item.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
        item.GetGlobalItem<SpoilLeatherHandler>().StartSpoilingTime = 0;
    }

    public override void UpdateInventory(Item item, Player player) {
        if (!UpdateMe(item)) {
            return;
        }
    }

    public override void PostUpdate(Item item) {
        if (!UpdateMe(item)) {
            return;
        }
    }

    private static void TryToSpoil(ref Item item) {
        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        ulong time = handler.StartSpoilingTime + handler.NeedToSpoilTime;
        bool flag = (ulong)Main.time > time;
        if (!flag) {
            return;
        }
        SpoilLeather(ref item);
    }

    internal static bool UpdateMe(Item item) {
        if (!IsValidToHandle(item)) {
            return false;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        if (Main.IsFastForwardingTime()) {
            SpoilLeather(ref item);
        }

        if (item.ModItem is AnimalLeather && (handler.StartSpoilingTime == 0 ||
            ((!Main.dayTime && Main.time > 32400.0 - handler.NeedToSpoilTime - 2) || (Main.dayTime && Main.time > 54000.0 - handler.NeedToSpoilTime - 2)))) {
            handler.StartSpoilingTime = (ulong)Main.time;
            return true;
        }
        TryToSpoil(ref item);

        return true;
    }
}

sealed class SkinningPlayer : ModPlayer {
    public bool skinning;

    // welcome to terraria
    private void UpdateAllLeatherInInventories() {
        if (Player.whoAmI != Main.myPlayer) {
            return;
        }

        if (Player.trashItem.ModItem is AnimalLeather)
            SpoilLeatherHandler.UpdateMe(Player.trashItem);

        //if (Main.mouseItem.type == 3822)
        //    Main.mouseItem.TurnToAir();

        //for (int i = 0; i < 59; i++) {
        //    Item item = Player.inventory[i];
        //    if (item.stack > 0 && item.ModItem is AnimalLeather)
        //        SpoilLeatherHandler.UpdateMe(Player.trashItem);
        //}

        if (Player.chest == -2) {
            Chest chest = Player.bank;
            for (int j = 0; j < 40; j++) {
                if (chest.item[j].stack > 0 && chest.item[j].ModItem is AnimalLeather)
                    SpoilLeatherHandler.UpdateMe(chest.item[j]);
            }
        }

        if (Player.chest == -4) {
            Chest chest2 = Player.bank3;
            for (int k = 0; k < 40; k++) {
                if (chest2.item[k].stack > 0 && chest2.item[k].ModItem is AnimalLeather)
                    SpoilLeatherHandler.UpdateMe(chest2.item[k]);
            }
        }

        if (Player.chest == -5) {
            Chest chest3 = Player.bank4;
            for (int l = 0; l < 40; l++) {
                if (chest3.item[l].stack > 0 && chest3.item[l].ModItem is AnimalLeather)
                    SpoilLeatherHandler.UpdateMe(chest3.item[l]);
            }
        }

        if (Player.chest == -3) {
            Chest chest4 = Player.bank2;
            for (int m = 0; m < 40; m++) {
                if (chest4.item[m].stack > 0 && chest4.item[m].ModItem is AnimalLeather)
                    SpoilLeatherHandler.UpdateMe(chest4.item[m]);
            }
        }

        if (Player.chest <= -1)
            return;

        Chest chest5 = Main.chest[Player.chest];
        for (int n = 0; n < 40; n++) {
            if (chest5.item[n].stack > 0 && chest5.item[n].ModItem is AnimalLeather) {
                SpoilLeatherHandler.UpdateMe(chest5.item[n]);
                //chest5.item[n].TurnToAir();
            }
        }
    }

    public override void PostUpdateBuffs() {
        if (!Main.mouseItem.IsAir && Main.mouseItem.ModItem is AnimalLeather) {
            SpoilLeatherHandler.UpdateMe(Main.mouseItem);
        }
        UpdateAllLeatherInInventories();
    //    static bool valid(Item item) {
    //        return !item.IsEmpty() && (item.type == (ushort)ModContent.ItemType<AnimalLeather>() || item.type == (ushort)ModContent.ItemType<RoughLeather>());
    //    }
    //    if (skinning) {
    //        int type = (ushort)ModContent.BuffType<Skinning>();
    //        if (Player.FindBuffIndex(type) != -1)
    //            return;
    //        goto reset;
    //    }
    //    else {
    //        if (Player.whoAmI == Main.myPlayer && !Main.mouseItem.IsEmpty() && valid(Main.mouseItem)) {
    //            int stack = Main.mouseItem.stack;
    //            Main.mouseItem.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //            Main.mouseItem.stack = stack;
    //        }
    //        return;
    //    }
    //reset:
    //    skinning = false;
    //    if (Player.chest >= 0) {
    //        for (int i = 0; i < Main.chest[Player.chest].item.Length; i++) {
    //            Item item = Main.chest[Player.chest].item[i];
    //            if (valid(item)) {
    //                int stack = item.stack;
    //                Main.chest[Player.chest].item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //                Main.chest[Player.chest].item[i].stack = stack;
    //            }
    //        }
    //    }
    //    for (int i = 0; i < Player.bank.item.Length; i++) {
    //        Item item = Player.bank.item[i];
    //        if (valid(item)) {
    //            int stack = item.stack;
    //            Player.bank.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //            Player.bank.item[i].stack = stack;
    //        }
    //    }
    //    for (int i = 0; i < Player.bank2.item.Length; i++) {
    //        Item item = Player.bank2.item[i];
    //        if (valid(item)) {
    //            int stack = item.stack;
    //            Player.bank2.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //            Player.bank2.item[i].stack = stack;
    //        }
    //    }
    //    for (int i = 0; i < Player.bank3.item.Length; i++) {
    //        Item item = Player.bank3.item[i];
    //        if (valid(item)) {
    //            int stack = item.stack;
    //            Player.bank3.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //            Player.bank3.item[i].stack = stack;
    //        }
    //    }
    //    for (int i = 0; i < Player.bank4.item.Length; i++) {
    //        Item item = Player.bank3.item[i];
    //        if (valid(item)) {
    //            int stack = item.stack;
    //            Player.bank4.item[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //            Player.bank4.item[i].stack = stack;
    //        }
    //    }
    //    for (int i = 0; i < Player.inventory.Length; i++) {
    //        Item item = Player.inventory[i];
    //        if (valid(item)) {
    //            int stack = item.stack;
    //            Player.inventory[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //            Player.inventory[i].stack = stack;
    //        }
    //    }
    //    Item trashItem = Player.trashItem;
    //    if (valid(trashItem)) {
    //        int stack = trashItem.stack;
    //        Player.trashItem.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //        Player.trashItem.stack = stack;
    //    }
    //    if (Player.whoAmI == Main.myPlayer && valid(Main.mouseItem)) {
    //        int stack = Main.mouseItem.stack;
    //        Main.mouseItem.SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
    //        Main.mouseItem.stack = stack;
    //    }
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
                //foreach (Item inventoryItem in Player.inventory)
                //    if (inventoryItem.type == item.type) {
                //        int removed = Math.Min(inventoryItem.stack, 1);
                //        inventoryItem.stack -= removed;
                //        if (inventoryItem.stack <= 0)
                //            inventoryItem.SetDefaults();
                //        break;
                //    }
                if (--item.stack <= 0) {
                    item.TurnToAir();
                }
                Vector2 vector = Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition;
                if (Main.SmartCursorIsUsed || PlayerInput.UsingGamepad)
                    vector = Player.Center;
                int stack = Main.rand.Next(2, 6);
                int item2 = Item.NewItem(Player.GetSource_ItemUse(item), (int)vector.X, (int)vector.Y, 1, 1, ItemID.Leather, stack, noBroadcast: false, -1);
                if (Main.netMode == NetmodeID.MultiplayerClient && item2 >= 0)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item2, 1f);
                item = new Item();
                item.SetDefaults(ModContent.ItemType<AnimalLeather>());
                item.timeSinceItemSpawned = 0;
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
        if (npc.ModNPC is not null && npc.ModNPC.Mod != RoA.Instance) {
            return;
        }
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