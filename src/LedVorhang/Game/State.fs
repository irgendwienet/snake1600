module Game.State

open Elmish
open Game.Model
       
let selectPlayersUpdate msg model mode =
    match msg with
    | Gamepad1DirectionPressed Right
    | Gamepad2DirectionPressed Right ->
        { model with CurrentPage = SelectPlayers MultiPlayer }, Cmd.none
    
    | Gamepad1DirectionPressed Left
    | Gamepad2DirectionPressed Left ->
        { model with CurrentPage = SelectPlayers SinglePlayer }, Cmd.none

    | Gamepad1ButtonPressed Start 
    | Gamepad2ButtonPressed Start ->
        let game = {
            Food = { X=0; Y=0 }
            
            Player1 = initSnake1()
            Player1Points = 0
            Player1Alive = true
            
            Player2 = initSnake2()
            Player2Points = 0
            Player2Alive = true    
        }
        
        let game = { game with Food = GameState.newFoodPos game }
        
        { model with CurrentPage = Game game }, Cmd.none
    | _ ->
        model, Cmd.none

let gameOverUpdate msg model game waitingtime =
    if waitingtime > 0 then
        { model with CurrentPage = GameOver (game, waitingtime - 1) }, Cmd.none
    else
        match msg with
        | Gamepad1ButtonPressed _ 
        | Gamepad2ButtonPressed _ ->
            { model with CurrentPage = SelectPlayers MultiPlayer }, Cmd.none
        | _ ->
            model, Cmd.none
        
let update msg (model:Model) =
    let model = 
        match msg with
        | Tick -> { model with Beat = not model.Beat }
        | _ -> model
    
    let msg = 
        match msg with
        | Gamepad1DirectionPressed Left when model.Player1ControlerMirrored -> Gamepad1DirectionPressed Right
        | Gamepad1DirectionPressed Right when model.Player1ControlerMirrored -> Gamepad1DirectionPressed Left
        | Gamepad2DirectionPressed Left when model.Player2ControlerMirrored -> Gamepad2DirectionPressed Right
        | Gamepad2DirectionPressed Right when model.Player2ControlerMirrored -> Gamepad2DirectionPressed Left
        | _ -> msg
     
    match msg with
    // Das ist unabhängig der Page
    | Gamepad1ButtonPressed Dragon ->
         {model with Player1ControlerMirrored = not model.Player1ControlerMirrored}, Cmd.none
    | Gamepad2ButtonPressed Dragon ->
         {model with Player2ControlerMirrored = not model.Player2ControlerMirrored}, Cmd.none
         
    | _ ->
        match model.CurrentPage with
        | SelectPlayers mode -> selectPlayersUpdate msg model mode
        | Game game -> GameState.gameUpdate msg model game
        | GameOver (game, waitingTime) -> gameOverUpdate msg model game waitingTime
