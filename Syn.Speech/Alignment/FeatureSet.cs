using Syn.Speech.Helper;

//PATROLLED
namespace Syn.Speech.Alignment
{
    public class FeatureSet 
    {
        private readonly LinkedHashMap<string, object> featureMap;
        //internal static DecimalFormat formatter;

        public FeatureSet()
        {
            featureMap = new LinkedHashMap<string, object>();
        }

        public virtual bool isPresent(string name)
        {
            return featureMap.ContainsKey(name);
        }

        public virtual void remove(string name)
        {
            featureMap.Remove(name);
        }

        public virtual string getString(string name)
        {
            return (string)getObject(name);
        }

        public virtual int getInt(string name)
        {
            return (int)getObject(name);
        }


        public virtual float getFloat(string name)
        {
            return (float) getObject(name);
        }


        public virtual object getObject(string name)
        {
            return featureMap.Get(name);
        }

        public virtual void setInt(string name, int value)
        {
            setObject(name, value);
        }

        public virtual void setFloat(string name, float value)
        {
            setObject(name, value);
        }

        public virtual void setString(string name, string value)
        {
            setObject(name, value);
        }

        public virtual void setObject(string name, object value)
        {
            featureMap.Put(name, value);
        }
    }
}
