// <copyright file="TriggerInterval.cs" company="MaaAssistantArknights">
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
using System.Globalization;
using MaaWpfGui.Constants.Enums;
using MaaWpfGui.Extensions;

namespace MaaWpfGui.Helper;

/// <summary>
/// 按游戏日判断触发间隔是否已到。
/// </summary>
public static class TriggerInterval
{
    /// <summary>
    /// 判断距上次成功是否已经到达触发间隔。
    /// </summary>
    /// <param name="lastSuccessTime">上次成功时间。没有记录时视为到期。</param>
    /// <param name="triggerInterval">触发间隔。</param>
    /// <returns>本次应当执行时返回 <see langword="true"/>。</returns>
    public static bool IsDue(DateTimeOffset? lastSuccessTime, UserDataUpdateTriggerInterval triggerInterval)
    {
        if (triggerInterval == UserDataUpdateTriggerInterval.EveryTime || !lastSuccessTime.HasValue)
        {
            return true;
        }

        var now = DateTimeOffset.UtcNow.ToYjDateTime().Date;
        var lastDate = lastSuccessTime.Value.ToYjDateTime().Date;

        return triggerInterval switch {
            UserDataUpdateTriggerInterval.Daily => now > lastDate,
            UserDataUpdateTriggerInterval.Weekly => ISOWeek.GetYear(now) != ISOWeek.GetYear(lastDate) || ISOWeek.GetWeekOfYear(now) != ISOWeek.GetWeekOfYear(lastDate),
            UserDataUpdateTriggerInterval.Monthly => now.Year != lastDate.Year || now.Month != lastDate.Month,
            _ => true,
        };
    }
}
