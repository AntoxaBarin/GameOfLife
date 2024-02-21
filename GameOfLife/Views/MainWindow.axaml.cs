using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GameOfLife.DB;
using GameOfLife.ViewModels;
using SkiaSharp;

namespace GameOfLife.Views;

public partial class MainWindow : Window
{
    private int cell_size;
    private int rows;
    private int columns;
    private int current_generation;

    private GameField game_field;
    private Gameplay gameplay;

    private Random random;

    private List<int> save_life_neighbours_number;
    private List<int> make_life_neighbours_number;
    
    private Dictionary<int, SKColor> _cellColors;
    private Dictionary<int, ISolidColorBrush> _buttonColors;
    private byte _currentColorButtonState;
    
    private readonly DispatcherTimer timer = new();
    private bool AutosaveField;

    private Database _database; 

    public MainWindow()
    {
        InitializeComponent();

        // Only absolute path to the local database. otherwise it doesn't work!
        _database = new Database("PUT ABSOLUTE PATH TO DATABASE HERE");
        _database.Open();
        _database.Create();

        // To draw cells correct 
        _cellColors = new Dictionary<int, SKColor>
        {
            {1, SKColors.White},
            {2, SKColors.Crimson},
            {3, SKColors.Blue},
            {4, SKColors.Green}
        };

        // For button which changes color of drawing cells
        _buttonColors = new Dictionary<int, ISolidColorBrush>
        {
            {1, Brushes.White},
            {2, Brushes.Crimson},
            {3, Brushes.Blue},
            {4, Brushes.Green}
        };
        _currentColorButtonState = 1;

        save_life_neighbours_number = new List<int>();
        make_life_neighbours_number = new List<int>();
        
        LoadRules();

        timer.Interval = TimeSpan.FromMilliseconds(60);
        timer.Tick += NextGeneration_wrapper;
        cell_size = 10;
        
        rows = (int)image.Height / cell_size;
        columns = (int)image.Width / cell_size;

        game_field = new GameField((int)image.Height, (int)image.Width);     // Game field
        gameplay = new Gameplay(game_field, save_life_neighbours_number, make_life_neighbours_number); // Game actions
        
        RenderImage();
    }

    // Loads user rules in the start
    private void LoadRules()
    {
        using (StreamReader reader = new StreamReader(File.Open("../../../Assets/GameRulesSave.txt", FileMode.Open)))
        {
            int rulesNumber = reader.Read() - 48;
            for (int i = 0; i < rulesNumber; i++)
            {
                make_life_neighbours_number.Add(reader.Read() - 48);
            }

            reader.ReadLine();
            rulesNumber = reader.Read() - 48;
            for (int i = 0; i < rulesNumber; i++)
            {
                save_life_neighbours_number.Add(reader.Read() - 48);
            }
        }

        if (make_life_neighbours_number.Contains(0)) CheckBoxMakeLife0.IsChecked = true;
        if (make_life_neighbours_number.Contains(1)) CheckBoxMakeLife1.IsChecked = true;
        if (make_life_neighbours_number.Contains(2)) CheckBoxMakeLife2.IsChecked = true;
        if (make_life_neighbours_number.Contains(3)) CheckBoxMakeLife3.IsChecked = true;
        if (make_life_neighbours_number.Contains(4)) CheckBoxMakeLife4.IsChecked = true;
        if (make_life_neighbours_number.Contains(5)) CheckBoxMakeLife5.IsChecked = true;
        if (make_life_neighbours_number.Contains(6)) CheckBoxMakeLife6.IsChecked = true;
        if (make_life_neighbours_number.Contains(7)) CheckBoxMakeLife7.IsChecked = true;
        if (make_life_neighbours_number.Contains(8)) CheckBoxMakeLife8.IsChecked = true;
        
        if (save_life_neighbours_number.Contains(0)) CheckBoxSaveLife0.IsChecked = true;
        if (save_life_neighbours_number.Contains(1)) CheckBoxSaveLife1.IsChecked = true;
        if (save_life_neighbours_number.Contains(2)) CheckBoxSaveLife2.IsChecked = true;
        if (save_life_neighbours_number.Contains(3)) CheckBoxSaveLife3.IsChecked = true;
        if (save_life_neighbours_number.Contains(4)) CheckBoxSaveLife4.IsChecked = true;
        if (save_life_neighbours_number.Contains(5)) CheckBoxSaveLife5.IsChecked = true;
        if (save_life_neighbours_number.Contains(6)) CheckBoxSaveLife6.IsChecked = true;
        if (save_life_neighbours_number.Contains(7)) CheckBoxSaveLife7.IsChecked = true;
        if (save_life_neighbours_number.Contains(8)) CheckBoxSaveLife8.IsChecked = true;
    }
    
    // Collect user rules from CheckBoxes
    private void get_player_rules()
    {
        // Delete old custom rules
        if (save_life_neighbours_number.Count != 0) save_life_neighbours_number.Clear(); 
        if (make_life_neighbours_number.Count != 0) make_life_neighbours_number.Clear();
        
        // Get custom rules
        if (CheckBoxSaveLife0.IsChecked == true) save_life_neighbours_number.Add(0);
        if (CheckBoxSaveLife1.IsChecked == true) save_life_neighbours_number.Add(1);
        if (CheckBoxSaveLife2.IsChecked == true) save_life_neighbours_number.Add(2);
        if (CheckBoxSaveLife3.IsChecked == true) save_life_neighbours_number.Add(3);
        if (CheckBoxSaveLife4.IsChecked == true) save_life_neighbours_number.Add(4);
        if (CheckBoxSaveLife5.IsChecked == true) save_life_neighbours_number.Add(5);
        if (CheckBoxSaveLife6.IsChecked == true) save_life_neighbours_number.Add(6);
        if (CheckBoxSaveLife7.IsChecked == true) save_life_neighbours_number.Add(7);
        if (CheckBoxSaveLife8.IsChecked == true) save_life_neighbours_number.Add(8);
        
        if (CheckBoxMakeLife0.IsChecked == true) make_life_neighbours_number.Add(0);
        if (CheckBoxMakeLife1.IsChecked == true) make_life_neighbours_number.Add(1);
        if (CheckBoxMakeLife2.IsChecked == true) make_life_neighbours_number.Add(2);
        if (CheckBoxMakeLife3.IsChecked == true) make_life_neighbours_number.Add(3);
        if (CheckBoxMakeLife4.IsChecked == true) make_life_neighbours_number.Add(4);
        if (CheckBoxMakeLife5.IsChecked == true) make_life_neighbours_number.Add(5);
        if (CheckBoxMakeLife6.IsChecked == true) make_life_neighbours_number.Add(6);
        if (CheckBoxMakeLife7.IsChecked == true) make_life_neighbours_number.Add(7);
        if (CheckBoxMakeLife8.IsChecked == true) make_life_neighbours_number.Add(8);

        // If rules not set by player, we set default rules
        if (save_life_neighbours_number.Count == 0) 
        {
            save_life_neighbours_number.Add(2);
            save_life_neighbours_number.Add(3);
        }

        if (make_life_neighbours_number.Count == 0)
        {
            make_life_neighbours_number.Add(3);
        }
    }
    
    private void NextGeneration_wrapper(object? sender, EventArgs? e) 
    {
        var result = gameplay.NextGeneration();
        if (!result)
        {
            timer.IsEnabled = false;
            ErrorMessageBox.Text = "Equal generations. Stop.";
            return;
        }
        
        RenderImage();
        current_generation += 1;
        GenerationNumber.Text = $"Generation: {current_generation}";
    }
    
    // Drawing eats some memory
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]")]
    public void RenderImage()
    {
        Bitmap result_bm;
        var res_image = new Image();
    
        SKImageInfo imageInfo = new SKImageInfo(1500, 1500);
        using (SKSurface surface = SKSurface.Create(imageInfo))
        {
            SKCanvas canvas = surface.Canvas;

            for (int i = 0; i < 150; i++)
            {
                for (int j = 0; j < 150; j++)
                {
                    if (game_field.get_cell_state(i, j) != 0)
                    {
                        SKPaint paint = new SKPaint();
                        paint.Color = _cellColors[game_field.get_cell_state(i, j)];
                        paint.Style = SKPaintStyle.Fill;
                        canvas.DrawRect(i * cell_size, j * cell_size, cell_size - 1, cell_size - 1, paint);
                    }
                }
            }


            using (SKImage image = surface.Snapshot())
            using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100))

            using (MemoryStream mStream = new MemoryStream(data.ToArray()))
            {
                result_bm = new Bitmap(mStream);
                res_image.Source = result_bm;
            }
        }

        image.Source = result_bm;
    }

    // Validation of mouse click
    private bool IsCorrectClick(long i, long j)
    {
        if (i < 0 || i > rows || j < 0 || j > columns)
        {
            return false;
        }
        return true;
    }
    
    // Mouse click = add/remove cell
    private void GameFieldImage_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(image);
        var i = (int)point.Position.X/cell_size;
        var j = (int)point.Position.Y/cell_size;

        if (point.Properties.IsLeftButtonPressed && IsCorrectClick(i, j))
        {
            game_field.set_cell(i, j, _currentColorButtonState);
        }
        if (point.Properties.IsRightButtonPressed && IsCorrectClick(i, j))
        {
            game_field.set_cell(i, j, 0);
        }
        
        GameMessageBox.Text = $"Cell is alive for {game_field.get_cell_generations(i, j)} gens";
        RenderImage();
    }
    
    private void GameFieldImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(image);
        var i = (int)point.Position.X/cell_size;
        var j = (int)point.Position.Y/cell_size;

        if (point.Properties.IsLeftButtonPressed && IsCorrectClick(i, j))
        {
            game_field.set_cell(i, j, _currentColorButtonState);
        }
        if (point.Properties.IsRightButtonPressed && IsCorrectClick(i, j))
        {
            game_field.set_cell(i, j, 0);
        }
        
        RenderImage();
    }

    private void StartButton_click(object? sender, RoutedEventArgs e)
    {
        if (timer.IsEnabled) 
        {
            return;
        }
        
        ErrorMessageBox.Text = "";

        if (RandomCheckBox.IsChecked == true)
        {
            game_field.random_setup_cell_states();
            RandomCheckBox.IsChecked = false;
        }
        
        get_player_rules();
        gameplay.set_make_life_neighbours_number(make_life_neighbours_number);
        gameplay.set_save_life_neighbours_number(save_life_neighbours_number);
        
        timer.Start();

        NextGeneration_wrapper(null, null);

    }
    
    // Stops the game
    private void StopButton_click(object? sender, RoutedEventArgs e)
    {
        ErrorMessageBox.Text = "";
        timer.IsEnabled = false;
    }
    
    // Clears game field and kills all cells
    private void ResetButton_click(object? sender, RoutedEventArgs e)
    {
        ErrorMessageBox.Text = "";
        timer.IsEnabled = false;
        game_field.setup_cell_states();
        RenderImage();

        current_generation = 0;
        GenerationNumber.Text = "Generation: 0";
    }

    // Makes one generation step
    private void Make_Steps_OnClick(object? sender, RoutedEventArgs e)
    {
        ErrorMessageBox.Text = "";
        timer.IsEnabled = false;
        
        if (current_generation == 0)
        {
            NextGeneration_wrapper(null, null);
            current_generation -= 1;
        }
        
        for (int i = 0; i < (int)StepsNumber.Value; i++)
        {
            NextGeneration_wrapper(null, null);
        }
    }

    private bool basic_figures_click_check()
    {
        if (timer.IsEnabled)
        {
            return false;
        }

        if (RandomCheckBox.IsChecked == true)
        {
            RandomCheckBox.IsChecked = false;
        }

        return true;
    }

    // Now basic figure loads from files
    private async void Glider_click(object? sender, RoutedEventArgs e) 
    {
        ErrorMessageBox.Text = "";
        if (!basic_figures_click_check())
        {
            return;
        }
        
        FileIO reader = new FileIO();
        var glider = await reader.LoadFieldFromLocalFile("../../../Assets/GliderSchema.txt");
        game_field.set_field(glider);
        RenderImage();
    }

    private async void Pulsar_click(object? sender, RoutedEventArgs e)
    {
        ErrorMessageBox.Text = "";
        if (!basic_figures_click_check())
        {
            return;
        }
        
        FileIO reader = new FileIO();
        var pulsar = await reader.LoadFieldFromLocalFile("../../../Assets/PulsarSchema.txt");
        game_field.set_field(pulsar);
        RenderImage();
    }

    private async void GosperGun_click(object? sender, RoutedEventArgs e)
    {
        ErrorMessageBox.Text = "";
        if (!basic_figures_click_check())
        {
            return;
        }

        FileIO reader = new FileIO();
        var gosperGun = await reader.LoadFieldFromLocalFile("../../../Assets/GosperGunSchema.txt");
        game_field.set_field(gosperGun);
        RenderImage();
    }
    
    // File reading/writing
    private async void SaveToFile_OnClick(object? sender, RoutedEventArgs e)
    {
        ErrorMessageBox.Text = "";
        if (timer.IsEnabled)
        {
            timer.IsEnabled = false;
        }

        var fileWriter = new FileIO(game_field);
        var result = await fileWriter.SaveFieldToTextFile();

        if (result == null)
        {
            ErrorMessageBox.Text = "File writing error!";
        }

        if (ImageSave.IsChecked == true)
        {
            var bitmap = new RenderTargetBitmap(new PixelSize((int)image.Width, (int)image.Height), new Vector(96d, 96d));
            image.Measure(new Size((int)image.Width, (int)image.Height));
            image.Arrange(new Rect(new Size((int)image.Width, (int)image.Height)));
            bitmap.Render(image);

            using (var FileStream = File.OpenWrite($"{fileWriter._path.Substring(0, fileWriter._path.Length - 4)}.png"))
            {
                bitmap.Save(FileStream);
            }
        }
    }

    private async void StartFromFile_OnClick(object? sender, RoutedEventArgs eventArgs) 
    {
        GameMessageBox.Text = "";
        if (timer.IsEnabled)
        {
            timer.IsEnabled = false;
        }   

        var fileReader = new FileIO();
        Cell[,] readenField;
        
        
        if (ImageSave.IsChecked == true)
        {
            readenField = await fileReader.ReadFieldFromTextFile(true);    
        }
        else
        {
            readenField = await fileReader.ReadFieldFromTextFile(false);
        }
        

        if (!fileReader.IsFailed && readenField != null)        
        {
            game_field.set_field(readenField);
            RenderImage();
        }
        else 
        {
            ErrorMessageBox.Text = "File reading error";
        }
    }

    // Loads autosaved field from previos game
    private async void AutoLoadField()
    {
        var loader = new FileIO();
        var loadedField = await loader.LoadFieldFromLocalFile("../../../Assets/GameFieldSave.txt");
        
        game_field.set_field(loadedField);
        RenderImage();
    }

    // Makes one "move"
    private void One_Step_OnClick(object? sender, RoutedEventArgs e)
    {
        GameMessageBox.Text = "";
        timer.IsEnabled = false;
        if (current_generation == 0)
        {
            NextGeneration_wrapper(null, null);
            current_generation -= 1;
        }
        NextGeneration_wrapper(null, null);
    }

    // Weird implementation of field scaling
    private void GameFieldImage_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        const double scaleValue = 1.1;
        var image = sender as Image;
        var transform = image.RenderTransform as ScaleTransform;

        if (transform == null)
        {
            image.RenderTransform = new ScaleTransform(scaleValue, scaleValue);
        }
        else
        {
            var zoom = e.Delta.Y > 0 ? scaleValue : 1 / scaleValue;
            transform.ScaleX *= zoom;
            transform.ScaleY *= zoom;
        }
    }

    // Auto saves player's rules and field when window closes
    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        get_player_rules();
        
        using (StreamWriter writer = new StreamWriter("../../../Assets/GameRulesSave.txt", false))
        {
            writer.Write(make_life_neighbours_number.Count);
            foreach (var number in make_life_neighbours_number)
            {
                writer.Write(number);
            }
            writer.WriteLine();
            
            writer.Write(save_life_neighbours_number.Count);
            foreach (var number in save_life_neighbours_number)
            {
                writer.Write(number);
            }
            writer.WriteLine();
        }

        if (AutosaveField)
        {
            using (StreamWriter writer = new StreamWriter(File.Open("../../../Assets/GameFieldSave.txt", FileMode.Open)))
            {
                for (int i = 0; i < game_field.Columns; i++)
                {
                    for (int j = 0; j < game_field.Rows; j++)
                    {
                        writer.Write(game_field.get_cell_state(j, i));
                    }
                    writer.WriteLine();
                }
            }
        }
        _database.Close();
    }
    
    
    private void GameFieldImage_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        GameMessageBox.Text = "";
    }

    private void ChangeColorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _currentColorButtonState += 1;
        if (_currentColorButtonState > 4) _currentColorButtonState %= 4;
        ChangeColorButton.Background = _buttonColors[_currentColorButtonState];
    }

    private void LoadField_OnClick(object? sender, RoutedEventArgs e)
    {
        if (timer.IsEnabled)
        {
            timer.IsEnabled = false;
        }
        AutoLoadField();
    }

    private void AutosaveField_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!AutosaveField)
        {
            AutosaveField = true;
        }
    }

    // Database reading/writing
    private void SaveFieldToDB_OnClick(object? sender, RoutedEventArgs e)
    {
        string FieldString = "";
        for (int i = 0; i < game_field.Columns; i++)
        {
            for (int j = 0; j < game_field.Rows; j++)
            {
                FieldString += game_field.get_cell_state(j, i).ToString();
            }
        }
        
        _database.AddField(FieldString);
        DBInfoTextBlock.Text = "DB info: Success";
    }

    private async void LoadFieldFromDB_OnClick(object? sender, RoutedEventArgs e)
    {
        string? FieldString = await _database.GetField((int)FieldID.Value);
        if (FieldString == null)
        {
            DBInfoTextBlock.Text = "DB info: Failure";
            return;
        }

        Cell[,] loaded_field = new Cell[game_field.Columns, game_field.Rows];
        for (int i = 0; i < game_field.Columns; i++)
        {
            for (int j = 0; j < game_field.Rows; j++)
            {
                loaded_field[i, j] = new Cell(0);
            }
        }

        for (int i = 0; i < game_field.Columns; i++)
        {
            for (int j = 0; j < game_field.Rows; j++)
            {
                loaded_field[j, i].State = (byte)(Convert.ToByte(FieldString[j + i * game_field.Columns]) - 48);
            }
        }
        
        game_field.set_field(loaded_field);
        RenderImage();
        DBInfoTextBlock.Text = "DB info: Success";
    }
}
