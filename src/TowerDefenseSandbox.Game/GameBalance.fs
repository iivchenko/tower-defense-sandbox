namespace TowerDefenseSandbox.Game

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame

type EnemyType = 
    | Standard
    | Fast
    | Hard

type EnemyInfo = 
    {Life: int
     Speed: float32<pixel/second>
     Pixels: int}

type TurretType = 
    | Regular
    | Slow
    | Splash

type TurretInfo =
    {ViewRadius: float32<pixel>
     Reload: float32<second>}

module GameBalance = 

    let enemyPrices = 
       Map.empty
          .Add(EnemyType.Standard, 10<pixel>)
          .Add(EnemyType.Fast,      1<pixel>)
          .Add(EnemyType.Hard,     30<pixel>)

    let fastEnemyInfo     = { Life = 100;  Speed = 150.0f<pixel/second>; Pixels = 5  }
    let regularEnemyInfo  = { Life = 300;  Speed =  50.0f<pixel/second>; Pixels = 10 }
    let hardEnemyInfo     = { Life = 2000; Speed =  25.0f<pixel/second>; Pixels = 15 }

    let regularTurretPrice =  75
    let slowTurretPrice    = 150
    let splashTurretPrice  = 300
    let regularTurretInfo  = { ViewRadius = 100.0f<pixel>; Reload = 0.1f<second>; }
    let slowTurretInfo     = { ViewRadius = 100.0f<pixel>; Reload = 1.5f<second>; }
    let splashTurretInfo   = { ViewRadius = 100.0f<pixel>; Reload = 2.5f<second>; }