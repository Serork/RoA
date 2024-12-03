using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
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

    public static IReadOnlyCollection<FormInfo> Forms => _formsByType.Values;
    public FormInfo CurrentForm => _currentForm;
    public bool IsInDruidicForm => CurrentForm != null;

    public bool Is<T>() where T : BaseForm => IsInDruidicForm && CurrentForm.BaseForm.GetType().Equals(typeof(T));

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

    public static void RegisterForm<T>(Predicate<Player> active = null) where T : BaseForm {
        FormInfo formInfo = new(ModContent.GetInstance<T>(), active);
        Type type = typeof(T);
        _formsByType.TryAdd(type, formInfo);
    }

    public static void ApplyForm<T>(Player player, T instance = null) where T : FormInfo {
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        if (handler.IsInDruidicForm) {
            return;
        }

        T formInstance = instance ?? GetForm<T>();
        handler._currentForm = formInstance;
        player.AddBuff(formInstance.MountBuff.Type, 2);
    }

    public static void ReleaseForm<T>(Player player, T instance = null) where T : FormInfo {
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        if (!handler.IsInDruidicForm) {
            return;
        }

        T formInstance = instance ?? GetForm<T>();
        handler._currentForm = null;
        player.ClearBuff(formInstance.MountBuff.Type);
    }

    public static void ToggleForm<T>(Player player, T instance = null) where T : FormInfo {
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

    public override void ResetEffects() {
        ResetActiveForm();
    }

    public override void PostUpdateEquips() {
        ClearForm();
        MakePlayerUnavailableToUseItems();
    }

    private void KeepFormActive() => _shouldBeActive = true;

    private void ResetActiveForm() => _shouldBeActive = false;

    private void MakePlayerUnavailableToUseItems() {
        if (!IsInDruidicForm) {
            return;
        }

        _shouldClear = true;

        Player.controlUseItem = false;
    }

    private void ClearForm() {
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
