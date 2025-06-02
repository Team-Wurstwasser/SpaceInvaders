namespace Spaceinvaders
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Reflection;

    class Program
    {
        // Spielfeld
        static readonly int weite = 57;
        static readonly int hoehe = 30;
        static char[,] grid = new char[hoehe, weite];
        static readonly int schutzHoehe = 3;
        static readonly int schutzBreite = 5;
        static readonly int[] schutzX = { 8, 20, 32, 44 };

        //spieler
        static readonly char player = 'A';
        static int playerX;
        static int playerY;
        static int leben;

        //ufo
        static int ufoY = 2;
        static bool ufoRichtung;
        static int ufoTimer = 0;

        //gegner
        static readonly int gegneranzahl = 50; // max 160
        static bool gegnerbewegung = false; // false == negativ
        static int gegnerbewegungdelay = 0;
        static int gegner;

        //screens
        static bool gameover;
        static bool gamescreen = true;
        static bool menu = false;

        //sonstige werte
        static bool spiel = true;
        static bool schuss = false;
        static int inputX;
        static int score;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            Thread inputThread = new(ReadInput);
            inputThread.Start();
            ShowMenu();
            InitialisiereSpiel();
            Render();
            Thread.Sleep(10);

            // Game Loop 
            while (spiel)
            {

                if (menu == true)
                {
                    ShowMenu();
                }

                if (leben == 0)
                    gameover = true;
                if (leben > 0 && gegner == 0)
                    InitialisiereSpiel();

                Update();   // Spielerposition aktualisieren
                Render();   // Spielfeld neu zeichnen
                Thread.Sleep(50); // Spieltempo regulieren (250 ms)

                if (gameover == true)
                {
                    ShowGameoverScreen();
                }
            }
            Console.Clear();
            inputThread.Join();
        }

        static void Update()
        {
            Random rand = new();

            for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
            {
                if (grid[ufoY, symbol] == 'U')
                {
                    if (!ufoRichtung)
                    {
                        if (grid[ufoY, symbol - 1] == '|')
                        {
                            grid[ufoY, symbol - 1] = ' ';
                            grid[ufoY, symbol] = ' ';
                            score += rand.Next(100, 300);
                        }
                        else if (grid[ufoY, symbol - 1] != ' ')
                        {
                            grid[ufoY, symbol] = ' ';
                        }
                        else
                        {
                            grid[ufoY, symbol - 1] = 'U';
                            grid[ufoY, symbol] = ' ';
                        }
                    }
                    
                }
            }

            for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
            {
                for (int reihe = grid.GetLength(0) - 1; reihe >= 0; reihe--)
                {
                    if (grid[reihe, symbol] == '*')
                    {
                        if (reihe + 1 < grid.GetLength(0) && grid[reihe + 1, symbol] == ' ')
                        {
                            if (rand.NextDouble() < 0.001)
                            {
                                grid[reihe + 1, symbol] = 'v'; // feindlicher Schuss
                            }
                        }
                        break;
                    }
                }
            }

            for (int reihe = grid.GetLength(0) - 1; reihe >= 1; reihe--)
            {
                for (int symbol = grid.GetLength(1) - 1; symbol >= 1; symbol--)
                {
                    if (grid[reihe, symbol] == 'v' && grid[reihe + 1, symbol] == ' ')
                    {
                        grid[reihe + 1, symbol] = 'v';
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == 'v' && grid[reihe + 1, symbol] == '#')
                    {
                        grid[reihe + 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == 'v' && grid[reihe + 1, symbol] == 'A')
                    {
                        leben--;
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == 'v' && grid[reihe + 1, symbol] != ' ')
                    {
                        grid[reihe, symbol] = ' ';
                    }
                }
            }

            //neue spielerposition erstelle 
            
            //spielerbewegung
            int newPlayerX = playerX + inputX;

            if (grid[playerY, newPlayerX] == ' ')
            {
                // neue position eintragen
                grid[playerY, newPlayerX] = player;
                // alte position löschen
                grid[playerY, playerX] = ' ';

                playerX = newPlayerX;
            }
            inputX = 0;


            //schießen
            if (schuss == true && (grid[playerY -1, playerX] == ' ') && (grid[playerY - 2, playerX] == ' ') && (grid[playerY - 2, playerX -1] != '|') && (grid[playerY - 2, playerX +1] != '|'))
            {
                grid[playerY - 1, playerX] = '|';
            }
            schuss = false;

            for (int reihe = 1; reihe < grid.GetLength(0) - 1; reihe++)
            {
                for (int symbol = 1; symbol < grid.GetLength(1) - 1; symbol++)
                {
                    if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == '*')
                    {
                        grid[reihe - 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
                        gegner--;
                        score += 10;
                    }
                    if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == 'v')
                    {
                        grid[reihe - 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == '#')
                    {
                        grid[reihe - 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == 'U')
                    {
                        grid[reihe - 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
                        score += rand.Next(100, 300);
                    }
                    else if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == ' ')
                    {
                        grid[reihe - 1, symbol] = '|';
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] != ' ')
                    {
                        grid[reihe, symbol] = ' ';
                    }
                }
            }

            // Gegnerbewegung
            gegnerbewegungdelay++;
            if (gegnerbewegungdelay == 3)
            {
                if (gegnerbewegung == false)
                {
                    // Bewegung nach links
                    if (grid[4, 1] != '*' && grid[5, 1] != '*' && grid[6, 1] != '*' && grid[7, 1] != '*' && grid[8, 1] != '*' && grid[9, 1] != '*' && grid[10, 1] != '*' && grid[11, 1] != '*')
                    {
                        for (int reihe = 0; reihe < grid.GetLength(0); reihe++)
                        {
                            for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
                            {
                                if (grid[reihe, symbol] == '*')
                                {
                                    if (grid[reihe, symbol - 1] == '|')
                                    {
                                        grid[reihe, symbol - 1] = ' ';
                                        grid[reihe, symbol] = ' ';
                                        gegner--;
                                        score += 10;
                                    }
                                    else
                                    {
                                        grid[reihe, symbol - 1] = '*';
                                        grid[reihe, symbol] = ' ';
                                    }
                                }
                            }
                        }
                    }
                    else gegnerbewegung = true;
                }
                else if (gegnerbewegung == true)
                {
                    // Bewegung nach rechts
                    if (grid[4, 55] != '*' && grid[5, 55] != '*' && grid[6, 55] != '*' && grid[7, 55] != '*' && grid[8, 55] != '*' && grid[9, 55] != '*' && grid[10, 55] != '*' && grid[11, 55] != '*')
                    {
                        for (int reihe = 0; reihe < grid.GetLength(0); reihe++)
                        {
                            for (int symbol = grid.GetLength(1) - 1; symbol > 0; symbol--)
                            {
                                if (grid[reihe, symbol] == '*')
                                {
                                    if (grid[reihe, symbol + 1] == '|')
                                    {
                                        grid[reihe, symbol + 1] = ' ';
                                        grid[reihe, symbol] = ' ';
                                        gegner--;
                                        score += 10;
                                    }
                                    else
                                    {
                                        grid[reihe, symbol + 1] = '*';
                                        grid[reihe, symbol] = ' ';
                                    }
                                }
                            }
                        }
                    }
                    else gegnerbewegung = false;
                }
                gegnerbewegungdelay = 0;
            }
        }

        static void Render()
        {
            Console.SetCursorPosition(0, 0);
            for (int reihe = 0; reihe < grid.GetLength(0); reihe++)
            {
                for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
                {
                    Console.Write(grid[reihe, symbol]);
                }
                Console.WriteLine();
            }
            // Lebensbalken anzeigen
            Console.WriteLine();
            Console.Write("Leben: ");
            Console.ForegroundColor = ConsoleColor.Red;
            string Lebensbalken = new string('♥', leben).PadRight(3, ' ');
            Console.WriteLine(Lebensbalken);
            Console.ResetColor();

            // Gegneranzahl und Punktestand
            Console.WriteLine($"Gegner: {gegner}");
            Console.WriteLine($"Score : {score}");
        }
        static void ReadInput()
        {
            while (spiel)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.RightArrow:
                            inputX = 1;
                            break;
                        case ConsoleKey.LeftArrow:
                            inputX = -1;
                            break;
                        case ConsoleKey.Escape:
                            gameover = true;
                            break;
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.Spacebar:
                            schuss = true;
                            break;
                    }
                }
            }
        }

        static void InitialisiereSpiel()
        {
            //Spieler zurücksetzen
            playerX = 28;
            playerY = 25;

            gegner = gegneranzahl;
            Random rand = new();

            //spielrand spawn
            Console.SetCursorPosition(0, 0);
            for (int reihe = 0; reihe < grid.GetLength(0); reihe++)
            {
                for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
                {

                    // Rand des Spielfelds markieren
                    if (reihe == 0 || reihe == grid.GetLength(0) - 1 || symbol == 0 || symbol == grid.GetLength(1) - 1)
                    {
                        grid[reihe, symbol] = '\u2588';
                    }
                    else
                    {
                        grid[reihe, symbol] = ' ';
                    }
                }
            }

            grid[ufoY, 52] = 'U';

            //gegner spawn
            for (int k = 1; k <= gegneranzahl;)
            {
                int reihe = rand.Next(4, 11);
                int symbol = rand.Next(4, 52);

                if (grid[reihe, symbol] != '*')
                {
                    grid[reihe, symbol] = '*';
                    k++; 
                }
            }

            // Spieler platzieren
            grid[playerY, playerX] = player;

            //Schutz plazieren
            int schutzY = playerY - 3;

            foreach (int startX in schutzX)
            {
                for (int reihe = 0; reihe < schutzHoehe; reihe++)
                {
                    for (int symbol = 0; symbol < schutzBreite; symbol++)
                    {
                        int x = startX + symbol;
                        int y = schutzY - reihe;
                        grid[y, x] = '#';
                    }
                }
            }

        }

        static void ShowGameoverScreen()
        {
            Console.Clear();
            Console.WriteLine("======================");
            Console.WriteLine("      Game  Over      ");
            Console.WriteLine("======================");
            Console.WriteLine(score);
            Console.WriteLine("Drücke Enter um zum Hauptmenü zurückzukehren");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    gamescreen = false;
                    menu = true;
                }
            }
            gamescreen = true;
            Console.Clear();
        }
        static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(
@"+===========================================================+ 
|               S P A C E   I N V A D E R S                 |
|                  - Von Sebi und Nils -                    |
|-----------------------------------------------------------|
|                                                           |
|       @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @       |
|      [*] [*] [*] [*] [*] [*] [*] [*] [*] [*] [*] [*]      |
|      \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/      |
|                                                           |
|       ###           ###           ###           ###       |
|      #####         #####         #####         #####      |
|                                                           |
|                                                           |
|                            /\                             |
|                           /  \                            |
|                          | || |                           |
|                          | || |                           |
|                         /|_||_|\                          |
|                        /_|_||_|_\                         |
|                       |__________|                        |
|                         |  ||  |                          |
|                         |__||__|                          |
|                           /__\                            |
|                                                           |
|                                                           |
|     [Enter] START     [M] Scoreboard     [Esc] QUIT       |
+===========================================================+");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    gamescreen = false;
                    gameover = false;
                    leben = 3;
                    score = 0;
                    InitialisiereSpiel();
                }

                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    gamescreen = false;
                    spiel = false;
                }
            }
            gamescreen = true;
            menu = false;
            Console.Clear();
        }
    }
}
