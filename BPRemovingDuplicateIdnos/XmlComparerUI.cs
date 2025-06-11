// UI now shows full XML side by side as text with line numbers and wrapped long lines
using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace DefaultNamespace;

public class XmlComparerUI
{
    public XMLDataEntry? Run(XMLDataEntry entryA, XMLDataEntry entryM)
    {
        XDocument docA = LoadDoc(entryA.PNFileName, "A Document");
        XDocument docM = LoadDoc(entryM.PNFileName, "M Document");

        if (docA == null || docM == null) return null;

        Console.WriteLine($"\nComparing: {entryA.PNNumber} (A/Left) ↔ {entryM.PNNumber} (M/Right)");

        PrintHeader(entryA.PNFileName, entryM.PNFileName);
        PrintFullTextSideBySide(docA, docM);

        return AskCommand(entryA, entryM);
    }

    private XDocument LoadDoc(string path, string label)
    {
        try
        {
            return XDocument.Load(path, LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error loading {label} ({path}): {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    private void PrintHeader(string fileA, string fileM)
    {
        string leftLabel = $"A: {Path.GetFileName(fileA)}";
        string rightLabel = $"M: {Path.GetFileName(fileM)}";
        Console.WriteLine("\n╔" + new string('═', 58) + "╦" + new string('═', 58) + "╗");
        Console.WriteLine($"║ {leftLabel,-56} ║ {rightLabel,-56} ║");
        Console.WriteLine("╠" + new string('═', 58) + "╬" + new string('═', 58) + "╣");
    }

    private void PrintFullTextSideBySide(XDocument docA, XDocument docM)
    {
        var aLines = GetWrappedLinesWithNumbers(docA);
        var mLines = GetWrappedLinesWithNumbers(docM);
        int maxLines = Math.Max(aLines.Count, mLines.Count);

        for (int i = 0; i < maxLines; i++)
        {
            string left = i < aLines.Count ? aLines[i] : "";
            string right = i < mLines.Count ? mLines[i] : "";
            Console.WriteLine($"║ {left,-56} ║ {right,-56} ║");
        }

        Console.WriteLine("╚" + new string('═', 58) + "╩" + new string('═', 58) + "╝");
    }

    private List<string> GetWrappedLinesWithNumbers(XDocument doc)
    {
        using var sw = new StringWriter();
        doc.Save(sw, SaveOptions.None);
        var rawLines = sw.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var wrapped = new List<string>();
        foreach (var (line, index) in rawLines.Select((val, idx) => (val, idx)))
        {
            string numbered = $"{index + 1,3}: {line}";
            if (numbered.Length <= 56)
            {
                wrapped.Add(numbered);
            }
            else
            {
                wrapped.Add(numbered.Substring(0, 56));
                int start = 56;
                while (start < numbered.Length)
                {
                    int length = Math.Min(53, numbered.Length - start);
                    wrapped.Add("     " + numbered.Substring(start, length));
                    start += length;
                }
            }
        }
        return wrapped;
    }

    private XMLDataEntry? AskCommand(XMLDataEntry entryA, XMLDataEntry entryM)
    {
        while (true)
        {
            Console.Write("\nCommand? (S = Skip, D = Delete <segs>): ");
            string input = Console.ReadLine()?.Trim().ToUpper();
            if (input == "S")
            {
                return null;
            }
            else if (input == "D")
            {
                Console.Write("Delete <segs> from which version? (A/M): ");
                string target = Console.ReadLine()?.Trim().ToUpper();
                if (target != "A" && target != "M") continue;

                Console.Write("Are you sure? (Y/N): ");
                string confirm = Console.ReadLine()?.Trim().ToUpper();
                if (confirm == "Y")
                {
                    var targetEntry = target == "A" ? entryA : entryM;
                    Console.WriteLine($"Marked for deletion of <segs>: {targetEntry.PNFileName}");
                    return targetEntry;
                }
                else
                {
                    Console.WriteLine("Cancelled.");
                    return null;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Invalid command.");
                Console.ResetColor();
            }
        }
    }

    private string Truncate(string str, int maxLength)
    {
        return str.Length <= maxLength ? str : str.Substring(0, maxLength - 3) + "...";
    }
}
