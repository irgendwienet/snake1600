module Game.State

open System
open Elmish
open Game
open Game.Model
open HardwareLayer
open Microsoft.VisualBasic.CompilerServices

let controller1CanChangeMirrored = (Config.getBool "Controller1CanChangeMirrored" false)
let controller2CanChangeMirrored = (Config.getBool "Controller2CanChangeMirrored" false)


let screensaverUpdate msg model  (s:IScreensaver) =       
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
        s.Update()
        model, Cmd.none
        
    | _ ->
        model, Cmd.none
       
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
       
let higscoreUpdate msg model (page:HighscorePage) =
    // Allowed characters sequence for name input: A-Z, 0-9, then SPACE
    let allowedChars : char array =
        Array.append [|'A'..'Z'|] (Array.append [|'0'..'9'|] [| ' ' |])

    let nextChar (c:char) =
        let idx = allowedChars |> Array.tryFindIndex (fun x -> x = Char.ToUpper c) |> Option.defaultValue 0
        let nextIdx = (idx + 1) % allowedChars.Length
        allowedChars[nextIdx]

    let prevChar (c:char) =
        let idx = allowedChars |> Array.tryFindIndex (fun x -> x = Char.ToUpper c) |> Option.defaultValue 0
        let prevIdx = (idx - 1 + allowedChars.Length) % allowedChars.Length
        allowedChars[prevIdx]

    let updateCharAt (s:string) (pos:int) (f:char -> char) =
        // pos is 1-based EditPosition; ensure within bounds of the name string
        if String.IsNullOrEmpty s then s
        else
            let i = Math.Clamp(pos - 1, 0, s.Length - 1)
            let chars = s.ToCharArray()
            chars[i] <- f chars[i]
            String chars
            
    let isCorrectController =
        match msg with
        | Gamepad1DirectionPressed _ when page.WinningPlayer = SinglePlayer1 -> true
        | Gamepad2DirectionPressed _ when page.WinningPlayer = SinglePlayer2 -> true
        | Gamepad1ButtonPressed _ when page.WinningPlayer = SinglePlayer1 -> true
        | Gamepad2ButtonPressed _ when page.WinningPlayer = SinglePlayer2 -> true
        | _ -> false

    if not isCorrectController then
        model, Cmd.none
    else
        match msg with
        | Gamepad1DirectionPressed Left
        | Gamepad2DirectionPressed Left ->
            let newPos = 
                if page.EditPosition < 4 then page.EditPosition + 1 else page.EditPosition 
            let page = { page with EditPosition = newPos }
            { model with CurrentPage = (AskForHighscore page) }, Cmd.none
            
        | Gamepad1DirectionPressed Right
        | Gamepad2DirectionPressed Right ->
            let newPos = 
                if page.EditPosition > 1 then page.EditPosition - 1 else page.EditPosition 
            let page = { page with EditPosition = newPos }
            { model with CurrentPage = (AskForHighscore page) }, Cmd.none

        | Gamepad1DirectionPressed Up
        | Gamepad2DirectionPressed Up ->
            let newName = updateCharAt page.Name page.EditPosition nextChar
            let page = { page with Name = newName }
            { model with CurrentPage = (AskForHighscore page) }, Cmd.none

        | Gamepad1DirectionPressed Down
        | Gamepad2DirectionPressed Down -> 
            let newName = updateCharAt page.Name page.EditPosition prevChar
            let page = { page with Name = newName }
            { model with CurrentPage = (AskForHighscore page) }, Cmd.none
            
        | Gamepad1ButtonPressed Start
        | Gamepad1ButtonPressed A
        | Gamepad2ButtonPressed Start
        | Gamepad2ButtonPressed A ->
            Database.LogHighscore page.Score page.Name
            
            { model with CurrentPage = ShowScore page.Game }, Cmd.none
            
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
                    
                    StartTime = DateTime.Now
                    
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
                    
                    StartTime = DateTime.Now
                    
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
                    
                    StartTime = DateTime.Now
                    
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


let showScoreUpdate msg model game =   
    match msg with
    | Gamepad1ButtonPressed _ 
    | Gamepad2ButtonPressed _ ->
        { model with
            CurrentPageOpenSince = DateTime.Now
            CurrentPage = SelectPlayers game.Mode }, Cmd.none
    | _ ->
        model, Cmd.none

let gameOverUpdate msg model game waitingtime =
    let model = 
        if waitingtime > 0 then
            { model with CurrentPage = GameOver (game, waitingtime - 1) }
        else
            
            let points = Math.Max(game.Player1Points, game.Player2Points)    
            match Database.isWithinTopN 5 points with
            | Some top ->             
                let highscorePage = {
                    Score = points
                    Name = "AAAA"
                    Position = top
                    EditPosition = 1
                    Game = game
                    WinningPlayer = if game.Player1Points > game.Player2Points then SinglePlayer1 else SinglePlayer2
                } 
                { model with CurrentPage = (AskForHighscore highscorePage) }
            | _ ->
                { model with CurrentPage = ShowScore game }
            
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
        | AskForHighscore _ when pageAge > TimeSpan.FromSeconds(120) ->
            { model with CurrentPage = startPage }
        | ShowScore _  when pageAge > TimeSpan.FromSeconds(120) ->
            { model with CurrentPage = startPage }
        | _ -> model
                                          
    match msg with
    // Das ist unabhängig der Page
    | Gamepad1ButtonPressed Dragon ->
         if controller1CanChangeMirrored then
            {model with Player1ControlerMirrored = not model.Player1ControlerMirrored}, Cmd.none
         else
             model, Cmd.none
    | Gamepad2ButtonPressed Dragon ->
         if controller2CanChangeMirrored then
            {model with Player2ControlerMirrored = not model.Player2ControlerMirrored}, Cmd.none
         else
             model, Cmd.none        
    | ViewRefreshed -> { model with ViewNeedsRefresh = false }, Cmd.none
         
    | _ ->
        match model.CurrentPage with
        | SelectPlayers mode -> selectPlayersUpdate msg model mode
        | Game game -> GameState.gameUpdate msg model game
        | GameOver (game, waitingTime) -> gameOverUpdate msg model game waitingTime
        | ShowScore game -> showScoreUpdate msg model game
        | Text page -> textUpdate msg model page
        | Screensaver s -> screensaverUpdate msg model s
        | AskForHighscore page -> higscoreUpdate msg model page
