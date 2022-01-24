using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace GeoGamr
{
    class Program
    {
        static void Main(string[] args)
        {
            Clear();

            // test if file is found
            if (!File.Exists("countries.json"))
            {
                Console.WriteLine("Game files missing... can't start. Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // read in the json file
            string json = File.ReadAllText("countries.json");

            // deserialize the json into a list of City objects
            List<Country> Countries = JsonSerializer.Deserialize<List<Country>>(json);

            Console.WriteLine("What is your name?");
            Console.Write("My name is: ");

            string name = "";
            while (name == "")
            {
                name = Console.ReadLine();
            }

            Console.WriteLine($"Hello {name}!");

            // create a new game
            Game game = new Game(name, Countries);


            // loop forever until the user wants to quit
            while (true)
            {
                Clear(name);
                string choice = ShowMenu();
                Clear(name);

                // if the user chooses 4, difficulty and region is not required
                if (choice == "4")
                {
                    Console.WriteLine("Which country?\nPs! Search for 'random', to get a random country!");
                    string country = Console.ReadLine();
                    game.ShowCountry(country);

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    continue;
                }

                string region = "";

                // if the user chooses 1, 2 or 3, choose a region
                if (choice is "1" or "2" or "3")
                {
                    // which region?
                    region = RegionMenu();
                    if (region == "Exit") continue;
                }

                Clear(name);

                // switch to get the desired function
                switch (choice.ToLower())
                {
                    case "1":
                        // which difficulty?
                        string difficulty = DifficultyMenu();
                        Clear();
                        if (difficulty == "Exit") continue;
                        game.MatchCapitalToCountry(difficulty, region);
                        break;
                    case "2":
                        difficulty = DifficultyMenu();
                        Clear();
                        if (difficulty == "Exit") continue;
                        game.MatchCountryToCapital(difficulty, region);
                        break;
                    case "3":
                        game.BigOrSmall(region);
                        break;
                    case "x":
                        // quit the game
                        Console.WriteLine("Goodbye!");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice...");
                        continue;
                }

                Console.WriteLine("\nGame ended.\nPress any key to continue...");

                // wait for the user to click a key before continuing
                Console.ReadKey();
            }
        }

        public static void Clear(string message = "")
        {
            // clear the terminal and write the custom header with optional message
            Console.Clear();
            Console.WriteLine("/////////////");
            Console.WriteLine("|| GEOGAMR ||");

            // print the optional message
            Console.WriteLine($"|| {message}\n");
        }

        private static string RegionMenu()
        {
            // loop until option is valid
            while (true)
            {
                Console.WriteLine("Which region?\n1 - 4 is practice mode\n5 is the challenge\n");
                Console.WriteLine("1. Europe");
                Console.WriteLine("2. Asia & Oceania");
                Console.WriteLine("3. Africa");
                Console.WriteLine("4. America (North & South)");
                Console.WriteLine("5. All");
                Console.WriteLine("x. Return to main menu");

                Console.Write("\nChoose a region [1-5]: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        return "Europe";
                    case "2":
                        return "Asia";
                    case "3":
                        return "Africa";
                    case "4":
                        return "America";
                    case "5":
                        return "All";
                    case "x":
                    case "X":
                        return "Exit";
                    default:
                        Clear("INVALID CHOICE");
                        break;
                }
            }
        }

        private static string DifficultyMenu()
        {
            // loop until option is valid
            while (true)
            {
                Console.WriteLine("Which difficulty?");
                Console.WriteLine("1. Easy");
                Console.WriteLine("2. Medium");
                Console.WriteLine("3. Hard");
                Console.WriteLine("x. Return");
                Console.Write("\nChoose a difficulty [1-3]: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        return "EASY";
                    case "2":
                        return "MEDIUM";
                    case "3":
                        return "HARD";
                    case "x":
                    case "X":
                        return "Exit";
                    default:
                        Clear("INVALID CHOICE");
                        break;
                }
            }
        }

        // menu
        static string ShowMenu()
        {
            Console.WriteLine("1. Match capital with country");
            Console.WriteLine("2. Match country with capital");
            Console.WriteLine("3. Larger or smaller");
            Console.WriteLine("4. Search for a country");
            Console.WriteLine("x. Exit");

            Console.Write("\nEnter your choice: ");
            return Console.ReadLine();
        }

        public class Country
        {
            public string name { get; set; }
            public string code { get; set; }

            public string capital { get; set; }

            public Country(string name, string capital)
            {
                this.name = name;
                this.capital = capital;
            }

            public string continent { get; set; }
            public float area { get; set; }
            public bool landlocked { get; set; }
            public List<string> borders { get; set; }
        }

        public class Game
        {
            private string _name;
            List<Country> Countries { get; set; }

            public Game(string name, List<Country> countries)
            {
                _name = name;
                Countries = countries;
            }

            private int Difficulty(string difficulty)
            {
                switch (difficulty)
                {
                    case "MEDIUM":
                        return 15;
                    case "HARD":
                        return 30;
                    default:
                        return 10;
                }
            }

            private List<Country> GetCountries(string continent)
            {
                if (continent == "All")
                {
                    return Countries;
                }

                List<Country> countries = new List<Country>();
                foreach (Country country in Countries)
                {
                    if (continent == "Asia")
                    {
                        if (country.continent is "Asia" or "Oceania")
                        {
                            countries.Add(country);
                        }
                    }
                    else if (continent == "America")
                    {
                        if (country.continent is "North America" or "South America")
                        {
                            countries.Add(country);
                        }
                    }
                    else if (country.continent == continent)
                    {
                        countries.Add(country);
                    }
                }

                return countries;
            }

            public void MatchCapitalToCountry(string difficulty, string region)
            {
                int pointsToAdd = Difficulty(difficulty);
                int points = 0;
                int correctGuesses = 0;
                int round = 1;
                string msg = "";

                // get countries from region
                List<Country> availableCountries = GetCountries(region);

                // get 10 random countries
                List<Country> randomCountries = availableCountries.OrderBy(x => Guid.NewGuid()).Take(10).ToList();


                int difficultyInt = difficulty == "EASY" ? 1 : difficulty == "MEDIUM" ? 2 : 3;

                foreach (Country country in randomCountries)
                {
                    Clear($"{round}/10 - {points} points {msg}");
                    msg = "\n|| ";

                    string answer = "";


                    Console.WriteLine($"What is the capital of {country.name}?\n");

                    switch (difficultyInt)
                    {
                        case 1:

                            List<Country> alternatives = GetAlternatives(availableCountries, country);
                            Console.WriteLine("A. " + alternatives[0].capital);
                            Console.WriteLine("B. " + alternatives[1].capital);
                            Console.WriteLine("C. " + alternatives[2].capital);
                            Console.WriteLine("D. " + alternatives[3].capital);
                            answer = GetChoice(alternatives).capital;

                            break;
                        case 2:

                            for (int i = 0; i < country.capital.Length; i++)
                            {
                                char letter = country.capital[i];
                                if (i == 0) Console.Write(country.capital[0]);
                                else if (!Char.IsLetter(letter)) Console.Write(letter);
                                else if (i % 2 == 0) Console.Write(letter);
                                else Console.Write("_");
                            }

                            Console.Write("\n\nYour answer: ");
                            answer = Console.ReadLine();
                            break;
                        case 3:
                            for (int i = 0; i < country.capital.Length; i++)
                            {
                                Char letter = country.capital[i];
                                if (!Char.IsLetter(letter)) Console.Write(letter);
                                else Console.Write("_");
                            }

                            Console.Write("\n\nYour answer: ");
                            answer = Console.ReadLine();
                            break;
                    }

                    msg += ($"Your answer: {answer}\n|| ");
                    round++;
                    if (Sanitize(answer.ToLower()) == Sanitize(country.capital.ToLower()))
                    {
                        msg += "CORRECT! ";
                        points += pointsToAdd;
                        correctGuesses++;
                    }
                    else
                    {
                        msg += "WRONG! ";
                    }

                    msg += $"The capital of {country.name} is {country.capital}\n";
                }

                printScore(msg, correctGuesses, points);
                saveScore("capToCountry", new HighScore(_name, points, difficulty), region);
            }

            public void MatchCountryToCapital(string difficulty, string region)
            {
                int pointsToAdd = Difficulty(difficulty);
                int points = 0;
                int correctGuesses = 0;
                int round = 1;
                string msg = "";


                // get countries from region
                List<Country> availableCountries = GetCountries(region);

                // get 10 random countries
                List<Country> randomCountries = availableCountries.OrderBy(x => Guid.NewGuid()).Take(10).ToList();

                int difficultyInt = difficulty == "EASY" ? 1 : difficulty == "MEDIUM" ? 2 : 3;

                foreach (Country country in randomCountries)
                {
                    Clear($"{round}/10 - {points} points {msg}");
                    msg = "\n|| ";

                    Console.WriteLine($"Which country has the capital {country.capital}?\n");

                    string answer = "";
                    switch (difficultyInt)
                    {
                        case 1:

                            // // easy gets four different options
                            List<Country> alternatives = GetAlternatives(availableCountries, country);


                            // print the alternatives
                            Console.WriteLine("A. " + alternatives[0].name);
                            Console.WriteLine("B. " + alternatives[1].name);
                            Console.WriteLine("C. " + alternatives[2].name);
                            Console.WriteLine("D. " + alternatives[3].name);

                            // get the answer
                            answer = GetChoice(alternatives).name;
                            Console.WriteLine("Your answer: " + answer);

                            break;
                        case 2:

                            for (int i = 0; i < country.name.Length; i++)
                            {
                                char letter = country.name[i];

                                if (i == 0) Console.Write(letter);

                                else if (!Char.IsLetter(letter)) Console.Write(letter);
                                else if (i % 2 == 0) Console.Write(letter);
                                else Console.Write("_");
                            }

                            Console.Write("\n\nYour answer: ");
                            answer = Console.ReadLine();

                            break;
                        case 3:
                            for (int i = 0; i < country.name.Length; i++)
                            {
                                char letter = country.name[i];
                                if (!Char.IsLetter(letter)) Console.Write(letter);
                                else Console.Write("_");
                            }

                            Console.Write("\n\nYour answer: ");
                            answer = Console.ReadLine();
                            break;
                    }

                    Clear();
                    msg += ($"Your answer: {answer}\n|| ");

                    round++;
                    if (Sanitize(answer.ToLower()) == Sanitize(country.name.ToLower()))
                    {
                        msg += "CORRECT! ";
                        points += pointsToAdd;
                        correctGuesses++;
                    }
                    else
                    {
                        msg += "WRONG! ";
                    }

                    Console.WriteLine($" {country.capital} is the capital of {country.name}\n");

                    msg += $"The capital of {country.name} is {country.capital}\n";
                }

                printScore(msg, correctGuesses, points);

                saveScore("countryToCap", new HighScore(_name, points, difficulty), region);
            }

            private void printScore(string msg, int correctGuesses, int points)
            {
                Clear($"{msg}|| GAME OVER!");
                Console.WriteLine("You got " + correctGuesses + " out of 10 correct!");
                Console.WriteLine("You scored " + points + " points!");
                Console.WriteLine("\n");
            }

            private List<Country> GetAlternatives(List<Country> availableCountries, Country answer)
            {
                // create a list of alternatives and add the correct answer
                List<Country> alternatives = new List<Country> {answer};
                // get three random capitals
                while (alternatives.Count < 4)
                {
                    // get a random country
                    Country tempCountry = availableCountries.OrderBy(x => Guid.NewGuid()).First();

                    // if the country already is in the list, don't add it
                    if (!alternatives.Contains(tempCountry))
                    {
                        alternatives.Add(tempCountry);
                    }
                }

                // shuffle alternatives in dictionary
                alternatives = alternatives.OrderBy(x => Guid.NewGuid()).ToList();

                // return the alternatives
                return alternatives;
            }

            private Country GetChoice(List<Country> alternatives)
            {
                Console.Write("Your answer: ");
                string answer = Console.ReadLine().ToLower();
                switch (answer)
                {
                    case "a":
                        return alternatives[0];
                    case "b":
                        return alternatives[1];
                    case "c":
                        return alternatives[2];
                    case "d":
                        return alternatives[3];
                    default:
                        return new Country(answer, answer);
                }
            }

            private void saveScore(string game, HighScore highScore, string region)
            {
                string fileName = $"_HS_{game}.json";
                List<HighScore> highScores = new List<HighScore>();
                if (File.Exists(fileName))
                {
                    string json = File.ReadAllText(fileName);
                    highScores = JsonSerializer.Deserialize<List<HighScore>>(json);

                    highScores.Add(highScore);

                    // order list by descending points and get the 10 highest
                    highScores = highScores.OrderByDescending(score => score.Points).Take(10).ToList();

                    Console.WriteLine("HIGHSCORES\n-----------------");
                    for (int i = 0; i < highScores.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {highScores[i].GetScore()}\n-----------------");
                    }
                }
                else
                {
                    File.Create(fileName).Close();
                    highScores.Add(highScore);
                    Console.WriteLine($"1. {highScore.GetScore()}");
                }


                // the highscore is only saved if all regions are played
                if (region == "All")
                {
                    // save to JSON
                    string json = JsonSerializer.Serialize(highScores);
                    File.WriteAllText(fileName, json);
                }
                else
                {
                    Console.WriteLine("Play all regions to save to the highscore!");
                }
            }

            public void BigOrSmall(string region)
            {
                List<Country> availableCountries = GetCountries(region);
                int points = 0;
                int pointsToAdd = 10;
                int correctGuesses = 0;

                // get 10 random countries
                List<Country> randomCountries = availableCountries.OrderBy(x => Guid.NewGuid()).Take(10).ToList();

                string msg = "";
                int round = 1;


                foreach (Country country in randomCountries)
                {
                    Clear($"{round}/10 - {points} points {msg}");
                    msg = "\n|| ";

                    Country randomCountry = Countries.OrderBy(x => Guid.NewGuid()).First();
                    while (true)
                    {
                        // get different random country
                        if (randomCountry.name != country.name)
                        {
                            break;
                        }

                        randomCountry = Countries.OrderBy(x => Guid.NewGuid()).First();
                    }

                    while (true)
                    {
                        Console.WriteLine(
                            $"Is {country.name} bigger than {randomCountry.name} ({randomCountry.area} km2)?\n");

                        Console.Write("Your answer [y/n]: ");
                        string guess = Console.ReadLine().ToLower();

                        if (guess == "y" || guess == "n")
                        {
                            if ((guess == "y" && country.area > randomCountry.area) ||
                                ((guess == "n" && country.area < randomCountry.area)))
                            {
                                points += pointsToAdd;
                                correctGuesses++;
                                msg += "CORRECT!";
                                break;
                            }

                            msg += "WRONG!";
                            break;
                        }

                        Clear("Invalid answer!");
                    }

                    round++;
                }

                Clear($"{msg}");
                Console.WriteLine("You got " + correctGuesses + " out of 10 correct!");
                Console.WriteLine("You scored " + points + " points!");
                Console.WriteLine("\n");

                // save score
                saveScore("bigOrSmall", new HighScore(_name, points), region);
            }

            public void ShowCountry(string search)
            {
                Country country;
                if (search.ToLower() == "random")
                {
                    // get random country
                    country = Countries.OrderBy(x => Guid.NewGuid()).First();
                }
                else
                {
                    country =
                        Countries.FirstOrDefault(x => Sanitize(x.name).ToLower() == Sanitize(search).ToLower());
                }

                if (country != null)
                {
                    Clear(country.name.ToUpper());
                    Console.WriteLine($"{country.name} - {country.code}");
                    Console.WriteLine($"The capital is {country.capital}");
                    Console.WriteLine($"The surface area is {country.area} km2");
                    Console.WriteLine($"It's located in {country.continent}");

                    if (country.landlocked)
                    {
                        Console.WriteLine("It's a landlocked country");
                    }
                    else
                    {
                        Console.WriteLine("It's not a landlocked country");
                    }

                    if (country.borders.Count > 0)
                    {
                        string word = country.borders.Count == 1 ? "country" : "countries";
                        Console.WriteLine($"\n{country.name} borders with {country.borders.Count} {word} ");
                        foreach (string border in country.borders)
                        {
                            Console.WriteLine($" - {border}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{country.name} doesn't border with any other country");
                    }
                }
                else
                {
                    Clear($"Searched for {search}");
                    Console.WriteLine("Country not found! Did you spell it correctly?");
                }

                Console.WriteLine("\n");
            }

            // StackOverflow
            // https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
            // Make ñ into n, ü into u, ç into c etc
            static string Sanitize(string text)
            {
                string normalizedString = text.Normalize(NormalizationForm.FormD);
                StringBuilder stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
            }
        }

        public class HighScore
        {
            private string _name;
            private int _points;
            private string _difficulty;

            public HighScore(string name, int points, string difficulty = "")
            {
                _name = name;
                _points = points;
                _difficulty = difficulty;
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            public int Points
            {
                get { return _points; }
                set { _points = value; }
            }

            public string Difficulty
            {
                get { return _difficulty; }
                set { _difficulty = value; }
            }

            public string GetScore()
            {
                if (_difficulty == "")
                {
                    return $"{_name}\n{_points}";
                }

                return $"{_name}\n{_points} - {_difficulty}";
            }
        }
    }
}