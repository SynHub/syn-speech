using System.Collections.Generic;
using System.IO;
//PATROLLED + REFACTORED
namespace Syn.Speech.SpeakerId
{
    public interface Identification
    {

        List<SpeakerCluster> Cluster(Stream stream);

        List<SpeakerCluster> Cluster(List<float[]> features);
    }
}
