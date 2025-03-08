module Game.View

open System.Drawing
open Game.Model
open HardwareLayer

let image = LedImage(40,40)

let setPixel (x:int) (y:int) (color:Color) = image.SetPixel(x, y, color)    
let clear () = image.Clear()

let showSnake (snake:Snake) =
    setPixel snake.Head.X snake.Head.Y Color.Red
    snake.Tail |> List.iter (fun pos -> setPixel pos.X pos.Y Color.Orange)

let showFood (p:Position) =
    setPixel p.X p.Y Color.Green

let showOuterBorder () =
    for x in 0..39 do
        setPixel x 0 Color.White
        setPixel x 39 Color.White
    
    for y in 0..39 do
        setPixel 0 y Color.White
        setPixel 39 y Color.White

let view (display:IDisplay) (model:Model) dispatch =
    
    clear()
       
    showOuterBorder()
    
    if not model.IsPreGame then
        model.Player1 |> showSnake
        
        model.Food |> showFood
        
    else
        for i in 0..10 do
                setPixel (15+i) (15+i) Color.Blue
                setPixel (15+i) (25-i) Color.Blue
                
    display.Update image
