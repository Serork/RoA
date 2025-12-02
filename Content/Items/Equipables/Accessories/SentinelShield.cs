//using Microsoft.Xna.Framework;

//using RoA.Content.Projectiles.Friendly.Miscellaneous;
//using RoA.Core.Utility;

//using Terraria;
//using Terraria.Audio;
//using Terraria.GameContent.Creative;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace RoA.Content.Items.Equipables.Accessories;

//[Autoload(false)]
//[AutoloadEquip(EquipType.Shield)]
//sealed class SentinelShield : ModItem {
//    public override void SetStaticDefaults() {
//        //DisplayName.SetDefault("Coffin");
//        //Tooltip.SetDefault("Shoot bones in random positions on taking damage");
//        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
//    }

//    public override void SetDefaults() {
//        Item.width = 26;
//        Item.height = 28;
//        Item.rare = ItemRarityID.Green;
//        Item.accessory = true;
//        Item.defense = 1;

//        Item.value = Item.sellPrice(0, 1, 0, 0);
//    }

//    public override void UpdateAccessory(Player player, bool hideVisual) {
//        player.GetModPlayer<SentinelShieldHandler>().IsEffectActive = true;
//    }

//    private class SentinelShieldHandler : ModPlayer {
//        public bool IsEffectActive;

//        public override void ResetEffects() {
//            IsEffectActive = false;
//        }

//        public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
//            int type = ModContent.ProjectileType<VengefulSpirit>();
//            if (IsEffectActive && Main.rand.NextChance(0.2)) {
//                Player.SetImmuneTimeForAllTypes(30);

//                SoundEngine.PlaySound(SoundID.NPCHit5, Player.Center);
//                if (Player.whoAmI == Main.myPlayer) {
//                    for (int i = 0; i < 3; i++) {
//                        Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 4f;
//                        Projectile.NewProjectile(Player.GetSource_OnHurt(modifiers.DamageSource), Player.Center.X + velocity.X, Player.Center.Y + velocity.Y, velocity.X, velocity.Y,
//                            type,
//                            40,
//                            1.5f,
//                            Player.whoAmI);
//                    }
//                }

//                modifiers.Cancel();
//            }
//        }
//    }
//}