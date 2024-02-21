using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameOfLife.ViewModels;
using StreamWriter = System.IO.StreamWriter;

namespace GameOfLife;

public class FileIO
{
    private GameField? _gameField = null;
    public bool IsFailed;
    private Cell[,] readenField;
    public string _path;

    public FileIO(GameField gameField)
    {
        _gameField = gameField;
        IsFailed = false;
    }

    public FileIO()
    {
        readenField = new Cell[150, 150];
        IsFailed = false;
    }
    
    
    public async Task<bool?> SaveFieldToTextFile()
    {
        try 
        {
            var dialogWindow = new SaveFileDialog();
            dialogWindow.Filters?.Add(new FileDialogFilter() { Name = "Text files", Extensions = { "txt" } } );
            var path = await dialogWindow.ShowAsync(new Window());
         
            if (path == null)
            {
                return null;
            }

            _path = path;
            
            using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.OpenOrCreate)))
            {
                writer.WriteLine("It is correct file for game of Life");
                writer.WriteLine(_gameField.Columns);
                
                for (int i = 0; i < _gameField.Rows; i++)
                {
                    for (int j = 0; j < _gameField.Columns; j++)
                    {
                        writer.Write(_gameField.get_cell_state(j, i));
                    }
                    writer.WriteLine();
                }
            }
            
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    public async Task<Cell[,]> ReadFieldFromTextFile(bool IsImage)
    {
        try
        {
            string[]? path;
            if (IsImage)
            {
                var dialogWindow = new OpenFileDialog();
                dialogWindow.Filters?.Add(new FileDialogFilter() { Name = "Png files", Extensions = { "png" } } );
                path = await dialogWindow.ShowAsync(new Window());
                path[0] = $"{path[0].Substring(0, path[0].Length - 4)}.txt";
            }
            else
            {
                var dialogWindow = new OpenFileDialog();
                dialogWindow.Filters?.Add(new FileDialogFilter() { Name = "Text files", Extensions = { "txt" } } );
                path = await dialogWindow.ShowAsync(new Window());
            }

            if (path == null)
            {
                IsFailed = true;
                return null;
            }

            Cell[,] newField;       
            
            using (StreamReader reader = new StreamReader(path[0]))
            {
                var validationString = await reader.ReadLineAsync();

                if (validationString != "It is correct file for game of Life")
                {
                    throw new FileLoadException();
                }
                
                int field_size = Convert.ToInt32(reader.ReadLine());
                newField = new Cell[field_size, field_size];
                int buffer;

                for (int i = 0; i < field_size; i++)
                {
                    for (int j = 0; j < field_size; j++)
                    {
                        newField[i, j] = new Cell(0);
                    }
                }

                for (int i = 0; i < field_size; i++)
                {
                    for (int j = 0; j < field_size; j++)
                    {
                        buffer = reader.Read();
                        newField[j, i].State = (byte)(buffer - 48);
                    }
                    reader.ReadLine();
                }
            }

            readenField = newField;
        }
        catch (Exception e)
        {
            IsFailed = true;
        }

        return readenField;
    }

    public async Task<Cell[,]> LoadFieldFromLocalFile(string path)
    {
        Cell[,] newField;       
            
        using (StreamReader reader = new StreamReader(File.Open(path, FileMode.Open)))
        {
            newField = new Cell[150, 150];
            
            for (int i = 0; i < 150; i++)
            {
                for (int j = 0; j < 150; j++)
                {
                    newField[i, j] = new Cell(0);
                }
            }

            int buffer;

            for (int i = 0; i < 150; i++)
            {
                for (int j = 0; j < 150; j++)
                {
                    buffer = reader.Read();
                    newField[j, i].State = (byte)(buffer - 48);
                }
                reader.ReadLine();
            }
        }
        readenField = newField;

        return readenField;
    }
}
