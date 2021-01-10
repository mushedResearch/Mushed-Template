open System
open Expecto
open Superfeed.Test

[<EntryPoint>]
let main argv =
    printfn "WOMBAT_PATH: %s" Tools.wombatPath
    printfn "ACTIVE_DIRECTORY: %s" (IO.Directory.GetCurrentDirectory())
    runTestsInAssembly defaultConfig argv
