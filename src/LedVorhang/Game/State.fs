module Game.State

open System
open Elmish
open Game.Model

let rnd = Random()

let rec newFoodPos (model:Model) =
    let p = { X = rnd.Next(1, 39); Y = rnd.Next(1, 39) }
    
    if p |> Snake.IsPartOfSnake model.Player1 then
        newFoodPos model
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
let update msg (model:Model) =
    match msg with
    | Gamepad1ButtonPressed Dragon ->
         {model with Player1ControlerMirrored = not model.Player1ControlerMirrored}, Cmd.none
    | Gamepad2ButtonPressed Dragon ->
         {model with Player2ControlerMirrored = not model.Player2ControlerMirrored}, Cmd.none
         
    | Gamepad1ButtonPressed Start // TODO recht 1x when?  // when model.IsPreGame 
    | Gamepad2ButtonPressed Start when model.IsPreGame ->
        { model with
            IsPreGame = false

            Player1 = initSnake1()
            Player1Points = 0
            Player2 = initSnake2()
            Player2Points = 0

            Food = newFoodPos model }, Cmd.none
         
    | Gamepad1ButtonPressed _  
    | Gamepad2ButtonPressed _ -> 
        model, Cmd.none
    
    | Gamepad1DirectionPressed direction ->
        { model with
            Player1 = moveSnake direction model.Player1ControlerMirrored model.Player1 }, Cmd.none
    | Gamepad2DirectionPressed direction ->
        { model with
            Player2 = moveSnake direction model.Player2ControlerMirrored model.Player2 }, Cmd.none
            
    | Tick when not model.IsPreGame ->
        let newPlayer1 = model.Player1 |> Snake.Move
        let newPlayer2 = model.Player2 |> Snake.Move
        
        let gameOver =
            Snake.IsCollisionWithBorder newPlayer1
            || Snake.IsCollisionWithTail newPlayer1
            || Snake.IsCollisionWithBorder newPlayer2
            || Snake.IsCollisionWithTail newPlayer2
            || Snake.IsCollisionWithEachOther newPlayer1 newPlayer2
            
        let cmd =
            if gameOver then
                Cmd.ofMsg GameOver
            else
                Cmd.none

        let model = { model with Player1 = newPlayer1; Player2 = newPlayer2 }
        
        let model =          
            if model.Player1.Head = model.Food then
                let newFood = newFoodPos model
                
                { model with
                    Player1Points = model.Player1Points + 1
                    Food = newFood }
            elif model.Player2.Head = model.Food then
                let newFood = newFoodPos model
                
                { model with
                    Player2Points = model.Player2Points + 1
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