using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.UI;
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

    public bool Activated { get; private set; }
    public bool IsFlickerOff { get; internal set; }
    private int _deactivatedTimer, _deactivatedTimer2;
    public bool Active => _deactivatedTimer <= ACTIVATIONTIME / 3;
    public Item? Dye1 { get; private set; } = null;
    public Item? Dye2 { get; private set; } = null;
    public Color? LightColor { get; internal set; } = null;
    public NixieTubePicker_RemadePicker.Category Category { get; internal set; }

    public NixieTubeTE() {
        Dye1 ??= new Item();
        Dye2 ??= new Item();
    }

    public void SetDye1(Item item) => Dye1 = item;
    public void SetDye2(Item item) => Dye2 = item;

    public void UpdateLightColor(bool checkForNull = false) {
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

        Color brightestColor = ColorUtils.GetBrightestColor(_tileTarget);
        return brightestColor;
    }

    public override void Update() {
        if (!Activated) {
            return;
        }

        if (!IsFlickerOff && Main.rand.NextBool(750)) {
            Activate(true);
        }

        if ((_deactivatedTimer -= 5) < 0) {
            _deactivatedTimer = _deactivatedTimer2 / 2;
            _deactivatedTimer2 /= 2;

            if (!Active && Main.rand.NextBool()) {
                Dust dust = Dust.NewDustPerfect(new Point16(Position.X - 1, Position.Y - 2).ToWorldCoordinates() + Main.rand.Random2(-8f, 16f + 8f, 8f, 32f), ModContent.DustType<Dusts.NixieTube>(), newColor: Color.Yellow);
                dust.velocity *= 0.5f;
                dust.alpha = 100;
                dust.customData = LightColor != null ? LightColor : new Color(224, 74, 0);
                if (!Dye1.IsEmpty()) {
                    dust.shader = GameShaders.Armor.GetShaderFromItemId(Dye1.type);
                }
            }
        }
    }

    public void Activate(bool keepActive = false) {
        Activated = keepActive || !Activated;

        ResetFlicker();
    }

    public void ResetFlicker() {
        if (!IsFlickerOff) {
            _deactivatedTimer = _deactivatedTimer2 = ACTIVATIONTIME;
            return;
        }

        _deactivatedTimer = _deactivatedTimer2 = 0;
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

    const string name = "nixietube";

    public override void SaveData(TagCompound tag) {
        tag[RoA.ModName + name + nameof(Category)] = Category;
        if (Dye1 is not null) {
            tag.Add(RoA.ModName + name + nameof(Dye1), ItemIO.Save(Dye1));
        }
        if (Dye2 is not null) {
            tag.Add(RoA.ModName + name + nameof(Dye2), ItemIO.Save(Dye2));
        }
        tag[RoA.ModName + name + nameof(IsFlickerOff)] = IsFlickerOff;
        tag[RoA.ModName + name + nameof(Activated)] = Activated;
        if (LightColor is null) {
            tag[RoA.ModName + name + nameof(LightColor) + "null"] = true;
            return;
        }
        Color lightColor = LightColor.Value;
        tag[RoA.ModName + name + nameof(LightColor) + "R"] = lightColor.R;
        tag[RoA.ModName + name + nameof(LightColor) + "G"] = lightColor.G;
        tag[RoA.ModName + name + nameof(LightColor) + "B"] = lightColor.B;
    }

    public override void LoadData(TagCompound tag) {
        Category = (NixieTubePicker_RemadePicker.Category)tag.GetByte(RoA.ModName + name + nameof(Category));
        if (tag.TryGet(RoA.ModName + name + nameof(Dye1), out TagCompound dye1)) {
            Dye1 = ItemIO.Load(dye1);
        }
        if (tag.TryGet(RoA.ModName + name + nameof(Dye2), out TagCompound dye2)) {
            Dye2 = ItemIO.Load(dye2);
        }
        IsFlickerOff = tag.GetBool(RoA.ModName + name + nameof(IsFlickerOff));
        Activated = tag.GetBool(RoA.ModName + name + nameof(Activated));
        bool isLightColorNull = !tag.GetBool(RoA.ModName + name + nameof(LightColor) + "null");
        if (isLightColorNull) {
            return;
        }
        LightColor = new Color(tag.GetByte(RoA.ModName + name + nameof(LightColor) + "R"), tag.GetByte(RoA.ModName + name + nameof(LightColor) + "G"), tag.GetByte(RoA.ModName + name + nameof(LightColor) + "B"), 255);
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
