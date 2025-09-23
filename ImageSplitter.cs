using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Программа работы с изображениями ===");
            Console.WriteLine();

            try
            {
                // Выбор режима работы
                int mode = GetWorkMode();
                
                if (mode == 1)
                {
                    // Режим разделения изображения
                    SplitMode();
                }
                else
                {
                    // Режим соединения кадров
                    MergeMode();
                }
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Ошибка: Недостаточно памяти для обработки изображения или неверный формат файла!");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Ошибка: Нет доступа к файлу или папке!");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Ошибка: Указанная папка не найдена!");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка: Указанный файл не найден!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// <summary>
        /// Запрашивает у пользователя выбор режима работы
        /// </summary>
        static int GetWorkMode()
        {
            Console.WriteLine("Выберите режим работы:");
            Console.WriteLine("1 - Разделить изображение на кадры");
            Console.WriteLine("2 - Соединить кадры в одно изображение");
            Console.WriteLine();
            
            int mode;
            do
            {
                Console.Write("Введите номер режима (1 или 2): ");
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out mode) && (mode == 1 || mode == 2))
                {
                    return mode;
                }
                
                Console.WriteLine("Пожалуйста, введите 1 или 2!");
            } while (true);
        }

        /// <summary>
        /// Режим разделения изображения на кадры
        /// </summary>
        static void SplitMode()
        {
            Console.WriteLine("\n=== Режим разделения изображения ===");
            
            // Запрос пути к файлу
            string imagePath = GetImagePath();
            
            // Проверка существования файла
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("Ошибка: Файл не найден!");
                return;
            }

            // Проверка формата файла
            if (!IsValidImageFormat(imagePath))
            {
                Console.WriteLine("Ошибка: Неподдерживаемый формат файла! Поддерживаются: PNG, JPG, JPEG, BMP");
                return;
            }

            // Загрузка изображения
            using (Image originalImage = Image.FromFile(imagePath))
            {
                Console.WriteLine($"Изображение загружено: {originalImage.Width}x{originalImage.Height} пикселей");
                
                // Запрос параметров разделения
                int rows = GetPositiveInteger("Введите количество строк: ");
                int columns = GetPositiveInteger("Введите количество столбцов: ");
                
                // Создание папки для результатов
                string outputDirectory = CreateOutputDirectory(imagePath);
                
                // Разделение изображения
                SplitImage(originalImage, rows, columns, outputDirectory);
                
                Console.WriteLine($"\nИзображение успешно разделено на {rows * columns} клеток!");
                Console.WriteLine($"Результаты сохранены в папке: {outputDirectory}");
            }
        }

        /// <summary>
        /// Режим соединения кадров в одно изображение
        /// </summary>
        static void MergeMode()
        {
            Console.WriteLine("\n=== Режим соединения кадров ===");
            
            // Запрос папки с кадрами
            string framesDirectory = GetFramesDirectory();
            
            // Получение списка файлов изображений
            string[] imageFiles = GetImageFiles(framesDirectory);
            
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("Ошибка: В указанной папке не найдено изображений!");
                return;
            }
            
            Console.WriteLine($"Найдено {imageFiles.Length} изображений");
            
            // Запрос параметров сетки
            int rows = GetPositiveInteger("Введите количество строк: ");
            int columns = GetPositiveInteger("Введите количество столбцов: ");
            
            if (rows * columns != imageFiles.Length)
            {
                Console.WriteLine($"Предупреждение: Количество ячеек ({rows * columns}) не соответствует количеству изображений ({imageFiles.Length})");
                Console.WriteLine($"Будут использованы первые {Math.Min(rows * columns, imageFiles.Length)} изображений");
            }
            
            // Запрос пути для сохранения результата
            string outputPath = GetOutputPath();
            
            // Соединение кадров
            MergeImages(imageFiles, rows, columns, outputPath);
            
            Console.WriteLine($"\nКадры успешно соединены!");
            Console.WriteLine($"Результат сохранен: {outputPath}");
        }

        /// <summary>
        /// Запрашивает у пользователя путь к изображению
        /// </summary>
        static string GetImagePath()
        {
            string? path;
            do
            {
                Console.Write("Введите путь к графическому файлу: ");
                path = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Путь не может быть пустым!");
                    continue;
                }
                
                // Убираем кавычки, если они есть
                if (path.StartsWith("\"") && path.EndsWith("\""))
                {
                    path = path.Substring(1, path.Length - 2);
                }
                
                break;
            } while (true);
            
            return path;
        }

        /// <summary>
        /// Проверяет, является ли файл поддерживаемым форматом изображения
        /// </summary>
        static bool IsValidImageFormat(string imagePath)
        {
            string extension = Path.GetExtension(imagePath).ToLowerInvariant();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp";
        }

        /// <summary>
        /// Запрашивает положительное целое число у пользователя
        /// </summary>
        static int GetPositiveInteger(string prompt)
        {
            int value;
            do
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out value) && value > 0)
                {
                    return value;
                }
                
                Console.WriteLine("Пожалуйста, введите положительное целое число!");
            } while (true);
        }

        /// <summary>
        /// Создает папку для сохранения результатов
        /// </summary>
        static string CreateOutputDirectory(string imagePath)
        {
            string? directory = Path.GetDirectoryName(imagePath);
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            
            // Если директория null, используем текущую папку
            if (string.IsNullOrEmpty(directory))
            {
                directory = Environment.CurrentDirectory;
            }
            
            string outputDir = Path.Combine(directory, $"{fileName}_split_{DateTime.Now:yyyyMMdd_HHmmss}");
            
            Directory.CreateDirectory(outputDir);
            return outputDir;
        }

        /// <summary>
        /// Разделяет изображение на клетки и сохраняет их
        /// </summary>
        static void SplitImage(Image originalImage, int rows, int columns, string outputDirectory)
        {
            int cellWidth = originalImage.Width / columns;
            int cellHeight = originalImage.Height / rows;
            
            Console.WriteLine($"\nРазмер каждой клетки: {cellWidth}x{cellHeight} пикселей");
            Console.WriteLine("Обработка...");
            
            int cellNumber = 1;
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // Вычисляем координаты и размеры текущей клетки
                    int x = col * cellWidth;
                    int y = row * cellHeight;
                    
                    // Для последней колонки и строки учитываем остаток
                    int currentCellWidth = (col == columns - 1) ? originalImage.Width - x : cellWidth;
                    int currentCellHeight = (row == rows - 1) ? originalImage.Height - y : cellHeight;
                    
                    // Создаем область для обрезки
                    Rectangle cropArea = new Rectangle(x, y, currentCellWidth, currentCellHeight);
                    
                    // Создаем новое изображение для клетки
                    using (Bitmap cellBitmap = new Bitmap(currentCellWidth, currentCellHeight))
                    {
                        using (Graphics graphics = Graphics.FromImage(cellBitmap))
                        {
                            // Копируем часть исходного изображения
                            graphics.DrawImage(originalImage, 
                                new Rectangle(0, 0, currentCellWidth, currentCellHeight),
                                cropArea,
                                GraphicsUnit.Pixel);
                        }
                        
                        // Формируем имя файла с нумерацией
                        string fileName = $"cell_{cellNumber:D3}_row{row + 1:D2}_col{col + 1:D2}.png";
                        string filePath = Path.Combine(outputDirectory, fileName);
                        
                        // Сохраняем клетку
                        cellBitmap.Save(filePath, ImageFormat.Png);
                        
                        Console.Write($"\rОбработано клеток: {cellNumber}/{rows * columns}");
                        cellNumber++;
                    }
                }
            }
            
            Console.WriteLine(); // Новая строка после прогресса
        }

        /// <summary>
        /// Запрашивает у пользователя путь к папке с кадрами
        /// </summary>
        static string GetFramesDirectory()
        {
            string? path;
            do
            {
                Console.Write("Введите путь к папке с кадрами: ");
                path = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Путь не может быть пустым!");
                    continue;
                }
                
                // Убираем кавычки, если они есть
                if (path.StartsWith("\"") && path.EndsWith("\""))
                {
                    path = path.Substring(1, path.Length - 2);
                }
                
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Папка не найдена!");
                    continue;
                }
                
                break;
            } while (true);
            
            return path;
        }

        /// <summary>
        /// Получает массив файлов изображений из указанной папки
        /// </summary>
        static string[] GetImageFiles(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return Array.Empty<string>();
                
            string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
            List<string> imageFiles = new List<string>();
            
            try
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file).ToLowerInvariant();
                    if (imageExtensions.Contains(extension))
                    {
                        imageFiles.Add(file);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Ошибка: Нет доступа к папке {directory}");
                return Array.Empty<string>();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Ошибка: Папка {directory} не найдена");
                return Array.Empty<string>();
            }
            
            // Сортируем файлы по имени для последовательного соединения
            imageFiles.Sort();
            return imageFiles.ToArray();
        }

        /// <summary>
        /// Запрашивает у пользователя путь для сохранения результата
        /// </summary>
        static string GetOutputPath()
        {
            string? path;
            do
            {
                Console.Write("Введите путь для сохранения результата (с расширением .png): ");
                path = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Путь не может быть пустым!");
                    continue;
                }
                
                // Убираем кавычки, если они есть
                if (path.StartsWith("\"") && path.EndsWith("\""))
                {
                    path = path.Substring(1, path.Length - 2);
                }
                
                // Добавляем расширение .png, если его нет
                if (!path.ToLowerInvariant().EndsWith(".png"))
                {
                    path += ".png";
                }
                
                // Проверяем, что папка существует
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Console.WriteLine("Папка не существует!");
                    continue;
                }
                
                break;
            } while (true);
            
            return path;
        }

        /// <summary>
        /// Проверяет, есть ли в изображениях альфа-канал (прозрачность)
        /// </summary>
        static bool CheckForAlphaChannel(string[] imageFiles)
        {
            foreach (string file in imageFiles.Take(5)) // Проверяем первые 5 файлов для оптимизации
            {
                try
                {
                    using (Image image = Image.FromFile(file))
                    {
                        // Проверяем формат пикселей на наличие альфа-канала
                        PixelFormat format = image.PixelFormat;
                        if (format == PixelFormat.Format32bppArgb || 
                            format == PixelFormat.Format32bppPArgb ||
                            format == PixelFormat.Format16bppArgb1555 ||
                            Image.IsAlphaPixelFormat(format))
                        {
                            Console.WriteLine("Обнаружена прозрачность в изображениях - будет сохранена в результате");
                            return true;
                        }
                        
                        // Дополнительная проверка для PNG файлов
                        if (Path.GetExtension(file).ToLowerInvariant() == ".png")
                        {
                            using (Bitmap bitmap = new Bitmap(image))
                            {
                                // Проверяем несколько пикселей на прозрачность
                                for (int x = 0; x < Math.Min(bitmap.Width, 10); x++)
                                {
                                    for (int y = 0; y < Math.Min(bitmap.Height, 10); y++)
                                    {
                                        Color pixel = bitmap.GetPixel(x, y);
                                        if (pixel.A < 255)
                                        {
                                            Console.WriteLine("Обнаружена прозрачность в изображениях - будет сохранена в результате");
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибки при проверке отдельных файлов
                    continue;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Соединяет массив изображений в одно изображение согласно указанной сетке
        /// </summary>
        static void MergeImages(string[] imageFiles, int rows, int columns, string outputPath)
        {
            if (imageFiles.Length == 0)
                return;
                
            // Загружаем первое изображение, чтобы определить размеры ячейки
            try
            {
                using (Image firstImage = Image.FromFile(imageFiles[0]))
            {
                int cellWidth = firstImage.Width;
                int cellHeight = firstImage.Height;
                
                // Проверяем корректность размеров
                if (cellWidth <= 0 || cellHeight <= 0)
                {
                    Console.WriteLine("Ошибка: Некорректные размеры первого изображения!");
                    return;
                }
                
                if (rows <= 0 || columns <= 0)
                {
                    Console.WriteLine("Ошибка: Некорректное количество строк или столбцов!");
                    return;
                }
                
                Console.WriteLine($"Размер каждой ячейки: {cellWidth}x{cellHeight} пикселей");
                
                // Создаем итоговое изображение
                long totalWidthLong = (long)cellWidth * columns;
                long totalHeightLong = (long)cellHeight * rows;
                
                // Проверяем на переполнение и разумные ограничения
                if (totalWidthLong > int.MaxValue || totalHeightLong > int.MaxValue)
                {
                    Console.WriteLine("Ошибка: Размер итогового изображения слишком большой!");
                    return;
                }
                
                if (totalWidthLong > 32767 || totalHeightLong > 32767)
                {
                    Console.WriteLine("Предупреждение: Очень большое изображение может вызвать проблемы с памятью!");
                }
                
                int totalWidth = (int)totalWidthLong;
                int totalHeight = (int)totalHeightLong;
                
                Console.WriteLine($"Размер итогового изображения: {totalWidth}x{totalHeight} пикселей");
                Console.WriteLine("Обработка...");
                
                // Определяем, нужна ли поддержка прозрачности
                bool hasAlpha = CheckForAlphaChannel(imageFiles);
                PixelFormat pixelFormat = hasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;
                
                using (Bitmap resultBitmap = new Bitmap(totalWidth, totalHeight, pixelFormat))
                {
                    using (Graphics graphics = Graphics.FromImage(resultBitmap))
                    {
                        // Настройка для правильной обработки альфа-канала
                        if (hasAlpha)
                        {
                            graphics.CompositingMode = CompositingMode.SourceOver;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                        }
                        
                        // Если есть альфа-канал, заполняем прозрачным цветом, иначе белым
                        graphics.Clear(hasAlpha ? Color.Transparent : Color.White);
                        
                        int imageIndex = 0;
                        int totalCells = Math.Min(rows * columns, imageFiles.Length);
                        
                        for (int row = 0; row < rows && imageIndex < imageFiles.Length; row++)
                        {
                            for (int col = 0; col < columns && imageIndex < imageFiles.Length; col++)
                            {
                                string currentFile = imageFiles[imageIndex];
                                try
                                {
                                    using (Image cellImage = Image.FromFile(currentFile))
                                    {
                                        int x = col * cellWidth;
                                        int y = row * cellHeight;
                                        
                                        // Рисуем изображение в соответствующей позиции
                                        graphics.DrawImage(cellImage, x, y, cellWidth, cellHeight);
                                        
                                        Console.Write($"\rОбработано изображений: {imageIndex + 1}/{totalCells}");
                                    }
                                }
                                catch (OutOfMemoryException)
                                {
                                    Console.WriteLine($"\nОшибка: Недостаточно памяти для обработки файла {currentFile}");
                                    // Рисуем пустую область вместо пропуска
                                    using (Brush brush = new SolidBrush(hasAlpha ? Color.Transparent : Color.White))
                                    {
                                        int x = col * cellWidth;
                                        int y = row * cellHeight;
                                        graphics.FillRectangle(brush, x, y, cellWidth, cellHeight);
                                    }
                                }
                                catch (FileNotFoundException)
                                {
                                    Console.WriteLine($"\nОшибка: Файл {currentFile} не найден");
                                    // Рисуем пустую область вместо пропуска
                                    using (Brush brush = new SolidBrush(hasAlpha ? Color.Transparent : Color.White))
                                    {
                                        int x = col * cellWidth;
                                        int y = row * cellHeight;
                                        graphics.FillRectangle(brush, x, y, cellWidth, cellHeight);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"\nОшибка при обработке файла {currentFile}: {ex.Message}");
                                    // Рисуем пустую область вместо пропуска
                                    using (Brush brush = new SolidBrush(hasAlpha ? Color.Transparent : Color.White))
                                    {
                                        int x = col * cellWidth;
                                        int y = row * cellHeight;
                                        graphics.FillRectangle(brush, x, y, cellWidth, cellHeight);
                                    }
                                }
                                finally
                                {
                                    imageIndex++;
                                }
                            }
                        }
                        
                        Console.WriteLine(); // Новая строка после прогресса
                    }
                    
                    // Сохраняем результат
                    resultBitmap.Save(outputPath, ImageFormat.Png);
                }
            }
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Ошибка: Недостаточно памяти для создания итогового изображения!");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Ошибка: Первый файл {imageFiles[0]} не найден!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании итогового изображения: {ex.Message}");
            }
        }
    }
}
