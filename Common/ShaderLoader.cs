using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
﻿using RoA.Common.World;
using RoA.Content.Backgrounds;
using RoA.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

using static Terraria.ModLoader.Core.TmodFile;

namespace RoA.Common;

sealed class ShaderLoader : ModSystem {
    public static class SunShader {
        private static float _time = 0f;
        private static Color _color = Color.Transparent;
        private static float _scale = 1f;
        private static float _rayAlpha = 1f;
        private static Vector2 _screenResolution = Vector2.Zero;
        private static Vector2 _position = Vector2.Zero;

        public static float Time {
            get => _time;
            set => Effect?.Parameters["uTime"].SetValue(_time = value);
        }

        public static Color Color {
            get => _color;
            set => Effect?.Parameters["uColor"].SetValue((_color = value).ToVector3());
        }

        public static float Scale {
            get => _scale;
            set => Effect?.Parameters["uScale"].SetValue(_scale = value);
        }

        public static float RayAlpha {
            get => _rayAlpha;
            set => Effect?.Parameters["uRayAlpha"].SetValue(_rayAlpha = value);
        }

        public static Vector2 ScreenResolution {
            get => _screenResolution;
            set => Effect?.Parameters["uScreenResolution"].SetValue(_screenResolution = value);
        }

        public static Vector2 Position {
            get => _position;
            set => Effect?.Parameters["uPos"].SetValue(_position = value);
        }

        public static Effect? Effect => _loadedShaders["Sun"].Value;
    }

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
            batch.End();
            batch.Begin(in snapshot);
        }
    }

    public static class VerticalAppearanceShader {
        private static float _progress;
        private static float _size, _size2;
        private static float _min = 0f;
        private static float _max = 1f;
        private static Color _drawColor;
        private static bool _fromDown = true;

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

        public static bool FromDown {
            get => _fromDown;
            set => Effect?.Parameters["fromDown"].SetValue(_fromDown = value);
        }

        public static Effect? Effect => _loadedShaders["VerticalAppearance"].Value;

        public static void Reset() {
            FromDown = true;
            DrawColor = Color.White;
            Min = 0f;
            Max = 1f;
            Size = Size2 = 0f;
            Progress = 0f;
        }
    }

    public static void Apply(SpriteBatch batch, Effect? effect, Action draw) {
        SpriteBatchSnapshot snapshot = batch.CaptureSnapshot();
        batch.End();
        batch.Begin(SpriteSortMode.Immediate, snapshot.blendState, snapshot.samplerState, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, snapshot.transformationMatrix);
        effect?.CurrentTechnique.Passes[0].Apply();
        draw();
        batch.End();
        batch.Begin(in snapshot);
    }

    private static IDictionary<string, Asset<Effect>> _loadedShaders = new Dictionary<string, Asset<Effect>>();

    public static readonly string BackwoodsSky = RoA.ModName + "Backwoods Sky";
    public static readonly string BackwoodsFog = RoA.ModName + "Backwoods Fog";
    public static readonly string LothorSky = RoA.ModName + "Lothor Sky";
    public static readonly string EnragedLothorSky = RoA.ModName + "Enraged Lothor Sky";
    public static readonly string FogVignette = RoA.ModName + "FogVignette";
    public static readonly string Vignette = RoA.ModName + "Vignette";

    public static VignetteScreenShaderData FogVignetteShaderData { get; private set; } = null!;
    public static Effect FogVignetteEffectData { get; private set; } = null!;

    public static VignetteScreenShaderData VignetteShaderData { get; private set; } = null!;
    public static Effect VignetteEffectData { get; private set; } = null!;

    public static ArmorShaderData WreathShaderData { get; private set; } = null!;

    public static Asset<Effect> TarDye => _loadedShaders["TarDye"];
    public static Asset<Effect> Wreath => _loadedShaders["Wreath"];
    public static Asset<Effect> MetaballEdgeShader => _loadedShaders["MetaballEdgeShader"];
    public static Asset<Effect> Sandfall => _loadedShaders["Sandfall"];
    public static Asset<Effect> GodDescent => _loadedShaders["GodDescent"];
    public static Asset<Effect> TerraDye => _loadedShaders["TerraDye"];
    public static Asset<Effect> ChromaticAberration => _loadedShaders["ChromaticAberration"];
    public static Asset<Effect> LightCompressor => _loadedShaders["LightCompressor"];

    public static Asset<Effect> SimpleReflection => _loadedShaders["SimpleReflection"];

    public override void OnModLoad() {
        if (Main.dedServ) {
            return;
        }

        void load01Shaders() {
            Asset<Effect> vignetteShader = ModContent.Request<Effect>(ResourceManager.Effects + "FogVignette", AssetRequestMode.ImmediateLoad);
            FogVignetteEffectData = vignetteShader.Value;
            FogVignetteShaderData = new VignetteScreenShaderData(vignetteShader.Value, "MainPS");
            Filters.Scene[FogVignette] = new Filter(FogVignetteShaderData, (EffectPriority)100);

            vignetteShader = ModContent.Request<Effect>(ResourceManager.Effects + "Vignette", AssetRequestMode.ImmediateLoad);
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

        WreathShaderData = new WreathArmorShaderData(ShaderLoader.Wreath, "WreathPass");
    }

    public sealed class WreathArmorShaderData(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
        public override void Apply(Entity entity, DrawData? drawData) {
            base.Apply(entity, drawData);
        }
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
