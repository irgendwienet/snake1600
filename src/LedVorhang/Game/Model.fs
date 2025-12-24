module Game.Model

open System
open Elmish
open HardwareLayer

type PlayMode =
    | SinglePlayer1
    | SinglePlayer2
    | MultiPlayer

type Game = {
    Mode: PlayMode
    
    Food: Position 

    StartTime: DateTime
           
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

type HighscorePage = {
    Score: int
    Position: int
    Name: string
    EditPosition: int
    WinningPlayer: PlayMode // nur 1 oder 2
    
    
    Game: Game
}

type Page =
    | Screensaver  of IScreensaver
    | Text of TextPage
    | SelectPlayers of PlayMode 
    | Game of Game
    | GameOver of Game * int
    | AskForHighscore of HighscorePage
    | ShowScore of Game * int

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

let startPage =
    Screensaver (new MatrixScreensaver(40, 40))
   //Text { Text = "Kidspace"; Position = 0}  

let init ()=
    {
      Player1ControlerMirrored = Config.getBool "controller1.mirrored" false
      Player2ControlerMirrored = Config.getBool "controller2.mirrored" false
      
      Beat = false
      ViewNeedsRefresh = true
      
      CurrentPageOpenSince = DateTime.Now
      CurrentPage = startPage  
    }, Cmd.none

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