using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class LRCManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    public LRCFormat lrc;

    private Regex lrcRegex = new Regex(@"\[(?<mm>\d\d):(?<ss>\d\d).(?<xx>\d\d)\](?<lyric>.*)|\[(?<tag>.*):(?<ctx>.*)\]");

    public void OnButtonPress()
    {
        lrc.Clear();
        LoadLrcText(inputField.text);
    }

    public void LoadLrcText(string text)
    {
        MatchCollection matches = lrcRegex.Matches(text);

        foreach (Match match in matches)
        {
            InsertIdTag(match.Groups["tag"].Value, match.Groups["ctx"].Value);
            InsertLyric(
                match.Groups["mm"].Value,
                match.Groups["ss"].Value,
                match.Groups["xx"].Value,
                match.Groups["lyric"].Value
                );
        }
    }

    private void InsertIdTag(string tag, string ctx)
    {
        if (tag == string.Empty) return;
        switch (tag)
        {
            case "ar":
                lrc.artist = ctx;
                break;

            case "al":
                lrc.album = ctx;
                break;

            case "ti":
                lrc.title = ctx;
                break;

            case "au":
                lrc.author = ctx;
                break;

            case "length":
                lrc.length = ctx;
                break;

            case "by":
                lrc.madeBy = ctx;
                break;

            case "offset":
                lrc.offset = double.Parse(ctx.Substring(1));
                lrc.offset *= ctx.Substring(0, 1) == "-" ? -1 : 1;
                break;

            case "re":
                lrc.editor = ctx;
                break;

            case "ve":
                lrc.version = ctx;
                break;

            default:
                Debug.LogWarning("\"" + tag + "\" is not a known tag.");
                break;
        }
    }

    private void InsertLyric(string mm, string ss, string xx, string lyric)
    {
        if (mm == string.Empty) return;
        lrc.lyrics.Add(new LRCLyricObject(float.Parse(mm), float.Parse(ss), float.Parse(xx), lyric));
    }
}

[System.Serializable]
public class LRCFormat
{
    /// <summary>
    /// ID Tag: ar
    /// Lyrics artist
    /// </summary>
    public string artist;
    /// <summary>
    /// ID Tag: al
    /// Album where the song is from
    /// </summary>
    public string album;
    /// <summary>
    /// ID Tag: ti
    /// Lyrics (song) title
    /// </summary>
    public string title;
    /// <summary>
    /// ID Tag: au
    /// Creator of the Songtext
    /// </summary>
    public string author;
    /// <summary>
    /// ID Tag: length
    /// How long the song is
    /// </summary>
    public string length;
    /// <summary>
    /// ID Tag: by
    /// Creator of the LRC file
    /// </summary>
    public string madeBy;
    /// <summary>
    /// ID Tag: offset
    /// +/- Overall timestamp adjustment in milliseconds, + shifts time up, - shifts down i.e. a positive value causes lyrics to appear sooner, a negative value causes them to appear later
    /// </summary>
    public double offset;
    /// <summary>
    /// ID Tag: re
    /// The player or editor that created the LRC file
    /// </summary>
    public string editor;
    /// <summary>
    /// ID Tag: ve
    /// version of program
    /// </summary>
    public string version;

    public List<LRCLyricObject> lyrics;
    public void Clear()
    {
        artist = string.Empty; 
        album = string.Empty;
        title = string.Empty;
        author = string.Empty;
        length = string.Empty;
        madeBy = string.Empty;
        offset = 0;
        editor = string.Empty;
        version = string.Empty;

        lyrics.Clear();
    }
}

[System.Serializable]
public struct LRCLyricObject
{
    public double time;
    public float minute;
    public float second;
    public float hundredth;
    public string lyric;

    public LRCLyricObject(float minute, float second, float hundredth, string lyric)
    {
        this.minute = minute;
        this.second = second;
        this.hundredth = hundredth;
        this.time = (minute * 60d) + second + (hundredth * .01d);
        this.lyric = lyric;
    }
}