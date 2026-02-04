# BPRemovingDuplicateIdnos

A .NET 9 C# console application to detect and resolve duplicate BP identifiers in your idp.data/Biblio XML corpus.  It:

1. **Discovers** all XML files under your `idp.data/Biblio` archive (recursively).  
2. **Extracts** each file’s BP number and other metadata via `XMLEntryGatherer`.  
3. **Sorts & finds duplicates**—any two files sharing the same BP number, where one file has `title[@level="a"]` and the other has `title[@level="m"]` (as the XSLT that drove the creation of PN Biblio created two files – both an article/chapter as well as the book that contains it – from a single BP fiche, which is the origin of the duplicates).
4. **Prompts** you (via `XmlComparerUI`) to choose which of the two duplicate entries should have its `seg[@resp="#BP"]` or `note[@resp="#BP"]` elements removed.  
5. **Deletes** the selected segments in the losing file, saves the updated XML, and logs every action.

---

## 🗂️ Folder Layout
The BPRemovingDuplicateIdnos project folder should be a sibling of idp.data in the local directory. 
```
├─idp.data/
│   └── Biblio
└─BPRemovingDuplicateIdnos/            ← C# console project
    ├── bin/
    ├── obj/
    ├── BPDataEntry.cs
    ├── BPEntryGatherer.cs
    ├── BPRemovingDuplicateIdnos.sln         ← Visual Studio solution
    ├── BPRemovingDuplicateIdnos.csproj
    ├── Program.cs                       ← entry point & orchestration
    ├── Logger.cs                        ← simple file‑and‑console logger
    ├── XMLEntryGatherer.cs              ← gathers & parses each TEI file
    ├── XMLDataEntry.cs                  ← model for parsed TEI fields
    ├── XmlComparerUI.cs                 ← console UI for duplicate resolution
    └── … (other helpers)
```
> The tool locates `idp.data` by walking up from your current directory, then finds the first subdirectory whose name contains “Biblio.” :contentReference[oaicite:1]{index=1}

---

## ⚙️ Prerequisites & Installation

1. **.NET 9 SDK**  
   Download & install from https://aka.ms/dotnet-download  
2. **Clone & restore**  
   ```bash
   git clone https://github.com/halosm1th/BPRemovingDuplicateIdnos.git
   cd BPRemovingDuplicateIdnos/BPRemovingDuplicateIdnos
   dotnet restore
    ```

---

## 🚀 Usage

From within the `BPRemovingDuplicateIdnos` project folder (where `Program.cs` lives):

```bash
dotnet run
```

You’ll see console output:

1. **Current directory** and location of `idp.data` & `Biblio`.
2. **List of duplicate pairs**:

   ```
   Found match in 1932‑BP1234 and 1932‑BP1234
   ```
3. **Interactive prompt** for each pair:

   ```
   Choose file to DELETE segments from:
   [A] 1932‑BP1234.xml (“a” title level)
   [M] 1932‑BP1234.xml (“m” title level)
   > M
   ```
4. **Deletes** all `<seg>` and `<note>` elements of the losing file, saves it, and logs the change.
5. **Log file** written by `Logger` (timestamped in working directory).

---

## 🔍 How It Works

* **`SetXMLFilepath()`**

  * Starts in your CWD, walks upward to find an `idp.data` directory.
  * Within that, finds the first subfolder containing “Biblio.”
* **`XMLEntryGatherer`**

  * Recursively reads every `.xml` file.
  * Constructs an `XMLDataEntry` holding BP number (`<idno type="bp">`), “title level” (`a` vs. `m`), and full path.
* **Duplicate detection**

  * Sorts entries by numeric BP value.
  * Any adjacent entries with equal BP are duplicates.
* **`XmlComparerUI`**

  * Presents both entries’ key fields for side‑by‑side comparison.
  * Returns the entry whose segments should be purged.
* **Deletion logic**

  * In `DeleteSegsOnFile()`, loads the losing file as `XmlDocument`, finds all `<seg>` or `<note>` nodes of various `@subtype` and removes them.
  * Optionally prompts before removing illustrations.
  * Saves the updated XML in place.

---

## 🐛 Troubleshooting

* **“Could not find idp.data directory”**

  * Ensure you run `dotnet run` from a directory that is sibling to the `idp.data` folder.
* **No duplicates found**

  * All BP numbers are unique—no action needed!
---

Created with the help of Chatgpt
