module Game.Model

open System.Drawing
open Elmish.Sub

type Model = {
    Food: Position
    
    Player1: Snake
    Player1Points: int
    Player1ControlerMirrored: bool
    
    IsPreGame: bool       
}

let init () =
    {
      Food = { X = 7; Y = 7 }
      
      Player1 = Snake.Init 20 20
      Player1Points = 0
      Player1ControlerMirrored = false
      
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
    | GamepadButtonPressed of GamepadButton
    | GamepadDirectionPressed of GamepadDirectionButton
    
    | Tick
    
    | GameOver