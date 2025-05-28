namespace Spaceinvaders
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Reflection;

    class Program
    {
        // Spielfeld
        static int weite = 40;
        static int hoehe = 20;
        static char[,] grid = new char[hoehe, weite];

        //spieler
        static char player = 'A';
        static int playerX = 20;
        static int playerY = 16;

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
            //neue spielerposition erstellen
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

            gegnerbewegungdelay++;
            if (gegnerbewegungdelay == 3)
            {
                // Gegnerbewegung
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
                    if (grid[3, 38] != '*' && grid[4, 38] != '*' && grid[5, 38] != '*' && grid[6, 38] != '*' && grid[7, 38] != '*')
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
            score = 0;
            Random rand = new Random();

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

            for (int k = 1; k <= gegneranzahl;)
            {
                int reihee = rand.Next(3, 8);
                int spaltee = rand.Next(4, 36);

                if (grid[reihee, spaltee] != '*')
                {
                    grid[reihee, spaltee] = '*';
                    k++; 
                }
            }

            // Spieler platzieren
            grid[playerY, playerX] = player;

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