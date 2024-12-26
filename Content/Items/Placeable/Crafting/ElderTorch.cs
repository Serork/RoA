using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class ElderTorch : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 100;

        ItemID.Sets.SingleUseInGamepad[Type] = true;
        ItemID.Sets.Torches[Type] = true;
    }

    public override void SetDefaults() {
        Item.DefaultToTorch(ModContent.TileType<Tiles.Crafting.ElderTorch>(), 0, false);
        Item.value = 50;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Torches;
    }

    public override void HoldItem(Player player) {
        if (player.wet) {
            return;
        }

        if (Main.rand.NextBool(player.itemAnimation > 0 ? 7 : 30)) {
            Dust dust = Dust.NewDustDirect(new Vector2(player.itemLocation.X + (player.direction == -1 ? -16f : 6f), player.itemLocation.Y - 14f * player.gravDir), 4, 4, ModContent.DustType<ElderTorchDust>(), 0f, 0f, 100);
            if (!Main.rand.NextBool(3)) {
                dust.noGravity = true;
            }

            dust.velocity *= 0.3f;
            dust.velocity.Y -= 1.5f;
            dust.position = player.RotatedRelativePoint(dust.position);
        }

        Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
        Lighting.AddLight(position, 0.25f, 0.65f, 0.85f);
    }

    public override void PostUpdate() {
        if (!Item.wet) {
            Lighting.AddLight(Item.Center, 0.25f, 0.65f, 0.85f);
        }
    }
}
