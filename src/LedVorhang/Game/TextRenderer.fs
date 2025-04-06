module Game.TextRenderer

open System
open HardwareLayer.Fonts

let printChar font ch color x y setPixel =
    let data, width = Fonts.Get(font, ch).ToTuple()
    
    let mutable j = -1
    for row in data do
        j <- j + 1
        for i in  [0 .. width-1] do
            if row &&& (1uy <<< (7-i)) > 0uy then
                setPixel (y+j) (x+i)  color
                
let printText font (text:string) color x y  setPixel =
    let _, width = Fonts.GetSize(font).ToTuple()
    
    let mutable i = -1  
    for ch in text do
        i <- i + 1
        printChar font ch color (x + i * width) y setPixel
    
    ()
    
let printTextOffset font (text:string) color textX textY startIndex screenWidth setPixel =
    let _, width = Fonts.GetSize(font).ToTuple()

    let endIndex = startIndex + screenWidth
        
    let clippingSetPixel y x c =
        if x >= startIndex && x < endIndex then
            setPixel y (x-startIndex+textX) c
        else
            ()
        
        
    let mutable i = -1  
    for ch in text do
        i <- i + 1
        printChar font ch color (textX + i * width) textY clippingSetPixel
    
    ()
