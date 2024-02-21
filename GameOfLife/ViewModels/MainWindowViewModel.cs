using System;
using System.Collections.Generic;
namespace GameOfLife.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    
}

public class Cell
{
    public byte State; // 0 = dead, 1 - alive white, 2 - alive red, 3 - alive blue, 4 - alive green
    private int NumberOfAliveGenerationsInARow; // number of generations cell is alive for 
    
    public Cell(byte state)
    {
        State = state;
        NumberOfAliveGenerationsInARow = 0;
    }
    
    public int GetNumberOfGenerations()
    {
        return NumberOfAliveGenerationsInARow;
    }

    public void SetGenerationsInARow(int value)
    {
        NumberOfAliveGenerationsInARow = value;
    }
}

public class GameField
{
    private Cell[,] _cellStates;

    public int Rows;
    public int Columns;
    
    private const int SizeOfCell = 10;
    
    private void set_rows(int value)
    {
        Rows = value;
    }

    private void set_columns(int value)
    {
        Columns = value;
    }

    public void set_field(Cell[,] field)
    {
        _cellStates = field;
    }

    public GameField(int field_height, int field_width)
    {
        set_rows(field_height / SizeOfCell);
        set_columns(field_width / SizeOfCell);
        _cellStates = new Cell[Columns, Rows];

        setup_cell_states();
    }
    
    public Cell[,] get_game_field()
    {
        return _cellStates;
    }

    public void set_cell(int i, int j, byte value) 
    {
        _cellStates[i, j].State = value;
    }
    
    public byte get_cell_state(int i, int j)
    {
        return _cellStates[i, j].State;
    }
    
    // generations = number of alive generations in a row for cell
    public int get_cell_generations(int i, int j)
    {
        return _cellStates[i, j].GetNumberOfGenerations();
    }
    
    public void setup_cell_states()
    {
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                _cellStates[i, j] = new Cell(0);
            }
        }
    }
    
    public void random_setup_cell_states()
    {
        Random random = new Random();
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                _cellStates[i, j].State = (byte)random.Next(0, 5); 
            }
        }
    }
    
}

public class Gameplay 
{
    private int _currentGenerationNumber;
    private readonly GameField _gamefield;
    private Cell[,] _previousGenerationField; // to check if generations are equal => stop the game

    private List<int> _makeLifeNeighboursNumber; // user rules
    private List<int> _saveLifeNeighboursNumber; 

    private Dictionary<byte, byte> AliveColorNeighbours; // to calculate color cell right

    public Gameplay(GameField gamefield, List<int> save_list, List<int> make_list)
    {
        _gamefield = gamefield;
        _currentGenerationNumber = 0;
        _makeLifeNeighboursNumber = make_list;
        _saveLifeNeighboursNumber = save_list;
        
        AliveColorNeighbours = new Dictionary<byte, byte>
        {
            {1, 0},  // White
            {2, 0},  // Red
            {3, 0},  // Blue
            {4, 0}   // Green
        };
    }

    // rules setting
    public void set_save_life_neighbours_number(List<int> save_life_neighbours_number)
    {
        _saveLifeNeighboursNumber = save_life_neighbours_number;
    }
    
    public void set_make_life_neighbours_number(List<int> make_life_neighbours_number)
    {
        _makeLifeNeighboursNumber = make_life_neighbours_number;
    }
    
    // Counts same color neighbours for alive cell
    private int NumberOfAliveNeighboursForAliveCells(int i, int j)
    {
        int counter = 0;
        byte current_cell_state = _gamefield.get_cell_state(i, j);

        for (int k = -1; k < 2; k++)
        {
            for (int l = -1; l < 2; l++)
            {
                int column = (_gamefield.Columns + i + k) % _gamefield.Columns;
                int row = (_gamefield.Rows + j + l) % _gamefield.Rows;

                bool isCurrentCell = (column == i && j == row);
                
                if (_gamefield.get_cell_state(column, row) == current_cell_state && !isCurrentCell)
                {
                    counter += 1;
                }
            }
        }

        return counter;
    }

    // Counts neighbours and returns color of dead cell on next generation if there are only one color neighbours
    private byte ColorForDeadCellCalculation(int i, int j)
    {
        AliveColorNeighbours[1] = 0;
        AliveColorNeighbours[2] = 0;
        AliveColorNeighbours[3] = 0;
        AliveColorNeighbours[4] = 0;
        
        for (int k = -1; k < 2; k++)
        {
            for (int l = -1; l < 2; l++)
            {
                int column = (_gamefield.Columns + i + k) % _gamefield.Columns;
                int row = (_gamefield.Rows + j + l) % _gamefield.Rows;

                bool isCurrentCell = (column == i && j == row);
                
                if (_gamefield.get_cell_state(column, row) != 0 && !isCurrentCell)
                {
                    AliveColorNeighbours[_gamefield.get_cell_state(column, row)] += 1;
                }
            }
        }

        byte stateOfDeadCellOnNextGeneration = 0;
        byte numberOfAliveNationsAround = 0;   // Nation - set of same color cells-neighbours 
        foreach (var pair in AliveColorNeighbours)
        {
            if (_makeLifeNeighboursNumber.Contains(pair.Value))
            {
                stateOfDeadCellOnNextGeneration = pair.Key;
                numberOfAliveNationsAround += 1;
            }
        }

        if (numberOfAliveNationsAround == 1)
        {
            return stateOfDeadCellOnNextGeneration;
        }
        return 0;
    }

    public bool NextGeneration()
    {
        if (_currentGenerationNumber > 0 && AreFieldsEqual(_previousGenerationField))
        {
            return false;
        }
        
        var newField = new Cell[_gamefield.Columns, _gamefield.Rows];
        
        
        for (int i = 0; i < 150; i++)
        {
            for (int j = 0; j < 150; j++)
            {
                newField[i, j] = new Cell(0);
            }
        }
        

        for (int i = 0; i < _gamefield.Columns; i++) 
        {
            for (int j = 0; j < _gamefield.Rows; j++) 
            {
                var neighbours = NumberOfAliveNeighboursForAliveCells(i, j);
                
                if (_gamefield.get_cell_state(i, j) == 0 && ColorForDeadCellCalculation(i, j) != 0)   
                {
                    newField[i, j].State = ColorForDeadCellCalculation(i, j);         // Cell become alive (with neighbours color)
                    newField[i, j].SetGenerationsInARow(1);
                }
                else if (_gamefield.get_cell_state(i, j) != 0 && !CheckNeighbours(neighbours))
                {
                    newField[i, j].State = 0;                // Cell dies
                    newField[i, j].SetGenerationsInARow(0);
                }
                else {
                    newField[i, j].State = _gamefield.get_cell_state(i, j);      // No changes
                    if (newField[i, j].State != 0)
                    {
                        newField[i, j].SetGenerationsInARow(_gamefield.get_cell_generations(i, j) + 1);
                    }
                }
            }
        }

        _currentGenerationNumber += 1;
        _previousGenerationField = _gamefield.get_game_field();
        _gamefield.set_field(newField);

        return true;
    }

    // checks user rules
    private bool CheckNeighbours(int neighbours_number)
    {
        bool result = false;
        
        foreach (var number in _saveLifeNeighboursNumber)
        {
            if (neighbours_number == number) result = true;
        }

        return result;
    }

    // Checks if previous and current generations are equal
    private bool AreFieldsEqual(Cell[,] previous_generation) 
    {
        for (int i = 0; i < _gamefield.Columns; i++)
        {
            for (int j = 0; j < _gamefield.Rows; j++)
            {
                if (_gamefield.get_cell_state(i, j) != previous_generation[i, j].State)
                {
                    return false;
                }
            }
        }

        return true;
    }
    
}