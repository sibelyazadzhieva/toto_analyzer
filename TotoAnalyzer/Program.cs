using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TotoAnalyzer.Models;
using TotoAnalyzer.Services;
using TotoAnalyzer.UI;

namespace TotoAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var dataLoader = new DataLoader();
            var statistics = new Statistics();
            var visualizer = new Visualizer();

            List<Draw> allDraws = new List<Draw>();
            bool dataLoaded = false;

            int currentStartYear = 1958;
            int currentEndYear = DateTime.Now.Year;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("============================================");
                Console.WriteLine("             ТОТО АНАЛИЗАТОР");
                Console.WriteLine("============================================");
                Console.WriteLine(" [1] Избери период (от година - до година)");
                Console.WriteLine(" [2] Топ N най-чести числа (Bar Chart)");
                Console.WriteLine(" [3] Горещи двойки");
                Console.WriteLine(" [4] Разпределение по десетици");
                Console.WriteLine(" [5] Топлинна карта на всички числа (Heat Map)");
                Console.WriteLine("\n [0] Изход");
                Console.WriteLine("============================================");
                Console.Write(" Избор: ");

                string input = Console.ReadLine();

                if (!int.TryParse(input, out int choice))
                {
                    ShowError("Невалиден вход! Моля, въведете число от менюто.");
                    continue;
                }

                if (choice == 0)
                {
                    Console.WriteLine("Изход от програмата.");
                    break;
                }

                if (choice != 1 && !dataLoaded)
                {
                    ShowError("Моля, първо изтеглете данните чрез Опция [1]!");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Clear();
                        Console.WriteLine("--- ИЗТЕГЛЯНЕ И ОБРАБОТКА НА ДАННИ ---");

                        Console.Write("Въведете начална година: ");
                        if (!int.TryParse(Console.ReadLine(), out currentStartYear) || currentStartYear < 1958 || currentStartYear > DateTime.Now.Year)
                        {
                            ShowError($"Невалидна начална година! Моля, въведете число между 1958 и {DateTime.Now.Year}.");
                            break;
                        }

                        Console.Write("Въведете крайна година: ");
                        if (!int.TryParse(Console.ReadLine(), out currentEndYear) || currentEndYear < currentStartYear || currentEndYear > DateTime.Now.Year)
                        {
                            ShowError($"Невалидна крайна година! Моля, въведете число между {currentStartYear} и {DateTime.Now.Year}.");
                            break;
                        }

                        Console.WriteLine("\nМоля, изчакайте. Файловете се свалят от info.toto.bg...\n");

                        var loadedData = await dataLoader.LoadAllDataAsync();

                        allDraws = loadedData.Where(d => d.Year >= currentStartYear && d.Year <= currentEndYear).ToList();

                        if (allDraws.Any())
                        {
                            dataLoaded = true;
                            Console.WriteLine($"\nУспешно бяха заредени {allDraws.Count} тиража за периода {currentStartYear}-{currentEndYear}!");
                        }
                        else
                        {
                            Console.WriteLine("\nНе бяха намерени или разпознати данни за този период.");
                        }
                        WaitForKey();
                        break;

                    case 2:
                        Console.Clear();
                        Console.Write("Въведете N (колко от най-честите числа да покажа): ");
                        if (int.TryParse(Console.ReadLine(), out int nNumbers) && nNumbers > 0)
                        {
                            Console.WriteLine("\nПоказва най-често изтегляните числа като хоризонтални ленти спрямо абсолютния им брой.");
                            var topNums = statistics.GetTopNNumbers(allDraws, nNumbers);
                            visualizer.DrawBarChart(topNums, $"Топ| числа ({currentStartYear}-{currentEndYear}):");
                            WaitForKey();
                        }
                        else
                        {
                            ShowError("Невалидно число за N.");
                        }
                        break;

                    case 3:
                        Console.Clear();
                        Console.Write("Въведете N (колко горещи двойки да покажа): ");
                        if (int.TryParse(Console.ReadLine(), out int nPairs) && nPairs > 0)
                        {
                            var hotPairs = statistics.GetHotPairs(allDraws, nPairs);
                            Console.WriteLine($"\n--- ТОП {nPairs} ГОРЕЩИ ДВОЙКИ ---");
                            Console.WriteLine("Показва кои две числа се падат най-често заедно.\n");
                            foreach (var pair in hotPairs)
                            {
                                Console.WriteLine($" Двойка ({pair.Item1,2}, {pair.Item2,2}) -> Изтеглени заедно {pair.Item3} пъти");
                            }
                            WaitForKey();
                        }
                        else
                        {
                            ShowError("Невалидно число за N.");
                        }
                        break;

                    case 4:
                        Console.Clear();
                        var distribution = statistics.GetDistributionByTens(allDraws);
                        Console.WriteLine("\n--- РАЗПРЕДЕЛЕНИЕ ПО ДЕСЕТИЦИ ---");
                        Console.WriteLine("Показва общия брой изтеглени числа във всеки диапазон.\n");

                        Console.WriteLine(" Диапазон | Брой изтегляния");
                        Console.WriteLine("----------+-----------------");
                        foreach (var kvp in distribution)
                        {
                            Console.WriteLine($" {kvp.Key,8} | {kvp.Value}");
                        }
                        Console.WriteLine("----------------------------");
                        WaitForKey();
                        break;

                    case 5:
                        Console.Clear();
                        Console.WriteLine("Показва всички числа от 1 до 49 в решетка. Оцветени са според тяхната честота на изтегляне (Топ 30% горещи, 40% неутрални, долни 30% студени).");
                        var allFreqs = statistics.GetTopNNumbers(allDraws, 49);
                        visualizer.DrawHeatMap(allFreqs);
                        WaitForKey();
                        break;

                    default:
                        ShowError("Невалиден избор! Моля, изберете съществуваща опция от менюто.");
                        break;
                }
            }
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nГРЕШКА: {message}");
            Console.ResetColor();
            WaitForKey();
        }

        static void WaitForKey()
        {
            Console.WriteLine("\nНатиснете произволен клавиш за връщане в менюто...");
            Console.ReadKey();
        }
    }
}