using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class CaptionDisplay : MonoBehaviour
{
    public TextAsset captionFile; // Reference to the text file containing captions
    public VideoPlayer videoPlayer; // Reference to the VideoPlayer component
    public TextMeshProUGUI textMesh; // Reference to the TextMeshPro object for displaying captions

    public List<Caption> captions = new List<Caption>();
    private int currentCaptionIndex = 0;

    [Serializable]
    public class Caption
    {
        public TimeSpan startTime;
        public TimeSpan endTime;
        public string text;
    }

    void Start()
    {
        if (captionFile != null)
        {
            captions.Add(new Caption { 
               endTime=new TimeSpan(0, 0, 0, 0),
               text = "test",
               startTime = new TimeSpan(0, 0,0,0),
            });
            Debug.Log("Loaded Caption File:\n" + captionFile.text);

            // Parse the captions from the text file
            ParseCaptions();
            videoPlayer.sendFrameReadyEvents = true;

            // Hook into the VideoPlayer's time event
            videoPlayer.started += OnVideoStarted;
            videoPlayer.frameReady += OnVideoFrameReady;
        }
        else
        {
            Debug.LogError("Caption file not assigned.");
        }
    }


    private void ParseCaptions()
    {
        string[] lines = captionFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Caption currentCaption = null;
        Regex timeRegex = new Regex(@"(\d\d:\d\d:\d\d,\d{3}) - (\d\d:\d\d:\d\d,\d{3})");

        foreach (string line in lines)
        {
            if (line.StartsWith("0") && timeRegex.IsMatch(line))
            {
                Match match = timeRegex.Match(line);
                TimeSpan startTime = TimeSpan.ParseExact(match.Groups[1].Value, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
                TimeSpan endTime = TimeSpan.ParseExact(match.Groups[2].Value, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);

                currentCaption = new Caption
                {
                    startTime = startTime,
                    endTime = endTime
                };
            }
            else if (currentCaption != null)
            {
                currentCaption.text = line.Trim();
                captions.Add(currentCaption);
                currentCaption = null;
            }
        }
    }



    private void OnVideoStarted(VideoPlayer vp)
    {
        // Start playing the video
        vp.Play();
    }

    private void OnVideoFrameReady(VideoPlayer vp, long frameIdx)
    {
        float currentTime = (float)vp.time;


        // Check if it's time to display a new caption
        while (currentCaptionIndex < captions.Count && TimeSpan.FromSeconds(currentTime) + TimeSpan.FromSeconds(0.1) >= captions[currentCaptionIndex].startTime)
        {
            if (TimeSpan.FromSeconds(currentTime) <= captions[currentCaptionIndex].endTime)
            {
                // Display the caption
                textMesh.text = captions[currentCaptionIndex].text;
                return;
            }
            currentCaptionIndex++;
        }

        // If no caption should be displayed, clear the text
        textMesh.text = "";
    }
}
