module Game.View

open System
open System.Drawing
open Game.Model
open HardwareLayer
open HardwareLayer.Fonts

let LightYellow=Color.FromArgb(90,90,0)    
let LowWhite = Color.FromArgb(180, 180, 180)

let tick() = DateTime.Now.Microsecond % 100 > 50

let image = LedImage(40,40)

let setPixel (x:int) (y:int) (color:Color) = image.SetPixel(x, y, color)    
let clear () = image.Clear()

let showSnake headColor tailColor (snake:Snake)=
    setPixel snake.Head.X snake.Head.Y headColor
    snake.Tail |> List.iter (fun pos -> setPixel pos.X pos.Y tailColor)

let showFood (p:Position) =
    setPixel p.X p.Y Color.Green

let drawBorder y x heigth width color =
    for i in 0 .. width do
        setPixel (x+i) y color
        setPixel (x+i) (y+heigth) color
    
    for i in 0 .. heigth do 
        setPixel x (y+i) color
        setPixel (x+width) (y+i) color

let showOuterBorder () =
    drawBorder 0 0 39 39 LowWhite

let showCrossHair color =
    for i in 0..10 do
        setPixel (15+i) (15+i) color
        setPixel (15+i) (25-i) color

let viewLogo y x color  =
    let data = 
        [|
            0b00111100uy
            0b01000010uy
            0b10000001uy
            0b10000001uy
            0b00000001uy
            0b00000001uy
            0b11111111uy
            0b10000001uy
            0b10100101uy
            0b10000001uy
            0b10100101uy
            0b10011001uy
            0b10000001uy
            0b11111111uy |]
    
    TextRenderer.printIcon data 8 color x y setPixel

let viewText page =
    let text = page.Text
    let position = page.Position

    TextRenderer.printTextOffset
        Font.Font_7x9
        text
        Color.Red
        1
        3
        position
        38
        setPixel
        
    viewLogo 15 16 Color.White

let viewSelectPlayers mode =
    
    TextRenderer.printText
        Font.Font_7x9
        "1"
        Color.Blue
        8
        15
        setPixel

    TextRenderer.printText
        Font.Font_7x9
        "2"
        Color.Blue
        25
        15
        setPixel
        
    match mode with
    | SinglePlayer1 
    | SinglePlayer2 ->
        drawBorder
            7 14
            9 10
            Color.Green
    | MultiPlayer ->        
        drawBorder
            24 14
            9 10 
            Color.Green

let viewGame (game:Game) =
    game.Player1 |> Option.iter (showSnake Color.Red Color.Yellow)
    game.Player2 |> Option.iter (showSnake Color.Blue Color.DarkOliveGreen)
    game.Food |> showFood   

let viewGameOver (game:Game) beat =
    viewGame game
    
    showCrossHair (if beat then Color.Red else Color.Yellow)
             
let viewScore (game:Game) beat =
    //let beat = if waitingtime > 25 then true else beat
        
    TextRenderer.printText
        Font.Font_7x9
        (if game.Mode = SinglePlayer2 then $"{game.Player2Points,3}" else $"{game.Player1Points,3}")
        Color.Red
        7
        5
        setPixel

    if (   (game.Mode = MultiPlayer && game.Player1Points >= game.Player2Points)
        || (game.Mode = SinglePlayer2 && game.Player2Points > 0)
        || (game.Mode = SinglePlayer1 && game.Player1Points > 0)) && beat then       
        drawBorder
            6 4
            28 10
            LightYellow

    if game.Mode = MultiPlayer then        
        TextRenderer.printText
            Font.Font_7x9
            $"{game.Player2Points,3}"
            Color.Blue
            7
            21
            setPixel

        if game.Player2Points >= game.Player1Points && beat then
            drawBorder
                6 20
                28 10
                LightYellow

let highscoreInput page =
    TextRenderer.printText
        Font.Font_7x9
        $"TOP{page.Position}"
        Color.Red
        3
        5
        setPixel
        
    TextRenderer.printText
        Font.Font_7x9
        page.Name
        Color.Blue
        3
        17
        setPixel
        
    drawBorder
        (2 + (page.EditPosition-1) * 9) 16
        8 10
        Color.DarkGreen

let view (display:IDisplay) (model:Model) dispatch =
        
    if model.ViewNeedsRefresh then
        clear()       
        showOuterBorder()
        
        match model.CurrentPage with
        | Screensaver s -> s.Draw image
        | Text page -> viewText page
        | SelectPlayers mode -> viewSelectPlayers mode
        | Game game -> viewGame game
        | GameOver (game, _)
             -> viewGameOver game model.Beat
        | ShowScore game
             -> viewScore game model.Beat
        | AskForHighscore page -> highscoreInput page

        display.Update image

        ViewRefreshed |> dispatch
    
    else
        ()