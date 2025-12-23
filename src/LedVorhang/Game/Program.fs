module Program

open System.Device.Spi
open System.Runtime.InteropServices
open Elmish
open Game
open Game.Config
open Game.Model
open HardwareLayer

// Initialize database (create file/table if needed) before starting the app
Database.init()

let createLedDisplay () =

    let createWs2812B chipSelectLine = 
        let settings = SpiConnectionSettings(0, chipSelectLine)
        settings.ClockFrequency <- 3_400_000
        settings.Mode <- SpiMode.Mode0
        settings.DataBitLength <- 8
        
        Ws2812Vorhang(SpiDevice.Create(settings), 20, 20)

    let device0 = createWs2812B(0)
    let device1 = createWs2812B(1)
    let device2 = createWs2812B(3)
    let device3 = createWs2812B(4)

    let display = QuadDeviceDisplay(device1, device3, device2, device0)
    display.IsMirrored <- (getBool "display.mirrored" false)
    display

let createConsoleDisplay () =
    AnsiConsoleDisplay(true)       

if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
   let display = createConsoleDisplay()
   let sub = Subscription.WindowsPc ()
   
   Program.mkProgram init State.update (View.view display)
   |> Program.withSubscription (fun _ -> sub) 
   |> Program.run
else
    let display = createLedDisplay()
    let sub = Subscription.RaspberryPi ()
        
    Program.mkProgram init State.update (View.view display)
    |> Program.withSubscription (fun _ -> sub)
    |> Program.run

printfn "It's running"

while true do
    System.Threading.Thread.Sleep(100000)
