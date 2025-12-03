//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using RoA.Common.Druid.Forms;
//using RoA.Common.Druid.Wreath;
//using RoA.Common.GlowMasks;
//using RoA.Common.Players;
//using RoA.Content.Forms;
//using RoA.Core.Defaults;
//using RoA.Core.Utility;
//using RoA.Core.Utility.Vanilla;

//using Terraria;
//using Terraria.DataStructures;
//using Terraria.Graphics.Shaders;
//using Terraria.ID;
//using Terraria.Localization;
//using Terraria.ModLoader;

//namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

//[AutoloadEquip(EquipType.Head)]
//sealed class HallowedTopfhelm : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask, IDoubleTap {
//    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
//        Color glowColor = WreathHandler.GetArmorGlowColor1(player, drawInfo.colorArmorHead) * 2f;
//        if (drawInfo.shadow != 0) {
//            color = glowColor;
//        }
//        else {
//            color = Color.White * player.GetWreathHandler().ActualProgress5 * 0.5f;
//        }
//    }

//    public override void SetStaticDefaults() {
//        Item.ResearchUnlockCount = 1;

//        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Head, this);

//        void setDrawSettings1(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
//            Color glowColor = WreathHandler.GetArmorGlowColor1(player, drawInfo.colorArmorBody) * 2f;
//            if (drawInfo.shadow != 0) {
//                color = glowColor;
//            }
//            else {
//                color = Color.White * player.GetWreathHandler().ActualProgress5 * 0.5f;
//            }
//        }
//        ItemGlowMaskHandler.RegisterArmorGlowMask(ArmorIDs.Body.HallowedPlateMail, EquipType.Body, (ushort)ItemID.HallowedPlateMail, setDrawSettings1);
//        void setDrawSettings2(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
//            Color glowColor = WreathHandler.GetArmorGlowColor1(player, drawInfo.colorArmorLegs) * 2f;
//            if (drawInfo.shadow != 0) {
//                color = glowColor;
//            }
//            else {
//                color = Color.White * player.GetWreathHandler().ActualProgress5 * 0.5f;
//            }
//        }
//        ItemGlowMaskHandler.RegisterArmorGlowMask(ArmorIDs.Legs.HallowedGreaves, EquipType.Legs, (ushort)ItemID.HallowedGreaves, setDrawSettings2);

//        BaseFormHandler.RegisterForm<HallowedGryphon>();
//    }

//    protected override void SafeSetDefaults() {
//        Item.SetSizeValues(22, 24);
//        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());         
//    }

//    public override void UpdateArmorSet(Player player) {
//        player.setBonus = Language.GetTextValue("ArmorSetBonus.Hallowed");
//        player.onHitDodge = true;

//        BaseFormHandler.KeepFormActive(player);
//    }

//    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
//        if (player.CanTransfromIntoDruidForm<HallowedTopfhelm>(direction)) {
//            BaseFormHandler.ToggleForm<HallowedGryphon>(player);
//        }
//    }

//    public override void ArmorSetShadows(Player player) {
//        player.armorEffectDrawOutlines = true;
//        player.armorEffectDrawShadow = true;
//    }

//    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemID.HallowedPlateMail && legs.type == ItemID.HallowedGreaves;

//    public override void EquipFrameEffects(Player player, EquipType type) {
//        if (player.isDisplayDollOrInanimate) {
//            return;
//        }

//        if (!player.HasSetBonusFrom<HallowedTopfhelm>()) {
//            return;
//        }

//        if (player.velocity.X != 0f && player.velocity.Y != 0f && Main.rand.Next(10) == 0) {
//            int num12 = Dust.NewDust(new Vector2(player.position.X - player.velocity.X * 2f, player.position.Y - 2f - player.velocity.Y * 2f), player.width, player.height, 43, 0f, 0f, 100, default(Color), 0.3f);
//            Main.dust[num12].fadeIn = 0.8f;
//            Main.dust[num12].velocity *= 0f;
//            Main.dust[num12].shader = GameShaders.Armor.GetSecondaryShader(player.ArmorSetDye(), player);
//        }
//    }
//}