module Game.View

open System
open System.Drawing
open Game.Model
open HardwareLayer
open HardwareLayer.Fonts

let LightYellow=Color.FromArgb(90,90,0)    
let LowWhite = Color.FromArgb(180, 180, 180)

let tick() = DateTime.Now.Microsecond % 100 > 50

let image = LedImage(40,40, true)

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

let printChar font ch color x y =
    let data, width = Fonts.Get(font, ch).ToTuple()
    
    let mutable j = -1
    for row in data do
        j <- j + 1
        for i in  [0 .. width-1] do
            if row &&& (1uy <<< (7-i)) > 0uy then
                setPixel (y+j) (x+i)  color
                
let printText font (text:string) color x y =
    let _, width = Fonts.GetSize(font).ToTuple()
    
    let mutable i = -1  
    for ch in text do
        i <- i + 1
        printChar font ch color (x + i * width) y
    
    ()

let viewSelectPlayers mode =
    printText
        Font.Font_7x9
        "1"
        Color.Blue
        8
        15

    printText
        Font.Font_7x9
        "2"
        Color.Blue
        25
        15
        
    match mode with
    | SinglePlayer -> 
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
    printText
        Font.Font_7x9
        $"{game.Player1Points,3}"
        Color.Red
        7
        5

    if game.Player1Points >= game.Player2Points && beat then       
        drawBorder
            6 4
            28 10
            LightYellow

    if game.Mode = MultiPlayer then        
        printText
            Font.Font_7x9
            $"{game.Player2Points,3}"
            Color.Blue
            7
            21

        if game.Player2Points >= game.Player1Points && beat then
            drawBorder
                6 20
                28 10
                LightYellow

let view (display:IDisplay) (model:Model) dispatch =
        
    if model.ViewNeedsRefresh then
        clear()       
        showOuterBorder()
        
        match model.CurrentPage with
        | SelectPlayers mode -> viewSelectPlayers mode
        | Game game -> viewGame game
        | GameOver (game, waitingTime) when waitingTime > 0
             -> viewGameOver game model.Beat
        | GameOver (game, _)
             -> viewScore game model.Beat
        
        display.Update image

        ViewRefreshed |> dispatch
    
    else
        ()