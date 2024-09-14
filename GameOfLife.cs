using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace GameOfLife;

public class GameOfLife
{
    
    public int col;
    public int row;
    public bool[,] currentGrid;
    private List<(int, int)> dirtyCells = new List<(int, int)>();


    public GameOfLife(int x, int y)
    {
        row = x;
        col = y;
        currentGrid = new bool[row, col];
    }


    public bool[,] GetCurrentState()
    {
        return currentGrid; // this returns the current game state.
    }

    public List<(int, int)> GetDirtyCells()
    {
        return dirtyCells;
    }


    public void UpdateGrid()
    {
        

        bool[,] newGrid = new bool[row, col];
        dirtyCells.Clear();

        for (int x = 0; x < row; x++)
        {
            for (int y = 0; y < col; y++)
            {
                int liveNeighbor = CountLiveNeighbors(currentGrid, x, y);

                if (currentGrid[x, y])
                {
                    newGrid[x, y] = liveNeighbor == 2 || liveNeighbor == 3;
                }
                else
                {
                    newGrid[x, y] = liveNeighbor == 3;
                }

                if (newGrid[x, y] != currentGrid[x, y])
                {
                    dirtyCells.Add((x, y));
                }
            }
        }

        Array.Copy(newGrid, currentGrid, newGrid.Length);
    }


    public int CountLiveNeighbors(bool[,] grid, int x, int y)
    {
        int count = 0;

        
        int[] directions = new int[] { -1, 0, 1 };

        for (int dx = 0; dx < 3; dx++)
        {
            for (int dy = 0; dy < 3; dy++)
            {
                if (directions[dx] == 0 && directions[dy] == 0) continue; 

                int neighborX = x + directions[dx];
                int neighborY = y + directions[dy];

                
                if (neighborX >= 0 && neighborX < row &&
                    neighborY >= 0 && neighborY < col)
                {
                    count += grid[neighborX, neighborY] ? 1 : 0;
                }
            }
        }

        return count;
    }
}