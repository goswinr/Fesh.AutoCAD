namespace Fesh.AutoCAD.Bootstrapper

open System
open System.Windows
open Velopack
open System.Windows.Media
open AvalonLog

module Main =

    let window(log:AvalonLog) =
        let win = Window()
        win.Content <- log
        win.Title <- "Fesh.AutoCAD.Bootstrapper"
        win.Width <- 800.0
        win.Height <- 600.0
        win


    let greet (log:AvalonLog) =
        if AutoCAD.isNetFramework then
            log.printfnBrush Brushes.Blue "Fesh.AutoCAD.Bootstrapper on .NET Framework 4.8"
            log.printfnBrush Brushes.Blue "for Registration and Updating Fesh.AutoCAD with AutoCAD 2024 or earlier versions."
        else
            log.printfnBrush Brushes.Blue "Fesh.AutoCAD.Bootstrapper on .NET 8"
            log.printfnBrush Brushes.Blue "for Registration and Updating Fesh.AutoCAD with AutoCAD 2025 or later versions."


    let velo(log:AvalonLog) =
        let vpk = VelopackApp.Build()

        vpk.SetAutoApplyOnStartup(false)
            |> ignore // to not install updates if they are downloaded

        vpk.WithFirstRun(fun _ ->
            AutoCAD.register(log)
            ) |> ignore // register the app with AutoCAD via xml file

        vpk.WithBeforeUninstallFastCallback(fun _ ->
            AutoCAD.unregister()
            ) |> ignore // delete xml file

        vpk.WithRestarted(                  fun a ->
            log.printfnBrush Brushes.DarkGreen $"Fesh.AutoCAD was updated to {a.Major}.{a.Minor}.{a.Patch} !"
            log.printfnBrush Brushes.Black $"\r\n\r\nYou can close this window now."
            ) |> ignore

        vpk.Run() //https://docs.velopack.io/getting-started/csharp


    [< EntryPoint >]
    [< STAThread >]
    let main (_args: string []) : int =
        try
            let log = AvalonLog()
            log.ShowLineNumbers <- false
            Console.SetOut   (log.GetTextWriter(Brushes.DarkGray))
            Console.SetError (log.GetTextWriter(Brushes.Red))
            greet(log)
            velo(log) // this might kill the app early !

            let win = window(log)
            // win.ContentRendered.Add(fun _ -> velo(log)) // delay till after render ?
            let app  = Application() // do first so that pack Uris work
            app.Run(win)

        with e ->
            eprintfn $"Fesh.AutoCAD.Bootstrapper Start Up Error:\r\n{e}" // can this ever be seen ?
            1



