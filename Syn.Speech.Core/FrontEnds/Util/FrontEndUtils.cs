//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    public class FrontEndUtils
    {
        /// <summary>
        /// Returns a the next <code>DataProcessor</code> of type <code>predecClass</code> which precedes <code>dp</code>
        /// </summary>
        public static T GetFrontEndProcessor<T>(IDataProcessor dp, T predecClass) where T : IDataProcessor
        {
            while (!predecClass.GetType().IsAssignableFrom(typeof(T)))
            {
                if (dp is FrontEnd)
                    dp = ((FrontEnd)dp).LastDataProcessor;
                else
                    dp = dp.Predecessor;

                if (dp == null)
                    return default(T);
            }


            return predecClass;
        }
    }

}
