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

        static void Main()
        {
            Console.CursorVisible = false;
            Thread inputThread = new Thread(ReadInput);
            inputThread.Start();
            ShowStartScreen();
            InitialisiereSpiel();
            Render();
            Thread.Sleep(1000);

            // Game Loop 
            while (spiel)
            {
                Update();   // Spielerposition aktualisieren
                Render();   // Spielfeld neu zeichnen
                Thread.Sleep(150); // Spieltempo regulieren (250 ms)
            }

            ShowGameOverScreen();
            inputThread.Join();
        }
        static void ShowStartScreen()
        {
            Console.Clear();
            Console.WriteLine("======================"); 
            Console.WriteLine("    KONSOLEN SPIEL    ");
            Console.WriteLine("======================");
            Console.WriteLine("Pfeiltasten: Links/Rechts");
            Console.WriteLine("Taste Enter zum Starten...");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.Clear();
        }
        static void ShowGameOverScreen()
        {
            Console.Clear();
            Console.WriteLine("======================");
            Console.WriteLine("       GAME OVER      ");
            Console.WriteLine("======================");
            Console.WriteLine("Drücke eine Taste zum Beenden...");
            Console.ReadKey(true);
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
                            spiel = false;
                            break;
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
    }
}