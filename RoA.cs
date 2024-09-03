using ReLogic.Content.Sources;

using RoA.Common.Networking;
using RoA.Content.Backgrounds;
using RoA.Core;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA;

sealed class RoA : Mod {
    public static readonly string ModSourcePath = Path.Combine(Program.SavePathShared, "ModSources");

    private static RoA? _instance;

    public RoA() {
        _instance = this;
    }

    public static RoA Instance => _instance ??= ModContent.GetInstance<RoA>();

    public static string ModName => Instance.Name;

    public override IContentSource CreateDefaultContentSource() => new CustomContentSource(base.CreateDefaultContentSource());

    public override void HandlePacket(BinaryReader reader, int sender) => MultiplayerSystem.HandlePacket(reader, sender);

    public override void Load() {
        TileHelper.Load();
    }

    public override void Unload() {
        TileHelper.Unload();
    }
}
