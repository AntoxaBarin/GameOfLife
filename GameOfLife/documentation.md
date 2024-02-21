The game is written in C# using the AvaloniaUI cross-platform library.

### Design and functionality

The `MainWindow.axaml` file contains all the design elements. The `SplitView` is used to split the screen into two zones:
the controls and the playing field. A `Canvas` of 1500x1500 pixels is used as the field. Live/dead cells are a `Rectangle` drawn on the playing field.
The 'Skia` library is used to draw on the playing field(Significant rendering acceleration compared to the first version of the program).
The controls are `Button`, `CheckBox`, `NumericUpDown'. Information about the current generation is stored in the `TextBox' element.


Buttons: **Start** - start the game. Two types of cell placement on the zero generation are implemented: mouse click on a dead cell (turning it into a living one),
random placement of living cells if there is a check mark in **Random start**.
**Stop** - stop the game. **Reset** - complete reset of the game: clearing the field, zeroing the generation counter. **One step** - allows you to make 1 "move".
Three basic shapes are also implemented (shapes are loaded from files, in the previous version there was an inconvenient implementation through classes): **Glider** - flies across the field, **Gosper's gun** - produces gliders, **Pulsar** - is restored every 3 generations. To start the example shape, click on the button of interest, and then **Start**.
**Make steps** - starts the automatic execution of the number of moves set by the user (Moves are indicated in the field to the left of the label 'Steps').
**Cell color** - a button to select a color for drawing live cells on the field. **Neighbors to save life** - flyout-an element, when clicked, the user can set their own rules for saving life in a cage in the next generation.
Similarly, **Neighbors to make life** are the rules for the appearance of life in the cage on the next turn. **Start from file** - loading a field from a text file on the hard disk. **Save to file** - loading the playing field into a text file. CheckBox **Image start/save** - the ability to save the playing field to a .png file (picture).
There is no download directly from the image, if the user selects an image to upload, then the field is loaded from a text file that is saved with the same name along with the image.
**Load autosaved field** - loads the field saved at the end of the previous game session (the field is in the file `GameFieldSave.txt `). **Autosave field** - confirmation of saving the state of the playing field when closing the application.
**Save to database** - the ability to save the current state of the playing field to a local SQLite database (Located in the `Data` folder).
**Load from database** - loads the playing field with the specified index in the ID field from the database.

### Technical part

The basis of the game are the classes `GameField' (a small wrapper over the playing field - a two-dimensional array of instances of the Cell type (the state of the cell, the number of generations it lives in a row)) and `Gameplay' (The implementation of game logic). The implementation of the classes lies in the file `MainWindowViewModel.cs`.
The main file `MainWindow.axaml.cs` implements a timer game process that starts the method of building the next generation every 60ms - `NextGeneration'. (`NextGeneration_wrapper` is a wrapper above `NextGeneration` that allows you to display information about the current generation number),
methods for all buttons and processing of a mouse click on a cell. Methods for drawing on the playing field are also implemented - `draw_rectangle' (drawing a rectangle) and `update_field' (drawing all living cells on the field). The basic shapes (glider, shotgun, pulsar) are implemented as inheritors of the IFigure class in the `BasicFigures.cs` file.

