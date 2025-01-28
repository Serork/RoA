using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace RoA.Common.WorldEvents;

sealed class EnragedLothorSky : CustomSky {
	private bool _hasTile;
	private float _intensity;
	private float _alpha = 0;

	public override void Update(GameTime gameTime) {
		if (!_hasTile) {
			if (_intensity > 0f) {
				_intensity -= 0.05f;
			}
			else {
				_intensity = 0f;
			}

			return;
        }
		if (_intensity < 1f) {
			_intensity += 0.05f;
        }
		else {
			_intensity = 1f;
		}
    }

	public override Color OnTileColor(Color inColor) {
		float amt = Helper.EaseInOut3(_intensity) * .5f * LothorSummoningHandler._alpha;
		return inColor.MultiplyRGB(new Color(1f - amt, 1f - amt, 1f - amt));
	}

	public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
		if (!Main.gameMenu) {
			if ((double)maxDepth >= 3.00000000549776E+38 && (double)minDepth < 3.00000000549776E+38) {
				Color color = Color.Lerp(Color.Red, Color.Black, 0.75f) * 0.75f;
				spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color * Math.Min(1f, (float)((Main.screenPosition.Y - 800.0) / 1000.0)) * _intensity);
			}
		}
	}
	
	public override void Activate(Vector2 position, params object[] args) => _hasTile = true;

	public override void Deactivate(params object[] args) => _hasTile = false;
	
	public override void Reset() {
		_intensity = 0f;
		_hasTile = false;
	}	

	public override bool IsActive() => _intensity > 0f;
}
