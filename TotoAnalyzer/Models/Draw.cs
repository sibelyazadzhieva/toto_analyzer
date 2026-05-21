using System;
using System.Collections.Generic;

namespace TotoAnalyzer.Models
{
    public class Draw
    {
        public int Year { get; set; }
        public int DrawNumber { get; set; }
        public List<int> Numbers { get; set; } = new List<int>();

        public override string ToString()
        {
            return $"Година: {Year}, Тираж: {DrawNumber}, Числа: {string.Join(", ", Numbers)}";
        }
    }
}