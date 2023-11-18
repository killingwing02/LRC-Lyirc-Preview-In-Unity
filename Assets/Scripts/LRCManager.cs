using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class LRCManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    public LRCFormat lrc;

    // Regex refence
    // https://stackoverflow.com/questions/14831484/validate-with-regex-for-lrc-file-in-javascript
    private Regex lrcTagRegex = new Regex(@"\[(?<tag>[a-z]+):(?<ctx>.*)\]");
    private Regex lrcRegex = new Regex(@"\[(?<mm>\d{2}):(?<ss>\d{2})(?>.(?<xx>\d{2}))?\](?>(?<gender>[FMD]): )?(?<lyric>.*)");

    public void OnButtonPress()
    {
        lrc.Clear();
        LoadLrcText(inputField.text);
    }

    public void LoadLrcText(string text)
    {
        MatchCollection matches = lrcTagRegex.Matches(text);
        foreach (Match match in matches)
        {
            InsertIdTag(match.Groups["tag"].Value, match.Groups["ctx"].Value);
        }

        InsertLyric(text);

        lrc.lyrics.Sort();
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

    private void InsertLyric(string lyricText)
    {
        if (lyricText == string.Empty) return;
        MultiLyricCheck(lyricText);
    }

    private LRCLyricObject MultiLyricCheck(string lyric)
    {
        LRCLyricObject lyricObj = new LRCLyricObject();
        var matches = lrcRegex.Matches(lyric);

        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                lyricObj.minute = float.Parse(match.Groups["mm"].Value);
                lyricObj.second = float.Parse(match.Groups["ss"].Value);
                lyricObj.hundredth = float.Parse(match.Groups["xx"].Value == string.Empty ? "0" : match.Groups["xx"].Value);
                lyricObj.lyric = MultiLyricCheck(match.Groups["lyric"].Value).lyric;

                switch (match.Groups["gender"].Value)
                {
                    case "M":
                        lyricObj.gender = Gender.Male;
                        break;
                    case "F":
                        lyricObj.gender = Gender.Female;
                        break;
                    case "D":
                        lyricObj.gender = Gender.Duet;
                        break;
                    default:
                        lyricObj.gender = Gender.None;
                        break;
                }

                lyricObj.Time();
                lrc.lyrics.Add(lyricObj);

            }
        }
        else
        {
            lyricObj.lyric = lyric;
        }

        return lyricObj;
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
public struct LRCLyricObject : IComparable<LRCLyricObject>
{
    public double time;
    public float minute;
    public float second;
    public float hundredth;
    public Gender gender;
    public string lyric;

    public double Time()
    {
        time = (minute * 60d) + second + (hundredth * .01d);
        return time;
    }

    public LRCLyricObject(float minute, float second, float hundredth, string lyric, Gender gender = Gender.None)
    {
        this.minute = minute;
        this.second = second;
        this.hundredth = hundredth;
        this.gender = gender;
        time = (minute * 60d) + second + (hundredth * .01d);
        this.lyric = lyric;
    }

    public int CompareTo(LRCLyricObject other)
    {
        return Time().CompareTo(other.Time());
    }
}

public enum Gender
{
    None = 0,
    Male = 1,
    Female = 2,
    Duet = 3
}