using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class GutwrenchingHooks : BaseClawsItem {
    protected override ushort UseTime => 18;

    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(12, 4f);

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 25, 0);

        NatureWeaponHandler.SetFillingRate(Item, 1f);
    }

    protected override (Color, Color) SlashColors(Player player) => (new Color(216, 73, 73), new Color(255, 114, 114));

    public override void SafeOnUse(Player player, ClawsHandler clawsStats) {
        int offset = 30 * player.direction;
        var position = new Vector2(player.Center.X + offset, player.Center.Y);
        Vector2 pointPosition = player.GetViableMousePosition();
        Vector2 point = Helper.VelocityToPoint(player.Center, pointPosition, 1.2f);
        clawsStats.SetSpecialAttackData<HemorrhageWave>(new ClawsHandler.AttackSpawnInfoArgs() {
            Owner = Item,
            SpawnPosition = new Vector2(position.X, position.Y - 14f),
            StartVelocity = point,
            PlaySoundStyle = SoundID.Item95
        });
    }
}
