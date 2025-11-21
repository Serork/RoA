using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class DreadheartCrimsonHelmet : NatureItem, IDoubleTap {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dreadheart Helmet");
        //Tooltip.SetDefault("4% increased nature critical strike chance");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        BaseFormHandler.RegisterForm<CrimsonInsectForm>();
    }

    protected override void SafeSetDefaults() {
        int width = 22; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 1, 0, 0);

        Item.defense = 4;
    }

    public override void UpdateEquip(Player player) => player.GetCritChance(DruidClass.Nature) += 5;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<DreadheartCrimsonChestplate>() && legs.type == ModContent.ItemType<DreadheartCrimsonLeggings>();

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * (1f - shadow) * drawPlayer.GetWreathHandler().ActualProgress5 * 0.9f;
    }

    public override void UpdateArmorSet(Player player) {
        string setBonus = Language.GetText("Mods.RoA.Items.Tooltips.DreadheartCrimsonSetBonus").WithFormatArgs(Helper.ArmorSetBonusKey).Value;
        player.setBonus = setBonus;

        player.GetModPlayer<DreadheartSetBonusHandler>().IsEffectActive = true;

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.CanTransfromIntoDruidForm<DreadheartCrimsonHelmet>(direction)) {
            BaseFormHandler.ToggleForm<CrimsonInsectForm>(player);
        }
    }

    internal class DreadheartSetBonusHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
            if (!IsEffectActive) {
                return;
            }

            WreathHandler handler = Player.GetWreathHandler();
            if (!handler.IsActualFull1) {
                return;
            }

            handler.ForcedHardReset();

            Player.SetImmuneTimeForAllTypes(15);

            SoundEngine.PlaySound(SoundID.NPCHit32, Player.position);
            if (Player.whoAmI == Main.myPlayer) {
                for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++) {
                    int insectDamage = 25;
                    float insectKnockback = 3f;
                    int damage = (int)Player.GetDamage(DruidClass.Nature).ApplyTo(insectDamage);
                    insectKnockback = Player.GetKnockback(DruidClass.Nature).ApplyTo(insectKnockback);
                    Vector2 spread = new Vector2(0, Main.rand.Next(-5, -2)).RotatedByRandom(MathHelper.ToRadians(90));
                    Projectile.NewProjectile(Player.GetSource_OnHurt(modifiers.DamageSource), new Vector2(Player.position.X, Player.position.Y + 4), new Vector2(spread.X, spread.Y),
                        Player.HasSetBonusFrom<DreadheartCorruptionHelmet>() ? (ushort)ModContent.ProjectileType<CrimsonInsect>() : (ushort)ModContent.ProjectileType<CorruptionInsect>(), damage, insectKnockback, Player.whoAmI);
                }
            }

            modifiers.Cancel();
        }
    }
}