module Game.Model

open System.Drawing
open Elmish.Sub

type Model = {
    Food: Position
    
    Player1: Snake
    Player1Points: int
    Player1ControlerMirrored: bool

    Player2: Snake
    Player2Points: int
    Player2ControlerMirrored: bool
        
    IsPreGame: bool       
}

let initSnake1 () = Snake.Init 10 20 Direction.Up
let initSnake2 () = Snake.Init 30 20 Direction.Down

let init () =
    {
      Food = { X = 7; Y = 7 }
      
      Player1 = initSnake1()
      Player1Points = 0
      Player1ControlerMirrored = false

      Player2 = initSnake2()
      Player2Points = 0
      Player2ControlerMirrored = false
      
      IsPreGame = true;
    }

type GamepadButton =
    | X
    | Y
    | A
    | B
    | Start
    | Back
    | ShoulderLeft
    | ShoulderRight
    | Dragon
    
type GamepadDirectionButton =
    | Up
    | Down
    | Left
    | Right

type Msg =
    | Gamepad1ButtonPressed of GamepadButton
    | Gamepad1DirectionPressed of GamepadDirectionButton
    | Gamepad2ButtonPressed of GamepadButton
    | Gamepad2DirectionPressed of GamepadDirectionButton
    
    | Tick
    
    | GameOver