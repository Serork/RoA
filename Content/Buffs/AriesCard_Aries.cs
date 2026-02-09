using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Aries : ModBuff {
    public override LocalizedText Description => base.Description.WithFormatArgs(Main.LocalPlayer.name);

    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.buffTime[buffIndex] = 18000;
        player.GetCommon().IsAriesActive = true;
        bool flag25 = true;
        int projType = ModContent.ProjectileType<Projectiles.Friendly.Pets.Aries>();
        if (player.ownedProjectileCounts[projType] > 0)
            flag25 = false;

        if (flag25 && player.whoAmI == Main.myPlayer)
            Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, projType, 0, 0f, player.whoAmI);
    }
}
