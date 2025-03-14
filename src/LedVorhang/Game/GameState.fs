module Game.GameState

open System
open Elmish
open Game.Model

let rnd = Random()

let rec newFoodPos (game:Game) =
    let p = { X = rnd.Next(1, 39); Y = rnd.Next(1, 39) }
    
    if p |> Snake.IsPartOfSnake game.Player1 then
        newFoodPos game
    else
        p
        
let moveSnake gamepadDirectionButton mirrored (snake:Snake)    =
    let f =
        match gamepadDirectionButton with
        | Up -> Snake.ChangeDirection Direction.Up
        | Down -> Snake.ChangeDirection Direction.Down
        | Left ->
            if mirrored then
                Snake.ChangeDirection Direction.Right
            else
                Snake.ChangeDirection Direction.Left        
        | Right ->
            if mirrored then
                Snake.ChangeDirection Direction.Left
            else
                Snake.ChangeDirection Direction.Right
                
    f snake
    
let gameUpdate msg (model:Model) (game:Game) =
    match msg with
    | Gamepad1DirectionPressed direction ->
        let g = { game with Player1 = moveSnake direction model.Player1ControlerMirrored game.Player1 }
        { model with CurrentPage = Game g }, Cmd.none

    | Gamepad2DirectionPressed direction ->
        let g = { game with Player2 = moveSnake direction model.Player1ControlerMirrored game.Player2 }
        { model with CurrentPage = Game g }, Cmd.none

    | Tick ->
        let newPlayer1 = if game.Player1Alive then game.Player1 |> Snake.Move else game.Player1
        let newPlayer2 = if game.Player2Alive then game.Player2 |> Snake.Move else game.Player2
        
        let player1Dies =
            Snake.IsCollisionWithBorder newPlayer1
            || Snake.IsCollisionWithTail newPlayer1
            || Snake.IsHeadCollisionWithAnother newPlayer1 newPlayer2
        
        let player2Dies =
            Snake.IsCollisionWithBorder newPlayer2
            || Snake.IsCollisionWithTail newPlayer2
            || Snake.IsHeadCollisionWithAnother newPlayer2 newPlayer1
        
        let game = { game with
                        Player1Alive = game.Player1Alive && not player1Dies
                        Player2Alive = game.Player2Alive && not player2Dies }
                    
        let cmd =
            if not game.Player1Alive && not game.Player2Alive then
                Cmd.ofMsg ItsGameOver
            else
                Cmd.none
        
        let game = { game with
                        Player1 = if game.Player1Alive then newPlayer1 else game.Player1
                        Player2 = if game.Player2Alive then newPlayer2 else game.Player2 }

        let game =          
            if game.Player1Alive && game.Player1.Head = game.Food then
                let newFood = newFoodPos game
                
                { game with
                    Player1.Growth = game.Player1.Growth + 1
                    Player1Points = game.Player1Points + 1
                    Food = newFood }
            elif game.Player2Alive && game.Player2.Head = game.Food then
                let newFood = newFoodPos game
                
                { game with
                    Player2.Growth = game.Player2.Growth + 1
                    Player2Points = game.Player2Points + 1
                    Food = newFood }
            else
                game
        
        { model with CurrentPage = Game game }, cmd

    | ItsGameOver ->
        { model with CurrentPage = GameOver (game, 25) }, Cmd.none
        
    | _ ->
        model, Cmd.none

