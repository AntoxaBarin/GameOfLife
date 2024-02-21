﻿## Реализация игры "Жизнь", Шаныгин Иван, группа Б09.

Игра написана на C# с использованием кроссплатформенной библиотеки AvaloniaUI.

### Дизайн и функциональность

В файле `MainWindow.axaml` находятся все элементы дизайна. `SplitView` используется для разделения экрана на две зоны: 
элементы управления и игровое поле. В качестве поля используется `Canvas` размером 1500x1500 точек. Живые/мертвые клетки - `Rectangle`, нарисованный на игровом поле.
Для рисования на игровом поле используется библиотека `Skia`(Существенное ускорение отрисовки по сравнению с первой версией программы).
В качестве элементов управления используются `Button`, `CheckBox`, `NumericUpDown`. Информация о текущем поколении хранится в элементе `TextBox`.  


Кнопки: **Start** - начало игры. Реализовано два вида размещения клеток на нулевом поколении: клик мышкой по мертвой клетке(превращение её в живую), 
случайная расстановка живых клеток при наличия галочки в **Random start**.
**Stop** - остановка игры. **Reset** - полный сброс игры: очистка поля, обнуление счетчика генераций. **One step** - позволяет сделать 1 "ход".
Также реализованы три базовых фигуры(фигуры загружаются из файлов, в предыдущей версии была неудобная реализация через классы): **Планер** - летает по полю, **Ружье Госпера** - производит планеры, **Пульсар** - восстанавливается каждые 3 поколения. Для запуска фигуры-примера нужно нажать на интересующую кнопку, а затем **Start**.
**Make steps** - запуск автоматического выполнения заданного пользователем количества ходов(Хода указываются в поле слева от надписи 'Steps'). 
**Cell color** - кнопка для выбора цвета для рисования живых клеток на поле. **Neighbours to save life** - flyout-элемент, при нажатии на который пользователь может установить свои правила для сохранения жизни в клетке в следующем поколении.
Аналогично, **Neighbours to make life** - правила для появления жизни в клетке на следующем ходу. **Start from file** - загрузка поля из текстового файла на жестком диске. **Save to file** - загрузка игрового поля в текстовый файл. CheckBox **Image start/save** - возможность сохранять игровое поля в файл .png(картинка).
Загрузки непосредственно с картинки нет, если пользователь выбирает картинку для загрузки, то поле загружается из текстового файла, который сохраняется с таким же именем вместе с картинкой.
**Load autosaved field** - загружает поле, сохраненное в конце предыдущей игровой сессии(поле лежит в файле `GameFieldSave.txt`). **Autosave field** - подтверждение сохранения состояния игрового поля при закрытии приложения.
**Save to database** - возможность сохранить текущее состояние игрового поля в локальную базу данных SQLite(Лежит в папке `Data`).
**Load from database** - загружает игровое поле с указанным индексом в поле ID из базы данных. 

### Техническая часть

Основа игры - классы `GameField`(небольшая обертка над игровым полем - двумерным массивом экземпляров типа Cell(состояние клетки, количество поколений, которое она живет подряд)) и `Gameplay`(Реализация игровой логики). Реализация классов лежит в файле `MainWindowViewModel.cs`.
В основном файле `MainWindow.axaml.cs` реализован процесс игры - таймер, который запускает метод построения следующего поколения каждые 60мс - `NextGeneration`. (`NextGeneration_wrapper` - обертка над `NextGeneration`, позволяющая отображать информацию о номере текущего поколения),
методы для всех кнопок и обработка клика мышью по клетке. Также реализованы методы для рисования на игровом поле - `draw_rectangle`(отрисовка прямоугольника) и `update_field`(отрисовка всех живых клеток на поле). Базовые фигуры(планер, ружье, пульсар) реализованы как наследники класса IFigure в файле `BasicFigures.cs`. 
На мой взгляд, это не лучшая идея для реализации примеров фигур. Я хочу заменить написание очередного класса-наследника IFigure на загрузку бинарного файла, где будет хранится массив состояний клеток на игровом поле. 

Возможно, я расписал не все аспекты устройства программы, постарался прокомментировать некоторые места.

### Реализованные пункты
Вдобавок к ранее сделанным пунктам 1,2,3,7,9,14 релизованы следующие:

`4` - Кнопки **Save to file**, **Start from file**. Логика лежит в файле `FileIO/FileIO.cs` (4 балла).


`5`,`6` - Поле лежит в файле `Assets/GameFieldSave.txt`. Логика в методах `AutoLoadField` и `Window_OnClosing`. 
Подтверждение загрузки/сохранения происходит при помощи кнопок **Load autosaved field** и **Autosave field** (2 + 2 баллов).

`8` - В методе `GameFieldImage_OnPointerMoved` выводится информация о поколении клетки в элемент **TextBox** (2 балла).

`10` - Правила устанавливаются пользователем при нажатии на кнопки в **Neighbours to make life** и **Neighbours to save life**.
Затем все CheckBox считываются и правила(количества клеток) переносятся в списки, которые используются при вычислении следующего поколения (4 балла).

`11` - Правила автоматически сохраняются в файле `Assets/GameRulesSave.txt`, логика - `LoadRules` и `Window_OnClosing` (2 балла).

`12` - Картинка сохраняется при нажатии на кнопку **Save to file** при условии, что есть галочка у пункта **Image start/save**. Логика лежит в `FileIO.cs`
Загрузка поля из картинки **Start from file** - загрузка из одноименного текстового файла, сохраняемого вместе с картинкой. (4 балла)

`13` - кнопка **Make steps**, логика в обработчике нажатий на эту кнопку `Make_Steps_OnClick` (Можно сделать от 1 до 10 ходов за раз) (2 балла).

`15` - Есть 4 цвета: Белый - 1, Красный - 2, Синий - 3, Зеленый - 4. Жизнь в клетке появляется как в одноцветной игре "Жизнь", но с небольшими ограничениями:  Пока в клетке есть жизнь одного цвета, жизнь другого цвета в ней не может возникнуть (т. е. клетка должна пройти через пустое состояние, прежде чем поменять цвет). Клетка выживает, только если рядом с ней есть требуемое количество соседей её цвета. То есть клетки других цветов считаются мёртвыми для конкретного цвета.
Если в одной клетке может возникнуть жизнь сразу нескольких цветов, то жизнь не появляется. Также есть кнопка, которая отображает цвет, которым можно нарисовать клетку на поле, для переключения цвета нужно нажать на кнопку. Логика - методы `NumberOfAliveNeighboursForAliveCells`, `ColorForDeadCellCalculation` (8 баллов).

`17` - Подключена локальная база данных SQLite - лежит в директории `Data`. Там же лежит файл `DB.cs`, в котором реализован класс для взаимодействия с базой данных - Database.
Сохранение поля и загрузка поля - кнопки **Save to database**, **Load from database** (еще надо указать ID - ключ для поля из базы данных) 
Один нюанс с базой данных - она работает только если указать в соединении абсолютный путь до неё, иначе данные можно записать и достать, но они каким-то образом не остаются в базе навсегда (12 баллов).

`18` - Если совпадает текущее и предыдущее состояние поля, то игра завершается(Появляется сообщение). Логика - проверка на равенство двух поколений при помощи `AreFieldsEqual` в классе `Gameplay` (2 балла).

Итог: 18 баллов за первую проверку + новые пункты 44 балла = примерно 62 балла. 