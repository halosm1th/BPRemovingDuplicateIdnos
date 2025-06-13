// UI now shows full XML side by side as text with line numbers and wrapped long lines
using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace DefaultNamespace;

public class XmlComparerUI
{
    private Logger? logger { get; set; }
    public XMLDataEntry? Run(XMLDataEntry entryA, XMLDataEntry entryM, Logger _logger)
    {
        XDocument docA = LoadDoc(entryA.PNFileName, "A Document");
        XDocument docM = LoadDoc(entryM.PNFileName, "M Document");
        logger = _logger;
        
        if (docA == null || docM == null) return null;
        var bpEntry = GetBPEntry(entryA.BPNumber, logger);

        Console.WriteLine($"\nComparing: {entryA.PNNumber} (A/Left) ↔ {entryM.PNNumber} (M/Right) || {bpEntry.BPNumber} (BP). ");

        
        
        PrintHeader(entryA.PNFileName, entryM.PNFileName, bpEntry.BPNumber);
        PrintFullTextSideBySide(docA, docM, bpEntry);

        return AskCommand(entryA, entryM);
    }

    private BPDataEntry? GetBPEntry(string? entryABpNumber, Logger logger)
    {
        var gatherer = new BPEntryGatherer(logger);
        var entry = gatherer.GetEntry(entryABpNumber);
        return entry;
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

    private void PrintHeader(string fileA, string fileM, string BPNumber)
    {
        string leftLabel = $"A: {Path.GetFileName(fileA)}";
        string rightLabel = $"M: {Path.GetFileName(fileM)}";
        string bpLabel = $"BP: {Path.GetFileName(BPNumber)}";
        Console.WriteLine("\n╔" + new string('═', 58) + "╦" + new string('═', 58) + "╦" + new string('═', 58) + "╗");
        Console.WriteLine($"║ {leftLabel,-56} ║ {rightLabel,-56} ║ {bpLabel,-56} ║");
        Console.WriteLine("╠" + new string('═', 58) + "╬" + new string('═', 58)+ "╬" + new string('═', 58)  + "╣");
    }

    private void PrintFullTextSideBySide(XDocument docA, XDocument docM, BPDataEntry entry)
    {
        var aLines = GetWrappedLinesWithNumbers(docA);
        var mLines = GetWrappedLinesWithNumbers(docM);
        var bpLines = GetWrappedLinesWithNumbers(entry);
        int maxLines = Math.Max(aLines.Count, mLines.Count);
        maxLines = Math.Max(maxLines, bpLines.Count);

        for (int i = 0; i < maxLines; i++)
        {
            string left = i < aLines.Count ? aLines[i] : "";
            string centre = i < mLines.Count ? mLines[i] : "";
            string right = i < bpLines.Count ? bpLines[i] : "";
            Console.WriteLine($"║ {left,-56} ║ {centre,-56} ║ {right, -56} ║");
        }

        Console.WriteLine("╚" + new string('═', 58) + "╩" + new string('═', 58)+ "╩" + new string('═', 58) + "╝");
    }

    private List<string> GetWrappedLinesWithNumbers(XDocument doc)
    {
        using var sw = new StringWriter();
        doc.Save(sw, SaveOptions.None);
        var rawLines = sw.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        return WrapLinesWithFixedLineNumbers(rawLines);
    }

    private List<string> GetWrappedLinesWithNumbers(BPDataEntry entry)
    {
        var rawLines = entry.ToDisplayString().Split(new[] { Environment.NewLine }, StringSplitOptions.TrimEntries);
        return WrapLinesWithFixedLineNumbers(rawLines);
    }
    
    
    private List<string> WrapLinesWithFixedLineNumbers(string[] rawLines)
    {
        var wrapped = new List<string>();
        const int lineNumberWidth = 4; // Adjust width to allow for padding (e.g., " 12: ")
        int contentWidth = 56 - (lineNumberWidth + 2); // 2 for ": "

        foreach (var (line, index) in rawLines.Select((val, idx) => (val, idx)))
        {
            string prefix = $"{index + 1, lineNumberWidth}: ";
            string remaining = line;

            while (remaining.Length > contentWidth)
            {
                wrapped.Add(prefix + remaining.Substring(0, contentWidth));
                remaining = remaining.Substring(contentWidth);
                prefix = new string(' ', lineNumberWidth + 2); // blank prefix for wrapped lines
            }
            wrapped.Add(prefix + remaining);
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
