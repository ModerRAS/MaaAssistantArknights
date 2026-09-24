// <copyright file="StoreShoppingSettingsUserControlModel.cs" company="MaaAssistantArknights">
// Part of the MaaWpfGui project, maintained by the MaaAssistantArknights team (Maa Team)
// Copyright (C) 2021-2025 MaaAssistantArknights Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License v3.0 only as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using MaaWpfGui.Configuration.Single.MaaTask;
using MaaWpfGui.Constants;
using MaaWpfGui.Constants.Enums;
using MaaWpfGui.Extensions;
using MaaWpfGui.Helper;
using MaaWpfGui.Models;
using MaaWpfGui.Models.AsstTasks;
using MaaWpfGui.Utilities.ValueType;
using Stylet;
using static MaaWpfGui.Main.AsstProxy;

namespace MaaWpfGui.ViewModels.UserControl.TaskQueue;

public class StoreShoppingSettingsUserControlModel : TaskSettingsViewModel, StoreShoppingSettingsUserControlModel.ISerialize
{
    private static readonly object _pendingGate = new();

    private static readonly Dictionary<int, PendingStore> _pendingStores = [];

    static StoreShoppingSettingsUserControlModel()
    {
        Instance = new();
        LocalizationHelper.LanguageChanged += Instance.RefreshLocalization;
    }

    public StoreShoppingSettingsUserControlModel()
    {
        Instances.AsstProxy.OnTaskStatusChanged += OnTaskStatusChanged;
    }

    public static StoreShoppingSettingsUserControlModel Instance { get; }

    public bool BuyGreenTicket
    {
        get => GetTaskConfig<StoreShoppingTask>().BuyGreenTicket;
        set => SetTaskConfig<StoreShoppingTask>(t => t.BuyGreenTicket == value, t => t.BuyGreenTicket = value);
    }

    public bool BuyYellowTicket
    {
        get => GetTaskConfig<StoreShoppingTask>().BuyYellowTicket;
        set => SetTaskConfig<StoreShoppingTask>(t => t.BuyYellowTicket == value, t => t.BuyYellowTicket = value);
    }

    public UserDataUpdateTriggerInterval TriggerInterval
    {
        get => GetTaskConfig<StoreShoppingTask>().TriggerInterval;
        set => SetTaskConfig<StoreShoppingTask>(t => t.TriggerInterval == value, t => t.TriggerInterval = value);
    }

    public LocalizedObservableList<UserDataUpdateTriggerInterval> TriggerIntervalList { get; } = new(
        (UserDataUpdateTriggerInterval.EveryTime, "EveryTime"),
        (UserDataUpdateTriggerInterval.Daily, "Daily"),
        (UserDataUpdateTriggerInterval.Weekly, "Weekly"),
        (UserDataUpdateTriggerInterval.Monthly, "Monthly"));

    public string GreenTicketLastSuccessText => FormatLastSuccess(GetTaskConfig<StoreShoppingTask>().GreenTicketLastSuccess);

    public string YellowTicketLastSuccessText => FormatLastSuccess(GetTaskConfig<StoreShoppingTask>().YellowTicketLastSuccess);

    public override void RefreshUI(BaseTask baseTask)
    {
        if (baseTask is StoreShoppingTask)
        {
            Refresh();
        }
    }

    public override (bool? IsSuccess, IEnumerable<int> TaskId) SerializeTask(BaseTask? baseTask, int? taskId = null) => (this as ISerialize).Serialize(baseTask, taskId);

    /// <summary>
    /// 日志里区分同一次商店购物追加的绿票、黄票任务。
    /// </summary>
    /// <param name="taskId">Core 任务 id。</param>
    /// <returns>商店名称后缀。没有对应记录时为空。</returns>
    public static string GetLogSuffix(int taskId)
    {
        lock (_pendingGate)
        {
            return _pendingStores.TryGetValue(taskId, out var pending)
                ? $" ({LocalizationHelper.GetString(pending.NameKey)})"
                : string.Empty;
        }
    }

    private static string FormatLastSuccess(DateTimeOffset? lastSuccess)
    {
        return lastSuccess.HasValue ? lastSuccess.Value.ToLocalTimeString() : string.Empty;
    }

    private void OnTaskStatusChanged(int taskId, TaskItemStatus status)
    {
        if (status != TaskItemStatus.Completed)
        {
            return;
        }

        PendingStore pending;
        lock (_pendingGate)
        {
            if (!_pendingStores.TryGetValue(taskId, out var found))
            {
                return;
            }

            pending = found;
            _pendingStores.Remove(taskId);
        }

        Execute.OnUIThread(() => {
            var now = DateTimeOffset.UtcNow;
            if (pending.Kind == StoreKind.Green)
            {
                pending.Task.GreenTicketLastSuccess = now;
            }
            else
            {
                pending.Task.YellowTicketLastSuccess = now;
            }

            if (ReferenceEquals(TaskSettingVisibilityInfo.CurrentTask, pending.Task))
            {
                Refresh();
            }
        });
    }

    private void RefreshLocalization()
    {
        TriggerIntervalList.RefreshLocalization();
    }

    private enum StoreKind
    {
        Green,
        Yellow,
    }

    private sealed class PendingStore(StoreShoppingTask task, StoreKind kind, string nameKey)
    {
        public StoreShoppingTask Task { get; } = task;

        public StoreKind Kind { get; } = kind;

        public string NameKey { get; } = nameKey;
    }

    private interface ISerialize : ITaskQueueModelSerialize
    {
        (bool? IsSuccess, IEnumerable<int> TaskId) ITaskQueueModelSerialize.Serialize(BaseTask? baseTask, int? taskId)
        {
            if (baseTask is not StoreShoppingTask store)
            {
                return (null, []);
            }

            if (taskId is > 0)
            {
                return (null, []);
            }

            if (!store.BuyGreenTicket && !store.BuyYellowTicket)
            {
                Instances.TaskQueueViewModel.AddLog(LocalizationHelper.GetString("StoreShoppingNothingSelected"), UiLogColor.Info);
                return (null, []);
            }

            List<int> ids = [];
            bool anyDue = false;
            if (!TryAppendStore(store, store.BuyGreenTicket, store.GreenTicketLastSuccess, StoreKind.Green, "GreenTicket@Store@Begin", "MiniGameNameGreenTicketStore", ids, ref anyDue)
                || !TryAppendStore(store, store.BuyYellowTicket, store.YellowTicketLastSuccess, StoreKind.Yellow, "YellowTicket@Store@Begin", "MiniGameNameYellowTicketStore", ids, ref anyDue))
            {
                ForgetPending(ids);
                return (false, []);
            }

            return anyDue ? (true, ids) : (null, []);
        }

        private static bool TryAppendStore(
            StoreShoppingTask store,
            bool enabled,
            DateTimeOffset? lastSuccess,
            StoreKind kind,
            string taskName,
            string nameKey,
            List<int> ids,
            ref bool anyDue)
        {
            if (!enabled)
            {
                return true;
            }

            if (!Helper.TriggerInterval.IsDue(lastSuccess, store.TriggerInterval))
            {
                Instances.TaskQueueViewModel.AddLog(
                    LocalizationHelper.GetStringFormat(
                        "StoreShoppingIntervalNotReached",
                        LocalizationHelper.GetString(nameKey),
                        LocalizationHelper.GetString(store.TriggerInterval.ToString())),
                    UiLogColor.Info);
                return true;
            }

            anyDue = true;
            var task = new AsstCustomTask { CustomTasks = [taskName] };
            var (isSuccess, id) = Instances.AsstProxy.AsstAppendTaskWithEncoding(TaskType.StoreShopping, task);
            if (!isSuccess)
            {
                return false;
            }

            lock (_pendingGate)
            {
                _pendingStores[id] = new PendingStore(store, kind, nameKey);
            }

            ids.Add(id);
            return true;
        }

        private static void ForgetPending(List<int> ids)
        {
            lock (_pendingGate)
            {
                foreach (var id in ids)
                {
                    _pendingStores.Remove(id);
                }
            }
        }
    }
}
