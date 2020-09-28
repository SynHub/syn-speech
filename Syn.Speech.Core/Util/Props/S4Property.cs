using System;

namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A tag which superclasses all sphinx property annotations. Because there is no real inheritance for annotations all
    /// child classes are annotated by this general property annotation.
    /// @author Holger Brandl
    /// </summary>
    public class S4Property: Attribute
    {
    }
}
