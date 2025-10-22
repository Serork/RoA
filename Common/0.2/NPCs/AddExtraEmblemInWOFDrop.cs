using MonoMod.Core.Utils;

using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Accessories.Hardmode;

using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class AddExtraEmblemInWOFDrop : IInitialize {
    void IInitialize.Initialize() {
        On_ItemDropDatabase.RegisterBoss_WOF += On_ItemDropDatabase_RegisterBoss_WOF;
    }

    private void On_ItemDropDatabase_RegisterBoss_WOF(On_ItemDropDatabase.orig_RegisterBoss_WOF orig, ItemDropDatabase self) {
        Conditions.NotExpert condition = new Conditions.NotExpert();
        short type = NPCID.WallofFlesh;
        self.RegisterToNPC(type, ItemDropRule.BossBag(ItemID.WallOfFleshBossBag));
        self.RegisterToNPC(type, ItemDropRule.MasterModeCommonDrop(ItemID.WallofFleshMasterTrophy));
        int masterModeDropRng = 4;
        self.RegisterToNPC(type, ItemDropRule.MasterModeDropOnAllPlayers(ItemID.WallOfFleshGoatMountItem, masterModeDropRng));
        self.RegisterToNPC(type, ItemDropRule.ByCondition(condition, ItemID.FleshMask, chanceDenominator: 7));
        self.RegisterToNPC(type, ItemDropRule.ByCondition(condition, ItemID.Pwnhammer));
        self.RegisterToNPC(type, new LeadingConditionRule(condition)).OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.WarriorEmblem, ItemID.RangerEmblem, ItemID.SorcererEmblem, ItemID.SummonerEmblem, ModContent.ItemType<DruidEmblem>()));
        self.RegisterToNPC(type, new LeadingConditionRule(condition)).OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BreakerBlade, ItemID.ClockworkAssaultRifle, ItemID.LaserRifle, ItemID.FireWhip));
    }
}