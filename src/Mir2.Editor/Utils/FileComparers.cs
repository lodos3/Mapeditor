using System;
using System.Collections;
using System.Collections.Generic;

namespace Mir2.Editor.Utils;

/// <summary>
/// Comparer for natural alphanumeric sorting
/// From archived Utility.cs - provides natural sorting for filenames and strings with numbers
/// </summary>
public class AlphanumComparatorFast : IComparer, IComparer<string>
{
    public int Compare(object? x, object? y)
    {
        if (x is string s1 && y is string s2)
            return Compare(s1, s2);
        
        return 0;
    }

    public int Compare(string? s1, string? s2)
    {
        if (s1 == null && s2 == null) return 0;
        if (s1 == null) return -1;
        if (s2 == null) return 1;

        int len1 = s1.Length;
        int len2 = s2.Length;
        int marker1 = 0;
        int marker2 = 0;

        // Walk through two the strings with two markers.
        while (marker1 < len1 && marker2 < len2)
        {
            char ch1 = s1[marker1];
            char ch2 = s2[marker2];

            // Some buffers we can build up characters in for each chunk.
            char[] space1 = new char[len1];
            int loc1 = 0;
            char[] space2 = new char[len2];
            int loc2 = 0;

            // Walk through all following characters that are digits or
            // characters in BOTH strings starting at the appropriate marker.
            // Collect char arrays.
            do
            {
                space1[loc1++] = ch1;
                marker1++;

                if (marker1 < len1)
                {
                    ch1 = s1[marker1];
                }
                else
                {
                    break;
                }
            } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

            do
            {
                space2[loc2++] = ch2;
                marker2++;

                if (marker2 < len2)
                {
                    ch2 = s2[marker2];
                }
                else
                {
                    break;
                }
            } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

            // If we have collected numbers, compare them numerically.
            // Otherwise, if we have strings, compare them alphabetically.
            string str1 = new string(space1, 0, loc1);
            string str2 = new string(space2, 0, loc2);

            int result;

            if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
            {
                int thisNumericChunk = int.Parse(str1);
                int thatNumericChunk = int.Parse(str2);
                result = thisNumericChunk.CompareTo(thatNumericChunk);
            }
            else
            {
                result = str1.CompareTo(str2);
            }

            if (result != 0)
            {
                return result;
            }
        }
        return len1 - len2;
    }
}

/// <summary>
/// Comparer mainly used for file name comparison with numeric sorting
/// From archived FilesNameComparer.cs - compares filenames naturally with numeric values
/// </summary>
public class FilesNameComparerClass : IComparer, IComparer<string>
{
    /// <summary>
    /// Compare two strings. If they contain numbers, the numbers are compared based on their size.
    /// </summary>
    /// <param name="x">First string</param>
    /// <param name="y">Second string</param>
    /// <returns>Comparison result</returns>
    public int Compare(object? x, object? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        
        if (x is string fileA && y is string fileB)
            return Compare(fileA, fileB);
            
        throw new ArgumentException("Parameters must be strings");
    }

    public int Compare(string? fileA, string? fileB)
    {
        if (fileA == null && fileB == null) return 0;
        if (fileA == null) return -1;
        if (fileB == null) return 1;

        char[] arr1 = fileA.ToCharArray();
        char[] arr2 = fileB.ToCharArray();
        int i = 0, j = 0;
        
        while (i < arr1.Length && j < arr2.Length)
        {
            if (char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
            {
                string s1 = "", s2 = "";
                while (i < arr1.Length && char.IsDigit(arr1[i]))
                {
                    s1 += arr1[i];
                    i++;
                }
                while (j < arr2.Length && char.IsDigit(arr2[j]))
                {
                    s2 += arr2[j];
                    j++;
                }
                if (int.Parse(s1) > int.Parse(s2))
                {
                    return 1;
                }
                if (int.Parse(s1) < int.Parse(s2))
                {
                    return -1;
                }
            }
            else
            {
                if (arr1[i] > arr2[j])
                {
                    return 1;
                }
                if (arr1[i] < arr2[j])
                {
                    return -1;
                }
                i++;
                j++;
            }
        }
        
        if (arr1.Length == arr2.Length)
        {
            return 0;
        }
        else
        {
            return arr1.Length > arr2.Length ? 1 : -1;
        }
    }
}