using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

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
        private static Asset<Texture2D> _respiratorTexture = null!;

        void ILoadable.Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_08_Backpacks += On_PlayerDrawLayers_DrawPlayer_08_Backpacks;

            if (!Main.dedServ) {
                _respiratorTexture = ModContent.Request<Texture2D>(ResourceManager.DeveloperEquipableTextures + "cleoRespirator");
            }
        }

        private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawinfo) {
            Player player = drawinfo.drawPlayer;
            int itemType = ModContent.ItemType<cleoChestguard>();
            bool flag = false;
            if (player.CheckArmorSlot(itemType, 1, 11) || player.CheckVanitySlot(itemType, 11)) {
                if (ExtraDrawLayerSupport.DrawBackpack(_respiratorTexture, ref drawinfo)) {
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
