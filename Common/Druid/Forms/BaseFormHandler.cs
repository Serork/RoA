using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;
sealed class BaseFormHandler : ModPlayer {
    public record FormInfo {
        public BaseForm BaseForm { get; init; }
        public Predicate<Player> ShouldBeActive { get; init; }

        public FormInfo(BaseForm baseForm, Predicate<Player> shouldBeActive = null) {
            BaseForm = baseForm;
            ShouldBeActive = shouldBeActive;
        }

        public BaseFormBuff MountBuff => BaseForm.MountBuff;
    }

    private PlayerDrawLayer[] _layers;
    private bool _shouldClear;

    private static readonly Dictionary<Type, FormInfo> _formsByType = [];
    private FormInfo _currentForm;
    private bool _shouldBeActive;
    private bool _startDecreasingWreath;

    public static IReadOnlyCollection<FormInfo> Forms => _formsByType.Values;
    public FormInfo CurrentForm => _currentForm;
    public bool IsInDruidicForm => CurrentForm != null;

    public bool UsePlayerSpeed { get; internal set; }

    public string Serialize() => CurrentForm.BaseForm.GetType().FullName;
    public FormInfo Deserialize(string typeName) => _formsByType[Type.GetType(typeName)];

    internal void InternalSetCurrentForm<T>(T formInstance) where T : FormInfo {
        if (formInstance != null) {
            _currentForm = formInstance;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new SpawnFormPacket(Player, Serialize()));
            }
        }
    }

    public bool IsConsideredAs<T>() where T : BaseForm => IsInDruidicForm && CurrentForm.BaseForm.GetType().Equals(typeof(T));

    public bool ShouldFormBeActive<T>(T instance = null) where T : FormInfo {
        T formInstance = instance ?? GetForm<T>();
        bool flag = formInstance.ShouldBeActive != null;
        return (!flag && _shouldBeActive) || (flag && formInstance.ShouldBeActive(Player));
    }

    public static void KeepFormActive(Player player) => player.GetModPlayer<BaseFormHandler>().KeepFormActive();

    public static T GetFormInstance<T>() where T : BaseForm {
        if (_formsByType.TryGetValue(typeof(T), out FormInfo form)) {
            return (T)form.BaseForm;
        }

        return default;
    }

    public static T GetForm<T>() where T : FormInfo {
        if (_formsByType.TryGetValue(typeof(T), out FormInfo form)) {
            return (T)form;
        }

        return default;
    }

    public static void RegisterForm<T>(Predicate<Player> active = null) where T : BaseForm => _formsByType.TryAdd(typeof(T), new(ModContent.GetInstance<T>(), active));

    public static void ApplyForm<T>(Player player, T instance = null) where T : FormInfo {
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        T formInstance = instance ?? GetForm<T>();
        if (!player.GetModPlayer<WreathHandler>().IsFull) {
            player.GetModPlayer<WreathHandler>().SlowlyActivateForm(instance);
            return;
        }
        player.AddBuffInStart(formInstance.MountBuff.Type, 3600);
        handler.InternalSetCurrentForm(formInstance);
    }

    public static void ReleaseForm<T>(Player player, T instance = null) where T : FormInfo {
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        T formInstance = instance ?? GetForm<T>();
        if (formInstance != null) {
            player.ClearBuff(formInstance.MountBuff.Type);
            player.GetModPlayer<WreathHandler>().Reset(true);
        }
        handler.HardResetActiveForm();
    }

    public static void ToggleForm<T>(Player player, T instance = null) where T : FormInfo {
        if (player.ItemAnimationActive) {
            return;
        }

        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        T formInstance = instance ?? GetForm<T>();
        if (!handler.ShouldFormBeActive(formInstance)) {
            return;
        }

        if (handler.IsInDruidicForm) {
            ReleaseForm(player, formInstance);

            return;
        }

        ApplyForm(player, formInstance);
    }

    public static void ToggleForm<T>(Player player) where T : BaseForm => ToggleForm(player, _formsByType[typeof(T)]);

    public override void Load() {
        On_TileObject.DrawPreview += On_TileObject_DrawPreview;
        On_Main.DrawInterface_40_InteractItemIcon += On_Main_DrawInterface_40_InteractItemIcon;
    }

    private void On_Main_DrawInterface_40_InteractItemIcon(On_Main.orig_DrawInterface_40_InteractItemIcon orig, Main self) {
        if (Main.player[Main.myPlayer].cursorItemIconID <= 0 && Main.LocalPlayer.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            return;
        }

        orig(self);
    }

    private void On_TileObject_DrawPreview(On_TileObject.orig_DrawPreview orig, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, TileObjectPreviewData op, Vector2 position) {
        if (Main.LocalPlayer.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            return;
        }

        orig(sb, op, position);
    }

    public override void ResetEffects() {
        ResetActiveForm();
    }

    public override void PostUpdateRunSpeeds() { }

    public override void PostUpdate() {
        UsePlayerSpeed = false;
    }

    public override void PostUpdateEquips() {
        ClearForm();
        MakePlayerUnavailableToUseItems();
    }

    private void KeepFormActive() => _shouldBeActive = true;

    private void ResetActiveForm() => _shouldBeActive = false;
    internal void HardResetActiveForm() => _currentForm = null;

    private void MakePlayerUnavailableToUseItems() {
        if (!IsInDruidicForm) {
            return;
        }

        _shouldClear = true;

        Player.controlUseItem = false;
    }

    private void ClearForm() {
        if (IsInDruidicForm && Player.mount.Active && Player.mount._type < MountID.Count) {
            ReleaseForm(Player, _currentForm);
        }

        if (Player.mount.Active && (!_shouldClear || !IsInDruidicForm || _shouldBeActive)) {
            return;
        }

        if (!Player.mount.Active || !ShouldFormBeActive(_currentForm)) {
            ReleaseForm(Player, _currentForm);
        }

        _shouldClear = false;
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
        if (!IsInDruidicForm) {
            return;
        }

        _layers = PlayerDrawLayerLoader.GetDrawLayers(drawInfo);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo) {
        if (!IsInDruidicForm || _layers == null) {
            return;
        }

        foreach (PlayerDrawLayer layer in _layers) {
            if (layer.FullName.Contains("Terraria") && !layer.FullName.Contains("Mount")) {
                layer.Hide();
            }
        }
    }
}
