module Game.State

open System
open Elmish
open Game.Model
open Microsoft.VisualBasic.CompilerServices
       
let textUpdate msg model page =       
    match msg with
    // AnyKey
    | Gamepad1ButtonPressed _ 
    | Gamepad2ButtonPressed _ 
    | Gamepad1DirectionPressed _ 
    | Gamepad2DirectionPressed _ ->
        { model with
            CurrentPageOpenSince = DateTime.Now
            CurrentPage = SelectPlayers MultiPlayer }, Cmd.none
        
    | Tick ->
        let newPos = 
            if page.Text.Length * 7 + 38 > page.Position then
                page.Position + 1
            else
                -38

        {model with CurrentPage = Text { page with Position = newPos }}, Cmd.none
        
    | _ ->
        model, Cmd.none
       
let selectPlayersUpdate msg model mode =
    match msg with
    | Gamepad1DirectionPressed Right
    | Gamepad2DirectionPressed Right ->
        { model with CurrentPage = SelectPlayers SinglePlayer1 }, Cmd.none
    | Gamepad1DirectionPressed Left
    | Gamepad2DirectionPressed Left ->
        { model with CurrentPage = SelectPlayers MultiPlayer }, Cmd.none
    
    | Gamepad1ButtonPressed Start
    | Gamepad1ButtonPressed A
    | Gamepad2ButtonPressed Start
    | Gamepad2ButtonPressed A ->
        let game =
            match mode, msg with
            | MultiPlayer, _ ->
                {
                    Food = { X=0; Y=0 }
                    Mode = MultiPlayer
                    
                    Player1 = Some (initSnake1())
                    Player1Points = 0
                    Player1Alive = true
                    
                    Player2 = Some (initSnake2())
                    Player2Points = 0
                    Player2Alive = true    
                }
            | _, Gamepad1ButtonPressed _ ->
                {
                    Food = { X=0; Y=0 }
                    Mode = SinglePlayer1
                    
                    Player1 = Some (initSnake1())
                    Player1Points = 0
                    Player1Alive = true
                    
                    Player2 = None
                    Player2Points = 0
                    Player2Alive = false
                }
            | _, Gamepad2ButtonPressed _ ->
                {
                    Food = { X=0; Y=0 }
                    Mode = SinglePlayer2
                    
                    Player1 = None
                    Player1Points = 0
                    Player1Alive = false
                    
                    Player2 = Some (initSnake1())
                    Player2Points = 0
                    Player2Alive = true
                }
            | _ -> failwith "Invalid mode"
        
        
        let game = { game with Food = GameState.newFoodPos game }
        
        { model with CurrentPage = Game game }, Cmd.none
    | _ ->
        model, Cmd.none

let gameOverUpdate msg model game waitingtime =
    let model = { model with CurrentPage = GameOver (game, waitingtime - 1) } 
    
    if waitingtime > 0 then
        model, Cmd.none
    else
        match msg with
        | Gamepad1ButtonPressed _ 
        | Gamepad2ButtonPressed _ ->
            { model with
                CurrentPageOpenSince = DateTime.Now
                CurrentPage = SelectPlayers game.Mode }, Cmd.none
        | _ ->
            model, Cmd.none
        
let update msg (model:Model) =
    let msg = 
        match msg with
        | Gamepad1DirectionPressed Left when model.Player1ControlerMirrored -> Gamepad1DirectionPressed Right
        | Gamepad1DirectionPressed Right when model.Player1ControlerMirrored -> Gamepad1DirectionPressed Left
        | Gamepad2DirectionPressed Left when model.Player2ControlerMirrored -> Gamepad2DirectionPressed Right
        | Gamepad2DirectionPressed Right when model.Player2ControlerMirrored -> Gamepad2DirectionPressed Left
        | _ -> msg

    let model = 
        match msg with
        | Tick ->
            { model with
                Beat = not model.Beat
                ViewNeedsRefresh = true }
        | _ -> model
        
    let pageAge = DateTime.Now.Subtract(model.CurrentPageOpenSince) 
    let model =
        match model.CurrentPage with
        | SelectPlayers _ when pageAge > TimeSpan.FromSeconds(120) ->
            { model with CurrentPage = startPage }
        | GameOver _  when pageAge > TimeSpan.FromSeconds(120) ->
            { model with CurrentPage = startPage }
        | _ -> model
                                          
    match msg with
    // Das ist unabhängig der Page
    | Gamepad1ButtonPressed Dragon ->
         {model with Player1ControlerMirrored = not model.Player1ControlerMirrored}, Cmd.none
    | Gamepad2ButtonPressed Dragon ->
         {model with Player2ControlerMirrored = not model.Player2ControlerMirrored}, Cmd.none
    | ViewRefreshed -> { model with ViewNeedsRefresh = false }, Cmd.none
         
    | _ ->
        match model.CurrentPage with
        | SelectPlayers mode -> selectPlayersUpdate msg model mode
        | Game game -> GameState.gameUpdate msg model game
        | GameOver (game, waitingTime) -> gameOverUpdate msg model game waitingTime
        | Text page -> textUpdate msg model page
