using Microsoft.Xna.Framework;

using ReLogic.Content.Sources;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Content.Backgrounds;

using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA;

sealed class RoA : Mod {
    public static readonly string ModSourcePath = Path.Combine(Program.SavePathShared, "ModSources");
    public static readonly string BackwoodsSky = "Backwoods Sky";

    private static RoA? _instance;

    public RoA() {
        _instance = this;
    }

    public static RoA Instance => _instance ??= ModContent.GetInstance<RoA>();

    public static string ModName => Instance.Name;

    public override IContentSource CreateDefaultContentSource() => new CustomContentSource(base.CreateDefaultContentSource());

    public override void Load() {
        TileHelper.Load();
        LoadFilters();
    }

    public override void Unload() {
        TileHelper.Unload();
    }

    private static void LoadFilters() {
        Filters.Scene[BackwoodsSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.2f, 0.2f, 0.2f).UseOpacity(0.05f), EffectPriority.High);
        SkyManager.Instance[BackwoodsSky] = new BackwoodsSky();
        Filters.Scene[BackwoodsSky].Load();
    }
}
