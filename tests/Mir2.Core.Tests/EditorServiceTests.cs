using Xunit;
using Mir2.Editor.Services;
using Mir2.Core.Models;

namespace Mir2.Core.Tests;

/// <summary>
/// Tests for EditorService functionality added from archived Editor.cs
/// </summary>
public class EditorServiceTests
{
    [Fact]
    public void EditorService_UndoClear_ClearsUndoStack()
    {
        // Arrange
        var editorService = new EditorService();
        var mapData = new MapData(10, 10);
        editorService.CurrentMap = mapData;
        
        var cellInfo = new CellInfo { BackIndex = 1 };
        var changes = new[] { new CellInfoData(0, 0, cellInfo) };
        
        // Act
        editorService.SaveState(changes);
        Assert.True(editorService.UndoCount > 0); // Verify we have undo data
        
        editorService.UndoClear();
        
        // Assert
        Assert.Equal(0, editorService.UndoCount);
    }

    [Fact]
    public void EditorService_RedoClear_ClearsRedoStack()
    {
        // Arrange
        var editorService = new EditorService();
        var mapData = new MapData(10, 10);
        editorService.CurrentMap = mapData;
        
        var cellInfo = new CellInfo { BackIndex = 1 };
        var changes = new[] { new CellInfoData(0, 0, cellInfo) };
        
        // Act - create undo data then undo to create redo data
        editorService.SaveState(changes);
        editorService.Undo(); // This creates redo data
        Assert.True(editorService.RedoCount > 0); // Verify we have redo data
        
        editorService.RedoClear();
        
        // Assert
        Assert.Equal(0, editorService.RedoCount);
    }

    [Fact]
    public void EditorService_UndoClear_DoesNotAffectRedoStack()
    {
        // Arrange
        var editorService = new EditorService();
        var mapData = new MapData(10, 10);
        editorService.CurrentMap = mapData;
        
        var cellInfo = new CellInfo { BackIndex = 1 };
        var changes = new[] { new CellInfoData(0, 0, cellInfo) };
        
        // Act
        editorService.SaveState(changes);
        editorService.Undo(); // Creates redo data
        var redoCountBeforeClear = editorService.RedoCount;
        
        editorService.UndoClear();
        
        // Assert - redo stack should be unchanged
        Assert.Equal(redoCountBeforeClear, editorService.RedoCount);
    }

    [Fact]
    public void EditorService_RedoClear_DoesNotAffectUndoStack()
    {
        // Arrange
        var editorService = new EditorService();
        var mapData = new MapData(10, 10);
        editorService.CurrentMap = mapData;
        
        var cellInfo = new CellInfo { BackIndex = 1 };
        var changes = new[] { new CellInfoData(0, 0, cellInfo) };
        
        // Act
        editorService.SaveState(changes);
        var undoCountBeforeClear = editorService.UndoCount;
        editorService.Undo(); // Creates redo data
        
        editorService.RedoClear();
        
        // Assert - undo stack should be unchanged after undo operation
        Assert.Equal(0, editorService.UndoCount); // Undo was performed, so should be 0
    }
}