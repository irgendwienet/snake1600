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
        
    let gampadSub path buttonPressed directionPressed dispatch =
        let gamepad = new GamepadController(path)
        
        gamepad.ButtonChanged.Add
            (fun args ->
                if args.Pressed then
                    match args.Button with
                    | 0uy -> GamepadButton.A |> buttonPressed |> dispatch
                    | 1uy -> GamepadButton.B |> buttonPressed |> dispatch
                    | 2uy -> GamepadButton.X |> buttonPressed |> dispatch
                    | 3uy -> GamepadButton.Y |> buttonPressed |> dispatch
                    | 4uy -> GamepadButton.ShoulderLeft |> buttonPressed |> dispatch
                    | 5uy -> GamepadButton.ShoulderRight |> buttonPressed |> dispatch
                    | 6uy -> GamepadButton.Back |> buttonPressed |> dispatch
                    | 7uy -> GamepadButton.Start |> buttonPressed |> dispatch
                    | 8uy -> GamepadButton.Dragon |> buttonPressed |> dispatch
                    | _ -> ())
            
        gamepad.AxisChanged.Add
            (fun args ->
                match args.Axis with
                | 7uy when args.Value < 0s -> GamepadDirectionButton.Up |> directionPressed |> dispatch
                | 7uy when args.Value > 0s -> GamepadDirectionButton.Down |> directionPressed |> dispatch
                | 6uy when args.Value < 0s -> GamepadDirectionButton.Left |> directionPressed |> dispatch
                | 6uy when args.Value > 0s -> GamepadDirectionButton.Right |> directionPressed |> dispatch
                | _ -> ())

        { new IDisposable with
            member _.Dispose() = gamepad.Dispose() }

    let keyboardSub dispatch =
        let keyboard = new ConsoleKeyboardController()

        keyboard.KeyPressed.Add
            (fun args ->
                match args.KeyCode with
                // Button mapping
                | ConsoleKey.Y -> GamepadButton.A |> Gamepad1ButtonPressed |> dispatch
                | ConsoleKey.X -> GamepadButton.B |> Gamepad1ButtonPressed |> dispatch
                | ConsoleKey.A -> GamepadButton.X |> Gamepad1ButtonPressed |> dispatch
                | ConsoleKey.S -> GamepadButton.Y |> Gamepad1ButtonPressed |> dispatch

                // Direction mapping
                | ConsoleKey.LeftArrow -> GamepadDirectionButton.Left |> Gamepad1DirectionPressed |> dispatch
                | ConsoleKey.RightArrow -> GamepadDirectionButton.Right |> Gamepad1DirectionPressed |> dispatch
                | ConsoleKey.UpArrow -> GamepadDirectionButton.Up |> Gamepad1DirectionPressed |> dispatch
                | ConsoleKey.DownArrow -> GamepadDirectionButton.Down |> Gamepad1DirectionPressed |> dispatch
                | _ -> ())

        { new IDisposable with
            member _.Dispose() = keyboard.Dispose() }
    
    [
        [ "timer" ], timerSub
        [ "gamepad1" ], (gampadSub "/dev/input/js0" Gamepad1ButtonPressed Gamepad1DirectionPressed) // keyboardSub // gampadSub
        [ "gamepad2" ], (gampadSub "/dev/input/js1" Gamepad2ButtonPressed Gamepad2DirectionPressed)    
    ]

Program.mkProgram (fun _ -> init(), Cmd.none) State.update (View.view display)
|> Program.withSubscription subscribe
|> Program.run

printfn "Drücke ENTER, um die Anwendung zu beenden..."
//Console.ReadLine() |> ignore

while true do
    System.Threading.Thread.Sleep(100000)
