using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Tiles.Decorations;

sealed class NixieTubeTE : ModTileEntity {
    public static ushort ACTIVATIONTIME => 100;

    private static RenderTarget2D _tileTarget;

    public bool Activated;
    public bool IsFlickerOff;
    public int DeactivatedTimer, DeactivatedTimer2, ActivatedForTimer;
    public bool Active => DeactivatedTimer <= ACTIVATIONTIME / 3;
    public Item? Dye1 = null;
    public Item? Dye2 = null;
    public Color? LightColor = null;

    public NixieTubeTE() {
        Dye1 ??= new Item();
        Dye2 ??= new Item();
    }

    public void SetDye1(Item item) => Dye1 = item;
    public void SetDye2(Item item) => Dye2 = item;

    public void UpdateLightColor(bool checkForNull = false, int dyeId = 0) {
        Main.QueueMainThreadAction(() => {
            if (!checkForNull || LightColor is null) {
                LightColor = GetBrightestColor();
            }
        });
    }

    public override void Unload() {
        if (!Main.dedServ) {
            Main.QueueMainThreadAction(() => {
                _tileTarget?.Dispose();
            });
        }
    }

    private Color? GetBrightestColor() {
        GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
        if (_tileTarget is null) {
            Main.QueueMainThreadAction(() => _tileTarget = new(Main.graphics.GraphicsDevice, 28, 48));
            return null;
        }
        SpriteBatch spriteBatch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        graphicsDevice.SetRenderTarget(_tileTarget);
        graphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.GraphicsDevice.RasterizerState, null, snapshot.transformationMatrix);
        DrawData drawData = new(TextureAssets.Tile[TileLoader.GetTile(ModContent.TileType<NixieTube>()).Type].Value,
                                Vector2.Zero,
                                new Rectangle(36, 112, 28, 48),
                                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        if (!Dye1.IsEmpty()) {
            GameShaders.Armor.GetShaderFromItemId(Dye1.type).Apply(null, drawData);
        }
        drawData.Draw(spriteBatch);
        graphicsDevice.SetRenderTarget(null);
        spriteBatch.End();
        float getBrightness(Color color) => (color.R * 0.299f + color.G * 0.587f + color.B * 0.114f) / 255f;
        Color[] pixelData = new Color[_tileTarget.Width * _tileTarget.Height];
        _tileTarget.GetData(pixelData);
        Color brightestPixel = Color.Black;
        float maxBrightness = 0f;
        for (int i = 0; i < pixelData.Length; i++) {
            Color pixel = pixelData[i];
            if (pixel.A > 0) {
                float brightness = getBrightness(pixel);
                if (brightness > maxBrightness) {
                    maxBrightness = brightness;
                    brightestPixel = pixel;
                }
            }
        }

        return brightestPixel;
    }

    public override void Update() {
        if (!Activated) {
            return;
        }

        if (!IsFlickerOff && Main.rand.NextBool(750)) {
            Activate();
        }

        if ((DeactivatedTimer -= 5) < 0) {
            DeactivatedTimer = DeactivatedTimer2 / 2;
            DeactivatedTimer2 /= 2;

            if (!Active && Main.rand.NextBool()) {
                Dust dust = Dust.NewDustPerfect(new Point16(Position.X - 1, Position.Y - 2).ToWorldCoordinates() + Main.rand.Random2(-8f, 16f + 8f, 8f, 32f), ModContent.DustType<Dusts.NixieTube>(), newColor: Color.Yellow);
                dust.velocity *= 0.5f;
                dust.alpha = 100;
                if (!Dye1.IsEmpty()) {
                    dust.shader = GameShaders.Armor.GetShaderFromItemId(Dye1.type);
                }
            }
        }
    }

    public void Activate() {
        Activated = true;

        if (!IsFlickerOff) {
            DeactivatedTimer = DeactivatedTimer2 = ACTIVATIONTIME;
            return;
        }

        DeactivatedTimer = DeactivatedTimer2 = 0;
    }

    public override void OnKill() {
        int i = Position.X, j = Position.Y;
        if (!Dye1.IsEmpty()) {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, Dye1.type);
        }
        if (!Dye2.IsEmpty()) {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, Dye2.type);
        }
    }

    public override void SaveData(TagCompound tag) {
        if (Dye1 is not null) {
            tag.Add(RoA.ModName + nameof(Dye1), ItemIO.Save(Dye1));
        }
        if (Dye2 is not null) {
            tag.Add(RoA.ModName + nameof(Dye2), ItemIO.Save(Dye2));
        }
        tag[RoA.ModName + nameof(IsFlickerOff)] = IsFlickerOff;
        tag[RoA.ModName + nameof(Activated)] = Activated;
        if (LightColor is null) {
            tag[RoA.ModName + nameof(LightColor) + "null"] = true;
            return;
        }
        Color lightColor = LightColor.Value;
        tag[RoA.ModName + nameof(LightColor) + "R"] = lightColor.R;
        tag[RoA.ModName + nameof(LightColor) + "G"] = lightColor.G;
        tag[RoA.ModName + nameof(LightColor) + "B"] = lightColor.B;
    }

    public override void LoadData(TagCompound tag) {
        if (tag.TryGet(RoA.ModName + nameof(Dye1), out TagCompound dye1)) {
            Dye1 = ItemIO.Load(dye1);
        }
        if (tag.TryGet(RoA.ModName + nameof(Dye2), out TagCompound dye2)) {
            Dye2 = ItemIO.Load(dye2);
        }
        IsFlickerOff = tag.GetBool(RoA.ModName + nameof(IsFlickerOff));
        Activated = tag.GetBool(RoA.ModName + nameof(Activated));
        bool isLightColorNull = !tag.GetBool(RoA.ModName + nameof(LightColor) + "null");
        if (isLightColorNull) {
            return;
        }
        LightColor = new Color(tag.GetByte(RoA.ModName + nameof(LightColor) + "R"), tag.GetByte(RoA.ModName + nameof(LightColor) + "G"), tag.GetByte(RoA.ModName + nameof(LightColor) + "B"), 255);
    }

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        ushort tapperTileType = (ushort)ModContent.TileType<NixieTube>();
        return tile.HasTile && tile.TileType == tapperTileType;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        int id = Place(i, j);
        return id;
    }
}
