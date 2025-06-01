namespace Spaceinvaders
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Reflection;

    class Program
    {
        // Spielfeld
        static int weite = 57;
        static int hoehe = 30;
        static char[,] grid = new char[hoehe, weite];
        static int schutzHoehe = 3;
        static int schutzBreite = 5;
        static int[] schutzX = { 8, 20, 32, 44 };

        //spieler
        static char player = 'A';
        static int playerX = 28;
        static int playerY = 25;

        //sonstige werte
        static bool spiel = true;
        static bool schuss = false;
        static bool gewonnen = false;
        static bool verloren = false;
        static bool gamescreen = true;
        static bool menu = false;
        static bool gegnerbewegung = false; // false == negativ
        static int gegnerbewegungdelay = 0;
        static int inputX;
        static int gegneranzahl = 30; // max 160
        static int score;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            Thread inputThread = new Thread(ReadInput);
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

                if (score == gegneranzahl)
                    gewonnen = true;

                Update();   // Spielerposition aktualisieren
                Render();   // Spielfeld neu zeichnen
                Thread.Sleep(50); // Spieltempo regulieren (250 ms)

                if (gewonnen == true)
                {
                    ShowWinningScreen();
                }

                if (verloren == true)
                {
                    ShowGameOverScreen();
                }
            }
            Console.Clear();
            inputThread.Join();
        }

        static void Update()
        {
            Random rand = new();

            for (int spalte = 0; spalte < grid.GetLength(1); spalte++)
            {
                for (int reihe = grid.GetLength(0) - 1; reihe >= 0; reihe--)
                {
                    if (grid[reihe, spalte] == '*')
                    {
                        if (reihe + 1 < grid.GetLength(0) && grid[reihe + 1, spalte] == ' ')
                        {
                            if (rand.NextDouble() < 0.02)
                            {
                                grid[reihe + 1, spalte] = 'v'; // feindlicher Schuss
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
                        score++;
                    }
                    if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == 'v')
                    {
                        grid[reihe - 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
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
                    if (grid[3, 1] != '*' && grid[4, 1] != '*' && grid[5, 1] != '*' && grid[6, 1] != '*' && grid[7, 1] != '*')
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
                                        score++;
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
                    if (grid[3, 55] != '*' && grid[4, 55] != '*' && grid[5, 55] != '*' && grid[6, 55] != '*' && grid[7, 55] != '*')
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
                                        score++;
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
            Console.WriteLine(score);
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
                            verloren = true;
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

            score = 0;
            Random rand = new Random();

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

            //gegner spawn
            for (int k = 1; k <= gegneranzahl;)
            {
                int reihe = rand.Next(3, 8);
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

        static void ShowGameOverScreen()
        {
            Console.Clear();
            Console.WriteLine("======================");
            Console.WriteLine("       GAME OVER      ");
            Console.WriteLine("======================");
            Console.WriteLine("Drücke Enter um zum hauptmenü zurückzukehren");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    verloren = false;
                    gamescreen = false;
                    menu = true;
                }
            }
            gamescreen = true;
            Console.Clear();
        }

        static void ShowWinningScreen()
        {
            Console.Clear();
            Console.WriteLine("======================");
            Console.WriteLine("    Spiel gewonnen    ");
            Console.WriteLine("======================");
            Console.WriteLine(score);
            Console.WriteLine("Drücke Enter um zum hauptmenü zurückzukehren");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    gewonnen = false;
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
            Console.WriteLine("+===========================================================+");
            Console.WriteLine("|               S P A C E   I N V A D E R S                 |");
            Console.WriteLine("|                  - Von Sebi und Nils -                    |");
            Console.WriteLine("|---------------------------------------------------------- |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|       @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @ @       |");
            Console.WriteLine("|      [*] [*] [*] [*] [*] [*] [*] [*] [*] [*] [*] [*]      |");
            Console.WriteLine("|      \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/      |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|       ###           ###           ###           ###       |");
            Console.WriteLine("|      #####         #####         #####         #####      |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|                            /\\                             |");
            Console.WriteLine("|                           /  \\                            |");
            Console.WriteLine("|                          | || |                           |");
            Console.WriteLine("|                          | || |                           |");
            Console.WriteLine("|                         /|_||_|\\                          |");
            Console.WriteLine("|                        /_|_||_|_\\                         |");
            Console.WriteLine("|                       |__________|                        |");
            Console.WriteLine("|                         |  ||  |                          |");
            Console.WriteLine("|                         |__||__|                          |");
            Console.WriteLine("|                           /__\\                            |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|     [Enter] START     [M] Scoreboard     [Esc] QUIT       |");
            Console.WriteLine("+===========================================================+");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    gamescreen = false;
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
