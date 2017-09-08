using System.IO;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    /// An interface which is describes the functionality which is required to handle new file signals fired by the audio-data sources.
    /// @author Holger Brandl
    /// </summary>
    public interface IAudioFileProcessListener : IConfigurable
    {
        /// <summary>
        /// This method is invoked whenever a new file is started to become processed by an audio file data source.
        /// </summary>
        /// <param name="audioFile">The name of the new audio file.</param>
        void AudioFileProcStarted(FileInfo audioFile);

        /// <summary>
        /// This method is invoked whenever a file processing has finished within a audio file data source.
        /// </summary>
        /// <param name="audioFile">The name of the processed audio file.</param>
        void AudioFileProcFinished(FileInfo audioFile);
    }

}
