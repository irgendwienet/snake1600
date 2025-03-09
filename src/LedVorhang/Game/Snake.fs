namespace Game

type Position = {
    X: int
    Y: int
}

type Direction = 
    | Up
    | Down
    | Left
    | Right

type Snake = {
    Head: Position
    Tail: Position list
    
    Direction: Direction
    Growth: int
}

module Snake =
    let Move (snake:Snake) =
        let oldHead = snake.Head
        
        let newHead =
            match snake.Direction with
            | Up -> { snake.Head with X = snake.Head.X - 1 }
            | Down -> { snake.Head with X = snake.Head.X + 1 }
            | Left -> { snake.Head with Y = snake.Head.Y + 1 }
            | Right -> { snake.Head with Y = snake.Head.Y - 1 }
        
        let newTail, growth = 
            if snake.Growth > 0 then
                oldHead :: snake.Tail, snake.Growth - 1
            else
                oldHead :: List.init (List.length snake.Tail - 1) (fun i -> snake.Tail |> List.item i), 0 
        
        { snake with Head = newHead; Tail = newTail; Growth = growth }
        
    let ChangeDirection (direction:Direction) (snake:Snake) =
        if (snake.Direction = Up && direction = Down) ||
           (snake.Direction = Down && direction = Up) ||
           (snake.Direction = Left && direction = Right) ||
           (snake.Direction = Right && direction = Left) then
            snake
        else
            { snake with Direction = direction }
        
    let Init x y dir=
        let pos = { X = x; Y = y }
        { Head = pos; Tail = []; Direction = dir; Growth = 5 }
        
    let IsCollisionWithBorder (snake:Snake) =
        let head = snake.Head
        head.X < 1 || head.X >= 39 || head.Y < 1 || head.Y >= 39
        
    let IsCollisionWithTail (snake:Snake) =
        let head = snake.Head
        List.exists (fun pos -> pos.X = head.X && pos.Y = head.Y) snake.Tail
        
    let IsCollisionWithEachOther (snake1:Snake) (snake2:Snake) =
        let head1 = snake1.Head
        let head2 = snake2.Head
        head1.X = head2.X && head1.Y = head2.Y || List.exists (fun pos -> pos.X = head2.X && pos.Y = head2.Y) snake1.Tail || List.exists (fun pos -> pos.X = head1.X && pos.Y = head1.Y) snake2.Tail
        
    let IsPartOfSnake (snake:Snake) (pos:Position) =
        pos.X = snake.Head.X && pos.Y = snake.Head.Y || List.exists (fun p -> p.X = pos.X && p.Y = pos.Y) snake.Tail