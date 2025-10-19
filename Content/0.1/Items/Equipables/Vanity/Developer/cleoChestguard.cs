using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Body)]
sealed class cleoChestguard : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Peegeon's Chestguard");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    private class RespiratorDrawLayer : ILoadable {
        void ILoadable.Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_08_Backpacks += On_PlayerDrawLayers_DrawPlayer_08_Backpacks;
        }

        private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawinfo) {
            Player player = drawinfo.drawPlayer;
            int itemType = ModContent.ItemType<cleoChestguard>();
            bool flag = false;
            if (!drawinfo.hideEntirePlayer && !player.dead) {
                if (player.CheckArmorSlot(itemType, 1, 11) || player.CheckVanitySlot(itemType, 11)) {
                    int type = drawinfo.heldItem.type;
                    int num2 = 1;
                    float num3 = -4f;
                    float num4 = -8f;
                    int shader = 0;
                    shader = drawinfo.cBody;

                    Vector2 vector3 = new Vector2(-4f * player.direction, 12f);
                    Vector2 vec5 = drawinfo.Position - Main.screenPosition + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0f, -4f) + vector3;
                    vec5 = vec5.Floor();
                    Vector2 vec6 = drawinfo.Position - Main.screenPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2) + new Vector2((-9f + num3) * (float)drawinfo.drawPlayer.direction, (2f + num4) * drawinfo.drawPlayer.gravDir) + vector3;
                    vec6 = vec6.Floor();

                    var asset = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "cleoRespirator");
                    DrawData item = new DrawData(asset.Value, vec5,
                        new Rectangle(0, 0, asset.Width(), asset.Height()), drawinfo.colorArmorBody, drawinfo.drawPlayer.bodyRotation, new Vector2((float)asset.Width() * 0.5f, drawinfo.bodyVect.Y), 1f, drawinfo.playerEffect);
                    item.shader = shader;
                    drawinfo.DrawDataCache.Add(item);

                    flag = true;
                }
            }

            ExtraDrawLayerSupport.DrawBackpacks(ref drawinfo);
            if (flag) {
                return;
            }

            orig(ref drawinfo);
        }

        void ILoadable.Unload() { }
    }
}
