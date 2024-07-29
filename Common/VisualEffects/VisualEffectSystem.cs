using System;
using System.Linq;

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

    public static T New<T>(int layer, Action<T>? preInitializer = null) where T : VisualEffect<T>, new() {
        T value = Fetch<T>();
        preInitializer?.Invoke(value);
        _layers[layer].Add(value);
        return value;
    }

    public override void Load() {
        _layers = new ParticleRenderer[VisualEffectLayer.Count];
        for (int i = 0; i < VisualEffectLayer.Count; i++) {
            _layers[i] = new ParticleRenderer();
        }
    }

    public override void Unload() {
        _layers = null;
    }

    public void InitWorldData() {
        foreach (ParticleRenderer layer in _layers) {
            layer.Particles.Clear();
        }
    }

    public override void OnWorldLoad() {
        InitWorldData();
    }

    public override void OnWorldUnload() {
        InitWorldData();
    }

    public override void PreUpdatePlayers() {
        foreach (ParticleRenderer layer in _layers) {
            layer.Particles = layer.Particles.Distinct().ToList();
            layer.Update();
        }
    }
}

class VisualEffectLayer {
    public const int BehindAllNPCs = 0;
    public const int AboveNPCs = 1;
    public const int BehindProjs = 2;
    public const int BehindPlayers = 3;
    public const int AbovePlayers = 4;
    public const int AboveDust = 5;
    public const int Count = 7;
}