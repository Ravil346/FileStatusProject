using System.Diagnostics;
using GenerateFile;
namespace FileStatusProject;


class Program
{
    // static async Task Main()
    // {
    //     int progress = 0;
    //     
    //     Console.WriteLine();
    //     var progressTask = Task.Run(() => DrawStatusBar(() => progress, 0, 2, ConsoleColor.Red, "Прогресс чтения", 1000));
    //     var progressTask2 = Task.Run(() => DrawStatusBar(() => progress, 0, 0, ConsoleColor.Yellow, "Прогресс чтения", 1000));
    //     await ReadFileAsync("numbers.csv", p => progress = p);
    //
    //     await progressTask; // Дожидаемся завершения прогресс-бара
    //
    //     Console.SetCursorPosition(0, 4);
    //     Console.WriteLine("\nЗавершено!");
    // }
    //
    // static async Task DrawStatusBar(Func<int> getProgress, int left, int top, ConsoleColor color, string label, int delay = 200)
    // {
    //     Console.CursorVisible = false;
    //     
    //     const int totalBlocks = 20;
    //     string[] states = { "░", "▒", "▓", "█" };
    //     int total = 100;
    //     int lastProgress = -1;
    //
    //     Console.SetCursorPosition(left, top);
    //     Console.Write(label); // Заголовок пишем один раз
    //
    //     var startTime = DateTime.UtcNow;
    //
    //     while (true)
    //     {
    //         int progress = getProgress();
    //         
    //         if (progress >= total) break; // Выход, если 100%
    //
    //         if (progress != lastProgress) // Только если прогресс изменился
    //         {
    //             lock (Console.Out)
    //             {
    //                 Console.SetCursorPosition(left, top + 1);
    //                 Console.Write("\r[ ");
    //
    //                 for (int j = 0; j < totalBlocks; j++)
    //                 {
    //                     int index = Math.Min((progress * totalBlocks * 4) / total - j * 4, 3);
    //                     if (index >= 0)
    //                     {
    //                         Console.ForegroundColor = color;
    //                         Console.Write(states[index] + " ");
    //                         Console.ResetColor();
    //                     }
    //                     else
    //                     {
    //                         Console.Write("  ");
    //                     }
    //                 }
    //
    //                 var elapsed = (float)(DateTime.UtcNow - startTime).TotalMilliseconds / 1000;
    //                 Console.Write($"] {progress}%   {elapsed:F2} с");
    //             }
    //
    //             lastProgress = progress;
    //         }
    //
    //         await Task.Delay(delay);
    //     }
    // }
    //
    // static async Task ReadFileAsync(string filePath, Action<int> reportProgress)
    // {
    //     long fileSize = new FileInfo(filePath).Length;
    //     long bytesRead = 0;
    //     
    //     using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    //     using (StreamReader sr = new StreamReader(fs))
    //     {
    //         string line;
    //         while ((line = await sr.ReadLineAsync()) != null)
    //         {
    //             bytesRead += line.Length + Environment.NewLine.Length;
    //             int percent = (int)((double)bytesRead / fileSize * 100);
    //             
    //             //if (percent % 1 == 0) // Чтобы не было слишком частого обновления
    //                 reportProgress(percent);
    //         }
    //     }
    // }
    //
    //
    
    //
    // static void UpdateProgress(int readCount, int sortedCount)
    // {
    //     Console.SetCursorPosition(0, 0);
    //     
    //     Console.WriteLine($"Прочитано строк: {readCount}");
    //     Console.WriteLine($"Отсортировано строк: {sortedCount}");
    //     
    // }
    //
    //
    // static void GenFile()
    // {
    //     Console.ForegroundColor = ConsoleColor.Yellow;
    //     Console.WriteLine("Генерация csv-файла:");
    //     
    //     const int totalBlocks = 20;
    //     string[] states = { "░", "▒", "▓", "█" }; // Градиент заполненности
    //
    //     int count;
    //     int progress = 0;
    //
    //     GenerateFile.GenerateFile.GenerateFileNum((current, total) =>
    //     {
    //         progress = (current * totalBlocks * 4) / total;
    //
    //         Console.Write("\r[ ");
    //         for (int j = 0; j < totalBlocks; j++)
    //         {
    //             int index = Math.Min(progress - j * 4, 3); // Вычисляем текущий уровень заполненности блока
    //             Console.Write(index >= 0 ? states[index] + " " : "  "); // Если индекс отрицательный – блок пустой
    //         }
    //
    //         Console.Write($"] {current * 100 / total}%");
    //     }, out count);
    // }

    static DateTime startTime = DateTime.UtcNow; // Засекаем стартовое время
    static async Task Main()
    {
        
        Console.Clear();
        
        //var task1 = DrawStatusBar(()=> progress,0, 0, ConsoleColor.Yellow, "Прогресс сортировки");
        Console.WriteLine("Прогресс чтения");
        var progressRead = new Progress<int>(p => DrawStatusBar(0, 1, ConsoleColor.Red, p));
        
        Console.WriteLine("\nПрогресс сортировки");
        var progressSort = new Progress<int>(p => DrawStatusBar(0, 3, ConsoleColor.Yellow, p));

        // Читаем файл, обновляя `progress` через лямбда-выражение
        await ReadFileAsync("numbers.csv", progressRead, progressSort);
        
        
        
        // string filePath = "numbers.csv";
        // //GenFile();
        // await ReadFileAsync(filePath);
        Console.SetCursorPosition(0, 4);
        Console.WriteLine("\nЗавершено!");
    }

    static void DrawStatusBar(int left, int top, ConsoleColor color, int progress)
    {
        Console.CursorVisible = false; // отключение курсора
        const int totalBlocks = 20; // количество квадратиков
        string[] states = { "░", "▒", "▓", "█" }; // символы отображения статуса
        int total = 100; // общее количество единиц
        
        //progress = (i * totalBlocks * 4) / total;
        lock (Console.Out)
        {
            
            Console.SetCursorPosition(left, top);
            
            Console.Write("\r[ "); // переходим в начало строки
            for (int j = 0; j < totalBlocks; j++)
            {
                int index = Math.Min((progress * totalBlocks * 4) / total - j * 4,
                    3); // Вычисляем текущий уровень заполненности блока
                if (index >= 0)
                {
                    Console.ForegroundColor = color; // Устанавливаем цвет перед выводом символа
                    Console.Write(states[index] + " ");
                    Console.ResetColor(); // Сбрасываем цвет обратно, чтобы текст не окрашивался
                }
                else
                {
                    Console.Write("  ");
                }
            }

            var elapsed = (float)(DateTime.UtcNow - startTime).TotalMilliseconds / 1000;

            //Console.Write($"] {i * 100 / total}%");
            Console.Write($"] {progress}%");
            Console.SetCursorPosition(left + totalBlocks * 2 + 10, top);
            Console.Write(new string(' ', 10));
            Console.Write($"{elapsed:F2} с");
        }
    }

    static async Task ReadFileAsync(string filePath, IProgress<int> progress, IProgress<int> progressSort)
    {
        long fileSize = new FileInfo(filePath).Length;
        long bytesRead = 0;
        
        using (FileStream fs = new FileStream(filePath,FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new StreamReader(fs))
        {
            List<Task> sortingTasks = new List<Task>(); // Храним задачи сортировки
            
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                bytesRead += line.Length + Environment.NewLine.Length;
                int percent = (int)((double)bytesRead / fileSize * 100);
                progress.Report(percent);
                
                // Запускаем сортировку в фоновом потоке
                var sortTask = SortDigitAsync(fileSize, line, progressSort);
                sortingTasks.Add(sortTask);
            }
            
            // Дожидаемся завершения всех сортировок
            await Task.WhenAll(sortingTasks);
        }
    }
    
    static async Task SortDigitAsync(long fileSize, string line, IProgress<int> progressSort)
    {
        long bytesRead = 0;
        
        await Task.Run( () =>
        {
            int[] exArray = new int[line.Length];
            for (int i = 0; i < line.Length; i++)
            {
                if (char.IsDigit(line[i]))
                {
                    exArray[i] = int.Parse(line[i].ToString());
                }
            }
            Array.Sort(exArray);
        });
        
        bytesRead += line.Length + Environment.NewLine.Length;
        int percent = (int)((double)bytesRead / fileSize * 100);
        progressSort.Report(percent);
    }

}