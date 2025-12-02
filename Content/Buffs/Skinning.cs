using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.CustomConditions;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Crafting;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.IO;

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
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<SkinningPlayer>().skinning = true;
}

sealed class SpoilLeatherHandler : GlobalItem {
    private static Asset<Texture2D> _expiryTexture = null!;

    public override bool InstancePerEntity => true;

    public ulong StartSpoilingTime;

    public ulong NeedToSpoilTime => 18000;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _expiryTexture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "Expiry");
    }

    public override void SaveData(Item item, TagCompound tag) {
        if (!IsValidToHandle(item)) {
            return;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        tag[RoA.ModName + nameof(handler.StartSpoilingTime)] = handler.StartSpoilingTime;
    }

    public override void LoadData(Item item, TagCompound tag) {
        if (!IsValidToHandle(item)) {
            return;
        }

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        handler.StartSpoilingTime = tag.Get<ulong>(RoA.ModName + nameof(handler.StartSpoilingTime));
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

        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        Texture2D texture = _expiryTexture.Value;
        int height = texture.Height / 5;
        int frames = 5;
        int usedFrame = (int)((TimeSystem.UpdateCount - handler.StartSpoilingTime) / (float)handler.NeedToSpoilTime * frames);
        spriteBatch.Draw(texture, position + TextureAssets.Item[item.type].Size() * 0.2f * item.scale,
            new Rectangle(0, height * usedFrame, texture.Width, height),
            drawColor, 0f, new Vector2(4f), scale, SpriteEffects.None, 0f);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!IsValidToHandle(item)) {
            return;
        }
        var handler = item.GetGlobalItem<SpoilLeatherHandler>();
        if (handler.StartSpoilingTime == 0) {
            return;
        }
        ulong ticks = handler.NeedToSpoilTime - (TimeSystem.UpdateCount - handler.StartSpoilingTime);
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
        bool flag = TimeSystem.UpdateCount > time;
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

        if (item.ModItem is AnimalLeather && handler.StartSpoilingTime == 0) {
            handler.StartSpoilingTime = TimeSystem.UpdateCount;
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
            }
        }
    }

    public override void PostUpdateBuffs() {
        if (!Main.mouseItem.IsAir && Main.mouseItem.ModItem is AnimalLeather) {
            SpoilLeatherHandler.UpdateMe(Main.mouseItem);
        }
        UpdateAllLeatherInInventories();
    }

    public override void PostItemCheck() {
        Item item = Player.inventory[Player.selectedItem];
        if (Player.whoAmI == Main.myPlayer && (item.type == (ushort)ModContent.ItemType<AnimalLeather>() || item.type == (ushort)ModContent.ItemType<RoughLeather>())
            && Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == (ushort)ModContent.TileType<TanningRack>()
            && Player.WithinPlacementRange(Player.tileTargetX, Player.tileTargetY)) {
            if (Player.ItemTimeIsZero
                && Player.itemAnimation > 0
                && Player.controlUseItem) {
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
        /*bool critters = (NPCID.Sets.CountsAsCritter[npcType] || npc.friendly) && !NPCID.Sets.GoldCrittersCollection.Contains(npcType) &&
            !NPCID.Sets.IsDragonfly[npcType] && !npc.FullName.Contains("utterfly") && !npc.FullName.Contains("ragonfly");
        bool enemies = (NPCID.Sets.Zombies[npcType] || npc.DeathSound == SoundID.NPCDeath39 || npc.DeathSound == SoundID.NPCDeath1 || npc.DeathSound == SoundID.NPCDeath31 || npc.HitSound == SoundID.NPCHit27) && !NPCID.Sets.Skeletons[npcType]
            && !npc.friendly && npc.aiStyle != 22;
        NPCsType type;
        type = critters ? NPCsType.Critters : enemies ? NPCsType.Enemies : NPCsType.None;
        if (type == NPCsType.None)
            return;
        int[] invalidTypes = [677, NPCID.SandElemental, NPCID.DungeonSpirit,
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
        */
        bool invalidType = npc.DeathSound == SoundID.NPCDeath6 || npc.SpawnedFromStatue || npc.value == 0 || npc.boss || npc.HitSound == SoundID.NPCHit2 || npc.HitSound == SoundID.NPCHit4 || npc.HitSound == SoundID.NPCHit5 || npc.HitSound == SoundID.NPCHit30 || npc.HitSound == SoundID.NPCHit34 || npc.HitSound == SoundID.NPCHit36 || npc.HitSound == SoundID.NPCHit39 || npc.HitSound == SoundID.NPCHit41 || npc.HitSound == SoundID.NPCHit49 || npc.HitSound == SoundID.NPCHit54;
        invalidType = invalidType || npc.FullName.Contains("Slime") || npc.FullName.Contains("Elemental") || npc.FullName.Contains("Golem") || npc.FullName.Contains("Dandelion") || npc.FullName.Contains("Skeleton") || npc.FullName.Contains("Skull");
        if (invalidType) return;
        int itemType = npc.aiStyle == 3 ? (ushort)ModContent.ItemType<RoughLeather>() : (ushort)ModContent.ItemType<AnimalLeather>();
        IItemDropRule rule = ItemDropRule.Common(itemType, 8);
        conditionalRule.OnSuccess(rule);
        npcLoot.Add(conditionalRule);
    }

    enum NPCsType {
        Critters,
        Enemies,
        None
    }
}