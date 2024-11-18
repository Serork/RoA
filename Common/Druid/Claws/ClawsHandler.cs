using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Claws;

sealed class ClawsHandler : ModPlayer {
    public ref struct AttackSpawnInfoArgs() {
        public Item Owner;
        public ushort ProjectileTypeToSpawn;
        public Vector2 SpawnPosition;
        public Vector2? StartVelocity = null;
        public SoundStyle? PlaySoundStyle = null;
        public bool ShouldReset = true;
        public bool ShouldSpawn = true;
        public Action<Player>? SpawnProjectile = null;
        public Action<Player>? OnSpawn = null;
    }

    public readonly struct SpecialAttackSpawnInfo(AttackSpawnInfoArgs args) {
        public readonly Item Owner = args.Owner;
        public readonly ushort ProjectileTypeToSpawn = args.ProjectileTypeToSpawn;
        public readonly Vector2 SpawnPosition = args.SpawnPosition;
        public readonly Vector2 StartVelocity = args.StartVelocity ?? Vector2.Zero;
        public readonly SoundStyle? PlaySoundStyle = args.PlaySoundStyle ?? null;
        public readonly bool ShouldReset = args.ShouldReset;
        public readonly bool ShouldSpawn = args.ShouldSpawn;
        public readonly Action<Player>? SpawnProjectile = args.SpawnProjectile;
        public readonly Action<Player>? OnSpawn = args.OnSpawn;
    }

    public (Color, Color) SlashColors { get; private set; }
    public SpecialAttackSpawnInfo SpecialAttackData { get; private set; }

    public void SetColors(Color firstSlashColor, Color secondSlashColor) => SlashColors = (firstSlashColor, secondSlashColor);

    public void SetSpecialAttackData<T>(AttackSpawnInfoArgs args) where T : NatureProjectile {
        ushort type = (ushort)ModContent.ProjectileType<T>();
        args.ProjectileTypeToSpawn = type;
        SpecialAttackData = new SpecialAttackSpawnInfo(args);
    }

    public void SetSpecialAttackData(AttackSpawnInfoArgs args) => SpecialAttackData = new SpecialAttackSpawnInfo(args);
}
