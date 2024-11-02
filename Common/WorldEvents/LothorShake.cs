using Microsoft.Xna.Framework;

using RoA.Utilities;

using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.WorldEvents;

sealed class LothorShake : ModSystem {
	internal static bool shake;

	internal static bool _before;
	private float _shakeIntensity = 0f;
	private float _timer;

	public override void PostUpdateNPCs() {
		if (shake) {
			if ((_before && _shakeIntensity < 0.08f) || (!_before && _shakeIntensity < 1f)) {
				_shakeIntensity += !_before ? 0.325f : 0.075f;
				_shakeIntensity *= 1.05f;
			}
			string shader = ShaderLoader.LothorSky;
			if (!Filters.Scene[shader].IsActive()) {
				Filters.Scene.Activate(shader);
			}
			else {
				float value = (!_before ? Helper.EaseInOut3(_shakeIntensity) : Helper.EaseInOut4(_shakeIntensity)) * (!_before ? 1.3f : 1.15f);
				Filters.Scene[shader].GetShader().UseOpacity(value).UseColor(Color.Red);
			}
			if (!_before && _shakeIntensity >= 1f) {
				_timer += TimeSystem.LogicDeltaTime * (!_before ? 1.5f : 1f);
				if ((_before && _timer >= 0.3f) || (!_before && _timer >= 0.735f)) {
					_timer = 0f;
                    if (Filters.Scene[shader].IsActive()) {
						Filters.Scene[shader].Deactivate();
					}
					_shakeIntensity = 0f;
					shake = false;
					_before = true;
					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendData(MessageID.WorldData);
					}
				}
			}
		}
	}

	public override void NetSend(BinaryWriter writer) => writer.Write(shake);
	public override void NetReceive(BinaryReader reader) => shake = reader.ReadBoolean();
}
