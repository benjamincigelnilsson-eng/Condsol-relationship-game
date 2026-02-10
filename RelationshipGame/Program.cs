// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
    }

    static Random rng = new();
    static int day = 1;
    static bool gameRunning = true;
    static List<Character> characters = new();
    static Character current = new(); // fixes warning
    static Location currentLocation = Location.Home;
    static string saveFile = "savegame.json";

    static void Main()
    {
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
            HandleInput();
            UpdateMood(current);
            CheckEnding(current);
        }
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
    }

    static void ShowCharacterStatus()
    {
        Console.WriteLine($"Attraction: {current.Attraction}");
        Console.WriteLine($"Trust: {current.Trust}");
        Console.WriteLine($"Comfort: {current.Comfort}");
        Console.WriteLine($"Mood: {current.Mood}\n");
    }

    static void ShowMenu()
    {
        Console.WriteLine("1. Talk");
        Console.WriteLine("2. Hang out");
        Console.WriteLine("3. Give gift");
        Console.WriteLine("4. Give space");
        Console.WriteLine("5. Push too fast");
        Console.WriteLine("6. Switch character");
        Console.WriteLine("7. Change location");
        Console.WriteLine("8. End day");
        Console.WriteLine("S. Save game");
        Console.WriteLine("Q. Quit");
    }

    // ---------- INPUT ----------
    static void HandleInput()
    {
        string choice = Console.ReadLine() ?? "";

        switch (choice.ToUpper())
        {
            case "1": Talk(); break;
            case "2": HangOut(); break;
            case "3": Gift(); break;
            case "4": Space(); break;
            case "5": Push(); break;
            case "6": SwitchCharacter(); break;
            case "7": ChangeLocation(); break;
            case "8": EndDay(); break;
            case "S": SaveGame(); break;
            case "Q": gameRunning = false; break;
        }
    }

    // ---------- ACTIONS ----------
    static void Talk()
    {
        Console.WriteLine("\nTopics:");
        Console.WriteLine("1. Food");
        Console.WriteLine("2. Hobbies");
        Console.WriteLine("3. Feelings");
        Console.WriteLine("4. Inappropriate");

        string t = Console.ReadLine() ?? "";

        ShowDialogue(t);
        AdvanceTime();
    }

    static void HangOut()
    {
        current.Attraction += 5;
        current.Comfort += 4;
        ShowDialogue("hangout");
        LocationQuest();
        StoryArcProgress();
        SideEvent();
        AdvanceTime();
    }

    static void Gift()
    {
        ShowDialogue(rng.Next(2) == 0 ? "gift_good" : "gift_bad");
        AdvanceTime();
    }

    static void Space()
    {
        current.Comfort += 5;
        ShowDialogue("space");
        AdvanceTime();
    }

    static void Push()
    {
        current.Trust -= 8;
        current.Comfort -= 15;
        ShowDialogue("push");
        AdvanceTime();
    }

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

    static void EndDay()
    {
        Console.WriteLine("\nDay ends...");
        day++;
        Console.ReadKey();
    }

    // ---------- CORE SYSTEMS ----------
    static void Clamp(Character c)
    {
        c.Attraction = Math.Clamp(c.Attraction, 0, 100);
        c.Trust = Math.Clamp(c.Trust, 0, 100);
        c.Comfort = Math.Clamp(c.Comfort, 0, 100);
    }

    static void AdvanceTime()
    {
        day++;
        Clamp(current);
    }

    static void UpdateMood(Character c)
    {
        if (c.Comfort <= 20 || c.Trust <= 20) c.Mood = Mood.Upset;
        else if (c.Comfort <= 40) c.Mood = Mood.Guarded;
        else if (c.Trust >= 70 && c.Comfort >= 70) c.Mood = Mood.Happy;
        else c.Mood = Mood.Neutral;
    }

    // ---------- EVENTS ----------
    static void ShowDialogue(string key)
    {
        Console.WriteLine();
        Console.WriteLine(key switch
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
        });
        Console.ReadKey();
    }

    static void LocationQuest()
    {
        if (!current.QuestLog.Contains(currentLocation.ToString()))
            current.QuestLog.Add($"Visited {currentLocation}");
    }

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
        if (current.StoryProgress[arc] < 3)
            current.StoryProgress[arc]++;
    }

    static void SideEvent()
    {
        if (rng.Next(5) == 0)
            Console.WriteLine($"{current.Name} shares a personal moment.");
    }

    // ---------- SAVE ----------
    static void SaveGame()
    {
        File.WriteAllText(saveFile, JsonSerializer.Serialize(characters));
        Console.WriteLine("Game saved!");
        Console.ReadKey();
    }

    static void LoadGame()
    {
        if (File.Exists(saveFile))
            characters = JsonSerializer.Deserialize<List<Character>>(File.ReadAllText(saveFile)) ?? new();
    }

    static void CheckEnding(Character c)
    {
        if (day >= 30)
        {
            Console.Clear();
            Console.WriteLine($"Your story with {c.Name} comes to an end.");
            gameRunning = false;
        }
    }
}
