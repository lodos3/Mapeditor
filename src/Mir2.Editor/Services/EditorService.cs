using Mir2.Core.Models;
using System.Collections.Generic;

namespace Mir2.Editor.Services;

/// <summary>
/// Service for handling map editing operations including undo/redo
/// </summary>
public class EditorService
{
    private readonly Stack<CellInfoData[]> _undoStack;
    private readonly Stack<CellInfoData[]> _redoStack;
    private MapData? _currentMap;

    public EditorService()
    {
        _undoStack = new Stack<CellInfoData[]>();
        _redoStack = new Stack<CellInfoData[]>();
    }

    /// <summary>
    /// Current map being edited
    /// </summary>
    public MapData? CurrentMap
    {
        get => _currentMap;
        set
        {
            _currentMap = value;
            ClearHistory();
        }
    }

    /// <summary>
    /// Number of undo operations available
    /// </summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Number of redo operations available
    /// </summary>
    public int RedoCount => _redoStack.Count;

    /// <summary>
    /// Saves current state for undo operation
    /// </summary>
    /// <param name="cellChanges">Array of cell changes to save</param>
    public void SaveState(CellInfoData[] cellChanges)
    {
        if (cellChanges?.Length > 0)
        {
            _undoStack.Push((CellInfoData[])cellChanges.Clone());
            _redoStack.Clear(); // Clear redo stack when new action is performed
        }
    }

    /// <summary>
    /// Performs undo operation
    /// </summary>
    /// <returns>Array of changes to apply, or null if nothing to undo</returns>
    public CellInfoData[]? Undo()
    {
        if (_undoStack.Count > 0 && _currentMap != null)
        {
            var changes = _undoStack.Pop();
            
            // Save current state for redo before applying changes
            var redoChanges = new CellInfoData[changes.Length];
            for (int i = 0; i < changes.Length; i++)
            {
                var change = changes[i];
                redoChanges[i] = new CellInfoData(change.X, change.Y, _currentMap.Cells[change.X, change.Y]);
                _currentMap.Cells[change.X, change.Y] = change.CellInfo;
            }
            
            _redoStack.Push(redoChanges);
            return changes;
        }
        return null;
    }

    /// <summary>
    /// Performs redo operation
    /// </summary>
    /// <returns>Array of changes to apply, or null if nothing to redo</returns>
    public CellInfoData[]? Redo()
    {
        if (_redoStack.Count > 0 && _currentMap != null)
        {
            var changes = _redoStack.Pop();
            
            // Save current state for undo before applying changes
            var undoChanges = new CellInfoData[changes.Length];
            for (int i = 0; i < changes.Length; i++)
            {
                var change = changes[i];
                undoChanges[i] = new CellInfoData(change.X, change.Y, _currentMap.Cells[change.X, change.Y]);
                _currentMap.Cells[change.X, change.Y] = change.CellInfo;
            }
            
            _undoStack.Push(undoChanges);
            return changes;
        }
        return null;
    }

    /// <summary>
    /// Clears all undo/redo history
    /// </summary>
    public void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>
    /// Clears only the undo history
    /// </summary>
    public void UndoClear()
    {
        _undoStack.Clear();
    }

    /// <summary>
    /// Clears only the redo history
    /// </summary>
    public void RedoClear()
    {
        _redoStack.Clear();
    }

    /// <summary>
    /// Sets cell data and saves state for undo
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="cellInfo">New cell information</param>
    public void SetCell(int x, int y, CellInfo cellInfo)
    {
        if (_currentMap == null || x < 0 || y < 0 || x >= _currentMap.Width || y >= _currentMap.Height)
            return;

        // Save current state for undo
        var oldCell = new CellInfoData(x, y, _currentMap.Cells[x, y]);
        SaveState(new[] { oldCell });

        // Apply change
        _currentMap.Cells[x, y] = cellInfo;
    }

    /// <summary>
    /// Sets multiple cells and saves state for undo
    /// </summary>
    /// <param name="changes">Array of cell changes to apply</param>
    public void SetCells(CellInfoData[] changes)
    {
        if (_currentMap == null || changes == null || changes.Length == 0)
            return;

        // Save current state for undo
        var oldCells = new CellInfoData[changes.Length];
        for (int i = 0; i < changes.Length; i++)
        {
            var change = changes[i];
            if (change.X >= 0 && change.Y >= 0 && change.X < _currentMap.Width && change.Y < _currentMap.Height)
            {
                oldCells[i] = new CellInfoData(change.X, change.Y, _currentMap.Cells[change.X, change.Y]);
                _currentMap.Cells[change.X, change.Y] = change.CellInfo;
            }
        }
        
        SaveState(oldCells);
    }
}