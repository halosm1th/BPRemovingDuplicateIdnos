// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using DefaultNamespace;

Console.WriteLine("Staring program");

Console.WriteLine("Setting xml path");
var xmlFilePath = SetXMLFilepath();

Console.WriteLine("Creating Logger");
var logger = new Logger();

Console.WriteLine("Gathering XML files");
var xmlFileGatherer = new XMLEntryGatherer(xmlFilePath, logger);
var entries = xmlFileGatherer.GatherEntries();
var matchingEntries = FindMatchingBPEntries(entries);

var comparedMatches = CompareMatchingEntries(matchingEntries);

foreach (var entry in comparedMatches)
{
    var strength = entry.LevelA.GetMatchStrength(entry.LevelM);
    var fg = Console.ForegroundColor;
    if (strength >= 9)
    {
        Console.ForegroundColor = ConsoleColor.Red;
    }else if (strength == 8)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
    }else if (strength == 7)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
    }else if (strength == 6)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
    }
    Console.WriteLine($"Found a troubling match between A: {entry.LevelA.PNNumber} & M: {entry.LevelM.PNNumber}. " +
                      $"Match Level (M compared to A): ({strength})");
    Console.ForegroundColor = fg;
}

//TODO process what happens once the match has been found.


static List<(XMLDataEntry LevelA, XMLDataEntry LevelM)> CompareMatchingEntries(
    List<(XMLDataEntry m1, XMLDataEntry m2)> matchingEntries)
{
    var entriesWeAreConcernedWith = new List<(XMLDataEntry LevelA, XMLDataEntry LevelM)>();

    foreach (var entry in matchingEntries)
    {
        XMLDataEntry? a = null;
        XMLDataEntry? m = null;
        
        if(entry.m1.TitleLevel == "a") a = entry.m1;
        else if (entry.m2.TitleLevel == "a") a = entry.m2;
        
        if(entry.m1.TitleLevel == "m") m = entry.m1;
        else if (entry.m2.TitleLevel == "m") m = entry.m2;

        if (a != null && m != null)
        {
            entriesWeAreConcernedWith.Add(new ValueTuple<XMLDataEntry, XMLDataEntry>(a,m));
        }
    }
    
    return entriesWeAreConcernedWith;
}

static List<(XMLDataEntry m1, XMLDataEntry m2)> FindMatchingBPEntries(List<XMLDataEntry> xmlDataEntries)
{
    var matches = new List<(XMLDataEntry m1, XMLDataEntry m2)>();
    xmlDataEntries.Sort(delegate(XMLDataEntry x, XMLDataEntry y)
                  {
                      if(BPNumbAsInt(x) > BPNumbAsInt(y)) return 1;
                      if (BPNumbAsInt(x) < BPNumbAsInt(y)) return -1;
                      if (BPNumbAsInt(x) == BPNumbAsInt(y)) return 0;
                      return 0;
                  });

    for (int i = 0; i < xmlDataEntries.Count; i++)
    {
        if (i + 1 == xmlDataEntries.Count) break;
        if ((xmlDataEntries[i].HasBPNum && xmlDataEntries[i + 1].HasBPNum)
            && (xmlDataEntries[i].BPNumber == xmlDataEntries[i + 1].BPNumber))
        {
            Console.WriteLine($"Found match in {xmlDataEntries[i].PNNumber} and {xmlDataEntries[i+1].PNNumber}");
            matches.Add(new ValueTuple<XMLDataEntry, XMLDataEntry>(xmlDataEntries[i],xmlDataEntries[i+1]));
        }
    }

    return matches;
}

static int BPNumbAsInt(XMLDataEntry entry)
{
    if (entry.HasBPNum)
    {
        var bpNumbCombined = entry.BPNumber.Replace("-", ""); 
        if (Int32.TryParse(bpNumbCombined, out var result))
        {
            return result;
        }
        else
        {
            //Console.WriteLine($"Error entry {entry.PNNumber} has an invalid BP Number {entry.BPNumber}");
            return int.MaxValue;
        }
        
    }
    else
    {
        //Console.WriteLine($"Error entry {entry.PNNumber} has no BP Number");
        return int.MaxValue;
    }
}

static string SetXMLFilepath()
{
    var currnetDir = Directory.GetCurrentDirectory();
    var idpDataDir = SearchForIDPDataDir(currnetDir);
    var biblioDir = GetBiblioDir(idpDataDir);
    
    return biblioDir;
}


static string GetBiblioDir(string? idpDataDir)
{
    var dirs = Directory.GetDirectories(idpDataDir);
    if (dirs.Any(x => x.ToLower().Contains("biblio")))
    {
        return dirs.First(x => x.ToLower().Contains("biblio"));
    }
    else
    {
        throw new DirectoryNotFoundException($"Could not find Biblio directory in: {idpDataDir}");
    }
}

static string SearchForIDPDataDir(string startPath)
{
    var dirs = Directory.GetDirectories(startPath);
    if (dirs.Length > 0)
    {
        if (dirs.Any(x => x.Contains("idp.data")))
        {
            var idpDir = dirs.First(x => x.Contains("idp.data"));
            
            return idpDir;
        } else if (startPath == "C:/" || startPath == "/")
        {
            throw new DirectoryNotFoundException("Could not find IDPData directory");
        }
        else
        {
            return (SearchForIDPDataDir(Directory.GetParent(startPath).FullName)); 
        }
    }
    else
    {
        return (SearchForIDPDataDir(Directory.GetParent(startPath).FullName)); 
    }

    throw new DirectoryNotFoundException("Could not find IDPData directory");
}


//TODO goals/objective
/*
nice to be able to see both version fo the xml side by side, with A on the left, and M on the right
Most frequent command is delete segs from one or the other (from A, or from M version based on title level)
And then have the segs actually be deleted, resaving them
not just hte segs to delete, wil lalso be the ntoe (resp="#bp")
highlight overlaps in green

commands:
ignore and move on to next
second command is delete segs, and if selected, we should be asked from which version
are you sure, yes, and then it deletes and immeaditely should write to disk
and because its git, will see changes, can diff them from git. Write changes directly to disk because git is the safety net
*/