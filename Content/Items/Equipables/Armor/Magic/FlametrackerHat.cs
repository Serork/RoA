using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Projectiles.Friendly.Magic;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

sealed class FlametrackerSetBonusHandler : ModPlayer {
    public bool IsActive { get; internal set; }

    public bool ShouldAttack;

    public override void ResetEffects() {
        if (ShouldAttack) {
            ShouldAttack = false;
            if (Player.whoAmI == Main.myPlayer) {
                Vector2 mousePos = Player.GetViableMousePosition();
                int direction = (mousePos - Player.GetPlayerCorePoint()).X.GetDirection();
                Vector2 projectilePos = Player.GetPlayerCorePoint() + new Vector2(10f * Player.direction, -26f);
                if (Player.gravDir < 0) {
                    projectilePos.Y += Player.defaultHeight;
                }
                if (Player.head != EquipLoader.GetEquipSlot(RoA.Instance, nameof(FlametrackerHat), EquipType.Head)) {
                    projectilePos = Player.GetPlayerCorePoint();
                }
                Projectile.NewProjectile(Player.GetSource_Misc("frametrackersetbonus"),
                    projectilePos - mousePos.SafeNormalize() * 4f * new Vector2(1f * Player.direction, 1f),
                    Helper.VelocityToPoint(projectilePos, mousePos, 5f),
                    ModContent.ProjectileType<TrackingBolt>(),
                    (int)Player.GetTotalDamage(DamageClass.Magic).ApplyTo(25),
                    Player.GetTotalKnockback(DamageClass.Magic).ApplyTo(1.5f),
                    Player.whoAmI);
            }
        }

        IsActive = false;
    }

    public override void OnConsumeMana(Item item, int manaConsumed) {
        if (IsActive) {
            if (Player.statMana == Player.statManaMax2 && Player.altFunctionUse != 2) {
                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "FireWoosh") { Volume = 5.5f }, Player.Center);
                ShouldAttack = true;
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
            Vector2 position = drawInfo.Position - Main.screenPosition + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2);
            position.X += shakePointX;
            position.Y += shakePointY;
            float x = position.X - player.direction;
            float y = position.Y;
            if (player.gravDir < 0) {
                y += 8;
            }
            DrawData drawData = new DrawData(texture, new Vector2(x, y) + new Vector2(drawInfo.headOnlyRender ? 1f * player.direction : 0f, 0f), new Rectangle?(bodyFrame), color, player.bodyRotation, new Vector2(texture.Width / 2f, height / 2f), 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}

sealed class FlametrackerHatMask : PlayerDrawLayer {
    private static Asset<Texture2D> _hatMaskTexture = null!;

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _hatMaskTexture = ModContent.Request<Texture2D>(ResourceManager.MagicArmorTextures + "FlametrackerMask_Up");
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

        Texture2D texture = _hatMaskTexture.Value;
        Rectangle bodyFrame = player.bodyFrame;
        Color color = drawInfo.colorArmorHead;

        int height = texture.Height / 20;
        Vector2 position = drawInfo.Position - Main.screenPosition + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2);
        float x = position.X;
        float y = position.Y + 4;
        if (player.gravDir < 0) {
            y += 0;
        }
        DrawData drawData = new DrawData(texture, new Vector2(x, y), new Rectangle?(bodyFrame), color, player.bodyRotation, new Vector2(texture.Width / 2f, height / 2f), 1f, drawInfo.playerEffect, 0);
        drawInfo.DrawDataCache.Add(drawData);
    }
}