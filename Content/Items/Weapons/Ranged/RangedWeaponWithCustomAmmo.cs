using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Items.Weapons.Ranged;

// also see Hooks_Player.cs
abstract class RangedWeaponWithCustomAmmo : ModItem {
    protected enum BaseMaxAmmoAmount : byte {
        Two,
        Three,
        Four
    }

    private static ushort BASETIMEFORAMMORECOVERYINTICKS => MathUtils.SecondsToFrames(2f);
    private static string AMMOSAVEKEY => RoA.ModName + "customammo";


    private static Asset<Texture2D> _specialAmmoTexture2 = null!,
                                    _specialAmmoTexture3 = null!,
                                    _specialAmmoTexture4 = null!;

    private byte _currentAmmoAmount = 0;
    private ushort _timeForAmmorRecoveryInTicks = 0;

    private byte CurrentAmmoAmount {
        get => _currentAmmoAmount;
        set {
            _currentAmmoAmount = (byte)Utils.Clamp(value, 0, CurrentMaxAmmoAmount);
        }
    }

    private bool HasAmmo => CurrentAmmoAmount > 0;
    private bool HasMaxAmmo => CurrentAmmoAmount == CurrentMaxAmmoAmount;

    private byte CurrentMaxAmmoAmount => (byte)(MaxAmmoAmount + 2);

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

        CurrentAmmoAmount--;
    }

    public void RecoverAmmo() {
        if (HasMaxAmmo) {
            return;
        }

        if (_timeForAmmorRecoveryInTicks++ > BASETIMEFORAMMORECOVERYINTICKS) {
            _timeForAmmorRecoveryInTicks = 0;

            CurrentAmmoAmount++;
        }
    }

    protected abstract BaseMaxAmmoAmount MaxAmmoAmount { get; }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _specialAmmoTexture2 = ModContent.Request<Texture2D>(ResourceManager.UITextures + "SpecialAmmo2");
        _specialAmmoTexture3 = ModContent.Request<Texture2D>(ResourceManager.UITextures + "SpecialAmmo3");
        _specialAmmoTexture4 = ModContent.Request<Texture2D>(ResourceManager.UITextures + "SpecialAmmo4");
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
        tag[AMMOSAVEKEY] = CurrentAmmoAmount;
    }

    public override void LoadData(TagCompound tag) {
        CurrentAmmoAmount = tag.GetByte(AMMOSAVEKEY);
    }

    public sealed override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        //void drawAmmo() {
        //    Vector2 drawPosition = position;
        //    drawPosition += Item.Size / 3f * scale;
        //    drawPosition += new Vector2(-4f, 8f) * scale;
        //    for (int i = MaxAmmoAmount - 1; i >= 0; i--) {
        //        Texture2D ammoTexture = AmmoTexture.Value;
        //        Rectangle ammoClip = ammoTexture.Bounds;
        //        Vector2 origin = ammoClip.Centered();
        //        Color ammoColor = drawColor;
        //        bool hasAmmo = i <= CurrentAmmoAmount - 1;
        //        float progress = RecoveryProgress;
        //        if (!hasAmmo) {
        //            if (i != CurrentAmmoAmount) {
        //                progress = 0f;
        //            }
        //            float colorProgress = MathHelper.Lerp(0.5f, 1f, progress);
        //            colorProgress = 0.5f;
        //            ammoColor = ammoColor.ModifyRGB(colorProgress);
        //        }
        //        int width = ammoTexture.Width;
        //        float scaleModifier = 2f - width / 8f + 0.2f;
        //        scaleModifier = MathF.Round(scaleModifier, 2);
        //        width = (int)(width * 2f * scale);
        //        DrawInfo drawInfo = new() {
        //            Clip = ammoClip,
        //            Origin = origin,
        //            Color = ammoColor,
        //            Scale = Vector2.One * scale * 1.25f
        //        };
        //        Vector2 drawOffset = new Vector2(8f, 0f) * scale;
        //        spriteBatch.Draw(ammoTexture, drawPosition + drawOffset, drawInfo, onScreen: false);
        //        drawPosition.X += width;
        //        drawPosition.X -= width * MaxAmmoAmount / 2f;
        //    }
        //}
        //drawAmmo();

        bool draw2 = true,
             draw3 = false,
             draw4 = false;
        switch (CurrentMaxAmmoAmount) {
            case 3:
                draw3 = true;
                draw2 = false;
                break;
            case 4:
                draw4 = true;
                draw2 = false;
                break;
        }

        Vector2 drawPosition = position;
        drawPosition += new Vector2(12f) * Main.inventoryScale;
        void drawSpecialAmmoIcon() {
            int frameCount = 3;
            Texture2D ammoTexture = _specialAmmoTexture2.Value;
            if (draw3) {
                ammoTexture = _specialAmmoTexture3.Value;
                frameCount = 4;
            }
            if (draw4) {
                ammoTexture = _specialAmmoTexture4.Value;
                frameCount = 5;
            }
            Rectangle ammoClip = Utils.Frame(ammoTexture, 1, frameCount, frameY: frameCount - 1 - CurrentAmmoAmount);
            Vector2 ammoOrigin = ammoClip.Centered();
            Color ammoColor = drawColor;
            DrawInfo drawInfo = new() {
                Clip = ammoClip,
                Origin = ammoOrigin,
                Color = ammoColor,
                Scale = Vector2.One * 0.75f * Main.inventoryScale
            };
            drawPosition.X -= (ammoOrigin.X - 10f);
            spriteBatch.Draw(ammoTexture, drawPosition, drawInfo, onScreen: false);
        }
        drawSpecialAmmoIcon();
    }

    public sealed override void HoldItem(Player player) {
        RecoverAmmo();
    }

    public sealed override void UpdateInventory(Player player) {
        if (player.GetSelectedItem() == Item) {
            return;
        }
        RecoverAmmo();
    }
}
