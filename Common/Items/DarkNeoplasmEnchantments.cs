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
using System.IO;
using System.Linq;
using System.Text;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    private static ushort MAXTARENCHANTMENTCOUNT => 4;
    private static ushort LIFEPERSTACK => 20;
    private static float DAMAGEMODIFIERPERSTACK => 0.1f;
    private static ushort KILLCOUNTTOMUTATE => 250;

    private static Asset<Texture2D> _tarEnchantmentIndicator = null!,
                                    _tarEnchantmentIndicator_Inventory1 = null!,
                                    _tarEnchantmentIndicator_Inventory2 = null!,
                                    _tarEnchantmentIndicator_Inventory3 = null!;

    public ulong TarKillCount;
    public float BorderOpacity;

    public static HashSet<Item> ItemToHandleInInventory = [];

    //private static bool _armorSwap;

    //public readonly record struct TarEnchantmentStat(ushort HP = 0, float HPModifier = 1f, ushort Damage = 0, float DamageModifier = 1f, ushort Defense = 0, float DefenseModifier = 1f)
    //    : TagSerializable {
    //    public static readonly Func<TagCompound, TarEnchantmentStat> DESERIALIZER = Load;

    //    public TagCompound SerializeData() {
    //        return new TagCompound {
    //            ["hp"] = HP,
    //            ["hpmodifier"] = HPModifier,
    //            ["damage"] = Damage,
    //            ["damagemodifier"] = DamageModifier,
    //            ["defense"] = Defense,
    //            ["damagemodifier"] = DefenseModifier,
    //        };
    //    }

    //    public static TarEnchantmentStat Load(TagCompound tag) {
    //        TarEnchantmentStat tarEnchantmentStat = new(
    //            (ushort)tag.GetShort("hp"), tag.GetFloat("hpmodifier"), (ushort)tag.GetShort("damage"), tag.GetFloat("damagemodifier"), (ushort)tag.GetShort("defense"), tag.GetFloat("damagemodifier"));
    //        return tarEnchantmentStat;
    //    }

    //    public int EnchantmentCount 
    //        => (HP != 0).ToInt() + (HPModifier != 1f).ToInt() + (Damage != 0).ToInt() + (DamageModifier != 1f).ToInt() + (Defense != 0).ToInt() + (DefenseModifier != 1f).ToInt();
    //}

    public readonly struct TarEnchantmentStat(byte level)
        : TagSerializable {
        private readonly byte _level = level;

        public byte Level => (byte)(_level + 1);

        public static readonly Func<TagCompound, TarEnchantmentStat> DESERIALIZER = Load;

        public TagCompound SerializeData() {
            return new TagCompound {
                ["level"] = Level
            };
        }

        public static TarEnchantmentStat Load(TagCompound tag) {
            TarEnchantmentStat tarEnchantmentStat = new();
            return tarEnchantmentStat;
        }
    }

    public List<TarEnchantmentStat> ActiveTarEnchantments { get; private set; } = [];

    public static string SaveKey => RoA.ModName + "tarenchantmentstats";

    public partial void TarEnchantmentSetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _tarEnchantmentIndicator = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator");
        _tarEnchantmentIndicator_Inventory1 = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator_Inventory1");
        _tarEnchantmentIndicator_Inventory2 = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator_Inventory2");
        _tarEnchantmentIndicator_Inventory3 = ModContent.Request<Texture2D>(ResourceManager.UITextures + "TarEnchantmentIndicator_Inventory3");
    }

    public partial void TarEnchantmentLoad() {
        PlayerCommon.PreItemCheckEvent += PlayerCommon_PreItemCheckEvent;

        //On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
        //On_ItemSlot.ArmorSwap += On_ItemSlot_ArmorSwap;

        On_Player.GrantArmorBenefits += On_Player_GrantArmorBenefits;

        On_ItemSlot.RightClick_ItemArray_int_int += On_ItemSlot_RightClick_ItemArray_int_int;

        On_ItemSlot.MouseHover_ItemArray_int_int += On_ItemSlot_MouseHover_ItemArray_int_int;
        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;

        PlayerCommon.OnHitNPCEvent += PlayerCommon_OnHitNPCEvent;
        PlayerCommon.PreUpdateEvent += PlayerCommon_PreUpdateEvent;
    }

    private void PlayerCommon_PreUpdateEvent(Player player) {
        ItemToHandleInInventory.Clear();
    }

    private void PlayerCommon_OnHitNPCEvent(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.life <= 0 && target.CanActivateOnHitEffect()) {
            foreach (Item item2 in ItemToHandleInInventory) {
                item2.GetCommon().TarKillCount++;
            }
        }
    }

    public override void Unload() {
        ItemToHandleInInventory.Clear();
        ItemToHandleInInventory = null!;
    }

    private void On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig,
        SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor) {
        orig(spriteBatch, inv, context, slot, position, lightColor);

        Player player = Main.player[Main.myPlayer];
        Item hoverItem = inv[slot];
        float inventoryScale = Main.inventoryScale;
        Color color = Color.White;
        if (lightColor != Color.Transparent)
            color = lightColor;
        if (!hoverItem.IsEmpty()) {
            if (hoverItem != Main.mouseItem) {
                Texture2D inventory = _tarEnchantmentIndicator_Inventory1.Value;
                spriteBatch.Draw(inventory, position - new Vector2(2.5f, 2f) * inventoryScale, null, color * MathUtils.Clamp01(hoverItem.GetCommon().BorderOpacity), 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
            }
        }
        //if (hoverItem != Main.mouseItem && !hoverItem.IsEmpty() && hoverItem.GetCommon().HasTarEnchantment()) {
        //    Texture2D inventory = _tarEnchantmentIndicator_Inventory3.Value;
        //    spriteBatch.Draw(inventory, position - new Vector2(2.5f, 0f) * inventoryScale, null, color * 0.3f, 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
        //}
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Player player = Main.player[Main.myPlayer];
        Item hoverItem = item;
        if (hoverItem.GetCommon().HasTarEnchantment()) {
            Texture2D inventory = _tarEnchantmentIndicator_Inventory2.Value;
            position += new Vector2(6f) * Main.inventoryScale;
            float xOffset = 0f;
            for (int i = 0; i < hoverItem.GetCommon().TarEnchantmentCount(); i++) {
                xOffset += inventory.Width / 2f * Main.inventoryScale; 
            }
            for (int i = 0; i < hoverItem.GetCommon().TarEnchantmentCount(); i++) {
                spriteBatch.Draw(inventory, position - Vector2.UnitX * xOffset / 2f, null, drawColor, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);
                position.X += inventory.Width / 2f * Main.inventoryScale;
            }
        }
    }

    private void On_ItemSlot_MouseHover_ItemArray_int_int(On_ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);
        Item hoverItem = inv[slot];
        if (!hoverItem.IsEmpty()) {
            if (context == 8) {
                TryToInsertTar(hoverItem);
            }
            if (HoveredWithTar2(hoverItem, out _) && CanApplyTarEnchantment(hoverItem)) {
                if (!hoverItem.GetCommon().HasTarEnchantment()) {
                    hoverItem.GetCommon().BorderOpacity = 1f;
                }
            }
        }
    }

    private void On_ItemSlot_RightClick_ItemArray_int_int(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        if (TryToInsertTar(inv[slot])) {
            return;
        }
        orig(inv, context, slot);
    }

    private static bool TryToInsertTar(Item item, bool onlyCheck = false) {
        Player player = Main.LocalPlayer;
        if (!item.IsEmpty() && !(player.itemAnimation > 0) && Main.mouseRight && Main.mouseRightRelease) {
            bool flag = false;
            HoveredWithTar(item, (focusedTar) => {
                if (!onlyCheck) {
                    item.GetCommon().ApplyTarEnchantment(focusedTar.GetAppliedEnchantment());
                    item.newAndShiny = false;
                    item.GetCommon().BorderOpacity = 1.5f;
                }
                flag = true;
            }, !onlyCheck);
            if (flag) {
                return true;
            }
        }
        return false;
    }

    private void On_Player_GrantArmorBenefits(On_Player.orig_GrantArmorBenefits orig, Player self, Item armorPiece) {
        orig(self, armorPiece);
        if (armorPiece.TryGetGlobalItem(out ItemCommon handler) && handler.HasTarEnchantment()) {
            handler.ActivateTarEnchantments(self, armorPiece);
        }
    }

    //private Item On_ItemSlot_ArmorSwap(On_ItemSlot.orig_ArmorSwap orig, Item item, out bool success) {
    //    if (HoveredWithTar(item)) {
    //        _armorSwap = true;
    //        success = false;
    //        return item;
    //    }

    //    return orig(item, out success);
    //}

    //private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item) {
    //    orig(item);
    //    HoveredWithTar(item, (focusedTar) => {
    //        item.GetCommon().ApplyTarEnchantment(focusedTar.GetAppliedEnchantment());
    //    }, true);
    //}

    public static bool HoveredWithTar(Item item, Action<DarkNeoplasm>? onHovered = null, bool click = true) {
        if (!CanApplyTarEnchantment(item)) {
            return false;
        }
        Item mouseItem = Main.mouseItem;
        if (mouseItem.IsEmpty()) {
            return false;
        }
        if (item.GetCommon().HasEnoughTarEnchantment()) {
            return false;
        }
        if (mouseItem.IsModded(out ModItem modItem) && modItem is DarkNeoplasm focusedTar) {
            //if (!_armorSwap)
            {
                if (click) {
                    if (mouseItem.stack-- <= 0) {
                        mouseItem.SetDefaults();
                    }
                }
            }
            //else {
            //    _armorSwap = false;
            //}
            onHovered?.Invoke(focusedTar);
            return true;
        }
        return false;
    }

    public static bool HoveredWithTar2(Item item, out DarkNeoplasm focusedTar) {
        focusedTar = null!;
        Item mouseItem = Main.mouseItem;
        if (mouseItem.IsEmpty()) {
            return false;
        }
        if (mouseItem.IsModded(out ModItem modItem) && modItem is DarkNeoplasm focusedTar2) {
            focusedTar = focusedTar2;
            return true;
        }
        return false;
    }

    private void PlayerCommon_PreItemCheckEvent(Player player) {
        Item heldItem = player.GetSelectedItem();
        if (!heldItem.IsEquippable() && heldItem.TryGetGlobalItem(out ItemCommon handler) && handler.HasTarEnchantment()) {
            handler.ActivateTarEnchantments(player, heldItem);
        }
    }

    public override void UpdateInventory(Item item, Player player) {
        ResetEnchantmentsOnFire(item, player);
    }

    public partial void TarEnchantmentUpdateEquip(Item item, Player player) {
        ResetEnchantmentsOnFire(item, player);
    }

    public partial void TarEnchantmentUpdateAccessory(Item item, Player player, bool hideVisual) {
        ResetEnchantmentsOnFire(item, player);
    }

    public void ResetEnchantmentsOnFire(Item item, Player player) {
        var handler2 = item.GetCommon();
        if (!OnFire(player)) {
            if (handler2.BorderOpacity > 0) {
                handler2.BorderOpacity = 0f;
            }

            if (handler2.HasTarEnchantment()) {
                ItemToHandleInInventory.Add(item);
                if (handler2.TarKillCount >= KILLCOUNTTOMUTATE) {
                    handler2.TarKillCount = 0;
                    handler2.ApplyTarEnchantment(new TarEnchantmentStat());
                }
            }
        }
        if (OnFire(player)) {
            if (handler2.HasTarEnchantment()) {
                handler2.ResetEnchantments(player);
            }
        }
    }

    public bool OnFire(Player player) => player.onFire || player.onFire2 || player.onFire3 || player.onFrostBurn || player.onFrostBurn2;

    public override void SaveData(Item item, TagCompound tag) {
        var handler2 = item.GetCommon();
        if (!handler2.HasTarEnchantment()) {
            return;
        }

        tag[SaveKey] = handler2.ActiveTarEnchantments.ToList();
        tag[SaveKey + nameof(handler2.TarKillCount)] = handler2.TarKillCount;
    }

    public override void LoadData(Item item, TagCompound tag) {
        var handler2 = item.GetCommon();
        if (!handler2.HasTarEnchantment()) {
            return;
        }

        handler2.ActiveTarEnchantments = [.. tag.GetList<TarEnchantmentStat>(SaveKey)];
        handler2.TarKillCount = tag.Get<ulong>(SaveKey + nameof(handler2.TarKillCount));
    }

    public override void NetReceive(Item item, BinaryReader reader) {
        var handler2 = item.GetCommon();
        if (!handler2.HasTarEnchantment()) {
            return;
        }

        handler2.TarKillCount = reader.ReadUInt64();
        int count = reader.ReadUInt16();
        for (int i = 0; i < count; i++) {
            handler2.ApplyTarEnchantment(new TarEnchantmentStat());
        }
    }

    public override void NetSend(Item item, BinaryWriter writer) {
        var handler2 = item.GetCommon();
        if (!handler2.HasTarEnchantment()) {
            return;
        }

        writer.Write(handler2.TarKillCount);
        writer.Write((ushort)handler2.TarEnchantmentCount());
    }

    public void RemoveEnchantmentsOnFire(Item item, Player player) {
        if (player.onFire || player.onFire2 || player.onFire3 || player.onFrostBurn || player.onFrostBurn2) {
            var handler2 = item.GetCommon();
            if (handler2.HasTarEnchantment()) {
                handler2.RemoveEnchantments(player);
            }
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!HasTarEnchantment()) {
            return;
        }
        //string active = " when hold";
        //if (item.accessory || item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0) {
        //    active = " when equipped";
        //}
        StringBuilder activeBonuses = new();
        //GetTarEnchantmentStats(out ushort sumHP, out float sumHPModifier, out ushort sumDamage, out float sumDamageModifier, out ushort sumDefense, out float sumDefenseModifier);
        //string plus = "+", minus = "-";
        //if (sumHP != 0) {
        //    string sign = sumHP >= 0 ? plus : minus;
        //    activeBonuses.Append($"{sign}{sumHP} life, ");
        //}
        //if (sumHPModifier != 1f) {
        //    string sign = sumHPModifier >= 1f ? plus : minus;
        //    activeBonuses.Append($"{sign}{MathUtils.GetPercentageFromModifier(sumHPModifier)}% life, ");
        //}
        //if (sumDamage != 0) {
        //    string sign = sumDamage >= 0 ? plus : minus;
        //    activeBonuses.Append($"{sign}{sumDamage} damage, ");
        //}
        //if (sumDamageModifier != 1f) {
        //    string sign = sumDamageModifier >= 1f ? plus : minus;
        //    activeBonuses.Append($"{sign}{MathUtils.GetPercentageFromModifier(sumDamageModifier)}% damage, ");
        //}
        //if (sumDefense != 0) {
        //    string sign = sumDefense >= 0 ? plus : minus;
        //    activeBonuses.Append($"{sign}{sumDefense} defense, ");
        //}
        //if (sumDefenseModifier != 1f) {
        //    string sign = sumDefenseModifier >= 1f ? plus : minus;
        //    activeBonuses.Append($"{sign}{MathUtils.GetPercentageFromModifier(sumDefenseModifier)}% defense, ");
        //}
        GetTarEnchantmentStats(out ushort hpToCut, out float damageToAdd);
        string plus = "+", minus = "-";
        bool hasHPCut = hpToCut != 0;
        if (damageToAdd != 0f) {
            string sign = damageToAdd >= 0 ? plus : minus;
            string text = Language.GetTextValue("Mods.RoA.TarDamage");
            int value = MathUtils.GetPercentageFromModifier(1f + damageToAdd);
            if (hasHPCut) {
                activeBonuses.AppendLine($"{sign}{value}% {text}");
            }
            else {
                activeBonuses.Append($"{sign}{value}% {text}");
            }
        }
        if (hasHPCut) {
            string sign = hpToCut >= 0 ? minus : plus;
            activeBonuses.Append($"{sign}{hpToCut} {Language.GetTextValue("Mods.RoA.TarMaximumLife")}");
        }
        //TooltipLine line = new(Mod, "tarenchantment", string.Concat(activeBonuses.ToString().AsSpan(0, activeBonuses.Length - 2), $"[c/{Color.White.Hex3()}:{active}]"));
        TooltipLine line;
        Color tarColor = new(153, 134, 158);
        line = new(Mod, "tarenchantment", activeBonuses.ToString());
        line.OverrideColor = tarColor;
        tooltips.Add(line);
        line = new(Mod, "tarenchantmentkillcount", $"{Language.GetTextValue("Mods.RoA.TarKillCount")}: {item.GetCommon().TarKillCount}/{KILLCOUNTTOMUTATE}");
        line.OverrideColor = tarColor;
        tooltips.Add(line);
        line = new(Mod, "tarenchantmentimage", "Space");
        tooltips.Add(line);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
        if (line.Name == "tarenchantmentimage") {
            float num19 = 1f;
            int num20 = (int)((float)(int)Main.mouseTextColor * num19);
            Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color(num20, num20, num20, num20);
            int num21 = line.X;
            int num22 = line.Y;
            var enchantments = item.GetCommon().ActiveTarEnchantments.ToList();
            for (int i = 0; i < enchantments.Count; i++) {
                var enchantment = enchantments[i];
                //int count = enchantment.EnchantmentCount;
                //bool hp = false,
                //     defense = false,
                //     damage = false;
                //Vector2 offset = Vector2.Zero,
                //        offset2 = offset;
                //Vector2 baseOffsetValue = _tarEnchantmentIndicator.Value.Size();
                //Vector2 offsetValue = baseOffsetValue / (float)count * new Vector2(-1f, 1f);

                Main.spriteBatch.Draw(_tarEnchantmentIndicator.Value, new Vector2(num21, num22), null, color2, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);

                //for (int i2 = 0; i2 < count - 1; i2++) {
                //    offset2 -= offsetValue;
                //}
                //for (int i2 = 0; i2 < count; i2++) {
                //    ulong seed = (ulong)(item.type + enchantment.GetHashCode());

                //    Texture2D enchantmentTexture = _tarEnchantmentIndicator_Life.Value;
                //    Texture2D defenseTexture = _tarEnchantmentIndicator_Defense.Value;
                //    Texture2D damageTexture = _tarEnchantmentIndicator_Damage.Value;
                //    bool chosen = false;
                //    void hpCheck() {
                //        bool hasHP = enchantment.HP != 0 || enchantment.HPModifier != 1f;
                //        if (!hp && hasHP) {
                //            hp = true;
                //            chosen = true;
                //        }
                //    }
                //    void damageCheck() {
                //        bool hasDamage = enchantment.Damage != 0 || enchantment.DamageModifier != 1f;
                //        if (!chosen && !damage && hasDamage) {
                //            enchantmentTexture = damageTexture;
                //            damage = true;
                //            chosen = true;
                //        }
                //    }
                //    void defenseCheck() {
                //        bool hasDefense = enchantment.Defense != 0 || enchantment.DefenseModifier != 1f;
                //        if (!chosen && !defense && hasDefense) {
                //            enchantmentTexture = defenseTexture;
                //            defense = true;
                //            chosen = true;
                //        }
                //    }
                //    List<Action> checks = [hpCheck, damageCheck, defenseCheck];
                //    List<Action> shuffledChecks = [.. checks.OrderBy(x => Utils.RandomInt(ref seed, 100))];
                //    foreach (Action check in shuffledChecks) {
                //        check();
                //    }

                //    for (int l = 0; l < 5; l++) {
                //        switch (l) {
                //            case 0:
                //                num21--;
                //                break;
                //            case 1:
                //                num21++;
                //                break;
                //            case 2:
                //                num22--;
                //                break;
                //            case 3:
                //                num22++;
                //                break;
                //        }

                //        float sin = TimeSystem.TimeForVisualEffects % 1f;
                //        Main.spriteBatch.Draw(enchantmentTexture, new Vector2(num21, num22)
                //            + baseOffsetValue.RotatedBy(sin * MathHelper.TwoPi * (i2 % 2 == 0).ToDirectionInt()) * 0.025f
                //            + offset / 2f - offset2 / (count + 1), null, color2 with { A = 150 }, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                //    }

                //    offset -= offsetValue;
                //}
                num21 += (int)(_tarEnchantmentIndicator.Width() * 1.5f);
                //yOffset += 16;

            }

            return false;
        }

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    public static bool CanApplyTarEnchantment(Item item) => !item.GetCommon().HasTarEnchantment() && /*item.damage > 0 || */!item.vanity && (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0);

    public void ApplyTarEnchantment(TarEnchantmentStat tarEnchantmentStat) {
        if (HasEnoughTarEnchantment()) {
            return;
        }

        ActiveTarEnchantments.Add(tarEnchantmentStat);
    }

    public void ActivateTarEnchantments(Player player, Item item) {
        foreach (TarEnchantmentStat tarEnchantmentStat in ActiveTarEnchantments) {
            //ref int statLife = ref player.statLifeMax2;
            //statLife += tarEnchantmentStat.HP;
            //statLife = (int)(statLife * tarEnchantmentStat.HPModifier);
            //DamageClass damageType = item.DamageType;
            //StatModifier damage = player.GetDamage(damageType);
            //damage.Flat += tarEnchantmentStat.Damage;
            //player.GetDamage(damageType) += tarEnchantmentStat.DamageModifier - 1f;
            //player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += tarEnchantmentStat.DamageModifier - 1f;
            //ref Player.DefenseStat statDefense = ref player.statDefense;
            //statDefense += tarEnchantmentStat.Defense;
            //statDefense *= tarEnchantmentStat.DefenseModifier;
            GetTarEnchantmentStats(out ushort hpToCut, out float damageToAdd);
            ref int statLife = ref player.statLifeMax2;
            statLife -= hpToCut;
            player.GetDamage(DamageClass.Generic) += damageToAdd;
        }
    }

    public void GetTarEnchantmentStats(out ushort hpToCut, out float damageToAdd) {
        hpToCut = 0;
        damageToAdd = 0f;

        if (!HasTarEnchantment()) {
            return;
        }
        switch (TarEnchantmentCount()) {
            case 1:
                damageToAdd = 0.04f;
                break;
            case 2:
                damageToAdd = 0.08f;
                hpToCut = 40;
                break;
            case 3:
                damageToAdd = 0.16f;
                hpToCut = 80;
                break;
            case 4:
                damageToAdd = 0.24f;
                hpToCut = 120;
                break;
        }
    }

    //public void GetTarEnchantmentStats(out ushort sumHP, out float sumHPModifier, out ushort sumDamage, out float sumDamageModifier, out ushort sumDefense, out float sumDefenseModifier) {
    //    sumHP = 0;
    //    sumHPModifier = 1f;
    //    sumDamage = 0;
    //    sumDamageModifier = 1f;
    //    sumDefense = 0;
    //    sumDefenseModifier = 1f;

    //    if (!HasTarEnchantment()) {
    //        return;
    //    }
    //    foreach (TarEnchantmentStat tarEnchantmentStat in ActiveTarEnchantments) {
    //        sumHP += tarEnchantmentStat.HP;
    //        sumHPModifier *= tarEnchantmentStat.HPModifier;
    //        sumDamage += tarEnchantmentStat.Damage;
    //        sumDamageModifier *= tarEnchantmentStat.DamageModifier;
    //        sumDefense += tarEnchantmentStat.Defense;
    //        sumDefenseModifier *= tarEnchantmentStat.DefenseModifier;
    //    }
    //}

    public bool HasTarEnchantment() => TarEnchantmentCount() > 0;
    public bool HasEnoughTarEnchantment() => TarEnchantmentCount() >= MAXTARENCHANTMENTCOUNT;
    public int TarEnchantmentCount() => ActiveTarEnchantments.Count;

    public void ResetEnchantments(Player player) {
        if (HasTarEnchantment()) {
            //for (int i = 0; i < ActiveTarEnchantments.Count; i++) {
            //    int number = Item.NewItem(player.GetSource_Misc("focusedtardrop"), (int)player.position.X, (int)player.position.Y, player.width, player.height, ModContent.ItemType<FocusedTar>());
            //    if (Main.netMode == NetmodeID.MultiplayerClient) {
            //        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
            //    }
            //}

            ActiveTarEnchantments.Clear();
            ApplyTarEnchantment(new TarEnchantmentStat());
        }
    }

    public void RemoveEnchantments(Player player) {
        if (HasTarEnchantment()) {
            for (int i = 0; i < TarEnchantmentCount(); i++) {
                int number = Item.NewItem(player.GetSource_Misc("focusedtardrop"), (int)player.position.X, (int)player.position.Y, player.width, player.height, ModContent.ItemType<DarkNeoplasm>());
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
                }
            }

            ActiveTarEnchantments.Clear();
        }
    }
}
