// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

class Program
{
    enum Mood { Happy, Neutral, Guarded, Upset }
    enum Personality { Shy, Open, Reserved }
    enum Location { Home, Cafe, Park }

    class Character
    {
        public string Name { get; set; } = "Unnamed";
        public int Attraction { get; set; } = 50;
        public int Trust { get; set; } = 50;
        public int Comfort { get; set; } = 50;
        public Mood Mood { get; set; } = Mood.Neutral;
        public Personality Personality { get; set; }
        public List<string> QuestLog { get; set; } = new();
        public Dictionary<string, int> StoryProgress { get; set; } = new();
        public List<string> DailyEvents { get; set; } = new();
    }

    static Random rng = new();
    static int day = 1;
    static bool gameRunning = true;
    static List<Character> characters = new();
    static Character current = new();
    static Location currentLocation = Location.Home;
    static string saveFile = "savegame.json";

    // ---------- ASCII ART ----------
    static Dictionary<string, string> CharacterArt = new()
    {
        {"Emilia", @"
    (\_._/)  
    ( o o )  
    (  -  )  
   o(__|__)o
" },
        {"Luna", @"
   (\_._/)  
   ( o_o )  
   (  v  )  
   o(__|__)o
" },
        {"Olivia", @"
   (\_._/)  
   ( ^ ^ )  
   (  ~  )  
   o(__|__)o
"}
    };

    static Dictionary<Location, string> LocationArt = new()
    {
        { Location.Home, @"
   [ Home Sweet Home ]
      ______________
     |    ____      |
     |   |    |     |
     |___|____|_____|
"},
        { Location.Cafe, @"
   [ Cozy Cafe ]
      ________
     |  __  |
     | |  | |
     | |__| |
     |______|
"},
        { Location.Park, @"
   [ Sunny Park ]
      🌳 🌳 🌳
      🌷 🌷 🌷
      🌞
"}
    };

    // ---------- MAIN ----------
    static void Main()
    {
        ShowIntro();
        LoadGame();

        if (characters.Count == 0)
            SetupCharacters();

        current = characters[0];

        while (gameRunning)
        {
            Console.Clear();
            ShowHeader();
            ShowCharacterStatus();
            ShowMenu();
            UpdateMood(current);
            CheckEnding(current);
        }
    }

    // ---------- INTRO ----------
    static void ShowIntro()
    {
        Console.Clear();
        int width = 70;
        string title = @"
 __        __   _                            ____  _       _                 
 \ \      / /__| | ___ ___  _ __ ___   ___  / ___|| | __ _| |_ ___  _ __ ___ 
  \ \ /\ / / _ \ |/ __/ _ \| '_ ` _ \ / _ \ \___ \| |/ _` | __/ _ \| '__/ _ \
   \ V  V /  __/ | (_| (_) | | | | | |  __/  ___) | | (_| | || (_) | | |  __/
    \_/\_/ \___|_|\___\___/|_| |_| |_|\___| |____/|_|\__,_|\__\___/|_|  \___|
";
        string subtitle = "Build relationships, make choices, and see how your story unfolds!";
        string prompt = "Press any key to start your adventure...";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('=', width));
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var line in title.Split('\n'))
        {
            foreach (char c in line) { Console.Write(c); Thread.Sleep(2); }
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('=', width));
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        foreach (char c in subtitle) { Console.Write(c); Thread.Sleep(10); }
        Console.WriteLine("\n\n");

        Console.ForegroundColor = ConsoleColor.Green;
        bool flash = true;
        while (!Console.KeyAvailable)
        {
            Console.SetCursorPosition((width - prompt.Length) / 2, Console.CursorTop);
            Console.Write(flash ? prompt : new string(' ', prompt.Length));
            flash = !flash;
            Thread.Sleep(500);
        }
        Console.ReadKey(true);
        Console.ResetColor();
    }

    // ---------- SETUP ----------
    static void SetupCharacters()
    {
        characters.Add(new Character { Name = "Emilia", Personality = Personality.Shy });
        characters.Add(new Character { Name = "Luna", Personality = Personality.Open });
        characters.Add(new Character { Name = "Olivia", Personality = Personality.Reserved });
    }

    // ---------- UI ----------
    static void ShowHeader()
    {
        Console.WriteLine($"DAY {day} - Location: {currentLocation}");
        Console.WriteLine($"Interacting with: {current.Name}");
        Console.WriteLine("---------------------------------\n");

        if (LocationArt.ContainsKey(currentLocation))
            Console.WriteLine(LocationArt[currentLocation]);

        if (CharacterArt.ContainsKey(current.Name))
            Console.WriteLine(CharacterArt[current.Name]);
    }

    static void ShowCharacterStatus()
    {
        void WriteStat(string name, int value)
        {
            Console.Write($"{name}: ");
            Console.ForegroundColor = GetStatColor(value);
            Console.Write(value.ToString().PadLeft(3));
            Console.Write(" ");
            DrawBar(value);
            Console.WriteLine();
            Console.ResetColor();
        }

        WriteStat("Attraction", current.Attraction);
        WriteStat("Trust", current.Trust);
        WriteStat("Comfort", current.Comfort);

        Console.Write("Mood: ");
        Console.ForegroundColor = current.Mood switch
        {
            Mood.Happy => ConsoleColor.Green,
            Mood.Neutral => ConsoleColor.White,
            Mood.Guarded => ConsoleColor.Yellow,
            Mood.Upset => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
        Console.WriteLine($"{current.Mood} {GetMoodIcon(current.Mood)}");
        Console.ResetColor();
        Console.WriteLine();
    }

    static string GetMoodIcon(Mood mood) => mood switch
    {
        Mood.Happy => "🙂",
        Mood.Neutral => "😐",
        Mood.Guarded => "😟",
        Mood.Upset => "😡",
        _ => ""
    };

    static ConsoleColor GetStatColor(int value)
    {
        if (value <= 20) return ConsoleColor.Red;
        if (value <= 50) return ConsoleColor.Yellow;
        if (value <= 80) return ConsoleColor.Cyan;
        return ConsoleColor.Green;
    }

    static void DrawBar(int value)
    {
        int total = 20;
        int filled = value * total / 100;
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('-', total - filled));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(new string('|', filled));
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("]");
    }

    // ---------- MENU ----------
    static void ShowMenu()
    {
        string[] inGameActions = { "Talk", "Hang out", "Give gift", "Give space", "Push too fast", "Switch character", "Change location", "End day" };
        string[] systemActions = { "Save game (J)", "Load game (K)", "Quit game (L)" };

        bool inSystem = false;
        int selectedIndex = 0;
        ConsoleKey key;

        do
        {
            Console.Clear();
            ShowHeader();
            ShowCharacterStatus();

            Console.WriteLine("IN-GAME ACTIONS:");
            for (int i = 0; i < inGameActions.Length; i++)
            {
                if (!inSystem && i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine($"{i + 1}. {inGameActions[i]}");
                Console.ResetColor();
            }

            Console.WriteLine("\nSYSTEM ACTIONS:");
            for (int i = 0; i < systemActions.Length; i++)
            {
                if (inSystem && i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine($"{i + 1}. {systemActions[i]}");
                Console.ResetColor();
            }

            Console.WriteLine("\nUse ↑↓ arrows to navigate, Enter to select, number keys 1-8 for actions, J/K/L for system shortcuts.");

            key = Console.ReadKey(true).Key;

            // Arrow navigation
            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex--;
                if (selectedIndex < 0)
                {
                    if (inSystem) { inSystem = false; selectedIndex = inGameActions.Length - 1; }
                    else selectedIndex = 0;
                }
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex++;
                if (!inSystem && selectedIndex >= inGameActions.Length) { inSystem = true; selectedIndex = 0; }
                else if (inSystem && selectedIndex >= systemActions.Length) { selectedIndex = systemActions.Length - 1; }
            }

            // Number keys 1-8 for in-game actions
            if (key >= ConsoleKey.D1 && key <= ConsoleKey.D8)
            {
                selectedIndex = key - ConsoleKey.D1;
                inSystem = false;
                key = ConsoleKey.Enter;
            }

            // System shortcuts
            if (key == ConsoleKey.J) { SaveGame(); return; }
            if (key == ConsoleKey.K) { LoadGame(); return; }
            if (key == ConsoleKey.L) { gameRunning = false; return; }

        } while (key != ConsoleKey.Enter);

        // Execute selection
        if (!inSystem)
        {
            switch (selectedIndex)
            {
                case 0: TalkBranching(); break;
                case 1: HangOut(); break;
                case 2: Gift(); break;
                case 3: Space(); break;
                case 4: Push(); break;
                case 5: SwitchCharacter(); break;
                case 6: ChangeLocation(); break;
                case 7: EndDaySummary(); break;
            }
        }
        else
        {
            switch (selectedIndex)
            {
                case 0: SaveGame(); break;
                case 1: LoadGame(); break;
                case 2: gameRunning = false; break;
            }
        }
    }

    // ---------- INTERACTIONS ----------
    static void TalkBranching()
    {
        Console.WriteLine("\nTopics:");
        Console.WriteLine("1. Food");
        Console.WriteLine("2. Hobbies");
        Console.WriteLine("3. Feelings");
        Console.WriteLine("4. Inappropriate");

        string t = Console.ReadLine() ?? "";
        int topic;
        if (!int.TryParse(t, out topic) || topic < 1 || topic > 4) topic = 0;

        switch (topic)
        {
            case 1: AdjustStatsByPersonalityAndMood(3, 1, 2, 1); ShowDialogue("1"); break;
            case 2: AdjustStatsByPersonalityAndMood(2, 3, 2, 2); ShowDialogue("2"); break;
            case 3: AdjustStatsByPersonalityAndMood(5, 4, 3, 3); ShowDialogue("3"); break;
            case 4: AdjustStatsByPersonalityAndMood(-5, -3, -5, 4); ShowDialogue("4"); break;
            default: ShowDialogue(""); break;
        }
        AdvanceTime();
    }

    static void AdjustStatsByPersonalityAndMood(int a, int t, int c, int topic)
    {
        // Personality modifiers
        switch (current.Personality)
        {
            case Personality.Shy:
                a = (topic == 3 || topic == 4) ? a / 2 : a;
                t = (topic == 4) ? t / 2 : t;
                break;
            case Personality.Open:
                a = (topic != 4) ? a + 2 : a;
                t = (topic != 4) ? t + 1 : t;
                break;
            case Personality.Reserved:
                a = (topic == 4) ? a / 2 : a;
                t = (topic == 3) ? t / 2 : t;
                c = (topic == 4) ? c - 2 : c;
                break;
        }

        // Mood modifiers
        switch (current.Mood)
        {
            case Mood.Happy: a += 2; t += 2; break;
            case Mood.Guarded: a -= 2; t -= 1; break;
            case Mood.Upset: a -= 4; t -= 3; c -= 2; break;
        }

        current.Attraction = Math.Clamp(current.Attraction + a, 0, 100);
        current.Trust = Math.Clamp(current.Trust + t, 0, 100);
        current.Comfort = Math.Clamp(current.Comfort + c, 0, 100);

        // Secret events
        if (current.Personality == Personality.Shy && current.Mood == Mood.Happy && rng.Next(4) == 0)
            current.DailyEvents.Add($"{current.Name} shyly admits they enjoyed the conversation!");
        if (current.Personality == Personality.Open && current.Mood == Mood.Guarded && rng.Next(4) == 0)
            current.DailyEvents.Add($"{current.Name} tries to open up despite feeling guarded.");
        if (current.Personality == Personality.Reserved && current.Mood == Mood.Upset && rng.Next(4) == 0)
            current.DailyEvents.Add($"{current.Name} silently withdraws, needing space.");
    }

    static void AdjustStats(int a, int t, int c)
    {
        current.Attraction = Math.Clamp(current.Attraction + a, 0, 100);
        current.Trust = Math.Clamp(current.Trust + t, 0, 100);
        current.Comfort = Math.Clamp(current.Comfort + c, 0, 100);
    }

    static void HangOut() { AdjustStats(5, 4, 4); ShowDialogue("hangout"); LocationQuest(); StoryArcProgress(); SideEvent(); AdvanceTime(); }
    static void Gift() { AdjustStats(rng.Next(0, 6), rng.Next(0, 4), rng.Next(0, 3)); ShowDialogue(rng.Next(2) == 0 ? "gift_good" : "gift_bad"); AdvanceTime(); }
    static void Space() { AdjustStats(0, 0, 5); ShowDialogue("space"); AdvanceTime(); }
    static void Push() { AdjustStats(-8, -8, -15); ShowDialogue("push"); AdvanceTime(); }

    static void SwitchCharacter()
    {
        Console.WriteLine("\nChoose:");
        for (int i = 0; i < characters.Count; i++)
            Console.WriteLine($"{i + 1}. {characters[i].Name}");
        if (int.TryParse(Console.ReadLine(), out int i2))
            current = characters[Math.Clamp(i2 - 1, 0, characters.Count - 1)];
    }

    static void ChangeLocation()
    {
        Console.WriteLine("\nChoose location:");
        foreach (var loc in Enum.GetValues<Location>())
            Console.WriteLine($"{(int)loc + 1}. {loc}");
        if (int.TryParse(Console.ReadLine(), out int l))
            currentLocation = (Location)(l - 1);
        SideEvent();
    }

    static void ShowDialogue(string key)
    {
        string message = key switch
        {
            "1" => $"{current.Name} smiles talking about food.",
            "2" => $"{current.Name} opens up about hobbies.",
            "3" => $"{current.Name} hesitates but shares feelings.",
            "4" => $"{current.Name} looks uncomfortable.",
            "hangout" => $"You spend a nice afternoon together.",
            "gift_good" => $"{current.Name} loves the gift!",
            "gift_bad" => $"{current.Name} forces a polite smile.",
            "space" => $"{current.Name} appreciates the space.",
            "push" => $"{current.Name} pulls away.",
            _ => "A quiet moment passes."
        };
        current.DailyEvents.Add(message);
        foreach (char c in message) { Console.Write(c); Thread.Sleep(20); }
        Console.WriteLine("\nPress any key...");
        Console.ReadKey(true);
    }

    static void EndDaySummary()
    {
        Console.Clear();
        Console.WriteLine($"DAY {day} SUMMARY - {current.Name}\n");
        ShowHeader();
        ShowCharacterStatus();

        if (current.DailyEvents.Count > 0)
        {
            Console.WriteLine("Today's Events:");
            foreach (var e in current.DailyEvents)
                Console.WriteLine("- " + e);
        }
        else Console.WriteLine("No notable events today.");

        current.DailyEvents.Clear();
        day++;
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    static void Clamp(Character c)
    {
        c.Attraction = Math.Clamp(c.Attraction, 0, 100);
        c.Trust = Math.Clamp(c.Trust, 0, 100);
        c.Comfort = Math.Clamp(c.Comfort, 0, 100);
    }

    static void AdvanceTime() { Clamp(current); }

    static void UpdateMood(Character c)
    {
        if (c.Comfort <= 20 || c.Trust <= 20) c.Mood = Mood.Upset;
        else if (c.Comfort <= 40) c.Mood = Mood.Guarded;
        else if (c.Trust >= 70 && c.Comfort >= 70) c.Mood = Mood.Happy;
        else c.Mood = Mood.Neutral;
    }

    static void LocationQuest() { if (!current.QuestLog.Contains(currentLocation.ToString())) current.QuestLog.Add($"Visited {currentLocation}"); }
    static void StoryArcProgress()
    {
        string arc = current.Name switch
        {
            "Emilia" => "Art Exhibition",
            "Luna" => "Dance Competition",
            "Olivia" => "Coding Project",
            _ => "Story"
        };
        current.StoryProgress.TryAdd(arc, 0);
        if (current.StoryProgress[arc] < 3) current.StoryProgress[arc]++;
    }

    static void SideEvent() { if (rng.Next(5) == 0) ShowDialogue($"{current.Name} shares a personal moment."); }

    static void SaveGame() { File.WriteAllText(saveFile, JsonSerializer.Serialize(characters)); Console.WriteLine("Game saved! Press any key..."); Console.ReadKey(true); }
    static void LoadGame() { if (File.Exists(saveFile)) { characters = JsonSerializer.Deserialize<List<Character>>(File.ReadAllText(saveFile)) ?? new(); Console.WriteLine("Game loaded! Press any key..."); } else { Console.WriteLine("No saved game found. Press any key..."); } Console.ReadKey(true); }

    static void CheckEnding(Character c)
    {
        if (day >= 30)
        {
            Console.Clear();
            Console.WriteLine($"Your story with {c.Name} comes to an end.");
            Console.WriteLine($"Final stats: Attraction {c.Attraction}, Trust {c.Trust}, Comfort {c.Comfort}, Mood {c.Mood}");
            gameRunning = false;
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }
    }
}

