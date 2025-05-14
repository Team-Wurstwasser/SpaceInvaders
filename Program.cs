namespace Spaceinvaders
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Reflection;

    class Program
    {
        // Spielstatus
        static bool spiel = true;

        // Spielfeldgröße (Breite x Höhe)
        static int weite = 40;
        static int hoehe = 20;

        // Spielfeld Array
        static char[,] grid = new char[hoehe, weite];

        //inputspeicher
        static int inputX;

        //spieler
        static char player = 'A';
        static int playerX = 20;
        static int playerY = 16;

        //sonstige werte
        static bool schuss = false;
        static int score = 0;
        static bool gewonnen = false;
        static bool verloren = false;
        static bool gamescreen = true;
        static bool playagain = false;
        static bool menu = false;

        static void Main()
        {
            Console.CursorVisible = false;
            Thread inputThread = new Thread(ReadInput);
            inputThread.Start();
            ShowStartMenu();
            InitialisiereSpiel();
            Render();
            Thread.Sleep(10);

            // Game Loop 
            while (spiel)
            {
                if (menu == true)
                {
                    ShowStartMenu();
                }

                Update();   // Spielerposition aktualisieren
                Render();   // Spielfeld neu zeichnen
                Thread.Sleep(250); // Spieltempo regulieren (250 ms)

                if (score == 160) { gewonnen = true; }
                if (gewonnen == true)
                {
                    score = 0;
                    ShowWinningScreen();
                    if (playagain == true)
                    {
                        InitialisiereSpiel();
                        playagain = false;
                    }
                }

                if (verloren == true)
                {
                    score = 0;
                    ShowGameOverScreen();
                    if (playagain == true)
                    {
                        InitialisiereSpiel();
                        playagain = false;
                    }
                }
            }
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
            if (schuss == true && (grid[playerY -1, playerX] == ' ') && (grid[playerY - 2, playerX] == ' '))
            {
                grid[playerY - 1, playerX] = '|';
            }
            schuss = false;

            for (int reihe = 0; reihe < grid.GetLength(0); reihe++)
            {
                for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
                {
                    if (grid[reihe, symbol]== '|' && grid[reihe - 1, symbol] == ' ')
                    {
                        grid[reihe - 1, symbol] = '|';
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == '*')
                    {
                        grid[reihe - 1, symbol] = ' ';
                        grid[reihe, symbol] = ' ';
                        score++;
                    }
                    else if (grid[reihe, symbol] == '|' && grid[reihe - 1, symbol] == '\u2588')
                    {
                        grid[reihe, symbol] = ' ';
                    }
                }
            }
        }
        static void Render()
        {
            // Cursor zurücksetzen "übermahlt" letztes Frame
            Console.SetCursorPosition(0, 0);
            for (int reihe = 0; reihe < grid.GetLength(0); reihe++)
            {
                for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
                {
                    Console.Write(grid[reihe, symbol]);
                }
                Console.WriteLine(); // Neue Zeile nach jeder Reihe
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
                            schuss = true;
                            break;
                    }
                }
            }
        }

        static void InitialisiereSpiel()
        {
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

                    if (reihe > 2 && reihe < 8 && symbol > 3 && symbol < 36)
                    {
                        grid[reihe, symbol] = '*';
                    }
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
            Console.WriteLine("Drücke Enter um neuzustarten oder esc um zum hauptmenü zurückzukehren");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    verloren = false;
                    gamescreen = false;
                    playagain = true;
                }

                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
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
            Console.WriteLine("Drücke Enter um neuzustarten oder esc um zum hauptmenü zurückzukehren");
            while (gamescreen == true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    gewonnen = false;
                    gamescreen = false;
                    playagain = true;
                }

                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    gewonnen = false;
                    gamescreen = false;
                    menu = true;
                }
            }
            gamescreen = true;
            Console.Clear();
        }
        static void ShowStartMenu()
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
                }

                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    gamescreen = false;
                }
            }
            gamescreen = true;
            menu = false;
            Console.Clear();
        }
    }
}