using Microsoft.Xna.Framework;

using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.World;

sealed class LothorShake : ModSystem {
    internal static bool shake;
    internal static bool before;

    private float _shakeIntensity = 0f, _shakeIntensity2;
    private float _timer;

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();

    private void Reset() {
        shake = before = false;
        _shakeIntensity = _shakeIntensity2 = _timer = 0f;
    }

    public override void PostUpdateNPCs() {
        if (Main.dedServ) {
            return;
        }

        string shader = ShaderLoader.LothorSky;
        bool flag = NPC.AnyNPCs(ModContent.NPCType<Lothor>());
        if (flag && !shake) {
            shake = true;
        }
        if (!shake && !before && !flag) {
            Reset();
            if (Filters.Scene[shader].IsActive()) {
                Filters.Scene[shader].Deactivate();
            }
        }
        if (shake) {
            if ((before && _shakeIntensity < 0.035f) || (!before && _shakeIntensity < 1f)) {
                _shakeIntensity += !before ? 0.325f : 0.2f;
                if (before) {
                    _shakeIntensity *= 0.1f;
                }
                _shakeIntensity *= !before ? 1.05f : 1.75f;
                if (before) {
                    _shakeIntensity *= 1.25f;
                }
            }
            if (!Filters.Scene[shader].IsActive()) {
                Filters.Scene.Activate(shader);
            }
            else {
                if (before) {
                    float max = 0.006f;
                    if (_shakeIntensity > max) {
                        _shakeIntensity = max;
                    }
                }
                float value = (!before ? Helper.EaseInOut3(_shakeIntensity) : (float)Math.Pow((double)Helper.EaseInOut4(_shakeIntensity * 7f) * 3f, 4.0) * 1.3f);
                float alpha = Utils.Remap(LothorSummoningHandler._alpha, 0f, 1f, 0f, 0.75f, true);
                Filters.Scene[shader].GetShader().UseOpacity(value * alpha).UseColor(Color.Red);
            }
            if (!before && _shakeIntensity >= 1f) {
                _timer += TimeSystem.LogicDeltaTime * (!before ? 1.5f : 1f);
                if ((before && _timer >= 0.1f) || (!before && _timer >= 0.5f)) {
                    _timer = 0f;
                    if (Filters.Scene[shader].IsActive()) {
                        Filters.Scene[shader].Deactivate();
                    }
                    _shakeIntensity = 0f;
                    shake = false;
                    before = true;
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
