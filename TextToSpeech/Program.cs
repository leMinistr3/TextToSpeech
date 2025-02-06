using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using SubtitlesParser.Classes;
using System.Diagnostics;
using TextToSpeech.AzureSynthesizer;
using TextToSpeech.AudioProcess;
using TextToSpeech.Model;

namespace TextToSpeech
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            AppConfig.WriteOnDisk = (config["WriteOnDisk"] == "True");
            AppConfig.SpeechKey = config["SpeechKey"];
            AppConfig.SpeechRegion = config["SpeechRegion"];
            AppConfig.SpeechVoice = config["SpeechVoice"];
            AppConfig.VttFilePath = config["VttFilePath"];

            if (string.IsNullOrEmpty(AppConfig.SpeechKey))
            {
                throw new ArgumentNullException(nameof(AppConfig.SpeechKey));
            }
            if (string.IsNullOrEmpty(AppConfig.SpeechRegion))
            {
                throw new ArgumentNullException(nameof(AppConfig.SpeechRegion));
            }
            if (string.IsNullOrEmpty(AppConfig.SpeechVoice))
            {
                throw new ArgumentNullException(nameof(AppConfig.SpeechVoice));
            }
            if (string.IsNullOrEmpty(AppConfig.VttFilePath))
            {
                throw new ArgumentNullException(nameof(AppConfig.VttFilePath));
            }

            VttFileToSpeech vttToSpeech = new VttFileToSpeech(AppConfig.SpeechKey, AppConfig.SpeechRegion, AppConfig.SpeechVoice);

            List<SegmentModel> segments = await vttToSpeech.VttFilePathToSegmentListAsync(AppConfig.VttFilePath, AppConfig.WriteOnDisk);

            segments = await Ffmpeg.CutAzureExtra800(segments);
            
            Console.ReadLine();
        }
    }
}
