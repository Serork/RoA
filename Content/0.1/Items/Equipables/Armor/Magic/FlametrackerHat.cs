using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Projectiles.Friendly.Magic;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

sealed class FlametrackerSetBonusHandler : ModPlayer {
    public bool IsActive { get; internal set; }

    public override void ResetEffects() {
        IsActive = false;
    }

    public override void OnConsumeMana(Item item, int manaConsumed) {
        if (IsActive) {
            if (Player.statMana == Player.statManaMax2) {
                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "FireWoosh") { Volume = 5.5f }, Player.Center);
                if (Player.whoAmI == Main.myPlayer) {
                    Vector2 mousePos = Player.GetViableMousePosition();
                    int direction = (mousePos - Player.Center).X.GetDirection();
                    Vector2 projectilePos = new Vector2(Player.Top.X + 16f * direction + (direction == 1 && mousePos.Y < Player.Bottom.Y ? -10f : 0f), Player.Top.Y - 2f);
                    Projectile.NewProjectile(Player.GetSource_Misc("frametrackersetbonus"),
                        projectilePos,
                        Helper.VelocityToPoint(projectilePos, mousePos, 5f),
                        ModContent.ProjectileType<TrackingBolt>(),
                        (int)Player.GetTotalDamage(DamageClass.Magic).ApplyTo(25),
                        Player.GetTotalKnockback(DamageClass.Magic).ApplyTo(1.5f),
                        Player.whoAmI);
                }
            }
        }
    }
}

[AutoloadEquip(EquipType.Head)]
sealed class FlametrackerHat : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("Greatly increases mana regeneration rate");
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 34; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 4;

        Item.value = Item.sellPrice(0, 0, 90, 0);
    }

    public override void UpdateEquip(Player player) {
        player.manaRegenBonus += 5;
        player.manaRegenDelayBonus += 2;
        Lighting.AddLight(player.Top - new Vector2(4f * player.direction, 0f), new Vector3(194, 44, 44) * 0.0035f);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
        => body.type == ModContent.ItemType<FlametrackerJacket>() && legs.type == ModContent.ItemType<FlametrackerPants>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.FlametrackerSetBonus").Value;
        player.GetModPlayer<FlametrackerSetBonusHandler>().IsActive = true;
    }
}

sealed class FlametrackerHatFlame : PlayerDrawLayer {
    private static Asset<Texture2D> _hatFlameTexture = null!;

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _hatFlameTexture = ModContent.Request<Texture2D>(ResourceManager.MagicArmorTextures + "FlametrackerHat_Flame");
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.CheckArmorSlot(ModContent.ItemType<FlametrackerHat>(), 0, 10) ||
           drawInfo.drawPlayer.CheckVanitySlot(ModContent.ItemType<FlametrackerHat>(), 10);

    public override Position GetDefaultPosition()
        => new AfterParent(PlayerDrawLayers.Head);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.hideEntirePlayer) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || player.face != -1 || player.dead/* || player.GetModPlayer<MagicArmorSetPlayer>().flameTrackerArmorSet*/ || player.statMana != player.statManaMax2)
            return;

        if (player.statMana != player.statManaMax2) {
            return;
        }

        Texture2D texture = _hatFlameTexture.Value;
        Rectangle bodyFrame = player.bodyFrame;
        Color color = new Color(255, 255, 255, 0) * 0.8f * 0.75f;

        ulong speed = (ulong)(player.miscCounter / 5);
        int height = texture.Height / 20;

        for (int i = 0; i < 7; ++i) {
            float shakePointX = Utils.RandomInt(ref speed, -10, 11) * 0.15f;
            float shakePointY = Utils.RandomInt(ref speed, -5, 6) * 0.3f;
            int x = (int)(drawInfo.Position.X + player.width / 2f - Main.screenPosition.X + shakePointX);
            int y = (int)(drawInfo.Position.Y + player.height / 2f - Main.screenPosition.Y + shakePointY - 4);
            DrawData drawData = new DrawData(texture, new Vector2(x, y) + new Vector2(drawInfo.headOnlyRender ? 1f * player.direction : 0f, 0f), new Rectangle?(bodyFrame), color, player.bodyRotation, new Vector2(texture.Width / 2f, height / 2f), 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}

sealed class FlametrackerHatMask : PlayerDrawLayer {
    private static Asset<Texture2D> hatMaskTexture = null!;

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        hatMaskTexture = ModContent.Request<Texture2D>(ResourceManager.MagicArmorTextures + "FlametrackerMask_Up");
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.CheckArmorSlot(ModContent.ItemType<FlametrackerHat>(), 0, 10) ||
           drawInfo.drawPlayer.CheckVanitySlot(ModContent.ItemType<FlametrackerHat>(), 10);

    public override Position GetDefaultPosition()
        => new AfterParent(PlayerDrawLayers.FaceAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.hideEntirePlayer) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || player.dead || !player.ZoneUnderworldHeight)
            return;

        Texture2D texture = hatMaskTexture.Value;
        Rectangle bodyFrame = player.bodyFrame;
        Color color = drawInfo.colorArmorHead;

        int height = texture.Height / 20;
        int x = (int)(drawInfo.Position.X + player.width / 2.0 - Main.screenPosition.X);
        int y = (int)(drawInfo.Position.Y + player.height / 2.0 - Main.screenPosition.Y - 4);
        DrawData drawData = new DrawData(texture, new Vector2(x, y), new Rectangle?(bodyFrame), color, player.bodyRotation, new Vector2(texture.Width / 2f, height / 2f), 1f, drawInfo.playerEffect, 0);
        drawInfo.DrawDataCache.Add(drawData);
    }
}