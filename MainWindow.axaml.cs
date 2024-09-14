using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Brushes = Avalonia.Media.Brushes;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;
using System.Threading;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace GameOfLife;

public partial class MainWindow : Window
{
    private GameOfLife gameOfLife = new GameOfLife(70,70);
    private bool[,] grid;
    private int delay;
    private double populationDensity;
    private bool updating = false;
    private bool paused = true;
    private bool runButtonClicked = false;
    private bool isPointerPressed = true;
    private CancellationTokenSource cancellationTokenSource;
    private int generation = 0;
    private Grid? mGrid;
    private Label? message;
    private Slider? speed;
    private Slider? population;
    private ToggleButton? pauseB;
    private Slider? size;
    private TextBlock? generationTxt;

    public MainWindow()
    {
        InitializeComponent();
        grid = gameOfLife.GetCurrentState();
        mGrid = this.FindControl<Grid>("MainGrid");
        speed = this.FindControl<Slider>("SliderSpeed");
        population = this.FindControl<Slider>("SliderPopulation");
        pauseB = this.FindControl<ToggleButton>("PauseButton");
        message = this.Find<Label>("statusMessage");
        size = this.FindControl<Slider>("SliderSize");
        generationTxt = this.FindControl<TextBlock>("txtGeneration");
        
        BuildGrid();
        //RandomGenerateGrid(grid);
        //MyAsyncMethod();

    }

   

    public bool Updating
    {
        get => updating;
        set
        {
            if (updating != value)
            {
                updating = value;
                OnUpdatingChanged(); // This method will be called whenever the value changes.
            }
        }
    }


    public async Task MyAsyncMethod()
    {
        if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }


        cancellationTokenSource = new CancellationTokenSource();

        Updating = true;

        // VisualizeGrid(grid);

        await UpdateGridAsync(cancellationTokenSource.Token);
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task UpdateGridAsync(CancellationToken cancellationToken)
    {

        while (Updating)
        {
            try
            {
                // Check for cancellation before starting any new work.
                cancellationToken.ThrowIfCancellationRequested();
                gameOfLife.UpdateGrid();
                VisualizeGridUpdate(grid);
                await Dispatcher.UIThread.InvokeAsync(() => {
                    delay = (int)speed.Value;
                });

                // Use the cancellationToken within the Task.Delay to respond to cancellations.
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // The task was canceled, likely because the user stopped the operation.
                // Cleanup or reset as necessary here. If you need to update the UI, make sure to do it on the UI thread.

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // For example, reset UI elements related to the operation's progress.
                    message.Content = "Operation was canceled."; // Hypothetical status label.
                });

                Console.WriteLine("Update grid task was canceled."); // For debugging, remove if not needed.

                // Exit the method upon cancellation to avoid further processing.
                return;
            }
            catch (Exception ex)
            {
                // Log or handle other types of exceptions as necessary.
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Depending on your application's requirement, you may choose to break the loop upon an error.
                break;
            }
        }
    }



    private void RandomGenerateGrid(bool[,] grid)
    {
        Random random = new Random();
        populationDensity = (int)population.Value;


        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (random.Next(0, 100) < populationDensity)
                {
                    grid[i, j] = true;
                }
            }
        }

    }

    private void BuildGrid()
    {

        if (mGrid != null)
        {
            double width = mGrid.Width / gameOfLife.col;
            double height = mGrid.Height / gameOfLife.row;

            mGrid.RowDefinitions.Clear();
            mGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < gameOfLife.row; i++)
            {
                mGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(height) });
            }

            for (int j = 0; j < gameOfLife.col; j++)
            {
                mGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(width) });
            }

        }


    }

    private void VisualizeGrid(bool[,] grid)
    {
        mGrid.Children.Clear();

        for (int i = 0; i < gameOfLife.row; i++)
        {
            for (int j = 0; j < gameOfLife.col; j++)
            {

                if (grid[i, j])
                {
                    var rect = new Rectangle();
                    rect.Fill = Brushes.Gray;
                    rect.Stroke = Brushes.White;
                    rect.StrokeThickness = 1;
                    Grid.SetColumn(rect, j);
                    Grid.SetRow(rect, i);
                    mGrid.Children.Add(rect);
                }

            }
        }
    }


    private void VisualizeGridUpdate(bool[,] grid)
    {

        var changedCells = gameOfLife.GetDirtyCells();

        foreach (var (x, y) in changedCells)
        {

            if (grid[x, y])
            {
                var rect = new Rectangle();
                rect.Fill = Brushes.Gray;
                rect.Stroke = Brushes.White;
                rect.StrokeThickness = 1;
                Grid.SetColumn(rect, y);
                Grid.SetRow(rect, x);
                mGrid.Children.Add(rect);
            }
            else
            {
                var deadRect = mGrid.Children
                    .OfType<Rectangle>()
                    .FirstOrDefault(r => Grid.GetRow(r) == x && Grid.GetColumn(r) == y);
                if (deadRect != null)
                {
                    mGrid.Children.Remove(deadRect);
                }
            }

        }

        generation++;

        if (generation != 0 && generationTxt != null)
        {
            generationTxt.Background = Brushes.LightSteelBlue;
            generationTxt.Text = $"Generation: {generation}    ";
        }

    }



    private void Grid_CellClicked(object sender, PointerPressedEventArgs e)
    {
       
        var position = e.GetPosition(mGrid);

        double width = mGrid.Width / gameOfLife.col;
        double height = mGrid.Height / gameOfLife.row;

        int column = (int)(position.X / width); 
        int row = (int)(position.Y / height);   

        
        grid[row, column] = !grid[row, column];


        if (grid[row, column])
        {
            Rectangle rect = new Rectangle();
            rect.Fill = Brushes.Black;
            rect.Stroke = Brushes.White;
            rect.StrokeThickness = 1;
            Grid.SetColumn(rect, column);
            Grid.SetRow(rect, row);
            mGrid.Children.Add(rect);
        }
        else
        {
            var deadRect = mGrid.Children
                    .OfType<Rectangle>()
                    .FirstOrDefault(r => Grid.GetRow(r) == row && Grid.GetColumn(r) == column);
            if (deadRect != null)
            {
                mGrid.Children.Remove(deadRect);
            }
        }

    }

    private async void ClearGrid()
    {
        mGrid.Children.Clear();
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = false;
            }
        }

        generationTxt.Background = Brushes.White;
        generationTxt.Text = "    ";
        generation = 0;
    }


    private async void Random_OnClick(object? sender, RoutedEventArgs e)
    {
        ClearGrid();
        RandomGenerateGrid(grid);

        if (isPointerPressed = false)
        {
            mGrid.PointerPressed += Grid_CellClicked;
            isPointerPressed = true;
        }
        else
        {
            mGrid.PointerPressed -= Grid_CellClicked;
            mGrid.PointerPressed += Grid_CellClicked;
        }

        VisualizeGrid(grid);
        //MyAsyncMethod();
        
    }

    private async void Draw_OnClick(object? sender, RoutedEventArgs e)
    {
        Updating = false;
        ClearGrid();
        if (isPointerPressed = false)
        {
            mGrid.PointerPressed += Grid_CellClicked;
            isPointerPressed = true;
        }
        else
        {
            mGrid.PointerPressed -= Grid_CellClicked;
            mGrid.PointerPressed += Grid_CellClicked;
        }

    }

    private async void Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        Updating = false;
        isPointerPressed = false;
        ClearGrid();
        if (isPointerPressed = true)
        {
            mGrid.PointerPressed -= Grid_CellClicked;
            isPointerPressed = false;
        }
        

    }


    private void Run_Checked(object? sender, RoutedEventArgs e)
    {
        if (runButtonClicked)
        {
            // Reset the flag and return
            runButtonClicked = false;
            return;
        }

        if (sender is ToggleButton toggleButton)
        {
            paused = false;
            toggleButton.Content = new TextBlock() { Text = "||" }; // Change symbol to indicate it's paused.
            MyAsyncMethod();
        }

    }

    private void Run_Unchecked(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            paused = true;
            cancellationTokenSource?.Cancel();  // Signal cancellation
            toggleButton.Content = new TextBlock() { Text = "▶" };
            Updating = false;
        }

    }
    

    private void OnUpdatingChanged()
    {
        // Check if the button is not null before trying to change its properties.
        if (pauseB != null)
        {
            // Updating the button based on the 'Updating' status.
            if (Updating)
            {
                pauseB.Content = new TextBlock() { Text = "||" }; // Simulation is running.
                pauseB.IsChecked = true; // Ensure the button is in the checked state.
            }
            else
            {
                pauseB.Content = new TextBlock() { Text = "▶" }; // Simulation is paused/stopped.
                pauseB.IsChecked = false; // Ensure the button is in the unchecked state.
            }
        }
    }


    private void NextGeneration_OnClick(object? sender, RoutedEventArgs e)
    {
        if (pauseB.IsChecked == true)
        {
            // If not already paused, pause the iteration
            pauseB.IsChecked = false;
        }
        // Perform the next generation logic
        gameOfLife.UpdateGrid();
        VisualizeGridUpdate(grid);

    }

    private async void UpdateGameSize(object? sender, RangeBaseValueChangedEventArgs e)
    {
        // 取消当前正在进行的任何更新。
        cancellationTokenSource?.Cancel();
        ClearGrid();
        if (isPointerPressed = true)
        {
            mGrid.PointerPressed -= Grid_CellClicked;
        }

        int row = (int)size.Value;
        int col = (int)size.Value;

        // 重新初始化游戏和网格
        gameOfLife = new GameOfLife(row, col);
        grid = gameOfLife.GetCurrentState();

        // 安全地更新UI元素
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            mGrid.Children.Clear();
            mGrid.RowDefinitions.Clear();
            mGrid.ColumnDefinitions.Clear();
            BuildGrid();
        });
    }

    /*
    private void OnTextChangeHandler(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            if (int.TryParse(boardW.Text, out int newWidth) && int.TryParse(boardH.Text, out int newHeight))
            {
                if (newWidth != grid.GetLength(0) || newHeight != grid.GetLength(1)) // if size actually changed
                {
                    UpdateGameSize(newWidth, newHeight);
                }
            }
        }
    }
    */
}