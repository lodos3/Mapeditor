using Xunit;
using Mir2.Editor.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Mir2.Core.Tests;

/// <summary>
/// Tests for utility classes added from archived files
/// </summary>
public class FileComparersTests
{
    [Fact]
    public void AlphanumComparatorFast_SortsNumericallyCorrect()
    {
        // Arrange
        var comparer = new AlphanumComparatorFast();
        var testData = new List<string>
        {
            "file1.txt",
            "file10.txt",
            "file2.txt",
            "file100.txt",
            "file20.txt"
        };

        // Act
        testData.Sort(comparer);

        // Assert
        var expected = new[]
        {
            "file1.txt",
            "file2.txt", 
            "file10.txt",
            "file20.txt",
            "file100.txt"
        };
        
        Assert.Equal(expected, testData);
    }

    [Fact]
    public void FilesNameComparerClass_SortsNumericallyCorrect()
    {
        // Arrange
        var comparer = new FilesNameComparerClass();
        var testData = new List<string>
        {
            "aa1",
            "aa100",
            "aa2",
            "aa10"
        };

        // Act
        testData.Sort(comparer);

        // Assert
        var expected = new[]
        {
            "aa1",
            "aa2",
            "aa10",
            "aa100"
        };
        
        Assert.Equal(expected, testData);
    }

    [Fact]
    public void AlphanumComparatorFast_HandlesNullValues()
    {
        // Arrange
        var comparer = new AlphanumComparatorFast();

        // Act & Assert
        Assert.Equal(0, comparer.Compare(null, null));
        Assert.Equal(-1, comparer.Compare(null, "test"));
        Assert.Equal(1, comparer.Compare("test", null));
    }

    [Fact]
    public void FilesNameComparerClass_HandlesNullValues()
    {
        // Arrange
        var comparer = new FilesNameComparerClass();

        // Act & Assert
        Assert.Equal(0, comparer.Compare(null, null));
        Assert.Equal(-1, comparer.Compare(null, "test"));
        Assert.Equal(1, comparer.Compare("test", null));
    }
}