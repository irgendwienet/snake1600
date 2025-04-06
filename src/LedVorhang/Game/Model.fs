module Game.Model

open System

type PlayMode =
    | SinglePlayer1
    | SinglePlayer2
    | MultiPlayer

type Game = {
    Mode: PlayMode
    
    Food: Position 
       
    Player1: Snake option
    Player1Points: int
    Player1Alive: bool
            
    Player2: Snake option
    Player2Points: int
    Player2Alive: bool
}

type TextPage = {
    Text: string
    Position: int
}

type Page =
    | Text of TextPage
    | SelectPlayers of PlayMode 
    | Game of Game
    | GameOver of Game * int

type Model = {
    Player1ControlerMirrored: bool
    Player2ControlerMirrored: bool
        
    Beat: bool
    ViewNeedsRefresh: bool
        
    CurrentPageOpenSince: DateTime
    CurrentPage: Page  
}

let initSnake1 () = Snake.Init 10 20 Direction.Up
let initSnake2 () = Snake.Init 30 20 Direction.Down

let startPage = Text { Text = "Kidspace"; Position = 0}  

let init controlerMirrored =
    {
      Player1ControlerMirrored = controlerMirrored
      Player2ControlerMirrored = controlerMirrored
      
      Beat = false
      ViewNeedsRefresh = true
      
      CurrentPageOpenSince = DateTime.Now
      CurrentPage = startPage  
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
    | ViewRefreshed
    
    | ItsGameOver