using System.Collections.Generic;
//PATROLLED
namespace Syn.Speech.Helper
{
    public class JProperties : Dictionary<string, string>
    {
        public string getProperty(string propName)
        {
            if (ContainsKey(propName)) return this[propName];
            return null;
        }

        public string getProperty(string propName, string defaultValue)
        {
            var prop = getProperty(propName);
            return prop ?? defaultValue;
        }

        public void setProperty(string propertyName, string propertyValue)
        {
            if (ContainsKey(propertyName))
            {
                this[propertyName] = propertyValue;
            }
            else { Add(propertyName, propertyValue); }
        }

        public string get(string propName)
        {
            return getProperty(propName);
        }

        public HashSet<string> keySet()
        {
            return new HashSet<string>(Keys);
        }
    }
}
