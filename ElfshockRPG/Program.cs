using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;

namespace ElfshockRPG
{

    class Program
    {
        enum GameState { MainMenu, CharacterSelect, InGame, Exit };
        GameState currentState = GameState.MainMenu;

        string baseRacesPath = "baseRaces.json";

        private Character player;

        int remainingPoints = 3;
        int[,] grid;
        List<Character> enemies = new List<Character>();
        static void Main(string[] args)
        {
            var main = new Program();
            while(true)main.Update();

            
        }

        private void Update()
        {
            Console.Clear();
            switch (currentState)
            {
                case GameState.MainMenu: MainMenu();
                    break;
                case GameState.CharacterSelect: CharacterSelect();
                    break;
                case GameState.InGame: InGame();
                    break;
                case GameState.Exit: Exit();
                    break;
                default:
                    break;
            }
        }

        private void MainMenu()
        {
            Console.WriteLine("Welcome\nPress any key to play.");
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape) Environment.Exit(0);
            currentState = GameState.CharacterSelect;
        }
        private void CharacterSelect()
        {
            int characterType = 0;
            remainingPoints = 3;

            string baseRacesJson = File.ReadAllText(baseRacesPath);
            List<Character> baseRaces = JsonSerializer.Deserialize<List<Character>>(baseRacesJson);
            Console.WriteLine(baseRaces[characterType]);

            do
            { 
                Console.WriteLine("Choose character type: \nOptions:");
                for (int i = 0; i < baseRaces.Count; i++)
                {
                    Console.WriteLine("{0}) {1}", i+1,baseRaces[i].Name);
                }
                Console.WriteLine("Your pick: ");

            } while(!int.TryParse(Console.ReadLine(), out characterType) || characterType < 1 || characterType > baseRaces.Count + 1);

            player = baseRaces[characterType - 1];
            Console.WriteLine("Selected race: " + player.Name);

            ConsoleKey response;
            do
            {
                Console.Write("\nWould you like to buff your stats before starting?       (Limit: 3 points total)\nResponse (Y/N): ");
                response = Console.ReadKey().Key;
            }
            while (response != ConsoleKey.Y && response != ConsoleKey.N);


            if (response == ConsoleKey.Y)
            {
                int bonusStrength = 0;
                int bonusAgility = 0;
                int bonusIntelligence = 0;
                do
                {
                    bonusStrength += AddPoints("Strength");
                    bonusAgility += AddPoints("Agility");
                    bonusIntelligence += AddPoints("Intelligence");

                } while (remainingPoints > 0);

                player.Strength += bonusStrength;
                player.Agility += bonusAgility;
                player.Intelligence += bonusIntelligence;

            }

            player.Setup();

            string pastCharactersJson = "";
            if (File.Exists("Past characters.json")) pastCharactersJson = File.ReadAllText("Past characters.json");
            pastCharactersJson += JsonSerializer.Serialize<Character>(player);
            File.WriteAllText("Past characters.json", pastCharactersJson);

            Console.ReadLine();
            grid = new int[10, 10];
            enemies.Clear();
            currentState = GameState.InGame;
        }
        private void InGame()
        {
            Random rnd = new Random();
            int monX, monY;
            do
            {
                monX = rnd.Next(0, 9);
                monY = rnd.Next(0, 9);
            } while (grid[monX, monY] != 0);
            enemies.Add(new Character("Goblin", rnd.Next(1, 3), rnd.Next(1, 3), rnd.Next(1, 3), 1, '◙', monX, monY));

            Draw();
            ConsoleKey response;
            do
            {
                Console.Write("\nChoose action\n1) Attack\n2) Move\n");
                response = Console.ReadKey(true).Key;
            }
            while (response != ConsoleKey.D1 && response != ConsoleKey.D2);

            if (response == ConsoleKey.D1) Attack(enemies);
            if (response == ConsoleKey.D2) for(int i = 0; i < player.Range; i++) Move();

            foreach (Character enemy in enemies)
            {
                enemy.AI(player, grid);
                //To "animate" the enemies moving
                Draw();
                System.Threading.Thread.Sleep(100);
            }

            if (player.Health <= 0) currentState = GameState.Exit;
        }

        private void Draw()
        {
            Console.Clear();
            Console.WriteLine("Health: {0}   Mana: {1}\n", player.Health, player.Mana);

            grid = new int[10, 10];
            
            foreach(Character enemy in enemies)
            {
                grid[enemy.posX, enemy.posY] = 2;
            }

            grid[player.posX, player.posY] = 1;

            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    switch (grid[x,y])
                    {
                        case 1: Console.Write(player.Sprite);
                            break;
                        case 2: Console.Write("☼");
                            break;
                        default:
                            Console.Write("▒");
                            break;
                    }
                }
                Console.WriteLine("");
            }
        }

        private void Attack(List<Character> enemies)
        {
            List<Character> enemiesInRange = new List<Character>();

            foreach(Character enemy in enemies)
            {
                int xSq = (enemy.posX - player.posX) * (enemy.posX - player.posX);
                int ySq = (enemy.posY - player.posY) * (enemy.posY - player.posY);

                //Make Range squared so I don't need to call Math.Sqrt
                if (player.Range * player.Range >= xSq + ySq) enemiesInRange.Add(enemy);
            }

            if(enemiesInRange.Count < 1)
            {
                Console.WriteLine("No available targets in your range");
                for (int j = 0; j < player.Range; j++) Move();
                return;
            }

            int i = 0;
            foreach(Character enemy in enemiesInRange)
            {
                Console.WriteLine("{0}) Goblin - {1} HP", i+1, enemy.Health);
                i++;
            }

            int selectedEnemy = 0;
            do
            {
                Console.WriteLine("Choose target: ");
            }
            while (!int.TryParse(Console.ReadLine(), out selectedEnemy) || selectedEnemy < 1 || selectedEnemy > enemiesInRange.Count + 1) ;
            selectedEnemy--;

            enemiesInRange[selectedEnemy].Health -= player.Damage;
            if (enemiesInRange[selectedEnemy].Health <= 0) enemies.Remove(enemiesInRange[selectedEnemy]);
        }

        private void Move()
        {
            Console.WriteLine("Press move key");
            ConsoleKey input = Console.ReadKey(true).Key;
            int addX = 0, addY = 0;

            if(input == ConsoleKey.W || input == ConsoleKey.Q || input == ConsoleKey.E) addY = -1;
            if (input == ConsoleKey.S || input == ConsoleKey.X || input == ConsoleKey.Z) addY = 1;
            if (input == ConsoleKey.A || input == ConsoleKey.Q || input == ConsoleKey.Z) addX = -1;
            if (input == ConsoleKey.D || input == ConsoleKey.E || input == ConsoleKey.X) addX = 1;

            player.posY = Math.Clamp(player.posY + addY, 0, 9);
            player.posX = Math.Clamp(player.posX + addX, 0, 9);

            Draw();
        }

        private void Exit()
        {
            Console.WriteLine("Game Over.\nPress any key to restart or Escape to exit");
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape) Environment.Exit(0);
            currentState = GameState.MainMenu;
        }

        private int AddPoints(string statName)
        {
            if (remainingPoints <= 0) return 0;

            Console.Write("\nRemaining Points: " + remainingPoints);
            Console.Write("\nAdd to {0}: ", statName);
            int bonusStat = 0;
            if (int.TryParse(Console.ReadLine(), out bonusStat))
            {
                if (bonusStat > remainingPoints || bonusStat < 0) Console.Write("\nNot enough points.");
                else remainingPoints -= bonusStat;
            }
            else Console.WriteLine("\nInvalid Number.");

            return bonusStat;
        }

        //Optional to generate the json file with the 3 races
        private void GenerateBaseRacesJSON()
        {
            Character mage = new Character("Mage", 2, 1, 3, 3, '*');
            Character warrior = new Character("Warrior", 3, 3, 0, 1, '@');
            Character archer = new Character("Archer", 2, 4, 0, 2, '#');

            List<Character> baseCharacters = new List<Character> { mage, warrior, archer };

            string baseRacesJson = JsonSerializer.Serialize(baseCharacters, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(baseRacesPath, baseRacesJson);
        }
    }
}
