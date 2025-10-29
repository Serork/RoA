using RoA.Common.Players;
using RoA.Content.Items.Consumables;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    private static ushort MAXTARENCHANTMENTCOUNT => 3;

    public readonly record struct TarEnchantmentStat(ushort HP = 0, float HPModifier = 1f, ushort Damage = 0, float DamageModifier = 1f, ushort Defense = 0, float DefenseModifier = 1f) : TagSerializable {
        public TagCompound SerializeData() {
            return new TagCompound {
                ["hp"] = HP,
                ["hpmodifier"] = HPModifier,
                ["damage"] = Damage,
                ["damagemodifier"] = DamageModifier,
                ["defense"] = Defense,
                ["damagemodifier"] = DefenseModifier,
            };
        }

        public static TarEnchantmentStat Load(TagCompound tag) {
            TarEnchantmentStat tarEnchantmentStat = new(
                (ushort)tag.GetShort("hp"), tag.GetFloat("hpmodifier"), (ushort)tag.GetShort("damage"), tag.GetFloat("damagemodifier"), (ushort)tag.GetShort("defense"), tag.GetFloat("damagemodifier"));
            return tarEnchantmentStat;
        }
    }

    public HashSet<TarEnchantmentStat> ActiveTarEnchantments { get; private set; } = [];

    public static string SaveKey => RoA.ModName + "tarenchantmentstats";

    public partial void TarEnchantmentLoad() {
        PlayerCommon.PreItemCheckEvent += PlayerCommon_PreItemCheckEvent;

        On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
    }

    private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item) {
        orig(item);

        Item mouseItem = Main.mouseItem;
        if (mouseItem.IsEmpty()) {
            return;
        }
        if (mouseItem.IsModded(out ModItem modItem) && modItem is FocusedTar focusedTar) {
            if (mouseItem.stack-- <= 0) {
                mouseItem.SetDefaults();
            }
            item.GetCommon().ApplyTarEnchantment(focusedTar.GetAppliedEnchantment());
        }
    }

    private static void PlayerCommon_PreItemCheckEvent(Player player) {
        Item heldItem = player.GetSelectedItem();
        if (heldItem.TryGetGlobalItem(out ItemCommon handler) && handler.HasTarEnchantment()) {
            handler.ActivateTarEnchantments(player, heldItem);
        }
    }

    public override void SaveData(Item item, TagCompound tag) => tag[SaveKey] = ActiveTarEnchantments.ToList();
    public override void LoadData(Item item, TagCompound tag) => ActiveTarEnchantments = [.. tag.GetList<TarEnchantmentStat>(SaveKey)];

    public void ApplyTarEnchantment(TarEnchantmentStat tarEnchantmentStat) {
        if (HasEnoughTarEnchantment()) {
            return;
        }

        ActiveTarEnchantments.Add(tarEnchantmentStat);
    }

    public void ActivateTarEnchantments(Player player, Item item) {
        foreach (TarEnchantmentStat tarEnchantmentStat in ActiveTarEnchantments) {
            ref int statLife = ref player.statLifeMax2;
            statLife += tarEnchantmentStat.HP;
            statLife = (int)(statLife * tarEnchantmentStat.HPModifier);
            DamageClass damageType = item.DamageType;
            StatModifier damage = player.GetDamage(damageType);
            damage.Flat += tarEnchantmentStat.Damage;
            player.GetDamage(damageType) += tarEnchantmentStat.DamageModifier - 1f;
            ref Player.DefenseStat statDefense = ref player.statDefense;
            statDefense += tarEnchantmentStat.Defense;
            statDefense *= tarEnchantmentStat.DefenseModifier;
        }
    }

    public bool HasTarEnchantment() => ActiveTarEnchantments.Count > 0;
    public bool HasEnoughTarEnchantment() => ActiveTarEnchantments.Count >= MAXTARENCHANTMENTCOUNT;
}
