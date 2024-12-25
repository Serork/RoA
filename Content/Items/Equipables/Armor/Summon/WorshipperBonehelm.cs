using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Summon;
using RoA.Utilities;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Summon;

[AutoloadEquip(EquipType.Head)]
sealed class WorshipperBonehelm : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Worshipper Bonehelm");
        // Tooltip.SetDefault("Increases your max number of minions by 1" + "\n8% increased minion damage");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.value = Item.sellPrice(silver: 75);
        Item.rare = ItemRarityID.Orange;
        Item.defense = 5;
    }

    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Summon) += 0.08f;
        player.maxMinions += 1;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<WorshipperMantle>() && legs.type == ModContent.ItemType<WorshipperGarb>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.WorshipperSetBonus").WithFormatArgs(Helper.ArmorSetBonusKey, Language.GetTextValue("Controls.RightClick")).Value;

        SummonHarpy(player);
    }

    private void SummonHarpy(Player player) {
        int type = ModContent.ProjectileType<BoneHarpy>();
        if (player.ownedProjectileCounts[type] < 1) {
            Projectile.NewProjectile(player.GetSource_Misc("worshipperarmorset"), player.MountedCenter, Vector2.Zero, type,
                (int)player.GetTotalDamage(DamageClass.Summon).ApplyTo(30),
                player.GetTotalKnockback(DamageClass.Summon).ApplyTo(2.5f),
                player.whoAmI);
        }
    }
}