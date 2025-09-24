using RoA.Common.Druid;
using RoA.Content.Items.Weapons.Nature;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Prefixes;

sealed class NaturePrefix(string name,
                           ushort druidDamage = 0,
                           float druidDamageMult = 1f,
                           ushort potentialDamage = 0,
                           float potentialDamageMult = 1f,
                           int druidCrit = 0,
                           float druidSpeedMult = 1f,
                           float potentialSpeedMult = 1f,
                           float druidKnockback = 0f,
                           float druidKnockbackMult = 1f,
                           float fillingRateMult = 1f,
                           float druidSize = 1f,
                           bool shouldApplyTipsy = false,
                           bool vanillaAdapted = false,
                           bool forClaws = false) : ModPrefix {
    private const string LOCALIZATION = "Mods.RoA.Prefixes.";

    public static string[] BestNotClaws => ["Bountiful", "Blooming", "Godly", "Demonic", "Ruthless"];
    public static string[] BestClaws => ["Legendary", "Godly", "Demonic", "Ruthless"];

    sealed class PrefixLoader : ILoadable {
        void ILoadable.Load(Mod mod) {
            mod.AddContent(new NaturePrefix("Ingrained",
                druidDamageMult: 1f, potentialDamageMult: 1.2f, druidSpeedMult: 0.9f, druidKnockbackMult: 1.12f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Fragrant",
                druidDamageMult: 1.2f, potentialDamageMult: 0.85f, druidSpeedMult: 0.95f, druidKnockbackMult: 1f, druidCrit: 2));
            mod.AddContent(new NaturePrefix("Bountiful",
                druidDamageMult: 1.05f, potentialDamageMult: 1.12f, druidSpeedMult: 1.1f, druidKnockbackMult: 1.05f, druidCrit: 4));
            mod.AddContent(new NaturePrefix("Vivid",
                druidDamageMult: 0.95f, potentialDamageMult: 1.12f, druidSpeedMult: 1.15f, druidKnockbackMult: 0.85f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Overripe",
                druidDamageMult: 1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0, shouldApplyTipsy: true));
            mod.AddContent(new NaturePrefix("Withered",
                druidDamageMult: 0.9f, potentialDamageMult: 0.85f, druidSpeedMult: 1f, druidKnockbackMult: 1.1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Rotten",
                druidDamageMult: 1f, potentialDamageMult: 0.8f, druidSpeedMult: 0.85f, druidKnockbackMult: 0.9f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Ragged",
                druidDamageMult: 0.92f, potentialDamageMult: 0.85f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 10));
            mod.AddContent(new NaturePrefix("Blooming",
                druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1.05f, druidKnockbackMult: 1f, druidCrit: 2));
            mod.AddContent(new NaturePrefix("Fertile",
                druidDamageMult: 1.12f, potentialDamageMult: 1f, druidSpeedMult: 1.08f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Nourished",
                druidDamageMult: 1.08f, potentialDamageMult: 1.12f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new NaturePrefix("Thorny",
                druidDamageMult: 1.1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 5));
            mod.AddContent(new NaturePrefix("Stumpy",
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1.05f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Irritated",
                druidDamageMult: 0.8f, potentialDamageMult: 0.95f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0));

            // vanilla
            mod.AddContent(new NaturePrefix("Keen", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new NaturePrefix("Superior", vanillaAdapted: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1.1f, druidCrit: 3));
            mod.AddContent(new NaturePrefix("Forceful", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Broken", vanillaAdapted: true,
                druidDamageMult: 0.7f, potentialDamageMult: 0.7f, druidSpeedMult: 1f, druidKnockbackMult: 0.8f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Damaged", vanillaAdapted: true,
                druidDamageMult: 0.85f, potentialDamageMult: 0.85f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Shoddy", vanillaAdapted: true,
                druidDamageMult: 0.9f, potentialDamageMult: 0.9f, druidSpeedMult: 1f, druidKnockbackMult: 0.85f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Hurtful", vanillaAdapted: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Strong", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Unpleasant", vanillaAdapted: true,
                druidDamageMult: 1.05f, potentialDamageMult: 1.05f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Weak", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 0.8f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Ruthless", vanillaAdapted: true,
                druidDamageMult: 1.18f, potentialDamageMult: 1.18f, druidSpeedMult: 1f, druidKnockbackMult: 0.9f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Godly", vanillaAdapted: true,
                druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 5));
            mod.AddContent(new NaturePrefix("Demonic", vanillaAdapted: true,
                druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 5));
            mod.AddContent(new NaturePrefix("Zealous", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 5));

            mod.AddContent(new NaturePrefix("Quick", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Deadly", vanillaAdapted: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Agile", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new NaturePrefix("Nimble", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.05f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Murderous", vanillaAdapted: true,
                druidDamageMult: 1.07f, potentialDamageMult: 1.07f, druidSpeedMult: 1.06f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new NaturePrefix("Slow", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.85f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Sluggish", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.8f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Lazy", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.92f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Annoying", vanillaAdapted: true,
                druidDamageMult: 0.8f, potentialDamageMult: 0.8f, druidSpeedMult: 0.85f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Nasty", vanillaAdapted: true,
                druidDamageMult: 0.95f, potentialDamageMult: 0.95f, druidSpeedMult: 1.1f, druidKnockbackMult: 0.9f, druidCrit: 2));

            // melee (claws)
            mod.AddContent(new NaturePrefix("Large", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 1.12f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Massive", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 1.18f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Dangerous", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1.05f, potentialDamageMult: 1.05f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 1.05f, druidCrit: 2));
            mod.AddContent(new NaturePrefix("Savage", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1.1f, druidSize: 1.1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Sharp", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Pointy", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Tiny", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 0.82f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Terrible", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 0.85f, potentialDamageMult: 0.85f, druidSpeedMult: 1f, druidKnockbackMult: 0.85f, druidSize: 0.87f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Small", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 0.9f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Dull", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 0.85f, potentialDamageMult: 0.85f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidSize: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Unhappy", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.9f, druidKnockbackMult: 0.9f, druidSize: 0.9f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Bulky", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1.05f, potentialDamageMult: 1.05f, druidSpeedMult: 0.85f, druidKnockbackMult: 1.1f, druidSize: 1.1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Shameful", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 0.9f, potentialDamageMult: 0.9f, druidSpeedMult: 1f, druidKnockbackMult: 0.8f, druidSize: 1.1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Heavy", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.9f, druidKnockbackMult: 1.15f, druidSize: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Light", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.15f, druidKnockbackMult: 0.9f, druidSize: 1f, druidCrit: 0));
            mod.AddContent(new NaturePrefix("Legendary", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1.1f, druidKnockbackMult: 1.15f, druidSize: 1.1f, druidCrit: 5));
        }

        void ILoadable.Unload() { }
    }

    public static Dictionary<int, NaturePrefix> NaturePrefixes { get; private set; } = [];
    public static Dictionary<int, string> NaturePrefixesNames { get; private set; } = [];

    public override void Load() {

    }

    public override void Unload() {
        NaturePrefixes.Clear();
        NaturePrefixes = null!;
        NaturePrefixesNames.Clear();
        NaturePrefixesNames = null!;
    }

    public override string Name => _name;

    internal readonly string _name = name;
    internal readonly ushort _druidDamage = druidDamage;
    internal readonly float _druidDamageMult = druidDamageMult;
    internal readonly float _druidKnockback = druidKnockback;
    internal readonly float _druidKnockbackMult = druidKnockbackMult;
    internal readonly int _druidCrit = druidCrit;
    internal readonly ushort _potentialDamage = potentialDamage;
    internal readonly float _potentialDamageMult = potentialDamageMult;
    internal readonly float _druidSpeedMult = druidSpeedMult;
    internal readonly float _potentialDruidSpeedMult = potentialSpeedMult;
    internal readonly float _fillingRateMult = fillingRateMult;
    internal readonly float _druidSize = druidSize;
    internal readonly bool _shouldApplyTipsy = shouldApplyTipsy;
    internal readonly bool _vanillaAdapted = vanillaAdapted;
    internal readonly bool _forClaws = forClaws;

    private static LocalizedText GetLocalizedText(string name) => Language.GetOrRegister(LOCALIZATION + name);

    public override LocalizedText DisplayName => GetLocalizedText(_name);

    public override float RollChance(Item item) => 1f;

    public override bool CanRoll(Item item) {
        bool flag = item.ModItem is ClawsBaseItem;
        return item.IsANatureWeapon() && ((!flag && !_forClaws) || (flag && (_forClaws || _vanillaAdapted)));
    }

    public override void SetStaticDefaults() {
        NaturePrefixes.Add(Type, this);
        NaturePrefixesNames.Add(Type, Name);
    }

    public override void ModifyValue(ref float valueMult) {
        switch (NaturePrefixesNames[Type]) {
            case "Ingrained":
                valueMult *= 1.15f;
                break;
            case "Fragrant":
                valueMult *= 1.15f;
                break;
            case "Bountiful":
                valueMult *= 1.6f;
                break;
            case "Vivid":
                valueMult *= 1.15f;
                break;
            case "Overripe":
                valueMult *= 1.05f;
                break;
            case "Withered":
                valueMult *= 1f;
                break;
            case "Rotten":
                valueMult *= 0.7f;
                break;
            case "Ragged":
                valueMult *= 1f;
                break;
            case "Blooming":
                valueMult *= 1.6f;
                break;
            case "Fertile":
                valueMult *= 1.175f;
                break;
            case "Nourished":
                valueMult *= 1.175f;
                break;
            case "Thorny":
                valueMult *= 1.1f;
                break;
            case "Stumpy":
                valueMult *= 1.1f;
                break;
            case "Irritated":
                valueMult *= 0.825f;
                break;

            case "Keen":
                valueMult *= 1.06f;
                break;
            case "Superior":
                valueMult *= 1.3f;
                break;
            case "Forceful":
                valueMult *= 1.15f;
                break;
            case "Broken":
                valueMult *= 0.7f;
                break;
            case "Damaged":
                valueMult *= 0.86f;
                break;
            case "Shoddy":
                valueMult *= 0.8f;
                break;
            case "Hurtful":
                valueMult *= 1.1f;
                break;
            case "Strong":
                valueMult *= 1.15f;
                break;
            case "Unpleasant":
                valueMult *= 1.22f;
                break;
            case "Weak":
                valueMult *= 0.82f;
                break;
            case "Ruthless":
                valueMult *= 1.06f;
                break;
            case "Godly":
                valueMult *= 1.5f;
                break;
            case "Demonic":
                valueMult *= 1.3f;
                break;
            case "Zealous":
                valueMult *= 1.115f;
                break;

            case "Quick":
                valueMult *= 1.11f;
                break;
            case "Deadly":
                valueMult *= 1.23f;
                break;
            case "Agile":
                valueMult *= 1.18f;
                break;
            case "Nimble":
                valueMult *= 1.05f;
                break;
            case "Murderous":
                valueMult *= 1.225f;
                break;
            case "Slow":
                valueMult *= 0.87f;
                break;
            case "Sluggish":
                valueMult *= 0.82f;
                break;
            case "Lazy":
                valueMult *= 0.92f;
                break;
            case "Annoying":
                valueMult *= 0.75f;
                break;
            case "Nasty":
                valueMult *= 1.08f;
                break;

            case "Large":
                valueMult *= 1.125f;
                break;
            case "Massive":
                valueMult *= 1.2f;
                break;
            case "Dangerous":
                valueMult *= 1.16f;
                break;
            case "Savage":
                valueMult *= 1.39f;
                break;
            case "Sharp":
                valueMult *= 1.16f;
                break;
            case "Pointy":
                valueMult *= 1.1f;
                break;
            case "Tiny":
                valueMult *= 0.84f;
                break;
            case "Terrible":
                valueMult *= 0.7f;
                break;
            case "Small":
                valueMult *= 0.91f;
                break;
            case "Dull":
                valueMult *= 0.86f;
                break;
            case "Unhappy":
                valueMult *= 0.77f;
                break;
            case "Bulky":
                valueMult *= 1.08f;
                break;
            case "Shameful":
                valueMult *= 0.82f;
                break;
            case "Heavy":
                valueMult *= 1.035f;
                break;
            case "Light":
                valueMult *= 1.035f;
                break;
            case "Legendary":
                valueMult *= 1.8f;
                break;
        }
    }

    public override void Apply(Item item) {
        if (!item.IsANatureWeapon()) {
            return;
        }
        NatureWeaponHandler natureWeaponHandler = item.GetGlobalItem<NatureWeaponHandler>();
        natureWeaponHandler.ActivePrefix = NaturePrefixes[Type];
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Item item) {
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        string baseExtra = !handler.HasPotentialDamage() ? "1" : string.Empty;
        bool claws = item.ModItem is ClawsBaseItem;
        if (_druidDamage != 0) {
            yield return new TooltipLine(Mod, "ExtraDruidDamage", GetLocalizedText("DruidDamageModifier" + baseExtra).Format(_druidDamage)) {
                IsModifier = true,
                IsModifierBad = _druidDamage < 0
            };
        }
        if (_druidDamageMult != 1f) {
            float value = _druidDamageMult;
            if (!handler.HasPotentialDamage() || _vanillaAdapted) {
                if (_vanillaAdapted && !handler.HasPotentialDamage()) {
                    value = 1f;
                    value += (_druidDamageMult + _potentialDamageMult - 2f) / (_forClaws || claws ? 2f : 4f);
                }
                else {
                    value += _potentialDamageMult;
                    value /= 2f;
                }
            }
            yield return new TooltipLine(Mod, "ExtraDruidDamageMult", GetLocalizedText("DruidDamageModifierMult" + baseExtra).Format(value * 100f - 100f)) {
                IsModifier = true,
                IsModifierBad = value < 1f
            };
        }
        if (_potentialDamage != 0) {
            yield return new TooltipLine(Mod, "ExtraDruidPotentialDamage", GetLocalizedText("DruidPotentialDamageModifier").Format(_potentialDamage)) {
                IsModifier = true,
                IsModifierBad = _potentialDamage < 0
            };
        }
        if (_potentialDamageMult != 1f && handler.HasPotentialDamage()) {
            float value = _potentialDamageMult;
            yield return new TooltipLine(Mod, "ExtraDruidPotentialDamageMult", GetLocalizedText("DruidPotentialDamageModifierMult").Format(value * 100f - 100f)) {
                IsModifier = true,
                IsModifierBad = value < 1f
            };
        }
        if (_druidSpeedMult != 1f) {
            yield return new TooltipLine(Mod, "ExtraDruidSpeed", GetLocalizedText("DruidSpeedModifier").Format(_druidSpeedMult * 100f - 100f)) {
                IsModifier = true,
                IsModifierBad = _druidSpeedMult < 1f
            };
        }
        if (handler.HasPotentialUseSpeed()) {
            if (_potentialDruidSpeedMult != 1f) {
                yield return new TooltipLine(Mod, "ExtraDruidPotentialSpeed", GetLocalizedText("DruidPotentialSpeedModifier").Format(_potentialDruidSpeedMult * 100f - 100f)) {
                    IsModifier = true,
                    IsModifierBad = _potentialDruidSpeedMult < 1f
                };
            }
        }
        if (_druidCrit != 0) {
            yield return new TooltipLine(Mod, "ExtraDruidCrit", GetLocalizedText("DruidCritModifier").Format(_druidCrit)) {
                IsModifier = true,
                IsModifierBad = _druidCrit < 0
            };
        }
        if (_druidSize != 1f) {
            float value = _druidSize;
            yield return new TooltipLine(Mod, "ExtraDruidSize", GetLocalizedText("DruidSizeModifier").Format(value * 100f - 100)) {
                IsModifier = true,
                IsModifierBad = value < 1f
            };
        }
        if (_druidKnockback != 0f) {
            yield return new TooltipLine(Mod, "ExtraDruidKnockback", GetLocalizedText("DruidKnockbackModifier").Format(_druidKnockback)) {
                IsModifier = true,
                IsModifierBad = _druidKnockback < 0f
            };
        }
        if (_druidKnockbackMult != 1f) {
            yield return new TooltipLine(Mod, "ExtraDruidKnockbackMult", GetLocalizedText("DruidKnockbackModifierMult").Format(_druidKnockbackMult * 100f - 100f)) {
                IsModifier = true,
                IsModifierBad = _druidKnockbackMult < 1f
            };
        }
        if (!handler.HasPotentialDamage() && !handler.HasPotentialUseSpeed()) {
            if (_fillingRateMult != 1f) {
                yield return new TooltipLine(Mod, "ExtraDruidFillingRateMult", GetLocalizedText("DruidFillingRateModifierMult").Format(_fillingRateMult * 100f - 100f)) {
                    IsModifier = true,
                    IsModifierBad = _fillingRateMult < 1f
                };
            }
        }
        if (_shouldApplyTipsy) {
            yield return new TooltipLine(Mod, "ApplyTipsyEffect", GetLocalizedText("TipsyEffect").Value) {
                IsModifier = true,
                IsModifierBad = true
            };
        }
    }
}
