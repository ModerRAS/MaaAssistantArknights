// <copyright file="StoreShoppingTask.cs" company="MaaAssistantArknights">
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
using MaaWpfGui.Constants.Enums;
using static MaaWpfGui.Main.AsstProxy;

namespace MaaWpfGui.Configuration.Single.MaaTask;

/// <summary>
/// 商店购物任务。购买绿票商店、黄票商店。
/// </summary>
public class StoreShoppingTask : BaseTask
{
    public StoreShoppingTask() => TaskType = TaskType.StoreShopping;

    public bool BuyGreenTicket { get; set; } = true;

    public bool BuyYellowTicket { get; set; }

    public UserDataUpdateTriggerInterval TriggerInterval { get; set; } = UserDataUpdateTriggerInterval.Monthly;

    public DateTimeOffset? GreenTicketLastSuccess { get; set; }

    public DateTimeOffset? YellowTicketLastSuccess { get; set; }
}
