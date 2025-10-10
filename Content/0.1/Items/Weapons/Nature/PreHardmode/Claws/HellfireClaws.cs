using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.Runtime.InteropServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using static System.Net.Mime.MediaTypeNames;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;

[WeaponOverlay(WeaponType.Claws, 0xffffff)]
sealed class HellfireClaws : ClawsBaseItem<HellfireClaws.HellfireClawsSlash> {
    public override Color? GetAlpha(Color lightColor) => Color.White;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26);
        Item.SetWeaponValues(30, 4.2f);

        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 34);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldReset = false;
    }

    protected override ushort? SpawnClawsProjectileType(Player player) {
        WreathHandler handler = player.GetWreathHandler();
        bool flag = handler.WillBeFull(handler.CurrentResource, true);
        ushort? result = null;
        if (flag) {
            result = (ushort)ModContent.ProjectileType<UltimateHellfireClawsSlash>();
        }
        return result;
    }

    protected override (Color, Color) SetSlashColors(Player player) => (new Color(255, 150, 20), new Color(200, 80, 10));

    public sealed class HellfireClawsSlash : ClawsSlash {
        public override void OnHitPlayer(Player target, Player.HurtInfo info) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(60, 180));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(60, 180));
        }
    }
}
