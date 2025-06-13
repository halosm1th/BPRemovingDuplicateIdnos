using System.Text;

namespace DefaultNamespace;

public class BPDataEntry
{
    private string? _bpNum = null;
    private string? _cr = null;
    private string? _index = null;
    private string? _indexBis = null;
    private string? _internet = null;
    private string? _name = null;
    private string? _no = null;
    private string? _publication = null;
    private string? _resume = null;
    private string? _sbandseg = null;
    private string? _title = null;

    private string? annee = null;

    //At a minimum all entries must have one number
    public BPDataEntry(string? number, Logger logger)
    {
        BPNumber = number;
        this.logger = logger;
    }

    protected Logger logger { get; }

    public string? Index
    {
        get => _index;
        set => _index = ReplaceInvalidText(value);
    }

    public bool HasIndex => _index != null;

    public string? IndexBis
    {
        get => _indexBis;
        set => _indexBis = ReplaceInvalidText(value);
    }

    public bool HasIndexBis => _indexBis != null;

    public string? Title
    {
        get => _title;
        set => _title = ReplaceInvalidText(value);
    }

    public bool HasTitle => _title != null;

    public string? Publication
    {
        get => _publication;
        set => _publication = ReplaceInvalidText(value);
    }

    public bool HasPublication => _publication != null;

    public string? Internet
    {
        get => _internet;
        set => _internet = ReplaceInvalidText(value);
    }

    public bool HasInternet => _internet != null;

    public string? SBandSEG
    {
        get => _sbandseg;
        set => _sbandseg = ReplaceInvalidText(value);
    }

    public bool HasSBandSEG => _sbandseg != null;

    public string? No
    {
        get => _no;
        set => _no = ReplaceInvalidText(value);
    }

    public bool HasNo => _no != null;

    public string? Annee
    {
        get => annee;
        set => annee = ReplaceInvalidText(value);
    }

    public bool HasAnnee => annee != null;

    public string? Resume
    {
        get => _resume;
        set => _resume = ReplaceInvalidText(value);
    }

    public bool HasResume => _resume != null;

    public string? CR
    {
        get => _cr;
        set => _cr = ReplaceInvalidText(value);
    }

    public bool HasCR => _cr != null;

    public string? Name
    {
        get => _name;
        set => _name = ReplaceInvalidText(value);
    }

    public bool HasName => _name != null;

    public string? BPNumber
    {
        get => _bpNum;
        set => _bpNum = ReplaceInvalidText(value);
    }

    public bool HasBPNum => _bpNum != null;

    private string? ReplaceInvalidText(string? value)
    {
        if (logger != null)
        {
            logger.LogProcessingInfo($"Replacing invalid text in {value ?? ""}");
        }

        value = value?.Replace("&", "&amp;");
        value = value?.Replace("<", "&lt;");
        value = value?.Replace(">", "&gt;");
        if (logger != null)
        {
            logger.LogProcessingInfo($"Replaced text resulted in: {value}");
        }

        return value;
    }

    public override string ToString()
    {
        return $"{Name ?? ""} {Internet ?? ""} {Publication ?? ""} " +
               $"{Resume ?? ""} {Title ?? ""} {Index ?? ""} {IndexBis ?? ""} " +
               $"{No ?? ""} {CR ?? ""} {BPNumber ?? ""} {SBandSEG ?? ""}";
    }
    
    public string ToDisplayString()
    {
        return (HasIndex ? $"Index: {Index ?? "None"}{Environment.NewLine}" : "") +
               (HasIndexBis ? $"Index Bis: {IndexBis ?? "None"}{Environment.NewLine}" : "") +
               (HasPublication ? $"Publication: {Publication ?? "None"}{Environment.NewLine}" : "") +
               (HasResume ? $"Resume: {Resume ?? "None"}{Environment.NewLine}" : "") +
               (HasTitle ? $"Title: {Title ?? "None"}{Environment.NewLine}" : "") +
               (HasName ? $"Name: {Name ?? "None"}{Environment.NewLine}" : "")+
               (HasCR ? $"CR: {CR ?? "None"}{Environment.NewLine}" : "") +
               (HasSBandSEG ? $"SB & SEG: {SBandSEG ?? "None"}" : "");
    }
}