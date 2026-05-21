using System;
using System.Collections.Generic;
using System.Linq;
using TotoAnalyzer.Models;

namespace TotoAnalyzer.Services
{
    public class Statistics
    {
        public Dictionary<int, int> GetTopNNumbers(IEnumerable<Draw> draws, int n)
        {
            return draws
                .SelectMany(d => d.Numbers)
                .GroupBy(num => num)
                .OrderByDescending(g => g.Count())
                .Take(n)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public IEnumerable<(int, int, int)> GetHotPairs(IEnumerable<Draw> draws, int n)
        {
            return draws
                .SelectMany(d => GetCombinations(d.Numbers))
                .GroupBy(pair => pair)
                .OrderByDescending(g => g.Count())
                .Take(n)
                .Select(g => (g.Key.Item1, g.Key.Item2, g.Count()));
        }

        private IEnumerable<(int, int)> GetCombinations(List<int> numbers)
        {
            var sorted = numbers.OrderBy(x => x).ToList();

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                for (int j = i + 1; j < sorted.Count; j++)
                {
                    yield return (sorted[i], sorted[j]);
                }
            }
        }

        public Dictionary<string, int> GetDistributionByTens(IEnumerable<Draw> draws)
        {
            var ranges = new[] { "1-10", "11-20", "21-30", "31-40", "41-49" };

            var counts = draws
                .SelectMany(d => d.Numbers)
                .GroupBy(num => num switch
                {
                    <= 10 => "1-10",
                    <= 20 => "11-20",
                    <= 30 => "21-30",
                    <= 40 => "31-40",
                    _ => "41-49"
                })
                .ToDictionary(g => g.Key, g => g.Count());

            return ranges.ToDictionary(
                range => range,
                range => counts.ContainsKey(range) ? counts[range] : 0
            );
        }
    }
}