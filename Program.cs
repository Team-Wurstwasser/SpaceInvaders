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
        static int weite = 57;
        static int hoehe = 30;
        static char[,] grid = new char[hoehe, weite];
        static int schutzHoehe = 3;
        static int schutzBreite = 5;
        static int[] schutzX = { 8, 20, 32, 44 };

        //spieler
        static char player = '█';
        static int playerX;
        static int playerY = 25;
        static int leben;
        static int livedif = 5;

        //ufo
        static int ufoY = 2;
        static bool ufoBewegung = false; // false == negativ
        static int ufoTimer;
        static bool ufoAktiv = false;

        //gegner
        static int gegneranzahl = 50; // max 160
        static bool gegnerbewegung = false; // false == negativ
        static int gegnerbewegungdelay = 0;
        static int gegner;
        static int schussanzahl = 7;

        //sonstige werte
        static bool spiel;
        static bool schuss = false;
        static int inputX;
        static int score;
        static bool exit;
        static bool schutz = true;
        static int optionenfarbe = 1;

        static List<string> Scoreboardlist = new();

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

                Update();
                Render();
                Thread.Sleep(50);

            }
            inputThread.Join();
            ShowGameoverScreen();
        }

        static void Update()
        {
            //ufo
            Random rand = new();

            if (!ufoAktiv && rand.Next(1, 1000) < 20 && ufoTimer > 70)
            {
                ufoAktiv = true;
                if (ufoBewegung)
                {
                    grid[ufoY, 52] = 'U';
                    ufoBewegung = false;
                }
                else
                {
                    grid[ufoY, 4] = 'U';
                    ufoBewegung = true;
                }
            }
            else
            {
                ufoTimer++;
            }

            if (ufoAktiv)
            {
                if (!ufoBewegung)
                {
                    if (grid[ufoY, 1] != 'U')
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
                                    ufoTimer = 0;
                                }
                                else
                                {
                                    grid[ufoY, symbol - 1] = 'U';
                                    grid[ufoY, symbol] = ' ';
                                }
                            }
                        }
                    }
                    else ufoBewegung = true;
                }

                else if (ufoBewegung)
                {
                    if (grid[ufoY, 55] != 'U')
                    {
                        for (int symbol = grid.GetLength(1) -1; symbol > 0; symbol--)
                        {
                            if (grid[ufoY, symbol] == 'U')
                            {
                                if (grid[ufoY, symbol + 1] == '|')
                                {
                                    grid[ufoY, symbol + 1] = ' ';
                                    grid[ufoY, symbol] = ' ';
                                    score += rand.Next(100, 300);
                                    ufoAktiv = false;
                                    ufoTimer = 0;
                                }
                                else
                                {
                                    grid[ufoY, symbol + 1] = 'U';
                                    grid[ufoY, symbol] = ' ';
                                }
                            }
                        }
                    }
                    else ufoBewegung = false;
                }
            }

            // gegner schuss
            for (int symbol = 0; symbol < grid.GetLength(1); symbol++)
            {
                for (int reihe = grid.GetLength(0) - 1; reihe >= 0; reihe--)
                {
                    if (grid[reihe, symbol] == '*')
                    {
                        if (reihe + 1 < grid.GetLength(0) && grid[reihe + 1, symbol] == ' ')
                        {
                            if (rand.Next(1, 1000) < schussanzahl)
                            {
                                grid[reihe + 1, symbol] = 'v';
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
                    else if (grid[reihe, symbol] == 'v' && grid[reihe + 1, symbol] == player && reihe != 28)
                    {
                        if (leben > 0)
                            leben--;
                        grid[reihe, symbol] = ' ';
                    }
                    else if (grid[reihe, symbol] == 'v' && grid[reihe + 1, symbol] != ' ')
                    {
                        grid[reihe, symbol] = ' ';
                    }
                }
            }

            //spielerbewegung
            int newPlayerX = playerX + inputX;
            if (newPlayerX - 2 >= 0 && newPlayerX + 2 < grid.GetLength(1))
            {
                // Spieler löschen
                grid[playerY, playerX] = ' ';
                grid[playerY + 1, playerX - 1] = ' ';
                grid[playerY + 1, playerX] = ' ';
                grid[playerY + 1, playerX + 1] = ' ';

                playerX = newPlayerX;

                // Spieler zeichnen
                grid[playerY, playerX] = player;
                grid[playerY + 1, playerX - 1] = player;
                grid[playerY + 1, playerX] = player;
                grid[playerY + 1, playerX + 1] = player;
            }
            inputX = 0;

            //spieler schuss
            if (schuss == true && (grid[playerY - 1, playerX] == ' ') && (grid[playerY - 2, playerX] == ' ') && (grid[playerY - 2, playerX - 1] != '|') && (grid[playerY - 2, playerX + 1] != '|'))
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
                        ufoAktiv = false;
                        ufoTimer = 0;
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
                if (!gegnerbewegung)
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
                else if (gegnerbewegung)
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
            string Lebensbalken = new string('♥', leben).PadRight(livedif, ' ');
            Console.WriteLine(Lebensbalken);
            Console.ResetColor();

            // Gegneranzahl und Punktestand
            Console.WriteLine($"Gegner: {gegner,-2}");
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
            int schutzY = 22;

            foreach (int startX in schutzX)
            {
                for (int reihe = 0; reihe < schutzHoehe; reihe++)
                {
                    for (int symbol = 0; symbol < schutzBreite; symbol++)
                    {
                        if (schutz == true)
                        {
                            int x = startX + symbol;
                            int y = schutzY - reihe;
                            grid[y, x] = '#';
                        }
                    }
                }
            }

        }

        static void ShowGameoverScreen()
        {
            Console.Clear();

            Console.WriteLine(
@"+----------------------------------------------+
|                                              |
|              G A M E   O V E R               |
|             - Schiff zerstört -              |
|                                              |
|                                              |
|                   *   *                      |
|                *   /|\   *                   |
|               *   /_X_\   *                  |
|                * /_/ \_\ *                   |
|                 /_/___\_\                    |
|                 [__*_*__]                    |
|                   /   \                      |
|                  /_____\                     |
|                ///     \\\                   |
|                 * BOOM! *                    |
|                                              |
|                                              |
+----------------------------------------------+");
            Console.WriteLine($"Punkteanzahl: {score}");
            Console.Write("Gib deinen Namen ein (max. 15 Zeichen, min. 1 Zeichen): ");
            string Name = Console.ReadLine();

            while (Name.Length > 16 || Name.Length < 1)
            {
                Console.Write("Gib einen gültigen Namen ein:");
                Name = Console.ReadLine();
            }

            Scoreboardlist.Add($"{Name,-30}{score}");
            Console.Write("Drücke ENTER um zum Hauptmenü zurückzukehren ");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
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
                        leben = livedif;
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

                            case 1:
                                Scoreboard();
                                break;
                            case 2:
                                ShowEinstellungen();
                                break;
                            case 3:
                                menu = false;
                                break;
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
|                        O p t i o n e n                        |
|                  - Bitte wähle eine Option -                  |
+---------------------------------------------------------------+
|                                                               |");

            for (int i = 0; i < 4; i++)
            {

                string option = i switch
                {
                    0 => "Scoreboard",
                    1 => "Game Difficulty",
                    2 => "Zurück zum Hauptmenü",
                    _ => ""
                };
                string zeiger = (i + 1 == selected) ? "> " : "  ";
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


        static void Scoreboard()
        {
            Console.Clear();
            Console.WriteLine(
@"+----------------------------------------------------------------------+
|                                                                      |
|                              SCOREBOARD                              |
|                                                                      |
+----------------------------------------------------------------------+
|                                                                      |");
            Scoreboardlist.OrderByDescending(scoreboardwerte => Convert.ToInt32(scoreboardwerte.Substring(30))).ToList();
            foreach (string eintrag in Scoreboardlist)
            {
                Console.WriteLine($"|          {eintrag,-60}|");
            }

            Console.WriteLine(
@"|                                                                      |
+----------------------------------------------------------------------+
|                                                                      |
|   Drücke eine beliebige Taste, um zu den Optionen zurückzukehren...  |
|                                                                      |
+----------------------------------------------------------------------+");
            Console.ReadKey();
            Console.Clear();
        }


        static void ShowEinstellungen()
        {
            Console.Clear();
            bool menu = true;
            int EinstellungenAuswahl = 1;

            do
            {
                ZeigeEinstellungen(EinstellungenAuswahl);

                while (Console.KeyAvailable)
                    Console.ReadKey(true);

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        EinstellungenAuswahl--;
                        if (EinstellungenAuswahl < 1) EinstellungenAuswahl = 6;
                        break;

                    case ConsoleKey.DownArrow:
                        EinstellungenAuswahl++;
                        if (EinstellungenAuswahl > 6) EinstellungenAuswahl = 1;
                        break;
                    case ConsoleKey.Escape:
                        menu = false;
                        break;

                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        Console.Clear();
                        switch (EinstellungenAuswahl)
                        {

                            case 1:
                                gegneranzahl = 50;
                                gegner = gegneranzahl;
                                schutz = true;
                                schussanzahl = 7;
                                livedif = 5;
                                optionenfarbe = 1;
                                break;

                            case 2:
                                gegneranzahl = 60;
                                gegner = gegneranzahl;
                                schutz = true;
                                schussanzahl = 10;
                                livedif = 3;
                                optionenfarbe = 2;
                                break;

                            case 3:
                                gegneranzahl = 70;
                                gegner = gegneranzahl;
                                schutz = true;
                                schussanzahl = 10;
                                livedif = 3;
                                optionenfarbe = 3;
                                break;

                            case 4:
                                gegneranzahl = 80;
                                gegner = gegneranzahl;
                                schutz = true;
                                schussanzahl = 15;
                                livedif = 3;
                                optionenfarbe = 4;
                                break;

                            case 5:
                                gegneranzahl = 99;
                                gegner = gegneranzahl;
                                schutz = false;
                                schussanzahl = 20;
                                livedif = 1;
                                optionenfarbe = 5;
                                break;

                            case 6:
                                menu = false;
                                break;
                        }
                        break;
                }

            } while (menu);
        }

        static void ZeigeEinstellungen(int selected)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(
@"+---------------------------------------------------------------+
|                 G a m e  D i f f i c u l t y                  |
|                  - Bitte wähle eine Option -                  |
+---------------------------------------------------------------+
|                                                               |");

            for (int i = 0; i < 6; i++)
            {

                string option = i switch
                {
                    0 => "Easy",
                    1 => "Normal",
                    2 => "Hard",
                    3 => "Expert",
                    4 => "Nightmare",
                    5 => "Zurück zu den Optionen",
                    _ => ""
                };
                string zeiger = (i + 1 == selected) ? "> " : "  ";
                Console.Write($"|  {zeiger}");
                if (optionenfarbe == i + 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write($" {option,-58}");
                Console.ResetColor();
                Console.WriteLine($"|");
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
