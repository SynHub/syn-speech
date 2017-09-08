using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public class FeatureSet 
    {
        private readonly LinkedHashMap<string, object> _featureMap;

        public FeatureSet()
        {
            _featureMap = new LinkedHashMap<string, object>();
        }

        public virtual bool IsPresent(string name)
        {
            return _featureMap.ContainsKey(name);
        }

        public virtual void Remove(string name)
        {
            _featureMap.Remove(name);
        }

        public virtual string GetString(string name)
        {
            return (string)GetObject(name);
        }

        public virtual int GetInt(string name)
        {
            return (int)GetObject(name);
        }


        public virtual float GetFloat(string name)
        {
            return (float) GetObject(name);
        }


        public virtual object GetObject(string name)
        {
            return _featureMap.Get(name);
        }

        public virtual void SetInt(string name, int value)
        {
            SetObject(name, value);
        }

        public virtual void SetFloat(string name, float value)
        {
            SetObject(name, value);
        }

        public virtual void SetString(string name, string value)
        {
            SetObject(name, value);
        }

        public virtual void SetObject(string name, object value)
        {
            _featureMap.Put(name, value);
        }
    }
}
