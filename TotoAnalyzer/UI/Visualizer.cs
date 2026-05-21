using System;
using System.Collections.Generic;
using System.Linq;

namespace TotoAnalyzer.UI
{
    public class Visualizer
    {
        public void DrawBarChart(Dictionary<int, int> topNumbers, string title)
        {
            Console.WriteLine($"\n--- {title} ---");
            if (topNumbers == null || !topNumbers.Any())
            {
                Console.WriteLine("Няма данни за визуализация.");
                return;
            }

            int maxCount = topNumbers.Values.Max();
            int maxBarLength = 40; 

            Console.WriteLine(" Топ | Числа");
            Console.WriteLine("-----+--------------------------------------------------");

            foreach (var kvp in topNumbers)
            {
                int number = kvp.Key;
                int count = kvp.Value;

                int barLength = (int)Math.Round((double)count / maxCount * maxBarLength);
                string bar = new string('#', barLength);

                Console.WriteLine($" {number,3} | {bar} {count}");
            }
            Console.WriteLine("--------------------------------------------------------");
        }

        public void DrawHeatMap(Dictionary<int, int> allFrequencies)
        {
            Console.WriteLine("\n--- Топлинна карта 7x7 (Честота на числата) ---");

            var fullFrequencies = Enumerable.Range(1, 49)
                .ToDictionary(n => n, n => allFrequencies.ContainsKey(n) ? allFrequencies[n] : 0);

            var sortedFreqs = fullFrequencies.Values.OrderByDescending(v => v).ToList();

            int top30Index = (int)(49 * 0.30);    
            int bottom30Index = 49 - top30Index; 

            int topThreshold = sortedFreqs[top30Index - 1];
            int bottomThreshold = sortedFreqs[bottom30Index]; 

            for (int i = 1; i <= 49; i++)
            {
                int freq = fullFrequencies[i];

                if (freq >= topThreshold)
                    Console.ForegroundColor = ConsoleColor.Red;      
                else if (freq <= bottomThreshold)
                    Console.ForegroundColor = ConsoleColor.Cyan;     
                else
                    Console.ForegroundColor = ConsoleColor.Yellow;   

                Console.Write($"{i,3} ");

                if (i % 7 == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(); 
                }
            }

            Console.ResetColor();
            Console.WriteLine("Легенда: [Червено] - Горещи (Топ 30%), [Жълто] - Неутрални, [Синьо] - Студени (Долни 30%)");
            Console.WriteLine("--------------------------------------------------------");
        }
    }
}