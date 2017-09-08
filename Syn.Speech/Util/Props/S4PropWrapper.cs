using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// Wraps annotations
    /// </summary>
    public class S4PropWrapper
    {
        public S4PropWrapper(Attribute annotation)
        {
            Annotation = annotation;
        }

        public Attribute Annotation { get; set; }
    }
}
