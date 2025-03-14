module Game.Model

type Game = {
    Food: Position
    
    Player1: Snake
    Player1Points: int
    Player1Alive: bool
            
    Player2: Snake
    Player2Points: int
    Player2Alive: bool
}

type Page =
    | SelectPlayers
    | Game of Game
    | GameOver of Game * int

type Model = {
    Player1ControlerMirrored: bool
    Player2ControlerMirrored: bool
        
    Beat: bool
        
    CurrentPage: Page  
}

let initSnake1 () = Snake.Init 10 20 Direction.Up
let initSnake2 () = Snake.Init 30 20 Direction.Down

let init () =
    {
      Player1ControlerMirrored = false
      Player2ControlerMirrored = false
      
      Beat = false
      
      CurrentPage = SelectPlayers    
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
    
    | ItsGameOver