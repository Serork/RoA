using System;
using System.Linq;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Common.VisualEffects;

sealed class VisualEffectSystem : ModSystem {
    internal class ParticlePools<T> where T : VisualEffect<T>, new() {
        public static ParticlePool<T> Pool;
    }

    private static ParticleRenderer[] _layers;

    public static T Fetch<T>() where T : VisualEffect<T>, new() => ParticlePools<T>.Pool.RequestParticle();

    public static ParticleRenderer GetLayer(int i) => _layers[i];

    public static T? New<T>(int layer, Action<T>? preInitializer = null, bool onServer = false) where T : VisualEffect<T>, new() {
        if (Main.dedServ && !onServer) {
            return null;
        }
        T value = Fetch<T>();
        preInitializer?.Invoke(value);
        _layers[layer].Add(value);
        return value;
    }

    public override void Load() {
        _layers = new ParticleRenderer[VisualEffectLayer.COUNT];
        for (int i = 0; i < VisualEffectLayer.COUNT; i++) {
            _layers[i] = new ParticleRenderer();
        }
    }

    public override void Unload() => _layers = null;

    public static void InitWorldData() {
        foreach (ParticleRenderer layer in _layers) {
            layer.Particles.Clear();
        }
    }

    public override void OnWorldLoad() => InitWorldData();
    public override void OnWorldUnload() => InitWorldData();

    public override void PreUpdatePlayers() {
        foreach (ParticleRenderer layer in _layers) {
            layer.Particles = layer.Particles.Distinct().ToList();
            layer.Update();
        }
    }
}