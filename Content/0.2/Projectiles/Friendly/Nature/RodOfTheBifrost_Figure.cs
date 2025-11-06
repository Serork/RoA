using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class MagicalBifrostBlock : NatureProjectile {
    private static ushort TIMELEFT => 300;
    private static byte BLOCKCOUNTINFIGURE => 9;

    public enum BifrostFigureType : byte {
        Red1,
        Red2,
        Purple1,
        Purple2,
        Purple3,
        Purple4,
        Blue1,
        Blue2,
        Aqua1,
        Green1,
        Green2,
        Green3,
        Green4,
        DarkOrange1,
        DarkOrange2,
        DarkOrange3,
        DarkOrange4,
        Orange1,
        Orange2,
        Count
    }

    public struct MagicalBifrostBlockInfo() {
        public Point16 StartOffset = Point16.Zero;
        public Point16 FrameCoords = Point16.Zero;

        public readonly bool Active => FrameCoords != Point16.Zero;
    }

    public MagicalBifrostBlockInfo[] MagicalBifrostBlockData = null!;

    public IEnumerable<MagicalBifrostBlockInfo> ActiveMagicalBifrostBlockData => MagicalBifrostBlockData.Where(blockInfo => blockInfo.Active);

    public ref float InitValue => ref Projectile.localAI[0];

    public ref float FigureTypeValue => ref Projectile.ai[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public BifrostFigureType FigureType {
        get => (BifrostFigureType)FigureTypeValue;
        set => FigureTypeValue = Utils.Clamp((byte)value, (byte)BifrostFigureType.Red1, (byte)(BifrostFigureType.Count - 1));
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.aiStyle = -1;
    }

    public override void AI() {
        void init() {
            if (Init) {
                return;
            }

            Player owner = Projectile.GetOwnerAsPlayer();
            Projectile.Center = owner.GetWorldMousePosition();

            if (Projectile.IsOwnerLocal()) {
                FigureType = Main.rand.GetRandomEnumValue<BifrostFigureType>();
                Projectile.netUpdate = true;
            }

            InitializeFigure();

            Init = true;
        }

        init();

        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            Dust.NewDustPerfect(GetBlockPosition(blockInfo), DustID.Adamantite, Vector2.Zero).noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Color color = lightColor;
        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            Vector2 position = GetBlockPosition(blockInfo);
            Point16 frameCoords = blockInfo.FrameCoords;
            Rectangle clip = new(frameCoords.X * 18, frameCoords.Y * 18, 16, 16);
            Vector2 origin = clip.Centered();
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color
            });
        }

        return false;
    }

    private Vector2 GetBlockPosition(MagicalBifrostBlockInfo blockInfo) {
        Vector2 result;
        result = (Projectile.Center.ToTileCoordinates16() + blockInfo.StartOffset).ToWorldCoordinates();
        return result;
    }

    private void InitializeFigure() {
        MagicalBifrostBlockData = new MagicalBifrostBlockInfo[BLOCKCOUNTINFIGURE];

        int currentIndexToAdd = 0;
        void addBlockInFigure(Point16 startOffset, Point16 frameCoords) {
            ref MagicalBifrostBlockInfo blockInfo = ref MagicalBifrostBlockData[currentIndexToAdd];
            if (blockInfo.Active) {
                currentIndexToAdd++;
                return;
            }
            blockInfo = new MagicalBifrostBlockInfo {
                StartOffset = startOffset,
                FrameCoords = frameCoords
            };
            currentIndexToAdd++;
        }
        switch (FigureType) {
            case BifrostFigureType.Red1:
                addBlockInFigure(new Point16(1, 0), new Point16(1, 0));
                addBlockInFigure(new Point16(2, 0), new Point16(2, 0));
                addBlockInFigure(new Point16(0, 1), new Point16(0, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(1, 1));
                break;
            case BifrostFigureType.Red2:
                addBlockInFigure(new Point16(0, 0), new Point16(0, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(0, 3));
                addBlockInFigure(new Point16(1, 1), new Point16(1, 3));
                addBlockInFigure(new Point16(1, 2), new Point16(1, 4));
                break;
            case BifrostFigureType.Purple1:
                addBlockInFigure(new Point16(0, 0), new Point16(3, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(4, 0));
                addBlockInFigure(new Point16(2, 0), new Point16(5, 0));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 1));
                break;
            case BifrostFigureType.Purple2:
                addBlockInFigure(new Point16(1, 0), new Point16(4, 2));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 3));
                addBlockInFigure(new Point16(1, 2), new Point16(4, 4));
                addBlockInFigure(new Point16(0, 1), new Point16(3, 3));
                break;
            case BifrostFigureType.Purple3:
                addBlockInFigure(new Point16(1, 0), new Point16(4, 5));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 6));
                addBlockInFigure(new Point16(0, 1), new Point16(3, 6));
                addBlockInFigure(new Point16(2, 1), new Point16(5, 6));
                break;
            case BifrostFigureType.Purple4:
                addBlockInFigure(new Point16(0, 0), new Point16(3, 7));
                addBlockInFigure(new Point16(0, 1), new Point16(3, 8));
                addBlockInFigure(new Point16(0, 2), new Point16(3, 9));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 8));
                break;
            case BifrostFigureType.Blue1:
                addBlockInFigure(new Point16(0, 0), new Point16(6, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(7, 0));
                addBlockInFigure(new Point16(1, 1), new Point16(7, 1));
                addBlockInFigure(new Point16(2, 1), new Point16(8, 1));
                break;
            case BifrostFigureType.Blue2:
                addBlockInFigure(new Point16(1, 0), new Point16(7, 2));
                addBlockInFigure(new Point16(1, 1), new Point16(7, 3));
                addBlockInFigure(new Point16(0, 1), new Point16(6, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(6, 4));
                break;
            case BifrostFigureType.Aqua1:
                addBlockInFigure(new Point16(0, 0), new Point16(9, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(10, 0));
                addBlockInFigure(new Point16(0, 1), new Point16(9, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(10, 1));
                break;
            case BifrostFigureType.Green1:
                addBlockInFigure(new Point16(0, 0), new Point16(11, 0));
                addBlockInFigure(new Point16(0, 1), new Point16(11, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(12, 1));
                addBlockInFigure(new Point16(2, 1), new Point16(13, 1));
                break;
            case BifrostFigureType.Green2:
                addBlockInFigure(new Point16(0, 0), new Point16(11, 2));
                addBlockInFigure(new Point16(1, 0), new Point16(12, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(11, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(11, 4));
                break;
            case BifrostFigureType.Green3:
                addBlockInFigure(new Point16(0, 0), new Point16(11, 5));
                addBlockInFigure(new Point16(1, 0), new Point16(12, 5));
                addBlockInFigure(new Point16(2, 0), new Point16(13, 5));
                addBlockInFigure(new Point16(2, 1), new Point16(13, 6));
                break;
            case BifrostFigureType.Green4:
                addBlockInFigure(new Point16(1, 0), new Point16(12, 7));
                addBlockInFigure(new Point16(1, 1), new Point16(12, 8));
                addBlockInFigure(new Point16(1, 2), new Point16(12, 9));
                addBlockInFigure(new Point16(0, 2), new Point16(11, 9));
                break;
            case BifrostFigureType.DarkOrange1:
                addBlockInFigure(new Point16(0, 1), new Point16(14, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(15, 1));
                addBlockInFigure(new Point16(2, 1), new Point16(16, 1));
                addBlockInFigure(new Point16(2, 0), new Point16(16, 0));
                break;
            case BifrostFigureType.DarkOrange2:
                addBlockInFigure(new Point16(0, 0), new Point16(14, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(14, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(14, 4));
                addBlockInFigure(new Point16(1, 2), new Point16(15, 4));
                break;
            case BifrostFigureType.DarkOrange3:
                addBlockInFigure(new Point16(0, 0), new Point16(14, 5));
                addBlockInFigure(new Point16(1, 0), new Point16(15, 5));
                addBlockInFigure(new Point16(2, 0), new Point16(16, 5));
                addBlockInFigure(new Point16(0, 1), new Point16(14, 6));
                break;
            case BifrostFigureType.DarkOrange4:
                addBlockInFigure(new Point16(0, 0), new Point16(14, 7));
                addBlockInFigure(new Point16(1, 0), new Point16(15, 7));
                addBlockInFigure(new Point16(1, 1), new Point16(15, 8));
                addBlockInFigure(new Point16(1, 2), new Point16(15, 9));
                break;
            case BifrostFigureType.Orange1:
                addBlockInFigure(new Point16(0, 0), new Point16(17, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(18, 0));
                addBlockInFigure(new Point16(2, 0), new Point16(19, 0));
                addBlockInFigure(new Point16(3, 0), new Point16(20, 0));
                break;
            case BifrostFigureType.Orange2:
                addBlockInFigure(new Point16(0, 0), new Point16(17, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(17, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(17, 4));
                addBlockInFigure(new Point16(0, 3), new Point16(17, 5));
                break;
        }
    }
}
