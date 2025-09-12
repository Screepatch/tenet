using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Программа разделения изображения на клетки ===");
            Console.WriteLine();

            try
            {
                // Запрос пути к файлу
                string imagePath = GetImagePath();
                
                // Проверка существования файла
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine("Ошибка: Файл не найден!");
                    Console.ReadKey();
                    return;
                }

                // Проверка формата файла
                if (!IsValidImageFormat(imagePath))
                {
                    Console.WriteLine("Ошибка: Неподдерживаемый формат файла! Поддерживаются: PNG, JPG, JPEG, BMP");
                    Console.ReadKey();
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
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Ошибка: Недостаточно памяти для обработки изображения или неверный формат файла!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// <summary>
        /// Запрашивает у пользователя путь к изображению
        /// </summary>
        static string GetImagePath()
        {
            string path;
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
            string extension = Path.GetExtension(imagePath).ToLower();
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
                string input = Console.ReadLine();
                
                if (int.TryParse(input, out value) && value > 0)
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
            string directory = Path.GetDirectoryName(imagePath);
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
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
    }
}
