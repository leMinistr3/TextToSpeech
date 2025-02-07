using Microsoft.CognitiveServices.Speech.Audio;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeech.Model
{
    public class SegmentModel
    {
        public int order { get; set; }
        public string wavPath { get; set; }
        public byte[] audioBytes { get; set; }
        public SubtitleItem subPath { get; set; }
        public TimeSpan audioDuration { get; set; }

        public SegmentModel(int myOrder, SubtitleItem subtitleItem, byte[] audiobytes, TimeSpan duration)
        {
            order = myOrder;
            subPath = subtitleItem;
            audioBytes = audiobytes;
            audioDuration = duration;
            wavPath = string.Empty;
        }
    }
}
