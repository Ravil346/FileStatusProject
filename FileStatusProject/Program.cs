using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Channels;
using CsvHelper;
using GenerateFile;
namespace FileStatusProject;


class Program
{
    // Время запуска процессов
    static DateTime startTime = DateTime.UtcNow; 
    // Позиция курсора по строке
    static int posititonTop = 1;
    static async Task Main()
    {
        // Запускаем генерацию файла, если его не существует
        if (!FileExist())
        {
            lock (Console.Out)
            {
                Console.SetCursorPosition(0,0+posititonTop);
            }
            var progressGen = new Progress<int>(p => DrawStatusBar(0, 0 + posititonTop, ConsoleColor.DarkGreen, p ));
            await GenerateFileNum(progressGen);
        }
        await Task.Delay(2000);

        // Обновляем позицию курсора и время
        posititonTop = 4;
        startTime = DateTime.UtcNow;
        
        // Запуск чтения
        Console.SetCursorPosition(0, posititonTop);
        Console.WriteLine("Прогресс чтения");
        var progressRead = new Progress<int>(p => DrawStatusBar(0, 1, ConsoleColor.Red, p ));
        
        // Запуск сортировки
        Console.SetCursorPosition(0,2 + posititonTop);
        Console.WriteLine("Прогресс сортировки");
        var progressSort = new Progress<int>(p => DrawStatusBar(0, 3, ConsoleColor.Yellow, p ));

        // Создаем канал для передачи данных между потоками, структура хранит максимум 10000 строк
        var channel = Channel.CreateBounded<string>(10000);
        
        // Создаем задачи для чтения и сортировки
        var readTask = ReadFileAsync("numbers.csv", channel.Writer, progressRead);
        var sortTask = SortDigitAsync("numbers.csv", channel.Reader, progressSort);

        // Ожидаем выполнение чтения и сортировки
        await readTask;
        channel.Writer.Complete(); // Завершаем канал
        await sortTask;
        
        lock (Console.Out)
        {
            Console.SetCursorPosition(0, 7 + posititonTop);
            Console.WriteLine("Завершено!");
        }
    }

    /// <summary>
    /// Отрисока статус бара
    /// </summary>
    static void DrawStatusBar(int left, int top, ConsoleColor color, int progress)
    {
        Console.CursorVisible = false;
        
        // Создание массива для плавной отрисоки
        // Количество квадратиков
        const int totalBlocks = 20; 
        // Символы отображения статуса
        string[] states = { "░", "▒", "▓", "█" }; 
        // общее количество прогресса
        int total = 100; 
        
        lock (Console.Out)
        {
            Console.SetCursorPosition(left, top + posititonTop);
            Console.Write("\r[ "); 
            for (int j = 0; j < totalBlocks; j++)
            {
                // Вычисляем текущий уровень заполненности блока
                int index = Math.Min((progress * totalBlocks * 4) / total - j * 4, 3); 
                if (index >= 0)
                {
                    Console.ForegroundColor = color; 
                    Console.Write(states[index] + " ");
                    Console.ResetColor(); 
                }
                else
                {
                    Console.Write("  ");
                }
            }

            var elapsed = (float)(DateTime.UtcNow - startTime).TotalMilliseconds / 1000;
            Console.Write($"] {progress}%");
            Console.SetCursorPosition(left + totalBlocks * 2 + 10, top + posititonTop);
            Console.Write(new string(' ', 10));
            Console.Write($"{elapsed:F2} с");
        }
    }

    /// <summary>
    /// Считаем количество строк в файле
    /// </summary>
    static async Task<int> CountLinesAsync(string filePath)
    {
        int count = 0;
        using (var sr = new StreamReader(filePath))
        {
            while (await sr.ReadLineAsync() != null)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Метод чтения файла
    /// </summary>
    /// <param name="progress">Интерфейс для асинхронного уведомления об операциях между потоками</param>
    static async Task ReadFileAsync(string filePath, ChannelWriter<string> writer, IProgress<int> progress)
    {
        long fileSize = new FileInfo(filePath).Length;
        long bytesRead = 0;
        int lineCount = await CountLinesAsync(filePath);
        
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new StreamReader(fs))
        {
            string? line;
            int processedLines = 0;
            
            lock (Console.Out)
            {
                Console.SetCursorPosition(0, 4 + posititonTop);
                Console.Write($"Всего строк в файле: {lineCount}");
            }
            
            
            while ((line = await sr.ReadLineAsync()) != null)
            {
                processedLines++;
                lock (Console.Out)
                {
                    Console.SetCursorPosition(0, 5 + posititonTop);
                    Console.Write($"Прочтено строк: {processedLines}");
                }
                
                // Записываем строку в канал для сортировки
                await writer.WriteAsync(line);
                
                bytesRead += line.Length + Environment.NewLine.Length;
                int percent = (int)((double)bytesRead / fileSize * 100);
                
                // Отправляем прогресс
                progress.Report(percent);
            }
            
            // Сообщаем прогресс 100, если дошли до конца файла
            if (bytesRead == fileSize)
            {
                progress.Report(100);
            }

            lock (Console.Out)
            {
                Console.SetCursorPosition(0, 6 + posititonTop);
                Console.Write("Чтение завершено!");
            }
        }
    }
    
    /// <summary>
    /// Сортировка строк
    /// </summary>
    /// <param name="progress">Интерфейс для асинхронного уведомления об операциях между потоками</param>
    static async Task SortDigitAsync(string filePath ,ChannelReader<string> reader, IProgress<int> progressSort)
    {
        long fileSize = new FileInfo(filePath).Length;
        long bytesRead = 0;
        int processedLines = 0;
        
        // Читаем строки из канала, цикл ждет новых данных
        await foreach (var line in reader.ReadAllAsync())
        {
            processedLines++;
            lock (Console.Out)
            {
                Console.SetCursorPosition(44, 5 + posititonTop);
                Console.Write($"Обработано строк: {processedLines}");
            }

            int[] numbers = line.Where(char.IsDigit).Select(c => c - '0').ToArray();
            Array.Sort(numbers);
            
            bytesRead += line.Length + Environment.NewLine.Length;
            int percent = (int)((double)bytesRead / fileSize * 100);
            
            // Отправляем прогресс
            progressSort.Report(percent);
        }
        
        // Сообщаем прогресс 100, если дошли до конца файла
        if (bytesRead == fileSize)
        {
            progressSort.Report(100);
        }
        lock (Console.Out)
        {
            Console.SetCursorPosition(44, 6 + posititonTop);
            Console.Write("Сортировка завершена!");
        }

    }
    
    /// <summary>
    /// Генерация файла
    /// </summary>
    /// <param name="progress">Интерфейс для асинхронного уведомления об операциях между потоками</param>
    static async Task GenerateFileNum(IProgress<int> progress)
    {
        Random rnd = new Random();
        string filePath = "numbers.csv";
        int count, percent, genLines = 0;
        
        // Создаем и открываем файл для записи
        using (StreamWriter writer = new StreamWriter(filePath, append: false))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            count = rnd.Next(10000, 100000);
            for (int i = 0; i < count; i++)
            {
                // Генерируем строку и записываем строку в файл
                int length = rnd.Next(100, 1001);
                StringBuilder numStr = new StringBuilder();
                numStr.Append(rnd.Next(1, 10));
                for (int j = 1; j < length; j++)
                {
                    numStr.Append(rnd.Next(0, 10)); 
                }
                csv.WriteField(numStr.ToString());
                genLines++;
                
                await csv.NextRecordAsync();
                
                //await writer.FlushAsync();
                // Высчитываем процент генерации файла
                percent = (int)((double)genLines / count * 100);
                
                // Передаем информацию о прогрессе
                progress.Report(percent);
                
                // Разгружаем поток, чтобы интерфейс не зависал
                if (i % 1000 == 0)
                {
                    await Task.Yield();
                }
            }
        }
    }
    
    /// <summary>
    /// Проверка на наличие файла
    /// </summary>
    static bool FileExist()
    {
        // Корневая папка приложения
        string rootPath = AppDomain.CurrentDomain.BaseDirectory; 
        string fileName = "numbers.csv"; 
        string[] files = Directory.GetFiles(rootPath, fileName);

        if (files.Length > 0)
        {
            Console.Write($"Нужный файл {fileName} найден:\n");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
            return true;
        }
        else
        {
            Console.WriteLine($"Нужный файл {fileName} не найден.\nГенерация файла");
            return false;
        }
    }
}