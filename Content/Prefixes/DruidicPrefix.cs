using RoA.Common.Druid;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Drawing;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Prefixes;
sealed class DruidicPrefix(string name,
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

    sealed class PrefixLoader : ILoadable {
		void ILoadable.Load(Mod mod) {
			mod.AddContent(new DruidicPrefix("Ingrained",
                druidDamageMult: 1f, potentialDamageMult: 1.2f, druidSpeedMult: 0.9f, druidKnockbackMult: 1.12f, druidCrit: 0));
			mod.AddContent(new DruidicPrefix("Fragrant", 
				druidDamageMult: 1.2f, potentialDamageMult: 0.85f, druidSpeedMult: 0.95f, druidKnockbackMult: 1f, druidCrit: 2));
            mod.AddContent(new DruidicPrefix("Bountiful",
                druidDamageMult: 1.05f, potentialDamageMult: 1.12f, druidSpeedMult: 1.1f, druidKnockbackMult: 1.05f, druidCrit: 4));
            mod.AddContent(new DruidicPrefix("Vivid",
                druidDamageMult: 0.95f, potentialDamageMult: 1.12f, druidSpeedMult: 1.15f, druidKnockbackMult: 0.85f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Overripe",
				druidDamageMult: 1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0, shouldApplyTipsy: true));
            mod.AddContent(new DruidicPrefix("Withered",
                druidDamageMult: 0.9f, potentialDamageMult: 0.85f, druidSpeedMult: 1f, druidKnockbackMult: 1.1f, druidCrit: 0));
			mod.AddContent(new DruidicPrefix("Rotten",
                druidDamageMult: 1f, potentialDamageMult: 0.8f, druidSpeedMult: 0.85f, druidKnockbackMult: 0.9f, druidCrit: 0));
			mod.AddContent(new DruidicPrefix("Ragged",
                druidDamageMult: 0.92f, potentialDamageMult: 0.85f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 10));
			mod.AddContent(new DruidicPrefix("Blooming",
                druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1.05f, druidKnockbackMult: 1f, druidCrit: 2));
			mod.AddContent(new DruidicPrefix("Fertile",
                druidDamageMult: 1.12f, potentialDamageMult: 1f, druidSpeedMult: 1.08f, druidKnockbackMult: 1f, druidCrit: 0));
			mod.AddContent(new DruidicPrefix("Nourished",
                druidDamageMult: 1.08f, potentialDamageMult: 1.12f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 3));
			mod.AddContent(new DruidicPrefix("Thorny",
                druidDamageMult: 1.1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 5));
			mod.AddContent(new DruidicPrefix("Stumpy",
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1.05f, druidCrit: 0));
			mod.AddContent(new DruidicPrefix("Irritated",
                druidDamageMult: 0.8f, potentialDamageMult: 0.95f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0));

			// vanilla
            mod.AddContent(new DruidicPrefix("Keen", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new DruidicPrefix("Superior", vanillaAdapted: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1.1f, druidCrit: 3));
            mod.AddContent(new DruidicPrefix("Forceful", vanillaAdapted: true,
				druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Broken", vanillaAdapted: true,
				druidDamageMult: 0.7f, potentialDamageMult: 0.7f, druidSpeedMult: 1f, druidKnockbackMult: 0.8f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Damaged", vanillaAdapted: true,
				druidDamageMult: 0.85f, potentialDamageMult: 0.85f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Shoddy", vanillaAdapted: true,
				druidDamageMult: 0.9f, potentialDamageMult: 0.9f, druidSpeedMult: 1f, druidKnockbackMult: 0.85f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Hurtful", vanillaAdapted: true,
				druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Strong", vanillaAdapted: true,
				druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Unpleasant", vanillaAdapted: true,
				druidDamageMult: 1.05f, potentialDamageMult: 1.05f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Weak", vanillaAdapted: true,
				druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 0.8f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Ruthless", vanillaAdapted: true,
				druidDamageMult: 1.18f, potentialDamageMult: 1.18f, druidSpeedMult: 1f, druidKnockbackMult: 0.9f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Godly", vanillaAdapted: true,
				druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1f, druidKnockbackMult: 1.15f, druidCrit: 5));
            mod.AddContent(new DruidicPrefix("Demonic", vanillaAdapted: true,
				druidDamageMult: 1.15f, potentialDamageMult: 1.15f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 5));
            mod.AddContent(new DruidicPrefix("Zealous", vanillaAdapted: true,
				druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1f, druidKnockbackMult: 1f, druidCrit: 5));

            mod.AddContent(new DruidicPrefix("Quick", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Deadly", vanillaAdapted: true,
                druidDamageMult: 1.1f, potentialDamageMult: 1.1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Agile", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new DruidicPrefix("Nimble", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.05f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Murderous", vanillaAdapted: true,
                druidDamageMult: 1.07f, potentialDamageMult: 1.07f, druidSpeedMult: 1.06f, druidKnockbackMult: 1f, druidCrit: 3));
            mod.AddContent(new DruidicPrefix("Slow", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.85f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Sluggish", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.8f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Lazy", vanillaAdapted: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 0.92f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Annoying", vanillaAdapted: true,
                druidDamageMult: 0.8f, potentialDamageMult: 0.8f, druidSpeedMult: 0.85f, druidKnockbackMult: 1f, druidCrit: 0));
            mod.AddContent(new DruidicPrefix("Nasty", vanillaAdapted: true,
                druidDamageMult: 0.95f, potentialDamageMult: 0.95f, druidSpeedMult: 1.1f, druidKnockbackMult: 0.9f, druidCrit: 2));

            // melee (claws)
            mod.AddContent(new DruidicPrefix("Large", vanillaAdapted: true, forClaws: true,
                druidDamageMult: 1f, potentialDamageMult: 1f, druidSpeedMult: 1.1f, druidKnockbackMult: 1f, druidSize: 1.12f, druidCrit: 0));
        }

        void ILoadable.Unload() { }
	}

	public static Dictionary<int, DruidicPrefix> DruidicPrefixes { get; private set; }
    public static Dictionary<int, string> DruidicPrefixesNames { get; private set; }

	public override void Load() {
		DruidicPrefixes = [];
		DruidicPrefixesNames = [];
    }

	public override void Unload() {
		DruidicPrefixes?.Clear();
        DruidicPrefixes = null;
		DruidicPrefixesNames?.Clear();
		DruidicPrefixesNames = null;
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
        bool flag = item.ModItem is BaseClawsItem;
        return item.IsADruidicWeapon() && ((!flag && !_forClaws) || (flag && (_forClaws || _vanillaAdapted)));
    }

	public override void SetStaticDefaults() {
		DruidicPrefixes.Add(Type, this);
		DruidicPrefixesNames.Add(Type, Name);
    }

	public override void ModifyValue(ref float valueMult) {
		switch (DruidicPrefixesNames[Type]) {
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
        }
	}

	public override void Apply(Item item) {
		if (!item.IsADruidicWeapon()) {
			return;
		}
		NatureWeaponHandler natureWeaponHandler = item.GetGlobalItem<NatureWeaponHandler>();
		natureWeaponHandler.ActivePrefix = DruidicPrefixes[Type];
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Item item) {
		NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
		string baseExtra = !handler.HasPotentialDamage() ? "1" : string.Empty;
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
                    value += (_druidDamageMult + _potentialDamageMult - 2f) / 4f;
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
            //value += _druidDamageMult;
            //value /= 2f;
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
