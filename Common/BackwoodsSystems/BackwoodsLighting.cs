﻿using Microsoft.Xna.Framework;

using RoA.Common.Tiles;
using RoA.Content.Biomes.Backwoods;

using System;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.BackwoodsSystems;

sealed class BackwoodsLighting : ModSystem {
	private float _alpha, _alpha2;

    public override void ClearWorld() {
		_alpha = 1f;
		_alpha2 = 0f;
    }

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(_alpha)] = _alpha;
        tag[nameof(_alpha2)] = _alpha2;
    }

    public override void LoadWorldData(TagCompound tag) {
        _alpha = tag.GetFloat(nameof(_alpha));
        _alpha2 = tag.GetFloat(nameof(_alpha2));
    }

    public override void ModifyLightingBrightness(ref float scale) {
		if (_alpha2 > 0f) {
            float strength = ModContent.GetInstance<TileCount>().BackwoodsTiles / 1500f;
			strength = Math.Min(strength, 1f) * 0.85f * _alpha * _alpha2;
            float value = Utils.Remap(strength, 0f, 0.78f, scale, 0.95f);
			scale = value;
		}
	}

	public override void PostUpdateWorld() {
        if (Main.dayTime && (double)_alpha > 0.95f) {
            _alpha -= BackwoodsBiome.TransitionSpeed;
        }
        if (!Main.dayTime && (double)_alpha < 1.025f) {
            _alpha += BackwoodsBiome.TransitionSpeed;
        }
    }

	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
		if (ModContent.GetInstance<TileCount>().BackwoodsTiles > 0) {
            if (_alpha2 < 1f) {
				_alpha2 += BackwoodsBiome.TransitionSpeed;
			}
			float strength = ModContent.GetInstance<TileCount>().BackwoodsTiles / 1500f;
			strength = Math.Min(strength, 1f) * 0.85f * _alpha * _alpha2/* + MathUtils.EaseInOut3(Math.Min(1f, OvergrownCoords.Strength + 0.25f))*/;
            int sunR = backgroundColor.R;
			int sunG = backgroundColor.G;
			int sunB = backgroundColor.B;
			int black = 175;
			float dark = black;
			sunR -= (int)(dark * strength * (backgroundColor.R / 255f));
			sunG -= (int)(dark * strength * (backgroundColor.G / 255f));
			sunB -= (int)(dark * strength * (backgroundColor.B / 255f));
			Main.ColorOfTheSkies.R -= (byte)(dark / 2 * strength * (backgroundColor.R / 255f));
			Main.ColorOfTheSkies.G -= (byte)(dark / 2 * strength * (backgroundColor.G / 255f));
			Main.ColorOfTheSkies.B -= (byte)(dark / 2 * strength * (backgroundColor.B / 255f));
			sunR = Utils.Clamp(sunR, 15, 255);
			sunG = Utils.Clamp(sunG, 15, 255);
			sunB = Utils.Clamp(sunB, 15, 255);

			// adapted vanilla
			float num23 = strength * 0.6f;
			int num24 = backgroundColor.R;
			int num25 = backgroundColor.G;
			int num26 = backgroundColor.B;
			num25 -= (int)(100f * num23 * (backgroundColor.G / 255f));
			num24 -= (int)(100f * num23 * (backgroundColor.R / 255f));
			num26 -= (int)(100f * num23 * (backgroundColor.B / 255f));
			if (num25 < 15) {
				num25 = 15;
			}
			if (num24 < 15) {
				num24 = 15;
			}
			if (num26 < 15) {
				num26 = 15;
			}
			backgroundColor.R = (byte)num24;
			backgroundColor.G = (byte)num25;
			backgroundColor.B = (byte)num26;
			tileColor = backgroundColor;
			tileColor.R = (byte)sunR;
			tileColor.G = (byte)sunG;
			tileColor.B = (byte)sunB;
			Color white = Color.White;
			Color white2 = Color.White;
			num24 = white.R;
			num25 = white.G;
			num26 = white.B;
			num25 -= (int)(10f * num23 * (white.R / 255f));
			num24 -= (int)(30f * num23 * (white.G / 255f));
			num26 -= (int)(10f * num23 * (white.B / 255f));
			if (num24 < 15) {
				num24 = 15;
			}
			if (num25 < 15) {
				num25 = 15;
			}
			if (num26 < 15) {
				num26 = 15;
			}
			white.R = (byte)num24;
			white.G = (byte)num25;
			white.B = (byte)num26;
			num24 = white2.R;
			num25 = white2.G;
			num26 = white2.B;
			num25 -= (int)(140f * num23 * (white2.R / 255f));
			num24 -= (int)(170f * num23 * (white2.G / 255f));
			num26 -= (int)(190f * num23 * (white2.B / 255f));
			if (num24 < 15) {
				num24 = 15;
			}
			if (num25 < 15) {
				num25 = 15;
			}
			if (num26 < 15) {
				num26 = 15;
			}
			white2.R = (byte)num24;
			white2.G = (byte)num25;
			white2.B = (byte)num26;

			return;
		}

		if (_alpha2 <= 0f) {
			return;
		}
        _alpha2 -= BackwoodsBiome.TransitionSpeed;
    }
}