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
        let newPlayer1 = game.Player1 |> Snake.Move
        let newPlayer2 = game.Player2 |> Snake.Move
        
        let gameOver =
            Snake.IsCollisionWithBorder newPlayer1
            || Snake.IsCollisionWithTail newPlayer1
            || Snake.IsCollisionWithBorder newPlayer2
            || Snake.IsCollisionWithTail newPlayer2
            || Snake.IsCollisionWithEachOther newPlayer1 newPlayer2
            
        let cmd =
            if gameOver then
                Cmd.ofMsg ItsGameOver
            else
                Cmd.none

        let game = { game with Player1 = newPlayer1; Player2 = newPlayer2 }
        
        let game =          
            if game.Player1.Head = game.Food then
                let newFood = newFoodPos game
                
                { game with
                    Player1Points = game.Player1Points + 1
                    Food = newFood }
            elif game.Player2.Head = game.Food then
                let newFood = newFoodPos game
                
                { game with
                    Player2Points = game.Player2Points + 1
                    Food = newFood }
            else
                game
        
        { model with CurrentPage = Game game }, cmd

    | ItsGameOver ->
        let score = {
            Player1Points = Some game.Player1Points
            Player2Points = Some game.Player2Points
        }
        
        { model with CurrentPage = GameOver score }, Cmd.none
        
    | _ ->
        model, Cmd.none

