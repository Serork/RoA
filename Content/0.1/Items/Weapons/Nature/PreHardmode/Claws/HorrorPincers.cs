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

sealed class HorrorPincers : ClawsBaseItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26);
        Item.SetWeaponValues(24, 3f);

        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 54, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 16, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 27);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.8f);
    }

    protected override (Color, Color) SetSlashColors(Player player) => (new Color(112, 75, 140), new Color(130, 100, 210));

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        int offset = 30 * player.direction;
        var position = new Vector2(player.Center.X + offset, player.Center.Y);
        Vector2 pointPosition = player.GetViableMousePosition();
        Vector2 point = Helper.VelocityToPoint(player.Center, pointPosition, 1.2f);
        args.ProjectileTypeToSpawn = (ushort)ModContent.ProjectileType<InfectedWave>();
        args.SpawnPosition = new Vector2(position.X, position.Y - 14f);
        args.StartVelocity = point;
        args.PlaySoundStyle = new SoundStyle(ResourceManager.ItemSounds + "ClawsWave") { Volume = 0.75f };
        args.OnAttack = () => {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 1, player.Center));
            }
        };
    }
}
