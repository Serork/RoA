using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Wreath;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;

using static RoA.Common.Druid.Forms.BaseForm;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class HallowLeaf : FormProjectile, IRequestAssets {
    private static byte FRAMECOUNT => 5;
    private static byte FRAMECOUNTER => 4;
    private static ushort TIMELEFT => 180;

    public enum HallowLeafRequstedTextureType : byte {
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)HallowLeafRequstedTextureType.Glow, Texture + "_Glow")];

    private readonly struct HallowLeafColorInfo(Color[] colors) {
        public readonly Color[] Colors = colors;
    }

    private readonly HallowLeafColorInfo[] _colors = 
        [new HallowLeafColorInfo([new Color(225, 110, 204), new Color(185, 99, 169), new Color(163, 74, 150)]),
         new HallowLeafColorInfo([new Color(169, 130, 202), new Color(134, 110, 188), new Color(97, 88, 169)]),
         new HallowLeafColorInfo([new Color(209, 97, 115), new Color(173, 76, 78), new Color(142, 59, 89)]),
         new HallowLeafColorInfo([new Color(204, 196, 130), new Color(201, 173, 126), new Color(187, 147, 106)])];

    private Color? _pickedColor;

    private int AvailableColorCount => _colors.Length - 1;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float PickedColorIndex => ref Projectile.ai[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value ? 1f : 0f;
    }

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

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

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        if (!Init) {
            Init = true;

            int colorCount = _colors.Sum(colorInfo => colorInfo.Colors.Length);
            if (owner.IsLocal()) {
                PickedColorIndex = Main.rand.Next(colorCount);

                Projectile.netUpdate = true;
            }

            ref Vector2 velocity = ref Projectile.velocity;
            float startSpeed = 5f;
            velocity = velocity.SafeNormalize() * startSpeed;
        }

        int colorIndex = (int)(PickedColorIndex / AvailableColorCount);
        int pickedColorIndex = (int)(PickedColorIndex % AvailableColorCount);
        _pickedColor ??= _colors[colorIndex].Colors[pickedColorIndex];

        Projectile.rotation = Projectile.velocity.ToRotation();

        Projectile.Animate(FRAMECOUNTER);
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<HallowLeaf>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Color color = _pickedColor!.Value.MultiplyRGB(lightColor);
        ProjectileUtils.QuickDrawAnimated(Projectile, color);
        Player owner = Projectile.GetOwnerAsPlayer();
        Color glowColor = WreathHandler.GetArmorGlowColor_HallowEnt(owner, color, BaseFormDataStorage.GetAttackCharge(owner));
        ProjectileUtils.QuickDrawAnimated(Projectile, glowColor);

        return false;
    }
}
