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
            Console.WriteLine("=== Image Processing Program ===");
            Console.WriteLine();

            try
            {
                // Выбор режима работы
                int mode = GetWorkMode();
                
                if (mode == 1)
                {
                    // Image splitting mode
                    SplitMode();
                }
                else
                {
                    // Frame merging mode
                    MergeMode();
                }
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Error: Insufficient memory to process the image or invalid file format!");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Error: No access to file or folder!");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Error: Specified folder not found!");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: Specified file not found!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Запрашивает у пользователя выбор режима работы
        /// </summary>
        static int GetWorkMode()
        {
            Console.WriteLine("Choose operation mode:");
            Console.WriteLine("1 - Split image into frames");
            Console.WriteLine("2 - Merge frames into one image");
            Console.WriteLine();
            
            int mode;
            do
            {
                Console.Write("Enter mode number (1 or 2): ");
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out mode) && (mode == 1 || mode == 2))
                {
                    return mode;
                }
                
                Console.WriteLine("Please enter 1 or 2!");
            } while (true);
        }

        /// <summary>
        /// Режим разделения изображения на кадры
        /// </summary>
        static void SplitMode()
        {
            Console.WriteLine("\n=== Image Splitting Mode ===");
            
                // Request file path
                string imagePath = GetImagePath();
                
                // Check file existence
                if (!File.Exists(imagePath))
                {
                Console.WriteLine("Error: File not found!");
                    return;
                }

                // Check file format
                if (!IsValidImageFormat(imagePath))
                {
                Console.WriteLine("Error: Unsupported file format! Supported formats: PNG, JPG, JPEG, BMP");
                    return;
                }

                // Load image
                using (Image originalImage = Image.FromFile(imagePath))
                {
                Console.WriteLine($"Image loaded: {originalImage.Width}x{originalImage.Height} pixels");
                    
                    // Request splitting parameters
                int rows = GetPositiveInteger("Enter number of rows: ");
                int columns = GetPositiveInteger("Enter number of columns: ");
                    
                    // Create output folder
                    string outputDirectory = CreateOutputDirectory(imagePath);
                    
                    // Split image
                    SplitImage(originalImage, rows, columns, outputDirectory);
                    
                Console.WriteLine($"\nImage successfully split into {rows * columns} cells!");
                Console.WriteLine($"Results saved to folder: {outputDirectory}");
            }
        }

        /// <summary>
        /// Режим соединения кадров в одно изображение
        /// </summary>
        static void MergeMode()
        {
            Console.WriteLine("\n=== Frame Merging Mode ===");
            
            // Запрос папки с кадрами
            string framesDirectory = GetFramesDirectory();
            
            // Получение списка файлов изображений
            string[] imageFiles = GetImageFiles(framesDirectory);
            
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("Error: No images found in the specified folder!");
                return;
            }
            
            Console.WriteLine($"Found {imageFiles.Length} images");
            
            // Запрос параметров сетки
            int rows = GetPositiveInteger("Enter number of rows: ");
            int columns = GetPositiveInteger("Enter number of columns: ");
            
            if (rows * columns != imageFiles.Length)
            {
                Console.WriteLine($"Warning: Number of cells ({rows * columns}) does not match number of images ({imageFiles.Length})");
                Console.WriteLine($"The first {Math.Min(rows * columns, imageFiles.Length)} images will be used");
            }
            
            // Запрос пути для сохранения результата
            string outputPath = GetOutputPath();
            
            // Соединение кадров
            MergeImages(imageFiles, rows, columns, outputPath);
            
            Console.WriteLine($"\nFrames successfully merged!");
            Console.WriteLine($"Result saved: {outputPath}");
        }

        /// <summary>
        /// Запрашивает у пользователя путь к изображению
        /// </summary>
        static string GetImagePath()
        {
            string? path;
            do
            {
                Console.Write("Enter path to image file: ");
                path = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Path cannot be empty!");
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
                
                Console.WriteLine("Please enter a positive integer!");
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
            
            Console.WriteLine($"\nSize of each cell: {cellWidth}x{cellHeight} pixels");
            Console.WriteLine("Processing...");
            
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
                        
                        Console.Write($"\rProcessed cells: {cellNumber}/{rows * columns}");
                        cellNumber++;
                    }
                }
            }
            
            Console.WriteLine(); // New line after progress
        }

        /// <summary>
        /// Запрашивает у пользователя путь к папке с кадрами
        /// </summary>
        static string GetFramesDirectory()
        {
            string? path;
            do
            {
                Console.Write("Enter path to frames folder: ");
                path = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Path cannot be empty!");
                    continue;
                }
                
                // Убираем кавычки, если они есть
                if (path.StartsWith("\"") && path.EndsWith("\""))
                {
                    path = path.Substring(1, path.Length - 2);
                }
                
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Folder not found!");
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
                Console.WriteLine($"Error: No access to folder {directory}");
                return Array.Empty<string>();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Error: Folder {directory} not found");
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
                Console.Write("Enter path to save result (with .png extension): ");
                path = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Path cannot be empty!");
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
                    Console.WriteLine("Folder does not exist!");
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
                            Console.WriteLine("Transparency detected in images - will be preserved in result");
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
                                            Console.WriteLine("Transparency detected in images - will be preserved in result");
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
                        Console.WriteLine("Error: Invalid dimensions of the first image!");
                        return;
                    }

                    if (rows <= 0 || columns <= 0)
                    {
                        Console.WriteLine("Error: Invalid number of rows or columns!");
                        return;
                    }

                    Console.WriteLine($"Size of each cell: {cellWidth}x{cellHeight} pixels");

                    // Создаем итоговое изображение
                    long totalWidthLong = (long)cellWidth * columns;
                    long totalHeightLong = (long)cellHeight * rows;

                    // Проверяем на переполнение и разумные ограничения
                    if (totalWidthLong > int.MaxValue || totalHeightLong > int.MaxValue)
                    {
                        Console.WriteLine("Error: Final image size is too large!");
                        return;
                    }

                    if (totalWidthLong > 32767 || totalHeightLong > 32767)
                    {
                        Console.WriteLine("Warning: Very large image may cause memory issues!");
                    }

                    int totalWidth = (int)totalWidthLong;
                    int totalHeight = (int)totalHeightLong;

                    Console.WriteLine($"Final image size: {totalWidth}x{totalHeight} pixels");
                    Console.WriteLine("Processing...");

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

                                            Console.Write($"\rProcessed images: {imageIndex + 1}/{totalCells}");
                                        }
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        Console.WriteLine($"\nError: Insufficient memory to process file {currentFile}");
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
                                        Console.WriteLine($"\nError: File {currentFile} not found");
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
                                        Console.WriteLine($"\nError processing file {currentFile}: {ex.Message}");
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

                            Console.WriteLine(); // New line after progress
                        }

                        // Сохраняем результат
                        resultBitmap.Save(outputPath, ImageFormat.Png);
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Error: Insufficient memory to create final image!");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: First file {imageFiles[0]} not found!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating final image: {ex.Message}");
            }
        }
    }
}
