using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using TestConsole.AudioProcess.Wave;
using TextToSpeech.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TextToSpeech.AudioProcess
{
    public static class Ffmpeg
    {
        public static async Task<List<SegmentModel>> CutAzureExtra350(List<SegmentModel> segments)
        {
            string tempPath = "";
            await Parallel.ForEachAsync(segments, new ParallelOptions { MaxDegreeOfParallelism = 6 }, async (segment, token) =>
            {
                double time = segment.audioDuration.TotalSeconds - 0.35;

                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = $" -y -t {time} -f wav -i pipe:0 -ss 0 -acodec pcm_s16le -ac 1 -ar 16000 -f wav pipe:1";

                process.Start();

                using (var ffmpegIn = process.StandardInput.BaseStream)
                using (var stream = new MemoryStream(segment.audioBytes))
                {
                    await stream.CopyToAsync(ffmpegIn);
                    ffmpegIn.Flush();
                    ffmpegIn.Close();
                }

                Stream baseStream = process.StandardOutput.BaseStream;

                WaveParser wave = new WaveParser(baseStream);
                segment = wave.UpdateSegment(segment);

                process.WaitForExit();

                string output = await process.StandardOutput.ReadToEndAsync();
                Console.WriteLine(output);
                string error = await process.StandardError.ReadToEndAsync();
                Console.WriteLine(error);

                process.Dispose();

                if (AppConfig.WriteOnDisk)
                {
                    tempPath = segment.wavPath.Replace($"{segment.order}", $"short{segment.order}");
                    File.WriteAllBytes(tempPath, segment.audioBytes);
                }
            });
            return segments;
        }
    }
}
