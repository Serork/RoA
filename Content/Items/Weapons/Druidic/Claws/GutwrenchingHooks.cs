using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class GutwrenchingHooks : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(24, 3f);

        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 54, 0);

        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 16, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 27);
        NatureWeaponHandler.SetFillingRate(Item, 0.8f);
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
            PlaySoundStyle = new SoundStyle(ResourceManager.ItemSounds + "ClawsWave") { Volume = 0.75f }
        });
    }
}
