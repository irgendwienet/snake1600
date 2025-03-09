module Game.State

open Elmish
open Game.Model
       
let selectPlayersUpdate msg model =
    match msg with
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

let gameOverUpdate msg model score =
    match msg with
    | Gamepad1ButtonPressed _ 
    | Gamepad2ButtonPressed _ ->
        { model with CurrentPage = SelectPlayers }, Cmd.none
    | _ ->
        model, Cmd.none
        
let update msg (model:Model) =
    match msg with
    // Das ist unabhängig der Page
    | Gamepad1ButtonPressed Dragon ->
         {model with Player1ControlerMirrored = not model.Player1ControlerMirrored}, Cmd.none
    | Gamepad2ButtonPressed Dragon ->
         {model with Player2ControlerMirrored = not model.Player2ControlerMirrored}, Cmd.none
         
    | _ ->
        match model.CurrentPage with
        | SelectPlayers -> selectPlayersUpdate msg model
        | Game game -> GameState.gameUpdate msg model game
        | GameOver score -> gameOverUpdate msg model score
