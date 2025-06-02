namespace Spaceinvaders
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics.Metrics;

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
        static int ufoTimer;
        static bool ufoAktiv;

        //gegner
        static readonly int gegneranzahl = 50; // max 160
        static bool gegnerbewegung = false; // false == negativ
        static int gegnerbewegungdelay = 0;
        static int gegner;

        //sonstige werte
        static bool spiel;
        static bool schuss = false;
        static int inputX;
        static int score;
        static bool exit;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            
            do
            {
                ShowMenu();

            } while (!exit);

        }

        static void Spiel()
        {
            Thread inputThread = new(ReadInput);
            inputThread.Start();
            Console.Clear();
            InitialisiereSpiel();
            // Game Loop 
            while (spiel)
            {
                if (leben == 0)
                    spiel = false;
                if (leben > 0 && gegner == 0)
                    InitialisiereSpiel();

                Update();   // Spielerposition aktualisieren
                Render();   // Spielfeld neu zeichnen
                Thread.Sleep(50); // Spieltempo regulieren (250 ms)

            }
            inputThread.Join();
            ShowGameoverScreen();
        }

        static void Update()
        {
            Random rand = new();

            if (!ufoAktiv && rand.Next(1, 1000) < 20 && ufoTimer > 700)
            {
                ufoAktiv = true;
                if (ufoRichtung)
                {
                    grid[ufoY, 52] = 'U';
                    ufoRichtung = false;
                }
                else
                {
                    grid[ufoY, 4] = 'U';
                    ufoRichtung = true;
                }
                ufoTimer = 0;
            }
            else
            {
                ufoTimer++;
            }


            if (ufoAktiv)
            {
                if (!ufoRichtung)
                {
                    for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
                    {
                        if (grid[ufoY, symbol] == 'U')
                        {
                            if (grid[ufoY, symbol - 1] == '|')
                            {
                                grid[ufoY, symbol - 1] = ' ';
                                grid[ufoY, symbol] = ' ';
                                score += rand.Next(100, 300);
                                ufoAktiv = false;

                            }
                            else if (grid[ufoY, symbol - 1] != ' ')
                            {
                                grid[ufoY, symbol] = ' ';
                                ufoAktiv = false;
                            }
                            else
                            {
                                grid[ufoY, symbol - 1] = 'U';
                                grid[ufoY, symbol] = ' ';
                            }
                        }

                    }
                }
                else
                {
                    for (int symbol = grid.GetLength(1) - 1; symbol > 0; symbol--)
                    {
                        if (grid[ufoY, symbol] == 'U')
                        {
                            if (grid[ufoY, symbol + 1] == '|')
                            {
                                grid[ufoY, symbol + 1] = ' ';
                                grid[ufoY, symbol] = ' ';
                                score += rand.Next(100, 300);
                                ufoAktiv = false;
                            }
                            else if (grid[ufoY, symbol + 1] != ' ')
                            {
                                grid[ufoY, symbol] = ' ';
                                ufoAktiv = false;
                            }
                            else
                            {
                                grid[ufoY, symbol + 1] = 'U';
                                grid[ufoY, symbol] = ' ';
                            }
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
                            if (rand.Next(1,1000) < 7)
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
                            spiel = false;
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
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
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
|     [Enter] START      [O] Optionen      [Esc] QUIT       |
+===========================================================+"
);
            bool menu = true;

            while (menu)
            {
                while (Console.KeyAvailable)
                    Console.ReadKey(true);

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Enter:
                        leben = 3;
                        score = 0;
                        spiel = true;
                        ufoTimer = 0;
                        menu = false;
                        Spiel();
                        break;

                    case ConsoleKey.Escape:
                        exit = true;
                        menu = false;
                        continue;

                    case ConsoleKey.O:
                        menu = false;
                        ShowOptionen();
                        break;
                }
            }
        }

        static void ShowOptionen()
        {
            Console.Clear();
            bool menu = true;
            int OptionenAuswahl = 1;

            do
            {
                ZeigeOptions(OptionenAuswahl);

                while (Console.KeyAvailable)
                    Console.ReadKey(true);

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        OptionenAuswahl--;
                        if (OptionenAuswahl < 1) OptionenAuswahl = 3;
                        break;

                    case ConsoleKey.DownArrow:
                        OptionenAuswahl++;
                        if (OptionenAuswahl > 3) OptionenAuswahl = 1;
                        break;
                    case ConsoleKey.Escape:
                        menu = false;
                        break;

                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        Console.Clear();
                        switch (OptionenAuswahl)
                        {
                            
                        }
                        break;
                }

            } while (menu);
        }

        static void ZeigeOptions(int selected)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(
@"+---------------------------------------------------------------+
|                        M  E  N  Ü                             |
|                 -Bitte wähle eine Option -                    |
+---------------------------------------------------------------+
|                                                               |");

            for (int i = 0; i < 4; i++)
            {

                string option = i switch
                {
                    0 => "Scoreboard",
                    1 => "Einstellungen",
                    2 => "Zurück zum Hauptmenü",
                    _ => ""
                };
                string zeiger = (i + 1 == selected) ? ">>" : "  ";
                Console.WriteLine($"|  {zeiger} {option,-58}|");
            }

            Console.WriteLine(
@"|                                                               |
| Benutze ↑ ↓ zum Navigieren und [Enter] zum Auswählen          |
|                                                               |
+---------------------------------------------------------------+
| Entwickler: Sebi und Nils                                     |
+---------------------------------------------------------------+"
);
        }
    }
}
