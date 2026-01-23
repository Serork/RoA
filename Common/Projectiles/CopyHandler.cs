using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

interface ISpawnCopies {
    public float CopyDeathFrequency { get; }
}

sealed class CopyHandler : GlobalProjectile {
    public struct CopyInfo {
        private float _opacity;

        public Vector2 Position;
        public float Rotation;
        public float Scale;
        public byte UsedFrame;

        public float Opacity {
            readonly get => _opacity;
            set => _opacity = MathUtils.Clamp01(value);
        }
    }

    private byte _currentCopyIndex;

    public CopyInfo[] CopyData { get; private set; } = null!;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.IsModded(out ModProjectile modProjectile) && modProjectile is ISpawnCopies;

    public static void InitializeCopies(Projectile projectile, int maxCopies) => projectile.GetGlobalProjectile<CopyHandler>().CopyData = new CopyInfo[maxCopies];

    public static void MakeCopy(Projectile projectile) {
        var handler = projectile.GetGlobalProjectile<CopyHandler>();
        int maxCopies = handler.CopyData.Length;
        if (handler._currentCopyIndex >= maxCopies) {
            handler._currentCopyIndex = 0;
        }
        handler.CopyData![handler._currentCopyIndex++] = new CopyInfo() {
            Position = projectile.Center,
            UsedFrame = (byte)projectile.frame,
            Rotation = projectile.rotation,
            Opacity = 1.25f,
            Scale = 1f
        };
    }

    public override void PostAI(Projectile projectile) {
        int maxCopies = CopyData.Length;
        for (int i = 0; i < maxCopies; i++) {
            ref CopyInfo copyData = ref CopyData[i];
            if (copyData.Opacity > 0f) {
                float deathFrequency = (projectile.ModProjectile as ISpawnCopies)!.CopyDeathFrequency;
                copyData.Scale -= deathFrequency;
                copyData.Opacity -= deathFrequency;
                copyData.Opacity = MathF.Max(0f, copyData.Opacity);
            }
        }
    }
}
