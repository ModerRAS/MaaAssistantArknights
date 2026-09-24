---
order: 6.5
icon: mdi:cart-outline
---

# Store Shopping

::: info UI-Only Feature
This page covers UI-layer features. See [Getting Started](../newbie.md#about-this-documentation) for details.
:::

Store Shopping adds the green ticket shop and yellow ticket shop to the one-click farming queue and buys on a trigger interval. The purchase rules match the corresponding entries in [Mini Game](./tools.md#mini-game). This task is separate from [Credit Store](./credit.md).

## Shops

Both shops can be selected. They run in the order green ticket shop, then yellow ticket shop.

### Green Ticket Shop

- Buy everything on the 1st floor.
- Buy Headhunting Permits and Recruitment Permits on the 2nd floor.

### Yellow Ticket Shop

Make sure you have at least 258 yellow tickets.

## Trigger Interval

One task shares a single interval. Each shop keeps its own last success time, shown in the settings.

| Option | Description |
| :--- | :--- |
| Every Time | Run every selected shop each time the task queue runs. |
| Daily | Succeed at most once per game day. |
| Weekly | Succeed at most once per ISO week. |
| Monthly | Succeed at most once per game month. Newly added tasks use this by default. |

A game day follows the current client time zone and starts at 04:00. Weekly compares the ISO week of the game day. Monthly compares the year and month of the game day.

The last success time updates only after that shop finishes successfully. A failure or a stop leaves it unchanged. Buying the same shop manually from Mini Game does not update the one-click farming record either. A shop that is not due is skipped. When every selected shop is not due, or none is selected, the whole task is skipped and the queue continues.
