using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly;

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Claws;

sealed class ClawsStats : ModPlayer {
    public readonly struct SpecialAttackSpawnInfo(Item owner, ushort projectileTypeToSpawn, Vector2 spawnPosition, Vector2? startVelocity = null, SoundStyle? playSoundStyle = null) {
        public readonly Item Owner = owner;
        public readonly ushort ProjectileTypeToSpawn = projectileTypeToSpawn;
        public readonly Vector2 SpawnPosition = spawnPosition;
        public readonly Vector2 StartVelocity = startVelocity ?? Vector2.Zero;
        public readonly SoundStyle? PlaySoundStyle = playSoundStyle ?? null;
    }

    public (Color, Color) SlashColors { get; private set; }
    public SpecialAttackSpawnInfo SpecialAttackData { get; private set; }

    public void SetColors(Color firstSlashColor, Color secondSlashColor) => SlashColors = (firstSlashColor, secondSlashColor);

    public void SetSpecialAttackData<T>(Item owner, Vector2 spawnPosition, Vector2? startVelocity = null, SoundStyle? playSoundStyle = null) where T : NatureProjectile {
        if (SpecialAttackData.Owner == owner) {
            return;
        }

        SpecialAttackData = new SpecialAttackSpawnInfo(owner, (ushort)ModContent.ProjectileType<T>(), spawnPosition, startVelocity, playSoundStyle);
    }
}
