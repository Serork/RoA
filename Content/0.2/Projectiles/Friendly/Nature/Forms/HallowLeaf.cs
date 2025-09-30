using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class HallowLeaf : FormProjectile, IRequestAssets {
    public static byte FRAMECOUNT => 5;
    private static byte FRAMECOUNTER => 4;
    private static ushort TIMELEFT => 180;

    public static float EXTRABRIGHTNESSMODIFIER => 0.25f;

    public enum HallowLeafRequstedTextureType : byte {
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)HallowLeafRequstedTextureType.Glow, Texture + "_Glow")];

    private readonly struct HallowLeafColorInfo(Color[] colors) {
        public readonly Color[] Colors = colors;
    }

    private readonly static HallowLeafColorInfo[] _colors = 
        [new HallowLeafColorInfo([new Color(225, 110, 204), new Color(185, 99, 169), new Color(163, 74, 150)]),
         new HallowLeafColorInfo([new Color(169, 130, 202), new Color(134, 110, 188), new Color(97, 88, 169)]),
         new HallowLeafColorInfo([new Color(209, 97, 115), new Color(173, 76, 78), new Color(142, 59, 89)]),
         new HallowLeafColorInfo([new Color(204, 196, 130), new Color(201, 173, 126), new Color(187, 147, 106)])];

    private Color? _pickedColor;

    public static int AvailableColorCount => _colors.Length - 1;
    public static int ColorSum { get; private set; } = _colors.Sum(colorInfo => colorInfo.Colors.Length);

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float PickedColorIndex => ref Projectile.ai[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value ? 1f : 0f;
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(FRAMECOUNT);
        Projectile.SetTrail(2, 12);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(20);

        Projectile.friendly = true;
        Projectile.penetrate = 1;

        Projectile.timeLeft = TIMELEFT;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (Projectile.timeLeft > TIMELEFT * 0.75f) {
            overPlayers.Add(index);
            return;
        }
        behindProjectiles.Add(index);
    }

    public static int PickIndex() => Main.rand.Next(ColorSum);
    public static int PickIndex(int baseIndex) => baseIndex % ColorSum;
    public static Color GetColor(int index) {
        int colorIndex = (int)((float)index / AvailableColorCount);
        int pickedColorIndex = (int)((float)index % AvailableColorCount);
        return _colors[colorIndex].Colors[pickedColorIndex];
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        if (!Init) {
            Init = true;

            if (owner.IsLocal()) {
                PickedColorIndex = PickIndex();

                Projectile.netUpdate = true;
            }

            ref Vector2 velocity = ref Projectile.velocity;
            float startSpeed = 5f;
            velocity = velocity.SafeNormalize() * startSpeed;
        }

        _pickedColor ??= GetColor((int)PickedColorIndex);

        Projectile.rotation = Projectile.velocity.ToRotation();

        Projectile.Animate(FRAMECOUNTER);
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 12;
        
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool PreDraw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<HallowLeaf>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Texture2D texture = ResourceManager.DefaultSparkle;
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Color color;
        Color pickedColor = _pickedColor!.Value;
        Player owner = Projectile.GetOwnerAsPlayer();
        float progress = BaseForm.BaseFormDataStorage.GetAttackCharge(owner);
        for (int k = 0; k < Projectile.oldPos.Length - 1; k++) {
            Vector2 drawPos = Projectile.oldPos[k] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            color = new Color(pickedColor.R + k * 2, pickedColor.G - k * 3, pickedColor.B + k * 2, 50);
            //color = WreathHandler.GetArmorGlowColor_HallowEnt(owner, color, MathUtils.Clamp01(0f + progress * 2f));
            Main.spriteBatch.Draw(texture, drawPos, null, color * 0.2f, Projectile.oldRot[k] + (float)MathHelper.Pi / 2, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length, effects, 0f);
            //spriteBatch.DrawSelf(texture, drawPos - Projectile.oldPos[k] * 0.5f + Projectile.oldPos[k + 1] * 0.5f, null, color * 0.45f, Projectile.oldRot[k] * 0.5f + Projectile.oldRot[k + 1] * 0.5f + (float)Math.PI / 2, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length, effects, 0f);
        }

        color = Color.Lerp(lightColor, Color.White, EXTRABRIGHTNESSMODIFIER * progress).MultiplyRGB(pickedColor);
        ProjectileUtils.QuickDrawAnimated(Projectile, color);
        Color glowColor = WreathHandler.GetArmorGlowColor_HallowEnt(owner, color, progress);
        ProjectileUtils.QuickDrawAnimated(Projectile, glowColor, texture: indexedTextureAssets[(byte)HallowLeafRequstedTextureType.Glow].Value);

        return false;
    }
}
