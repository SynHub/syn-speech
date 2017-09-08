using System;
using System.Collections.Generic;
using System.Threading;

namespace Syn.Speech.Helper
{
    public class EnumMap : IDisposable
    {
        private readonly Dictionary<Type, Dictionary<string, object>> _stringsToEnums =
            new Dictionary<Type, Dictionary<string, object>>();

        private readonly Dictionary<Type, Dictionary<int, string>> _enumNumbersToStrings =
            new Dictionary<Type, Dictionary<int, string>>();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public object EnumFromString(Type type, string value)
        {
            populateIfNotPresent(type);

            return _stringsToEnums[type][value];
        }

        public string StringFromEnum(object theEnum)
        {
            Type typeOfEnum = theEnum.GetType();

            populateIfNotPresent(typeOfEnum);

            return _enumNumbersToStrings[typeOfEnum][(int) theEnum];
        }

        private void populateIfNotPresent(Type type)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_stringsToEnums.ContainsKey(type))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        populate(type);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private void populate(Type type)
        {
            Array values = Enum.GetValues(type);
            _stringsToEnums[type] = new Dictionary<string, object>(values.Length);
            _enumNumbersToStrings[type] = new Dictionary<int, string>(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                object value = values.GetValue(i);
                _stringsToEnums[type].Add(value.ToString(), value);
                _enumNumbersToStrings[type].Add((int) value, value.ToString());
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
