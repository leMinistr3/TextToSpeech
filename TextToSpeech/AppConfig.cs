using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeech
{
    public static class AppConfig
    {
        public static bool WriteOnDisk { get; set; }
        public static string? SpeechKey { get; set; }
        public static string? SpeechRegion { get; set; }
        public static string? SpeechVoice { get; set; }
        public static string? VttFilePath { get; set; }
    }
}
