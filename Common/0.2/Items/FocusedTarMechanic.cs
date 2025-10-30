using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Content.Items.Consumables;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    private static ushort MAXTARENCHANTMENTCOUNT => 3;

    private static Asset<Texture2D> _tarEnchantmentIndicator = null!,
                                    _tarEnchantmentIndicator_Damage = null!,
                                    _tarEnchantmentIndicator_Defense = null!,
                                    _tarEnchantmentIndicator_Life = null!;

    public readonly record struct TarEnchantmentStat(ushort HP = 0, float HPModifier = 1f, ushort Damage = 0, float DamageModifier = 1f, ushort Defense = 0, float DefenseModifier = 1f)
        : TagSerializable {
        public static readonly Func<TagCompound, TarEnchantmentStat> DESERIALIZER = Load;

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

        public int EnchantmentCount 
            => (HP != 0).ToInt() + (HPModifier != 1f).ToInt() + (Damage != 0).ToInt() + (DamageModifier != 1f).ToInt() + (Defense != 0).ToInt() + (DefenseModifier != 1f).ToInt();
    }

    public HashSet<TarEnchantmentStat> ActiveTarEnchantments { get; private set; } = [];

    public static string SaveKey => RoA.ModName + "tarenchantmentstats";

    public partial void TarEnchantmentSetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _tarEnchantmentIndicator = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator");
        _tarEnchantmentIndicator_Damage = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator_Damage");
        _tarEnchantmentIndicator_Defense = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator_Defense");
        _tarEnchantmentIndicator_Life = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator_Life");
    }

    public partial void TarEnchantmentLoad() {
        PlayerCommon.PreItemCheckEvent += PlayerCommon_PreItemCheckEvent;

        On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
        On_ItemSlot.ArmorSwap += On_ItemSlot_ArmorSwap;
    }

    private Item On_ItemSlot_ArmorSwap(On_ItemSlot.orig_ArmorSwap orig, Item item, out bool success) {
        if (HoveredWithTar(item)) {
            success = false;
            return item;
        }

        return orig(item, out success);
    }

    public bool HoveredWithTar(Item item, Action<FocusedTar>? onHovered = null, bool click = true) {
        if (!CanApplyTarEnchantment(item)) {
            return false;
        }
        Item mouseItem = Main.mouseItem;
        if (mouseItem.IsEmpty()) {
            return false;
        }
        if (mouseItem.IsModded(out ModItem modItem) && modItem is FocusedTar focusedTar) {
            if (click) {
                if (mouseItem.stack-- <= 0) {
                    mouseItem.SetDefaults();
                }
            }
            onHovered?.Invoke(focusedTar);
            return true;
        }
        return false;
    }

    private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item) {
        orig(item);

        HoveredWithTar(item, (focusedTar) => {
            item.GetCommon().ApplyTarEnchantment(focusedTar.GetAppliedEnchantment());
        }, true);
    }

    private void PlayerCommon_PreItemCheckEvent(Player player) {
        Item heldItem = player.GetSelectedItem();
        if (heldItem.TryGetGlobalItem(out ItemCommon handler) && handler.HasTarEnchantment()) {
            handler.ActivateTarEnchantments(player, heldItem);
        }
    }

    public override void SaveData(Item item, TagCompound tag) => tag[SaveKey] = ActiveTarEnchantments.ToList();
    public override void LoadData(Item item, TagCompound tag) => ActiveTarEnchantments = [.. tag.GetList<TarEnchantmentStat>(SaveKey)];

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!HasTarEnchantment()) {
            return;
        }
        string active = " when hold";
        if (item.accessory || item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0) {
            active = " when equipped";
        }
        StringBuilder activeBonuses = new();
        GetTarEnchantmentStats(out ushort sumHP, out float sumHPModifier, out ushort sumDamage, out float sumDamageModifier, out ushort sumDefense, out float sumDefenseModifier);
        string plus = "+", minus = "-";
        if (sumHP != 0) {
            string sign = sumHP >= 0 ? plus : minus;
            activeBonuses.Append($"{sign}{sumHP} life, ");
        }
        if (sumHPModifier != 1f) {
            string sign = sumHPModifier >= 1f ? plus : minus;
            activeBonuses.Append($"{sign}{MathUtils.GetPercentageFromModifier(sumHPModifier)}% life, ");
        }
        if (sumDamage != 0) {
            string sign = sumDamage >= 0 ? plus : minus;
            activeBonuses.Append($"{sign}{sumDamage} damage, ");
        }
        if (sumDamageModifier != 1f) {
            string sign = sumDamageModifier >= 1f ? plus : minus;
            activeBonuses.Append($"{sign}{MathUtils.GetPercentageFromModifier(sumDamageModifier)}% damage, ");
        }
        if (sumDefense != 0) {
            string sign = sumDefense >= 0 ? plus : minus;
            activeBonuses.Append($"{sign}{sumDefense} defense, ");
        }
        if (sumDefenseModifier != 1f) {
            string sign = sumDefenseModifier >= 1f ? plus : minus;
            activeBonuses.Append($"{sign}{MathUtils.GetPercentageFromModifier(sumDefenseModifier)}% defense, ");
        }
        TooltipLine line = new(Mod, "tarenchantment", string.Concat(activeBonuses.ToString().AsSpan(0, activeBonuses.Length - 2), $"[c/{Color.White.Hex3()}:{active}]"));
        line.OverrideColor = new Color(153, 134, 158);
        tooltips.Add(line);
        line = new(Mod, "tarenchantmentimage", "Space");
        tooltips.Add(line);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
        if (line.Name == "tarenchantmentimage") {
            float num19 = 1f;
            int num20 = (int)((float)(int)Main.mouseTextColor * num19);
            int num21 = line.X;
            int num22 = line.Y;
            var enchantments = item.GetCommon().ActiveTarEnchantments.ToList();
            for (int i = 0; i < enchantments.Count; i++) {
                Microsoft.Xna.Framework.Color color2 = Microsoft.Xna.Framework.Color.White;
                var enchantment = enchantments[i];
                for (int l = 0; l < 5; l++) {
                    if (l == 4)
                        color2 = new Microsoft.Xna.Framework.Color(num20, num20, num20, num20);

                    color2.A = 150;

                    switch (l) {
                        case 0:
                            num21--;
                            break;
                        case 1:
                            num21++;
                            break;
                        case 2:
                            num22--;
                            break;
                        case 3:
                            num22++;
                            break;
                    }

                    Main.spriteBatch.Draw(_tarEnchantmentIndicator.Value, new Vector2(num21, num22), null, color2, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);

                    bool hp = false,
                         defense = false,
                         damage = false;
                    Vector2 offset = Vector2.Zero,
                            offset2 = offset;
                    int count = enchantment.EnchantmentCount;
                    Vector2 baseOffsetValue = _tarEnchantmentIndicator.Value.Size();
                    Vector2 offsetValue = baseOffsetValue / (float)count * new Vector2(-1f, 1f);
                    for (int i2 = 0; i2 < count - 1; i2++) {
                        offset2 -= offsetValue;
                    }
                    for (int i2 = 0; i2 < count; i2++) {
                        Texture2D enchantmentTexture = _tarEnchantmentIndicator_Life.Value;
                        Texture2D defenseTexture = _tarEnchantmentIndicator_Defense.Value;
                        Texture2D damageTexture = _tarEnchantmentIndicator_Damage.Value;
                        bool chosen = false;
                        bool hasHP = enchantment.HP != 0 || enchantment.HPModifier != 1f;
                        if (!hp && hasHP) {
                            hp = true;
                            chosen = true;
                        }
                        bool hasDamage = enchantment.Damage != 0 || enchantment.DamageModifier != 1f;
                        if (!chosen && !damage && hasDamage) {
                            enchantmentTexture = damageTexture;
                            damage = true;
                            chosen = true;
                        }
                        bool hasDefense = enchantment.Defense != 0 || enchantment.DefenseModifier != 1f;
                        if (!chosen && !defense && hasDefense) {
                            enchantmentTexture = defenseTexture;
                            defense = true;
                            chosen = true;
                        }
                        float sin = (Main.GlobalTimeWrappedHourly + num21 * 10f) % 1f;
                        Main.spriteBatch.Draw(enchantmentTexture, new Vector2(num21, num22)
                            + baseOffsetValue.RotatedBy(sin * MathHelper.TwoPi * (i2 % 2 == 0).ToDirectionInt()) * 0.025f
                            + offset / 2f - offset2 / (count + 1), null, color2, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                        offset -= offsetValue;
                    }
                }
                num21 += (int)(_tarEnchantmentIndicator.Width() * 1.5f);
                //yOffset += 16;

            }

            return false;
        }

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    public bool CanApplyTarEnchantment(Item item) => item.damage > 0 || item.accessory || (!item.vanity && (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0));

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

    public void GetTarEnchantmentStats(out ushort sumHP, out float sumHPModifier, out ushort sumDamage, out float sumDamageModifier, out ushort sumDefense, out float sumDefenseModifier) {
        sumHP = 0;
        sumHPModifier = 1f;
        sumDamage = 0;
        sumDamageModifier = 1f;
        sumDefense = 0;
        sumDefenseModifier = 1f;

        if (!HasTarEnchantment()) {
            return;
        }
        foreach (TarEnchantmentStat tarEnchantmentStat in ActiveTarEnchantments) {
            sumHP += tarEnchantmentStat.HP;
            sumHPModifier *= tarEnchantmentStat.HPModifier;
            sumDamage += tarEnchantmentStat.Damage;
            sumDamageModifier *= tarEnchantmentStat.DamageModifier;
            sumDefense += tarEnchantmentStat.Defense;
            sumDefenseModifier *= tarEnchantmentStat.DefenseModifier;
        }
    }

    public bool HasTarEnchantment() => ActiveTarEnchantments.Count > 0;
    public bool HasEnoughTarEnchantment() => ActiveTarEnchantments.Count >= MAXTARENCHANTMENTCOUNT;
}
