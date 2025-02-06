using Microsoft.CognitiveServices.Speech;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestConsole.AudioProcess.Wave;
using TextToSpeech.Model;

namespace TextToSpeech.AzureSynthesizer
{
    public class VttFileToSpeech
    {
        public SpeechSynthesizer _speechSynthesizer { get; private set; }

        public VttFileToSpeech(string speechKey, string speechRegion, string speechVoice)
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            // The neural multilingual voice can speak different languages based on the input text.
            speechConfig.SpeechSynthesisVoiceName = speechVoice;

            _speechSynthesizer = new SpeechSynthesizer(speechConfig, null);
        }

        /// <summary>
        /// Splitt a VTT file in section an generate a Audio using text to speech and populate the SegmentModel
        /// </summary>
        /// <param name="filePath">File path of the VttFile</param>
        /// <param name="WriteOnDisk">Create a copy of the audio file in the path WAVFile on the app root</param>
        /// <returns></returns>
        public async Task<List<SegmentModel>> VttFilePathToSegmentListAsync(string filePath, bool WriteOnDisk)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return await VttFileStreamToSegmentListAsync(fileStream, WriteOnDisk);
            }
        }

        /// <summary>
        /// Splitt a VTT file in section an generate a Audio using text to speech and populate the SegmentModel
        /// </summary>
        /// <param name="file">Stream of the VttFile</param>
        /// <param name="WriteOnDisk">Create a copy of the audio file in the path WAVFile on the app root</param>
        /// <returns></returns>
        public async Task<List<SegmentModel>> VttFileStreamToSegmentListAsync(Stream file, bool WriteOnDisk)
        {
            List<SubtitleItem> items = null;
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();
            items = parser.ParseStream(file);

            List<SegmentModel> segments = new List<SegmentModel>();
            for (int i = 0; i < items.Count; i++)
            {
                string text = items[i].PlaintextLines[0] +
                ((items[i].PlaintextLines.Count > 1) ? " " + items[i].PlaintextLines[1] : "");

                var speechSynthesisResult = await _speechSynthesizer.SpeakTextAsync(text);

                var audioBytes = speechSynthesisResult.AudioData;
                WaveParser wave = new WaveParser(audioBytes);

                var segment = new SegmentModel(i, items[i], audioBytes, wave.totalTime);

                if (WriteOnDisk)
                {
                    File.WriteAllBytes($@"WAVFile\{i}.wav", audioBytes);
                    segment.wavPath = $@"WAVFile\{i}.wav";
                }

                segments.Add(segment);
            }
            return segments;
        }
    }
}
