module Game.State

open System
open Elmish
open Game.Model

let rec newFoodPos (model:Model) =
    let rnd = System.Random()
    let p = { X = rnd.Next(1, 39); Y = rnd.Next(1, 39) }
    
    if p |> Snake.IsPartOfSnake model.Player1 then
        newFoodPos model
    else
        p

let update msg (model:Model) =
    match msg with
    | GamepadButtonPressed Dragon ->
         {model with Player1ControlerMirrored = not model.Player1ControlerMirrored}, Cmd.none
         
    | GamepadButtonPressed Start when model.IsPreGame ->
        let player1 = Snake.Init 20 20
        { model with
            IsPreGame = false
            Food = newFoodPos model
            Player1 = player1 }, Cmd.none
         
    | GamepadButtonPressed _ -> 
        model, Cmd.none
    
    | GamepadDirectionPressed Up ->
        { model with
            Player1 = model.Player1 |> Snake.ChangeDirection Direction.Up }, Cmd.none
    | GamepadDirectionPressed Down ->
        { model with
            Player1 = model.Player1 |> Snake.ChangeDirection Direction.Down }, Cmd.none
    | GamepadDirectionPressed Left ->
        let dir = if model.Player1ControlerMirrored then Direction.Right else Direction.Left
        { model with
            Player1 = model.Player1 |> Snake.ChangeDirection dir }, Cmd.none    
    | GamepadDirectionPressed Right ->
        let dir = if model.Player1ControlerMirrored then Direction.Left else Direction.Right
        { model with
            Player1 = model.Player1 |> Snake.ChangeDirection dir }, Cmd.none
            
    | Tick when not model.IsPreGame ->
        let newPlayer1 = model.Player1 |> Snake.Move
        
        let gameOver = Snake.IsCollisionWithBorder newPlayer1 || Snake.IsCollisionWithTail newPlayer1 
        let cmd =
            if gameOver then
                Cmd.ofMsg GameOver
            else
                Cmd.none

        let model = { model with Player1 = newPlayer1 }
        
        let model =          
            if model.Player1.Head = model.Food then
                let newFood = newFoodPos model
                
                Console.WriteLine $"Points: {model.Player1Points + 1}"
                
                { model with
                    Player1Points = model.Player1Points + 1
                    Food = newFood }
            else
                model
        
        model, cmd
        
    | Tick ->
        model, Cmd.none
        
    | GameOver ->
        {
            model with IsPreGame = true
        }, Cmd.none