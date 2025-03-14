module Game.View

open System
open System.Drawing
open Game.Model
open HardwareLayer
open HardwareLayer.Fonts

let image = LedImage(40,40)

let setPixel (x:int) (y:int) (color:Color) = image.SetPixel(x, y, color)    
let clear () = image.Clear()

let showSnake headColor tailColor (snake:Snake)=
    setPixel snake.Head.X snake.Head.Y headColor
    snake.Tail |> List.iter (fun pos -> setPixel pos.X pos.Y tailColor)

let showFood (p:Position) =
    setPixel p.X p.Y Color.Green

let showOuterBorder () =
    for x in 0..39 do
        setPixel x 0 Color.White
        setPixel x 39 Color.White
    
    for y in 0..39 do
        setPixel 0 y Color.White
        setPixel 39 y Color.White

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

let viewSelectPlayers () =
    showCrossHair Color.Green
    

let viewGame (game:Game) =
    game.Player1 |> showSnake Color.Red Color.Yellow
    game.Player2 |> showSnake Color.Blue Color.DarkOliveGreen
    game.Food |> showFood   

let viewGameOver (game:Game) =
    viewGame game
    showCrossHair Color.Red

let view (display:IDisplay) (model:Model) dispatch =

        
    clear()       
    showOuterBorder()
    
    match model.CurrentPage with
    | SelectPlayers -> viewSelectPlayers ()
    | Game game -> viewGame game
    | GameOver game -> viewGameOver game
    
    display.Update image
