using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;

sealed class ThornyClaws : ClawsBaseItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26);
        Item.SetWeaponValues(26, 4f);

        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 0, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 20, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 28);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override (Color, Color) SetSlashColors(Player player) => (new Color(75, 167, 85), new Color(100, 200, 110));

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        ushort type = (ushort)ModContent.ProjectileType<Snatcher>();
        bool shouldReset = true;
        int count = 0;
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (count > 1) {
                break;
            }
            if (projectile.type == type && projectile.owner == player.whoAmI) {
                if (projectile.timeLeft < 30) {
                    count++;
                }
            }
        }
        if (count > 1) {
            shouldReset = false;
        }
        args.SpawnPosition = player.Center;
        args.StartVelocity = Helper.VelocityToPoint(player.Center, player.GetViableMousePosition(), 1f).SafeNormalize(Vector2.Zero);
        args.ProjectileTypeToSpawn = type;
        args.ShouldReset = shouldReset;
        args.ShouldSpawn = player.ownedProjectileCounts[type] < 2;
        args.PlaySoundStyle = new SoundStyle(ResourceManager.ItemSounds + "Leaves2") { Pitch = 0.3f, Volume = 1.2f };
        args.OnAttack = () => {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 2, player.Center));
            }
        };
    }
}
