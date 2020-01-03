namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

type Effect = 
| DamageEffect of amount: int
| SlowDownEffect of period: float32<second> * koefficient: float32
