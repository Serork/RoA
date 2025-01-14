using RoA.Common.Druid;
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
						   bool shouldApplyTipsy = false) : ModPrefix {
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
	internal readonly bool _shouldApplyTipsy = shouldApplyTipsy;


    private static LocalizedText GetLocalizedText(string name) => Language.GetOrRegister(LOCALIZATION + name);

    public override LocalizedText DisplayName => GetLocalizedText(_name);

    public override float RollChance(Item item) => 1f;

	public override bool CanRoll(Item item) => item.IsADruidicWeapon();

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
                valueMult *= 1.2f;
                break;
			case "Nourished":
                valueMult *= 1.2f;
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
			if (!handler.HasPotentialDamage()) {
				value += _potentialDamageMult;
				value /= 2f;
            }
            yield return new TooltipLine(Mod, "ExtraDruidDamageMult", GetLocalizedText("DruidDamageModifierMult" + baseExtra).Format(value * 100f - 100f)) {
				IsModifier = true,
				IsModifierBad = value < 1f
			};
		}
		if (handler.HasPotentialDamage()) {
            if (_potentialDamage != 0) {
				yield return new TooltipLine(Mod, "ExtraDruidPotentialDamage", GetLocalizedText("DruidPotentialDamageModifier").Format(_potentialDamage)) {
					IsModifier = true,
					IsModifierBad = _potentialDamage < 0
				};
			}
			if (_potentialDamageMult != 1f) {
				yield return new TooltipLine(Mod, "ExtraDruidPotentialDamageMult", GetLocalizedText("DruidPotentialDamageModifierMult").Format(_potentialDamageMult * 100f - 100f)) {
					IsModifier = true,
					IsModifierBad = _potentialDamageMult < 1f
				};
			}
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
                IsModifierBad = false
            };
        }
    }
}
