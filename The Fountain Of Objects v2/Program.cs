

Intro.ColoredMessage();
Console.WriteLine();
Intro_a.ColoredMessage();
Console.WriteLine();
Intro_b.ColoredMessage();
Console.WriteLine();
Intro_c.ColoredMessage();
Console.WriteLine();
Map.Generate();
Map.Show();
Player.CheckRoom();

while(Player.IsDead==false && Player.IsWin==false)
{
    Player.Move();
    Map.Show();
    Player.CheckRoom();
}

static class Map
{
    public static (int row, int column) Size { get; private set; }
    public static (string mark, RoomType room)[,] Rooms { get; set; }= new (string, RoomType)[0,0];
    public static void Generate()
    {
        ChooseSize();
        Rooms = new (string, RoomType)[Size.row, Size.column];
        for (int i=0;i<Size.row;i++)
        {
            for (int j = 0; j < Size.column; j++)
            {
                Rooms[i, j] = (" ", RoomType.blank);
            }
        }
        Map.Rooms[0, 0] = ("O", RoomType.entrance);
        Fountain.SetLocation();
        GenerateTraps(" ", RoomType.pit);
        GenerateTraps(" ", RoomType.maelstrom);
        GenerateTraps(" ", RoomType.amarok);
    }
    private static void ChooseSize()
    {
        Console.WriteLine("Choose map size:");
        Console.WriteLine("1 - Small (4x4)");
        Console.WriteLine("2 - Medium (6x6)");
        Console.WriteLine("3 - Large (8x8)");
        string? input = Console.ReadLine();
        if (int.TryParse(input, out int num) && num >= 1 && num <= 3)
        {
            Size = num switch
            {
                1 => (4, 4),
                2 => (6, 6),
                3 => (8, 8),
                _ => (0, 0)
            };
        }
        else
        {
            Console.Clear();
            ChooseSize();
        }
    }
    public static void Show()
    {
        Console.Clear ();               
        for (int i = 0; i < Size.row; i++)
        {
            Console.Write("|");
            for (int j = 0; j < Size.column; j++)
            {
                Console.Write($" {Rooms[i,j].mark} |");
            }
            Console.WriteLine();
            for(int k=0;k<=Size.column*4;k++)
            {
                Console.Write("-");
            }
            Console.WriteLine() ;
        }
    }
    /*public static string Mark(int row, int column)
    {
        return Rooms[row, column].room switch
        {
            RoomType.pit => "!",
            RoomType.maelstrom => "?",
            RoomType.amarok => "X",
            RoomType.fountain => "*",
            _ => " "
        };
    }*/

    public static void GenerateTraps(string mark,RoomType roomType)
    {
        Random random = new Random();
        int numberOfTraps = Map.Size switch
        {
            (4, 4) => 1,
            (6, 6) => 2,
            (8, 8) => 3,
            _ => 0
        };
        for (int i = 0; i < numberOfTraps; i++)
        {
            int row = random.Next(0, Map.Size.row - 1);
            int column = random.Next(0, Map.Size.column - 1);
            if (Map.Rooms[row, column].room == RoomType.blank)
            {
                Map.Rooms[row, column] = (mark, roomType);
            }
            else i--;
        }
    }
}

static class Player
{
    public static (int row, int column) Location { get; set; } = (0, 0);
    public static bool IsDead { get;private set; }=false;
    private static int Arrows { get; set; } = 5;
    public static bool IsWin { get; private set; } = false;
    public static void Move()
    {
        int row= Location.row;
        int column= Location.column;
        if (IsDead == true || IsWin == true) return;
        ScanNearbyRooms();
        Console.WriteLine("\nPress W, A, S, or D to move to another room or press SPACEBAR to shoot arrows.");
        ConsoleKey key=Console.ReadKey().Key;
        if (key == ConsoleKey.W && row-1>= 0) row--;
        else if (key == ConsoleKey.A && column-1 >= 0) column--;
        else if (key == ConsoleKey.S && row+1 < Map.Size.row) row++;
        else if (key == ConsoleKey.D && column+1 < Map.Size.column) column++;
        else if (key == ConsoleKey.Spacebar) Shoot();
        else return;

        Map.Rooms[Location.row, Location.column] = (" ", Map.Rooms[Location.row,Location.column].room);
        Location = (row, column);
        Map.Rooms[Location.row, Location.column] = ("O", Map.Rooms[Location.row, Location.column].room);
    }
    private static void Shoot()
    {
        if(Arrows<=0)
        {
            OutOfArrows.ColoredMessage();
            return;
        }
        int rowToShoot=0;
        int columnToShoot=0;
        Console.WriteLine($"\nArrows: {Arrows}/5.\nPress W, A, S, or D to choose direction to shoot or press other keys to cancel.");
        ConsoleKey key= Console.ReadKey(true).Key;
        if (key == ConsoleKey.W) rowToShoot = -1;
        else if (key == ConsoleKey.A) columnToShoot = -1;
        else if (key == ConsoleKey.S) rowToShoot = 1;
        else if (key == ConsoleKey.D) columnToShoot = 1;
        else return;
        Arrows--;
        try
        {
            RoomType roomType = Map.Rooms[Location.row + rowToShoot, Location.column + columnToShoot].room;
            if (roomType == RoomType.amarok)
            {
                Map.Rooms[Location.row + rowToShoot, Location.column + columnToShoot] = (" ", RoomType.blank);
                AmarokKilled.ColoredMessage();
                CheckRoom();
            }
        }
        catch(IndexOutOfRangeException)
        {
            Nothing.ColoredMessage();
        }
    }

    public static void CheckRoom()
    {
        RoomType roomType=Map.Rooms[Location.row, Location.column].room;
        if (roomType==RoomType.fountain)
        {
            if (!Fountain.IsActivated) Fountain.Activate();
            else FountainActivated.ColoredMessage();
        }
        else if (roomType==RoomType.pit)
        {
            IsDead = true;
            PlayerDead.ColoredMessage();
        }
        else if (roomType==RoomType.entrance)
        {
            if(!Fountain.IsActivated) Entrance.ColoredMessage();
            else
            {
                IsWin = true;
                PlayerWins.ColoredMessage();
            }
        }
        else if (roomType==RoomType.maelstrom)
        {
            bool isMaelstromActive = true;
            int playerRow=Location.row;
            int playerColumn=Location.column;
            if (playerRow - 1 >= 0) playerRow--;
            if (playerColumn + 1 < Map.Size.column) playerColumn++;
            if (playerColumn + 1 < Map.Size.column) playerColumn++;
            int maelstromRow=Location.row;
            int maelstromColumn=Location.column;
            Map.Rooms[maelstromRow, maelstromColumn] = (" ", RoomType.blank);
            Map.Rooms[Location.row, Location.column] = (" ", Map.Rooms[Location.row, Location.column].room);
            Location = (playerRow, playerColumn);
            Map.Rooms[Location.row, Location.column] = ("O", Map.Rooms[Location.row, Location.column].room);

            if (maelstromRow + 1 < Map.Size.row && Map.Rooms[maelstromRow+1,maelstromColumn].room==RoomType.blank) maelstromRow++;
            else if(maelstromRow + 1 < Map.Size.row && Map.Rooms[maelstromRow + 1, maelstromColumn].room == RoomType.pit) isMaelstromActive = false;
            
            if (maelstromColumn-1 >= 0 && Map.Rooms[maelstromRow, maelstromColumn-1].room == RoomType.blank && isMaelstromActive) maelstromColumn--;
            else if (maelstromRow + 1 < Map.Size.row && Map.Rooms[maelstromRow + 1, maelstromColumn].room == RoomType.pit) isMaelstromActive = false;

            if (maelstromColumn - 1 >= 0 && Map.Rooms[maelstromRow, maelstromColumn - 1].room == RoomType.blank && isMaelstromActive) maelstromColumn--;
            else if (maelstromRow + 1 < Map.Size.row && Map.Rooms[maelstromRow + 1, maelstromColumn].room == RoomType.pit) isMaelstromActive = false;

            if(isMaelstromActive) Map.Rooms[maelstromRow, maelstromColumn] = (" ", RoomType.maelstrom);
            Map.Show();
            Swept.ColoredMessage();
            CheckRoom();
        }
        else if(roomType==RoomType.amarok)
        {
            IsDead = true;
            Eaten.ColoredMessage();
        }
    }
    private static void ScanNearbyRooms()
    {
        for(int i = -1;i<=1;i++)
        {
            for (int j = -1;j<=1;j++)
            {
                if ((i, j) == (0, 0)) continue;
                try
                {
                    if (Map.Rooms[Location.row + i, Location.column + j].room == RoomType.pit) NearbyPit.ColoredMessage();
                    if (Map.Rooms[Location.row + i, Location.column + j].room == RoomType.maelstrom) NearbyMaelstrom.ColoredMessage();
                    if (Map.Rooms[Location.row + i, Location.column + j].room == RoomType.amarok) NearbyAmarok.ColoredMessage();
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
        }
    }
}

static class Fountain
{
    public static bool IsActivated { get; private set; } = false;
    public static void SetLocation()
    {
        Random random = new Random();
        int row=random.Next(0,Map.Size.row-1);
        int column=random.Next(0,Map.Size.column-1);
        if (Map.Rooms[row, column].room == RoomType.blank)
        {
            Map.Rooms[row, column] = (" ", RoomType.fountain);
        }
        else SetLocation();
    }
    public static void Activate()
    {
        FountainFound.ColoredMessage();
        Console.WriteLine("\nPress ENTER to activate fountain.");
        ConsoleKey key=Console.ReadKey(true).Key;
        if (key == ConsoleKey.Enter)
        {
            IsActivated = true;
            FountainActivated.ColoredMessage();
        }
        else Console.WriteLine("Fountain not activated");
    }
}

class Narrative
{
    protected static string Message { get; set; } = "";
    protected static ConsoleColor Color {  get; set; }

    protected static void ShowMessage()
    {
        Console.ForegroundColor = Color;
        Console.WriteLine(Message);
        Console.ForegroundColor = ConsoleColor.White;
        Console.ReadKey(true);
    }
}

class Intro : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"You enter the Cavern of Objects, a maze of rooms filled with dangerous pits in search of the Fountain of Objects.
Light is visible only in the entrance, and no other light is seen anywhere in the caverns.
You must navigate the Caverns with your other senses.
Find the Fountain of Objects, activate it, and return to the entrance.";
        Color=ConsoleColor.Cyan;
        ShowMessage();
    }
}

class Intro_a : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"Look out for pits. You will feel a breeze if a pit is in an adjacent room. 
If you enter a room with a pit, you will die.";
        Color = ConsoleColor.Cyan;
        ShowMessage();
    }
}

class Intro_b : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"Maelstroms are violent forces of sentient wind. Entering a room with one could transport you to any other location in the caverns. 
You will be able to hear their growling and groaning in nearby rooms.";
        Color = ConsoleColor.Cyan;
        ShowMessage();
    }
}

class Intro_c : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"Amaroks roam the caverns. Encountering one is certain death, but you can smell their rotten stench in nearby rooms.";
        Color = ConsoleColor.Cyan;
        ShowMessage();
    }
}

class FountainFound:Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You hear water dripping in this room. 
The Fountain of Objects is here!";
        Color = ConsoleColor.Blue;
        ShowMessage();
    }
}

class FountainActivated : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You hear the rushing waters from the Fountain of Objects. It has been reactivated!
Go back to entrance to exit the cave.";
        Color = ConsoleColor.Blue;
        ShowMessage();
    }
}

class PlayerDead : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You fell on a deep pit!
You died.";
        Color = ConsoleColor.DarkRed;
        ShowMessage();
    }
}

class PlayerWins : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You have activated the Fountain of Objects and safely exited the cave!
You win!";
        Color = ConsoleColor.Green;
        ShowMessage();
    }
}

class Entrance : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You see light in this room coming from outside the cavern. 
This is the entrance.";
        Color = ConsoleColor.Yellow;
        ShowMessage();
    }
}

class NearbyPit : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You feel a draft. 
There is a pit in a nearby room.";
        Color = ConsoleColor.Red;
        ShowMessage();
    }
}

class Swept : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You are swept away by a maelstrom! 
You are thrown to another room.";
        Color = ConsoleColor.DarkMagenta;
        ShowMessage();
    }
}

class NearbyMaelstrom : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You hear the growling and groaning of a maelstrom nearby.";
        Color = ConsoleColor.Magenta;
        ShowMessage();
    }
}

class NearbyAmarok : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You can smell the rotten stench of an amarok in a nearby room.";
        Color = ConsoleColor.Red;
        ShowMessage();
    }
}

class Eaten : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You are eaten by an amarok.
You died.";
        Color = ConsoleColor.DarkRed;
        ShowMessage();
    }
}

class Nothing : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
Nothing happened.";
        Color = ConsoleColor.Cyan;
        ShowMessage();
    }
}

class AmarokKilled : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You have killed an amarok!";
        Color = ConsoleColor.Cyan;
        ShowMessage();
    }
}

class OutOfArrows : Narrative
{
    public static void ColoredMessage()
    {
        Message = @"
You are out of arrows.";
        Color = ConsoleColor.Cyan;
        ShowMessage();
    }
}

enum RoomType { blank, entrance, fountain, pit, maelstrom, amarok }