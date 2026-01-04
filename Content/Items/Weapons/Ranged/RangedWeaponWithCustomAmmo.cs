using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Items.Weapons.Ranged;

abstract class RangedWeaponWithCustomAmmo : ModItem {
    private static string AMMOTEXTUREKEY => "_Ammo";
    private static ushort BASETIMEFORAMMORECOVERYINTICKS => MathUtils.SecondsToFrames(2f);
    private static string AMMOSAVEKEY => RoA.ModName + "customammo";


    private static Asset<Texture2D> _ammoTexture = null!;

    private static bool IsAmmoTextureLoaded => _ammoTexture?.IsLoaded == true;

    private byte _currentAmmoAmount = 0;
    private ushort _timeForAmmorRecoveryInTicks = 0;

    private bool HasAmmo => CurrentAmmoCount > 0;
    private bool HasMaxAmmo => CurrentAmmoCount == MaxAmmoCount;

    private byte CurrentAmmoCount {
        get => _currentAmmoAmount;
        set {
            _currentAmmoAmount = (byte)Utils.Clamp(value, 0, MaxAmmoCount);
        }
    }

    private float RecoveryProgress => _timeForAmmorRecoveryInTicks / (float)BASETIMEFORAMMORECOVERYINTICKS;

    private void UseAmmo(Player player) {
        if (!HasAmmo) {
            return;
        }

        var handler = player.GetModPlayer<RangedArmorSetPlayer>();
        if (handler.AllAmmoConsumptionReduce > 0f) {
            if (Main.rand.NextFloat() <= handler.AllAmmoConsumptionReduce) {
                return;
            }
        }

        if (player.ammoBox && Main.rand.Next(5) == 0)
            return;

        if (player.ammoPotion && Main.rand.Next(5) == 0)
            return;

        if (player.huntressAmmoCost90 && Main.rand.Next(10) == 0)
            return;

        if (player.chloroAmmoCost80 && Main.rand.Next(5) == 0)
            return;

        if (player.ammoCost80 && Main.rand.Next(5) == 0)
            return;

        if (player.ammoCost75 && Main.rand.Next(4) == 0)
            return;

        CurrentAmmoCount--;
    }

    private void RecoveryAmmo() {
        if (HasMaxAmmo) {
            return;
        }

        if (_timeForAmmorRecoveryInTicks++ > BASETIMEFORAMMORECOVERYINTICKS) {
            _timeForAmmorRecoveryInTicks = 0;

            CurrentAmmoCount++;
        }
    }

    protected abstract byte MaxAmmoCount { get; }

    public sealed override void Load() {
        if (!Main.dedServ) {
            _ammoTexture = ModContent.Request<Texture2D>(Texture + AMMOTEXTUREKEY);
        }
    }

    public sealed override void SetDefaults() {
        SafeSetDefaults();
    }

    protected virtual void SafeSetDefaults() { }

    public sealed override bool? UseItem(Player player) {
        bool justUsedItem = player.ItemAnimationJustStarted;
        if (justUsedItem) {
            UseAmmo(player);
        }

        return base.UseItem(player);
    }

    public sealed override bool CanUseItem(Player player) => HasAmmo;

    public override void SaveData(TagCompound tag) {
        tag[AMMOSAVEKEY] = CurrentAmmoCount;
    }

    public override void LoadData(TagCompound tag) {
        CurrentAmmoCount = tag.GetByte(AMMOSAVEKEY);
    }

    public sealed override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!IsAmmoTextureLoaded) {
            return;
        }

        void drawAmmo() {
            Vector2 drawPosition = position;
            drawPosition += Item.Size / 3f * scale;
            drawPosition += new Vector2(-4f, 4f);
            for (int i = MaxAmmoCount - 1; i >= 0; i--) {
                Texture2D texture = _ammoTexture.Value;
                Rectangle clip = texture.Bounds;
                Vector2 origin = clip.Centered();
                Color color = drawColor;
                bool hasAmmo = i <= CurrentAmmoCount - 1;
                float progress = RecoveryProgress;
                if (!hasAmmo) {
                    if (i != CurrentAmmoCount) {
                        progress = 0f;
                    }
                    float colorProgress = MathHelper.Lerp(0.5f, 1f, progress);
                    colorProgress = 0.5f;
                    color = color.ModifyRGB(colorProgress);
                }
                int width = texture.Width;
                width = (int)(width * 2.25f * scale);
                DrawInfo drawInfo = new() {
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Scale = Vector2.One * scale
                };
                Vector2 drawOffset = new Vector2(8f, 0f) * scale;
                spriteBatch.Draw(texture, drawPosition + drawOffset, drawInfo, onScreen: false);
                drawPosition.X += width;
                drawPosition.X -= width * MaxAmmoCount / 2f;
            }
        }
        drawAmmo();
    }

    public sealed override void UpdateInventory(Player player) {
        RecoveryAmmo();
    }
}
