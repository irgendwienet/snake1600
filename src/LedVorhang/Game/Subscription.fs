[<RequireQualifiedAccess>]
module Game.Subscription

open System
open Game.Model
open Gamepad
open HardwareLayer

let private timerSub dispatch =
    let timer = new System.Timers.Timer(100.0)
    timer.Elapsed.Add (fun _ -> dispatch Tick)
    timer.Start()
    { new IDisposable with
        member _.Dispose() = timer.Dispose() }
    
let private gampadSub path buttonPressed directionPressed dispatch =
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

            | 1uy when args.Value < -10000s -> GamepadDirectionButton.Up |> directionPressed |> dispatch
            | 1uy when args.Value > 10000s -> GamepadDirectionButton.Down |> directionPressed |> dispatch
            | 0uy when args.Value < -10000s -> GamepadDirectionButton.Left |> directionPressed |> dispatch
            | 0uy when args.Value > 10000s -> GamepadDirectionButton.Right |> directionPressed |> dispatch

            | _ -> ())

    { new IDisposable with
        member _.Dispose() = gamepad.Dispose() }

let private keyboardSub dispatch =
    let keyboard = new ConsoleKeyboardController()

    keyboard.KeyPressed.Add
        (fun args ->
            match args.KeyCode with
            // Button mapping
            | ConsoleKey.Y -> GamepadButton.A |> Gamepad1ButtonPressed |> dispatch
            | ConsoleKey.X -> GamepadButton.B |> Gamepad1ButtonPressed |> dispatch
            | ConsoleKey.A -> GamepadButton.X |> Gamepad1ButtonPressed |> dispatch
            | ConsoleKey.S -> GamepadButton.Y |> Gamepad1ButtonPressed |> dispatch
            
            | ConsoleKey.Spacebar -> GamepadButton.Start |> Gamepad1ButtonPressed |> dispatch
            | ConsoleKey.M -> GamepadButton.Dragon |> Gamepad1ButtonPressed |> dispatch

            // Direction mapping
            | ConsoleKey.LeftArrow -> GamepadDirectionButton.Left |> Gamepad1DirectionPressed |> dispatch
            | ConsoleKey.RightArrow -> GamepadDirectionButton.Right |> Gamepad1DirectionPressed |> dispatch
            | ConsoleKey.UpArrow -> GamepadDirectionButton.Up |> Gamepad1DirectionPressed |> dispatch
            | ConsoleKey.DownArrow -> GamepadDirectionButton.Down |> Gamepad1DirectionPressed |> dispatch
            | _ -> ())

    { new IDisposable with
        member _.Dispose() = keyboard.Dispose() }
    
let RaspberryPi model =    
    [
        [ "timer" ], timerSub
        [ "gamepad1" ], (gampadSub "/dev/input/js0" Gamepad1ButtonPressed Gamepad1DirectionPressed)
        [ "gamepad2" ], (gampadSub "/dev/input/js1" Gamepad2ButtonPressed Gamepad2DirectionPressed)    
    ]
    
let WindowsPc model =
    [
        [ "gamepad1" ], keyboardSub    
        [ "timer" ], timerSub
    ]
