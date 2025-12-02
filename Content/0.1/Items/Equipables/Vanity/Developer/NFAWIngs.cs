using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Wings)]
sealed class NFAWings : ModItem {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(0, 0f);

        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Wings_Glow");
    }

    private class NFAWingsGlowMask : PlayerDrawLayer {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
            drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(RoA.Instance, nameof(NFAWings), EquipType.Wings);

        protected override void Draw(ref PlayerDrawSet drawinfo) {
            if (drawinfo.drawPlayer.dead || drawinfo.hideEntirePlayer) {
                return;
            }

            int num11 = 0;
            int num12 = 0;
            int num13 = 4;
            Vector2 directions = drawinfo.drawPlayer.Directions;
            Vector2 vector = drawinfo.Position - Main.screenPosition + drawinfo.drawPlayer.Size / 2f;
            Vector2 vector2 = new Vector2(0f, 7f);
            vector = drawinfo.Position - Main.screenPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2) + vector2;
            Vector2 vector12 = vector + new Vector2(num12 - 9, num11 + 2) * directions;
            Color glowMaskColor = Color.White * 0.9f;
            glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
            DrawData item = new DrawData(_glowTexture.Value,
                vector12.Floor(), new Rectangle(0, TextureAssets.Wings[drawinfo.drawPlayer.wings].Height() / num13 * drawinfo.drawPlayer.wingFrame, TextureAssets.Wings[drawinfo.drawPlayer.wings].Width(), TextureAssets.Wings[drawinfo.drawPlayer.wings].Height() / num13),
                glowMaskColor, drawinfo.drawPlayer.bodyRotation, new Vector2(TextureAssets.Wings[drawinfo.drawPlayer.wings].Width() / 2, TextureAssets.Wings[drawinfo.drawPlayer.wings].Height() / num13 / 2), 1f, drawinfo.playerEffect);
            item.shader = drawinfo.cWings;
            drawinfo.DrawDataCache.Add(item);
        }
    }

    public override void SetDefaults() {
        Item.width = 18;
        Item.height = 42;

        Item.accessory = true;
        Item.rare = ItemRarityID.Cyan;

        Item.value = 400000;
    }

    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
    }
}
