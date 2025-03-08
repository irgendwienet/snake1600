module Startup

open System
open System.Device.Spi
open Elmish
open Game
open Game.Model
open Gamepad
open HardwareLayer

let createLedDisplay () =

    let createWs2812B chipSelectLine = 
        let settings = SpiConnectionSettings(0, chipSelectLine)
        settings.ClockFrequency <- 3_400_000
        settings.Mode <- SpiMode.Mode0
        settings.DataBitLength <- 8
        
        Ws2812Vorhang(SpiDevice.Create(settings), 20, 20)

    let device0 = createWs2812B(0)
    let device1 = createWs2812B(1)
    let device2 = createWs2812B(3)
    let device3 = createWs2812B(4)

    QuadDeviceDisplay(device1, device3, device2, device0)

let createConsoleDisolay () =
    ConsoleDisplay(true)
    
//let display = createConsoleDisolay()
let display = createLedDisplay()

let subscribe model =
    let timerSub dispatch =
        let timer = new System.Timers.Timer(100.0)
        timer.Elapsed.Add (fun _ -> dispatch Tick)
        timer.Start()
        { new IDisposable with
            member _.Dispose() = timer.Dispose() }
        
    let gampadSub dispatch =
        let gamepad = new GamepadController("/dev/input/js0")
        
        gamepad.ButtonChanged.Add
            (fun args ->
                if args.Pressed then
                    match args.Button with
                    | 0uy -> GamepadButton.A |> GamepadButtonPressed |> dispatch
                    | 1uy -> GamepadButton.B |> GamepadButtonPressed |> dispatch
                    | 2uy -> GamepadButton.X |> GamepadButtonPressed |> dispatch
                    | 3uy -> GamepadButton.Y |> GamepadButtonPressed |> dispatch
                    | 4uy -> GamepadButton.ShoulderLeft |> GamepadButtonPressed |> dispatch
                    | 5uy -> GamepadButton.ShoulderRight |> GamepadButtonPressed |> dispatch
                    | 6uy -> GamepadButton.Back |> GamepadButtonPressed |> dispatch
                    | 7uy -> GamepadButton.Start |> GamepadButtonPressed |> dispatch
                    | 8uy -> GamepadButton.Dragon |> GamepadButtonPressed |> dispatch
                    | _ -> ())
            
        gamepad.AxisChanged.Add
            (fun args ->
                match args.Axis with
                | 7uy when args.Value < 0s -> GamepadDirectionButton.Up |> GamepadDirectionPressed |> dispatch
                | 7uy when args.Value > 0s -> GamepadDirectionButton.Down |> GamepadDirectionPressed |> dispatch
                | 6uy when args.Value < 0s -> GamepadDirectionButton.Left |> GamepadDirectionPressed |> dispatch
                | 6uy when args.Value > 0s -> GamepadDirectionButton.Right |> GamepadDirectionPressed |> dispatch
                | _ -> ())

        { new IDisposable with
            member _.Dispose() = gamepad.Dispose() }

    let keyboardSub dispatch =
        let keyboard = new ConsoleKeyboardController()

        keyboard.KeyPressed.Add
            (fun args ->
                match args.KeyCode with
                // Button mapping
                | ConsoleKey.Y -> GamepadButton.A |> GamepadButtonPressed |> dispatch
                | ConsoleKey.X -> GamepadButton.B |> GamepadButtonPressed |> dispatch
                | ConsoleKey.A -> GamepadButton.X |> GamepadButtonPressed |> dispatch
                | ConsoleKey.S -> GamepadButton.Y |> GamepadButtonPressed |> dispatch

                // Direction mapping
                | ConsoleKey.LeftArrow -> GamepadDirectionButton.Left |> GamepadDirectionPressed |> dispatch
                | ConsoleKey.RightArrow -> GamepadDirectionButton.Right |> GamepadDirectionPressed |> dispatch
                | ConsoleKey.UpArrow -> GamepadDirectionButton.Up |> GamepadDirectionPressed |> dispatch
                | ConsoleKey.DownArrow -> GamepadDirectionButton.Down |> GamepadDirectionPressed |> dispatch
                | _ -> ())

        { new IDisposable with
            member _.Dispose() = keyboard.Dispose() }
    
    [
        [ "timer" ], timerSub
        [ "gamepad" ], gampadSub // keyboardSub // gampadSub
    
    ]

Program.mkProgram (fun _ -> init(), Cmd.none) State.update (View.view display)
|> Program.withSubscription subscribe
|> Program.run

printfn "Drücke ENTER, um die Anwendung zu beenden..."
//Console.ReadLine() |> ignore

while true do
    System.Threading.Thread.Sleep(100000)
