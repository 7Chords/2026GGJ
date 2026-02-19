# 战斗系统重构说明

## 结构概览

- **IBattleContext / BattleContext**  
  抽象当前战斗的数据与操作（伤害、治疗、移除部位、结束战斗等）。  
  扩展方向：可替换为多敌人、观战、回放等不同实现。

- **BattlePartExecutionQueue**  
  单方部位执行队列：支持按序执行、任意位置插入、按引用删除。  
  与回合流程解耦，仅负责队列与“执行下一个”的驱动。

- **BattleCancelToken**  
  战斗流程取消令牌。`TerminateBattle` 或 `OnDiscard` 时调用 `Cancel()`，所有基于 `SCTimeCaller` 的延迟回调在每步前检查 `IsCancelled`，不再执行后续步骤。无需依赖协程的 `KillAllCoroutines`，逻辑更清晰。

- **战斗流程不再使用协程**  
  单部位执行（开始 → 效果 → 结束）改为 **延迟回调链**：用 `SCTimeCaller.CallDealy` 串联多步，每步前检查 `_cancelToken.IsCancelled`。便于取消、测试和扩展（例如后续可改为可配置的步骤序列或时间轴）。

- **BattleManager**  
  组合上述组件：设置 `BattleContext.Current`、持有 `_cancelToken`、驱动双方队列、处理回合结束与战斗终止。  
  对外 API 保持不变（`StartBattle`、`StartExecuteParts`、`InsertPartAt`、`RemovePartFromList` 等）。

- **IPartEffectHandler + PartEffectHandlerRegistry**  
  效果按 `EAttributeType` 注册，新增效果只需实现 `IPartEffectHandler` 并 `Register`，无需改 `PartLogicFactory`。

## 扩展新效果

1. 在 `GameCore.Battle.Effects` 下新建类，实现 `IPartEffectHandler`。
2. 在 `PartEffectHandlerRegistry.RegisterDefaults()` 中注册（或运行时 `PartEffectHandlerRegistry.Register(type, handler)`）。

## 扩展战斗流程

- 替换 `BattleContext`：实现 `IBattleContext`，在 `StartBattle` 前设置 `BattleContext.Current = yourContext`。
- 修改回合顺序：在 `BattleManager.StartBattle` 中调整 `StartExecuteParts` 的调用顺序与回调即可。
