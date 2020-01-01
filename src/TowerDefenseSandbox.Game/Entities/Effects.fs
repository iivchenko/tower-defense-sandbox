namespace TowerDefenseSandbox.Game.Entities

open System

type Effect = 
| DamageEffect of amount : int
| SlowDownEffect of period : TimeSpan * koefficient : float32
