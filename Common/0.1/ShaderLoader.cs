using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.WorldEvents;
using RoA.Content.Backgrounds;
using RoA.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

using static Terraria.ModLoader.Core.TmodFile;

namespace RoA.Common;

sealed class ShaderLoader : ModSystem {
    public static class WavyShader {
        private static float _waveFactor = 0f;
        private static float _strengthX = 0f;
        private static float _strengthY = 0f;
        private static float _timeFactor = 0f;
        private static float _yFactor = 0f;
        private static Color _drawColor = Color.White;

        public static float WaveFactor {
            get => _waveFactor;
            set => Effect?.Parameters["waveFactor"].SetValue(_waveFactor = value);
        }

        public static float StrengthX {
            get => _strengthX;
            set => Effect?.Parameters["strengthX"].SetValue(_strengthX = value);
        }

        public static float StrengthY {
            get => _strengthY;
            set => Effect?.Parameters["strengthY"].SetValue(_strengthY = value);
        }

        public static float TimeFactor {
            get => _timeFactor;
            set => Effect?.Parameters["timeFactor"].SetValue(_timeFactor = value);
        }

        public static float YFactor {
            get => _yFactor;
            set => Effect?.Parameters["yFactor"].SetValue(_yFactor = value);
        }

        public static Color DrawColor {
            get => _drawColor;
            set => Effect?.Parameters["drawColor"].SetValue((_drawColor = value).ToVector4());
        }

        public static Effect? Effect => _loadedShaders["Wavy"].Value;

        public static void Apply(SpriteBatch batch, Action draw) {
            SpriteBatchSnapshot snapshot = batch.CaptureSnapshot();
            batch.End();
            batch.Begin(SpriteSortMode.Immediate, snapshot.blendState, snapshot.samplerState, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, snapshot.transformationMatrix);
            Effect?.CurrentTechnique.Passes[0].Apply();
            draw();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
        }
    }

    public static class VerticalAppearanceShader {
        private static float _progress;
        private static float _size, _size2;
        private static float _min = 0f;
        private static float _max = 1f;
        private static Color _drawColor;

        public static float Progress {
            get => _progress;
            set => Effect?.Parameters["progress"].SetValue(_progress = value);
        }

        public static float Size {
            get => _size;
            set => Effect?.Parameters["size"].SetValue(_size = value);
        }

        public static float Size2 {
            get => _size2;
            set => Effect?.Parameters["size2"].SetValue(_size2 = value);
        }

        public static float Min {
            get => _min;
            set => Effect?.Parameters["min"].SetValue(_min = value);
        }

        public static float Max {
            get => _max;
            set => Effect?.Parameters["max"].SetValue(_max = value);
        }

        public static Color DrawColor {
            get => _drawColor;
            set => Effect?.Parameters["drawColor"].SetValue((_drawColor = value).ToVector4());
        }

        public static Effect? Effect => _loadedShaders["VerticalAppearance"].Value;
    }

    private static IDictionary<string, Asset<Effect>> _loadedShaders = new Dictionary<string, Asset<Effect>>();

    public static readonly string BackwoodsSky = RoA.ModName + "Backwoods Sky";
    public static readonly string BackwoodsFog = RoA.ModName + "Backwoods Fog";
    public static readonly string LothorSky = RoA.ModName + "Lothor Sky";
    public static readonly string EnragedLothorSky = RoA.ModName + "Enraged Lothor Sky";
    public static readonly string Vignette = RoA.ModName + "Vignette";

    public static VignetteScreenShaderData VignetteShaderData { get; private set; }
    public static Effect VignetteEffectData { get; private set; }

    public static Asset<Effect> TarDye => _loadedShaders["TarDye"];

    public override void OnModLoad() {
        if (Main.dedServ) {
            return;
        }

        void load01Shaders() {
            Asset<Effect> vignetteShader = ModContent.Request<Effect>(ResourceManager.Effects + "Vignette", AssetRequestMode.ImmediateLoad);
            VignetteEffectData = vignetteShader.Value;
            VignetteShaderData = new VignetteScreenShaderData(vignetteShader.Value, "MainPS");
            Filters.Scene[Vignette] = new Filter(VignetteShaderData, (EffectPriority)100);

            Filters.Scene[BackwoodsSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.2f, 0.2f, 0.2f).UseOpacity(0.05f), EffectPriority.High);
            SkyManager.Instance[BackwoodsSky] = new BackwoodsSky();
            Filters.Scene[BackwoodsSky].Load();

            Filters.Scene[BackwoodsFog] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.5f, 0.5f, 0.5f).UseOpacity(0.75f), EffectPriority.Medium);
            Filters.Scene[BackwoodsFog].Load();

            SkyManager.Instance[LothorSky] = new LothorShakeSky();
            Filters.Scene[LothorSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon"), EffectPriority.High);
            Filters.Scene[LothorSky].Load();

            Filters.Scene[EnragedLothorSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon"), EffectPriority.Medium);
            SkyManager.Instance[EnragedLothorSky] = new EnragedLothorSky();
            Filters.Scene[EnragedLothorSky].Load();

            SkyManager.Instance["CustomAmbience"] = new CustomSkyAmbience.CustomAmbientSky();
        }
        void load02Shaders() {
            var tmodfile = (TmodFile)typeof(RoA).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(RoA.Instance);
            var files = (IDictionary<string, FileEntry>)typeof(TmodFile).GetField("files", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tmodfile);
            string assetsDirectory = $"Resources/";
            foreach (KeyValuePair<string, FileEntry> kvp in files.Where(x => x.Key.Contains(assetsDirectory))) {
                string shaderDirectory = assetsDirectory + "Effects/";
                if (kvp.Key.Contains(shaderDirectory) && kvp.Key.Contains(".xnb")) {
                    string shaderPath = RemoveExtension(kvp.Key, ".xnb");
                    string shaderKey = RemoveDirectory(shaderPath, shaderDirectory);
                    if (shaderKey != "Vignette") {
                        Asset<Effect> shaderAssetToLoad = Mod.Assets.Request<Effect>(shaderPath, AssetRequestMode.ImmediateLoad);
                        _loadedShaders.Add(shaderKey, shaderAssetToLoad);
                    }
                }
            }
        }

        load01Shaders();
        load02Shaders();
    }

    public override void OnModUnload() {
        if (Main.dedServ) {
            return;
        }

        _loadedShaders.Clear();
        _loadedShaders = new Dictionary<string, Asset<Effect>>();
    }

    private static string RemoveExtension(string input, string extensionType) => input[..^extensionType.Length];
    private static string RemoveDirectory(string input, string directory) => input[directory.Length..];
}
