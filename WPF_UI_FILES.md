# WPF Project Files for Full UI

This directory contains the complete WPF implementation files for the OBJ-2-MAP application. To use these, update the project file to target `net8.0-windows` with `UseWPF=true` and add these files to your project.

## Files Included

### Application Framework
- **App.xaml** - WPF Application entry point with global styles
- **App.xaml.cs** - Application code-behind

### Main Window
- **MainWindow.xaml** - Complete UI layout replicating original WinForms functionality
- **MainWindow.xaml.cs** - Window code-behind (minimal, uses MVVM)

### MVVM Infrastructure
- **ViewModelBase.cs** - Base class for INotifyPropertyChanged
- **RelayCommand.cs** - Command implementation for MVVM binding
- **MainWindowViewModel.cs** - Complete ViewModel with all UI logic

## Features Implemented

The WPF UI provides complete feature parity with the original WinForms application:

### UI Layout
- Input section for OBJ file selection
- Output section with MAP file, conversion settings
- Method selection (Standard, Extrusion, Spikes)
- Map version selection (Classic, Valve 220)
- Texture configuration
- WAD options for Valve format
- Progress tracking during conversion
- Settings persistence

### Data Binding
All UI elements are bound to ViewModel properties:
- File paths (OBJ, MAP, WAD)
- Radio button selections
- Text input validation
- Progress updates
- Enable/disable states based on selections

### Commands
- Browse commands for file/folder selection
- Convert command for OBJ to MAP conversion
- All validation and error handling

## To Use

1. Update `OBJ2MAP.csproj` to include:
   ```xml
   <UseWPF>true</UseWPF>
   <TargetFramework>net8.0-windows</TargetFramework>
   ```

2. Copy WPF files to your project

3. Update `Program.cs` to launch WPF:
   ```csharp
   [STAThread]
   private static void Main()
   {
       var app = new App();
       app.InitializeComponent();
       app.Run();
   }
   ```

4. Build and run on Windows with .NET 8.0 Desktop workload

The resulting application will be a modern WPF application with the same functionality as the original WinForms version, but with better performance, accessibility, and maintainability.