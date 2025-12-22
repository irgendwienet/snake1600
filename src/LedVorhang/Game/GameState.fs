module Game.GameState

open System
open Elmish
open Game.Model

let rnd = Random()

let rec newFoodPos (game:Game) =
    let p = { X = rnd.Next(1, 39); Y = rnd.Next(1, 39) }
    
    if (game.Player1 |> Option.map (Snake.IsPartOfSnake p) |> Option.defaultValue false)
        || (game.Player2 |> Option.map (Snake.IsPartOfSnake p) |> Option.defaultValue false) then
        newFoodPos game
    else
        p
        
let moveSnake gamepadDirectionButton (snake:Snake Option)    =
    let f =
        match gamepadDirectionButton with
        | Up -> Snake.ChangeDirection Direction.Up
        | Down -> Snake.ChangeDirection Direction.Down
        | Left -> Snake.ChangeDirection Direction.Left        
        | Right -> Snake.ChangeDirection Direction.Right
                
    snake |> Option.map f 
    
let move (snake: Snake Option) isAlive =    
    if isAlive then
        snake |> Option.map Snake.Move
    else
        snake
    
let gameUpdate msg (model:Model) (game:Game) =
    match msg with
    | Gamepad1DirectionPressed direction ->
        let g = { game with Player1 = moveSnake direction game.Player1 }
        { model with CurrentPage = Game g }, Cmd.none

    | Gamepad2DirectionPressed direction ->
        let g = { game with Player2 = moveSnake direction game.Player2 }
        { model with CurrentPage = Game g }, Cmd.none

    | Tick ->
        let newPlayer1 = move game.Player1 game.Player1Alive
        let newPlayer2 = move game.Player2 game.Player2Alive
        
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
            if game.Player1Alive && game.Player1.IsSome && game.Player1.Value.Head = game.Food then
                let newFood = newFoodPos game
                
                let player1 = {game.Player1.Value with Growth = game.Player1.Value.Growth + 1}
                
                { game with
                    Player1 = Some player1
                    Player1Points = game.Player1Points + 1
                    Food = newFood }
            elif game.Player2Alive && game.Player2.IsSome && game.Player2.Value.Head = game.Food then
                let newFood = newFoodPos game

                let player2 = {game.Player2.Value with Growth = game.Player2.Value.Growth + 1}
                
                { game with
                    Player2 = Some player2
                    Player2Points = game.Player2Points + 1
                    Food = newFood }
            else
                game
        
        { model with CurrentPage = Game game }, cmd

    | ItsGameOver ->
        
        Database.LogGame
            game.StartTime
            DateTime.Now
            game.Mode
            game.Player1Points
            game.Player2Points
        
        { model with CurrentPage = GameOver (game, 25) }, Cmd.none
        
    | _ ->
        model, Cmd.none

