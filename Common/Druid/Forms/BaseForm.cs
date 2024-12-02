using Microsoft.Xna.Framework;

using RoA.Utilities;

using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;

[Autoload(false)]
sealed class BaseFormBuff(BaseForm parent) : ModBuff {
    public ModMount Parent { get; private set; } = parent;

    public override string Name => Parent.Name;
    public override string Texture => Parent.NamespacePath() + $"/{Parent.Name.Replace("Mount", "")}Buff";

    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.mount.SetMount(Parent.Type, player);

        player.buffTime[buffIndex] = 10;
    }
}

abstract class BaseForm : ModMount { 
    public readonly BaseFormBuff MountBuff;

    public BaseForm() => MountBuff = new BaseFormBuff(this);

    public override void Load() {
        Mod.AddContent(MountBuff);
    }

    public sealed override void SetStaticDefaults() {
        MountData.buff = MountBuff.Type;

        SafeSetDefaults();

        MountData.playerYOffsets = Enumerable.Repeat(0, MountData.totalFrames).ToArray();
        MountData.xOffset = 0;
        MountData.bodyFrame = 0;
        MountData.yOffset = -10;
        MountData.playerHeadOffset = -10;
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeUpdateEffects(Player player) { }
    protected virtual void SafeUpdateFrame(Player mountedPlayer, int state, Vector2 velocity) { }

    public sealed override void UpdateEffects(Player player) {
        MountData.buff = MountBuff.Type;

        player.GetModPlayer<BaseFormHandler>().IsInDruidicForm = true;

        SafeUpdateEffects(player);
    }

    public sealed override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
        SafeUpdateFrame(mountedPlayer, state, velocity);

        return base.UpdateFrame(mountedPlayer, state, velocity);
    }
}
