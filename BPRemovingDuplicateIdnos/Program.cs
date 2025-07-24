// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using DefaultNamespace;

class BPRemovingDuplicates
{
    private static Logger logger;
    public static void Main(string[] args)
    {
        Console.WriteLine("Staring program");
        Console.WriteLine("Creating Logger");
        logger = new Logger();
        logger.Log("I, the logger, have come into being.");

        Console.WriteLine("Setting xml path");
        logger.Log("Setting xmlPath");
        var xmlFilePath = SetXMLFilepath();


        Console.WriteLine("Gathering XML files");
        logger.Log("Creating and gathering xml entries.");
        var xmlFileGatherer = new XMLEntryGatherer(xmlFilePath, logger);
        var entries = xmlFileGatherer.GatherEntries();
        
        logger.Log("Collecting matching XMl entries from gathered entries.");
        var matchingEntries = FindMatchingBPEntries(entries);
        logger.Log($"Found {matchingEntries.Count} matching entries.");
        
        logger.Log("Comparing matching entries to one another to find a/m title pairs.");
        var comparedMatches = CompareMatchingEntries(matchingEntries);
        logger.Log($"Found {comparedMatches.Count} entries to compare.");

        logger.Log("Processing Copmarisons");
        ProcessComparisons(comparedMatches);

    }

    private static void ProcessComparisons(List<(XMLDataEntry LevelA, XMLDataEntry LevelM)> comparedMatches)
    {
        logger.LogProcessingInfo($"Processing {comparedMatches.Count} comparisons.");
        Console.WriteLine($"Handling {comparedMatches.Count} comparisons.");
        if (comparedMatches.Count > 0)
        {
            logger.LogProcessingInfo("Starting XmlUIComparer");
            var xmlUI = new XmlComparerUI();
            
            foreach (var entry in comparedMatches)
            {
                logger.LogProcessingInfo($"Running XMLUIComparer to check {entry.LevelA.PNNumber} against {entry.LevelM.PNNumber}");
                var entryToDelete = xmlUI.Run(entry.LevelA, entry.LevelM, logger);
                var spareEntry = entryToDelete == entry.LevelA? entry.LevelM: entry.LevelA;
                
                var text = entryToDelete != null ? entryToDelete.PNNumber : "Neither";
                Console.WriteLine($"Finished comparision. File to delete segs of is: {text}");
                logger.LogProcessingInfo($"Finished comparision. File to delete segs of is: {text}");
                
                logger.LogProcessingInfo("Moving to delete segs.");
                DeleteSegsOnFile(entryToDelete, spareEntry);
                logger.LogProcessingInfo("Segs deleted.");
            }
        }
    }


    static void DeleteSegsOnFile(XMLDataEntry? xmlDataEntry, XMLDataEntry? otherEntry)
    {
        if (xmlDataEntry == null) return;
        logger.LogProcessingInfo($"Deleting the segs on the file {xmlDataEntry.PNFileName}");
        
        var file = new XmlDocument();

        // Create namespace manager
        var nsManager = new XmlNamespaceManager(file.NameTable);
        nsManager.AddNamespace("tei", "http://www.tei-c.org/ns/1.0");

        Console.WriteLine("Loading file");
        logger.LogProcessingInfo("Loading file.");
        
        file.Load(xmlDataEntry.PNFileName);
        
        var root = file.DocumentElement;
        if (root == null) throw new FileNotFoundException("Error finding file to delete segs from");
        
        var filename = xmlDataEntry.PNFileName;
        Console.WriteLine($"File {filename} loaded. Now finding segs to remove.");
        logger.LogProcessingInfo($"File {filename} loaded. Now finding segs to remove.");


        var name = root.SelectSingleNode("//tei:seg[@subtype='nom'][@resp='#BP']", nsManager);
        var idno = root.SelectSingleNode("//tei:idno[@type='bp']", nsManager);
        var idnoOldBP = root.SelectSingleNode("//tei:idno[@type='bp_old']", nsManager);
        var index = root.SelectSingleNode("//tei:seg[@subtype='index'][@resp='#BP']", nsManager);
        var indexBis = root.SelectSingleNode("//tei:seg[@subtype='indexBis']", nsManager);
        var title = root.SelectSingleNode("//tei:seg[@subtype='titre']", nsManager);
        var publisher = root.SelectSingleNode("//tei:seg[@subtype='publication']", nsManager);
        var resume = root.SelectSingleNode("//tei:seg[@subtype='resume']", nsManager);
        var sbandSeg = root.SelectSingleNode("//tei:seg[@subtype='sbSeg']", nsManager);
        var cr = root.SelectSingleNode("//tei:seg[@subtype='cr']", nsManager);
        var internet = root.SelectSingleNode("//tei:seg[@subtype='internet']", nsManager);
        var note = root.SelectSingleNode("//tei:note[@resp='#BP']", nsManager);
        var illustration = root.SelectSingleNode("//tei:note[@type='illustration']", nsManager);

        RemoveItem(root, name, "name", filename);
        RemoveItem(root, idno, "idno bp", filename);
        RemoveItem(root, idnoOldBP, "idno bp_old", filename);
        RemoveItem(root, index, "index", filename);
        RemoveItem(root, indexBis, "indexBis", filename);
        RemoveItem(root, title, "title", filename);
        RemoveItem(root, publisher, "publisher", filename);
        RemoveItem(root, note, "note", filename);
        RemoveItem(root, resume, "resume", filename);
        RemoveItem(root, sbandSeg, "sbandSeg", filename);
        RemoveItem(root, cr, "cr", filename);
        RemoveItem(root, internet, "internet", filename);

        if (illustration != null)
        {
            Console.WriteLine($"Entry {xmlDataEntry.PNFileName} has an illustration:\n\t{illustration.InnerXml}.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Do you want to delete this illustration? (Enter y to delete)");
            Console.ResetColor();

            var delete = Console.ReadLine().ToLower();
            if (delete == "y")
            {
                RemoveItem(root, illustration, "illustration", filename);
                var otherDoc = new XmlDocument();
                otherDoc.Load(otherEntry.PNFileName);
                var import = otherDoc.ImportNode(illustration.CloneNode(true), true);
                otherDoc?.DocumentElement?.InsertAfter(import, otherDoc.DocumentElement.LastChild);
                otherDoc?.Save(otherEntry.PNFileName);
                Console.WriteLine("added illustration to: {otherEntry.PNFileName}.}");
                logger.LogProcessingInfo("added illustration to: {otherEntry.PNFileName}.}");
            }
        }
        
        logger.LogProcessingInfo($"Removed segs from {filename}");
        logger.Log($"Removed segs from {filename}");

        Console.WriteLine("finished updating selected file, saving.");
        logger.LogProcessingInfo($"Finished updating {xmlDataEntry.PNFileName}.");
        file.Save(xmlDataEntry.PNFileName);
        logger.LogProcessingInfo($"Saved updated file {xmlDataEntry.PNFileName}.");
        Console.WriteLine($"Saved updated file {xmlDataEntry.PNFileName}.");
    }

    static void RemoveItem(XmlElement root, XmlNode? childNode, string name, string fileName)
    {
        if (childNode != null)
        {
            logger.LogProcessingInfo($"Trying to remove {name} from {Path.GetFileName(fileName)}.");
            root.RemoveChild(childNode);
            Console.WriteLine($"Removed {name} from {Path.GetFileName(fileName)}.");
            logger.LogProcessingInfo($"Removed {name} from {Path.GetFileName(fileName)}.");}
        else
        {
            logger.LogProcessingInfo($"No node to delete with {name} in {fileName}.");
        }
    }

    static List<(XMLDataEntry LevelA, XMLDataEntry LevelM)> CompareMatchingEntries(
        List<(XMLDataEntry m1, XMLDataEntry m2)> matchingEntries)
    {
        var entriesWeAreConcernedWith = new List<(XMLDataEntry LevelA, XMLDataEntry LevelM)>();

        foreach (var entry in matchingEntries)
        {
            XMLDataEntry? a = null;
            XMLDataEntry? m = null;

            if (entry.m1.TitleLevel == "a") a = entry.m1;
            else if (entry.m2.TitleLevel == "a") a = entry.m2;

            if (entry.m1.TitleLevel == "m") m = entry.m1;
            else if (entry.m2.TitleLevel == "m") m = entry.m2;

            if (a != null && m != null)
            {
                entriesWeAreConcernedWith.Add(new ValueTuple<XMLDataEntry, XMLDataEntry>(a, m));
                logger.LogProcessingInfo($"Matching entry {entry.m1.TitleLevel} with {entry.m2.TitleLevel}. ");
            }
        }

        return entriesWeAreConcernedWith;
    }

    static List<(XMLDataEntry m1, XMLDataEntry m2)> FindMatchingBPEntries(List<XMLDataEntry> xmlDataEntries)
    {
        var matches = new List<(XMLDataEntry m1, XMLDataEntry m2)>();
        xmlDataEntries.Sort(delegate(XMLDataEntry x, XMLDataEntry y)
        {
            if (BPNumbAsInt(x) > BPNumbAsInt(y)) return 1;
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
                Console.WriteLine($"Found match in {xmlDataEntries[i].PNNumber} and {xmlDataEntries[i + 1].PNNumber}");
                logger.LogProcessingInfo($"Found match in {xmlDataEntries[i].PNNumber} and {xmlDataEntries[i + 1].PNNumber}");
                matches.Add(new ValueTuple<XMLDataEntry, XMLDataEntry>(xmlDataEntries[i], xmlDataEntries[i + 1]));
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
        logger.LogProcessingInfo($"Current dir: {currnetDir}");
        var idpDataDir = SearchForIDPDataDir(currnetDir);
        var biblioDir = GetBiblioDir(idpDataDir);

        return biblioDir;
    }


    static string GetBiblioDir(string? idpDataDir)
    {
        logger.LogProcessingInfo("Getting biblioDirectory");
        
        var dirs = Directory.GetDirectories(idpDataDir);
        if (dirs.Any(x => x.ToLower().Contains("biblio")))
        {
            var dir = dirs.First(x => x.ToLower().Contains("biblio"));
            logger.LogProcessingInfo($"Found directory: {dir}");
            return dir;
        }
        else
        {
            var error = new DirectoryNotFoundException($"Could not find Biblio directory in: {idpDataDir}");
            logger.LogProcessingInfo("could not find biblio directory");
            logger.LogError("Could not find biblio directory, make sure its in the idp.data directory", error);
            throw error;
        }
    }

    static string SearchForIDPDataDir(string startPath)
    {
        logger.LogProcessingInfo("Searching for IPDDataDir");
        var dirs = Directory.GetDirectories(startPath);
        if (dirs.Length > 0)
        {
            if (dirs.Any(x => x.Contains("idp.data")))
            {
                var idpDir = dirs.First(x => x.Contains("idp.data"));
                logger.LogProcessingInfo($"Found IDPDATA directory: {idpDir}");
                return idpDir;
            }
            else if (startPath == "C:/" || startPath == "/")
            {
                var error = new DirectoryNotFoundException("Could not find IDPData directory");
                logger.LogProcessingInfo("Could not find IDPDir.");
                logger.LogError("Error in finding idpdata directory. Please make sure that the directory is one level above hte folder where I am located.!", error);
                throw error;
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
}



//Easier if it goes up a step to parent directory and down a step into idp
//WHen going to delete, be more specific, have segs with a resp="#bp" flag.
//If there is a note element and the resp="BP" or resp="#BP" when its pulled from resume,the N attribute doesn't matter.

//For the logging segs removed from (filename)

/*
Illustration comes from Bp segs note type="illustrsation"
*/