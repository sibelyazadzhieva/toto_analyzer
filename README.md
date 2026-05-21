# TOTO ANALYZER

A .NET console application that downloads historical data from the official Bulgarian Sports Toto website (6/49 draws), analyzes it, and provides interactive statistical visualizations.

## 🚀 Key Features

* **Data Extraction:** Automatically downloads and parses historical `.txt` and `.docx` data, safely bypassing basic server protections by mimicking a real web browser.
* **LINQ Analysis:** Performs all statistical processing (Top N numbers, Hot Pairs, Distributions) entirely through clean and efficient LINQ queries.
* **Interactive UI:** A crash-resistant console menu that allows users to filter data by custom time periods and select different analyses.
* **Console Visualizations:** Renders a normalized ASCII Bar Chart and a color-coded 7x7 Heat Map to display number frequencies.

## 🛠 Technologies & Libraries

* **C# / .NET 8.0**
* `System.Net.Http.HttpClient` (for web scraping and network communication)
* `System.Linq` (for complex data querying and analysis)
* `DocumentFormat.OpenXml` (the only allowed external NuGet package, used for parsing Word `.docx` documents)
* Regular Expressions (`Regex`) for text parsing and data extraction.

## 📊 Project Structure

The project follows a clean architecture, separating responsibilities into distinct classes:
* `Models/Draw.cs`: Represents a single lottery draw (Year, Draw Number, and the 6 winning numbers).
* `Services/DataLoader.cs`: Handles web scraping, file downloading, and parsing both plain text and Word document contents.
* `Services/Statistics.cs`: Contains all LINQ queries (Top N numbers, Hot Pairs, Distribution by tens).
* `UI/Visualizer.cs`: Responsible for rendering the ASCII charts and colored heat maps in the console.
* `Program.cs`: The entry point containing the interactive menu and application flow logic.

## 💻 How to Run the Project

1. Clone this repository to your local machine.
2. Open the project folder in your preferred terminal or Visual Studio.
3. Restore the required NuGet packages by running:
   ```bash
   dotnet restore