# ImageSplitter - Image Processing Program.

A console C# application for splitting and merging image files with configurable rows and columns.

## Features

### Image Splitting
- Support for formats: PNG, JPG, JPEG, BMP
- Split images into specified number of rows and columns
- Automatic numbering of saved cells
- Handle remainders for uneven divisions
- Input validation and error handling
- Create separate output folder with timestamp

### Image Merging
- Merge multiple frames into a single image
- Configurable grid layout (rows × columns)
- Automatic transparency detection and preservation
- Alpha channel support for PNG images
- Sequential frame arrangement
- Memory-efficient processing

## Installation

1. Ensure you have .NET 6.0 or newer installed
2. Clone the repository or download the source code
3. Open command prompt in the project folder
4. Run the command:
   ```
   dotnet build
   ```

## Usage

1. Start the program:
   ```
   dotnet run
   ```

2. Choose operation mode:
   - **Mode 1**: Split image into frames
   - **Mode 2**: Merge frames into one image

### Mode 1: Image Splitting

1. Enter path to the image file (you can drag and drop the file into console)
2. Specify number of rows for splitting
3. Specify number of columns for splitting
4. Program creates a results folder and saves all cells

#### Example:
```
=== Image Processing Program ===

Choose operation mode:
1 - Split image into frames
2 - Merge frames into one image

Enter mode number (1 or 2): 1

=== Image Splitting Mode ===

Enter path to image file: C:\images\photo.jpg
Image loaded: 1200x800 pixels
Enter number of rows: 3
Enter number of columns: 4

Size of each cell: 300x266 pixels
Processing...
Processed cells: 12/12

Image successfully split into 12 cells!
Results saved to folder: C:\images\photo_split_20250912_143022
```

### Mode 2: Frame Merging

1. Enter path to folder containing frame images
2. Specify number of rows for the grid
3. Specify number of columns for the grid
4. Enter output path for the merged image
5. Program combines all frames into a single image

#### Example:
```
=== Frame Merging Mode ===

Enter path to frames folder: C:\images\frames
Found 12 images
Enter number of rows: 3
Enter number of columns: 4
Enter path to save result (with .png extension): C:\images\merged.png

Size of each cell: 300x266 pixels
Final image size: 1200x798 pixels
Transparency detected in images - will be preserved in result
Processing...
Processed images: 12/12

Frames successfully merged!
Result saved: C:\images\merged.png
```

## Dependencies

- .NET 6.0 or newer
- System.Drawing.Common (automatically installed via NuGet)

## Output File Structure

### Split Mode
Cells are saved in PNG format with names:
- `cell_001_row01_col01.png` - first cell (top-left corner)
- `cell_002_row01_col02.png` - second cell (first row, second column)
- etc.

### Merge Mode
- Single PNG file with all frames combined
- Preserves transparency if detected in source images
- Uses RGB format for opaque images, ARGB for transparent images

## Error Handling

The program handles the following situations:
- Non-existent files or folders
- Unsupported image formats
- Insufficient memory
- Invalid input data (negative numbers, non-numeric values)
- Access permission issues
- Corrupted image files
- Memory overflow for very large images

## Advanced Features

### Transparency Support
- Automatic detection of alpha channels in source images
- Preservation of transparency in merged results
- Optimized pixel formats (RGB vs ARGB)
- High-quality compositing for transparent images

### Memory Management
- Efficient resource disposal using `using` statements
- Protection against memory overflow
- Warnings for very large images
- Graceful handling of memory-related errors

### Robust Processing
- Continues processing even if individual frames fail
- Fills empty areas for missing or corrupted frames
- Detailed error messages with file-specific information
- Progress tracking for long operations