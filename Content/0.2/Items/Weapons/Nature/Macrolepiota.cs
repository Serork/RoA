using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Common.Projectiles;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class Macrolepiota : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(28, 40);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsageValues(ItemUseStyleID.HiddenAnimation, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public override void HoldItem(Player player) {
        if (!player.IsLocal()) {
            return;
        }

        if (player.HasProjectile<Macrolepiota_Hold>()) {
            return;
        }

        ProjectileHelper.SpawnPlayerOwnedProjectile<Macrolepiota_Hold>(new ProjectileHelper.SpawnProjectileArgs(player, player.GetSource_ItemUse(Item)));
    }
}

sealed class Macrolepiota_Hold : NatureProjectile_NoTextureLoad {
    private const byte FRAMECOUNT = 4;

    private static Asset<Texture2D>? _holdTexture;

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSize(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
    }

    public override void Load() {
        LoadMacrolepiotaTextures();
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        owner.heldProj = Projectile.whoAmI;

        Projectile.timeLeft = 2;

        if (!owner.Holding<Macrolepiota>()) {
            Projectile.Kill();
        }
        if (!owner.IsAliveAndFree()) {
            Projectile.Kill();
        }

        Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter, true);
        Projectile.Center = Utils.Floor(Projectile.Center);
    }

    protected override void Draw(ref Color lightColor) {
        if (_holdTexture?.IsLoaded != true) {
            return;
        }

        Texture2D holdTexture = _holdTexture.Value;
        SpriteFrame spriteFrame = new(1, FRAMECOUNT);
        Vector2 position = Projectile.Center;
        Color color = lightColor;
        Rectangle clip = spriteFrame.GetSourceRectangle(holdTexture);
        Main.spriteBatch.Draw(holdTexture, position, DrawInfo.Default with {
            Origin = Utils.Bottom(clip),
            Color = color,
            Clip = clip
        });
    }

    public override void OnKill(int timeLeft) {

    }

    private void LoadMacrolepiotaTextures() {
        if (Main.dedServ) {
            return;
        }

        _holdTexture = ModContent.Request<Texture2D>(ItemUtils.GetTexturePath<Macrolepiota>() + "_Hold");
    }
}