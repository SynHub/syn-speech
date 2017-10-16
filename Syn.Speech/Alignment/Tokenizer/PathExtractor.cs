using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public class PathExtractor
    {

        //private static bool INTERPRET_PATHS = String.instancehelper_equals(java.lang.System.getProperty("com.sun.speech.freetts.interpretCartPaths", "false"), (object) "true");
        //private static bool LAZY_COMPILE = String.instancehelper_equals(java.lang.System.getProperty("com.sun.speech.freetts.lazyCartCompile", "true"), (object)"true");

        private static readonly bool InterpretPaths;
        private static readonly bool LazyCompile;
        public const string InterpretPathsProperty = "com.sun.speech.freetts.interpretCartPaths";
        public const string LazyCompileProperty = "com.sun.speech.freetts.lazyCartCompile";
        private readonly string _pathAndFeature;
        private readonly string _path;
        private readonly string _feature;
        private object[] _compiledPath;

        static PathExtractor()
        {
            var interpretPaths = Environment.GetEnvironmentVariable("com.sun.speech.freetts.interpretCartPaths");
            InterpretPaths = !string.IsNullOrEmpty(interpretPaths) && bool.Parse(interpretPaths);

            var lazyCompile = Environment.GetEnvironmentVariable("com.sun.speech.freetts.lazyCartCompile");
            LazyCompile = string.IsNullOrEmpty(lazyCompile) || bool.Parse(lazyCompile);
        }

        public PathExtractor(string pathAndFeature, bool wantFeature)
        {
            _pathAndFeature = pathAndFeature;
            if (InterpretPaths)
            {
                _path = pathAndFeature;
                return;
            }

            if (wantFeature)
            {
                int lastDot = pathAndFeature.LastIndexOf(".", StringComparison.Ordinal);
                // string can be of the form "p.feature" or just "feature"

                if (lastDot == -1)
                {
                    _feature = pathAndFeature;
                    _path = null;
                }
                else
                {
                    _feature = pathAndFeature.Substring(lastDot + 1);
                    _path = pathAndFeature.Substring(0, lastDot);
                }
            }
            else
            {
                _path = pathAndFeature;
            }

            if (!LazyCompile)
            {
                _compiledPath = Compile(_path);
            }
        }

        public virtual Item FindItem(Item item)
        {

            if (InterpretPaths)
            {
                return item.FindItem(_path);
            }

            if (_compiledPath == null)
            {
                _compiledPath = Compile(_path);
            }

            Item pitem = item;

            for (int i = 0; pitem != null && i < _compiledPath.Length; )
            {
                OpEnum op = (OpEnum)_compiledPath[i++];
                if (op == OpEnum.Next)
                {
                    pitem = pitem.GetNext();
                }
                else if (op == OpEnum.Prev)
                {
                    pitem = pitem.GetPrevious();
                }
                else if (op == OpEnum.NextNext)
                {
                    pitem = pitem.GetNext();
                    if (pitem != null)
                    {
                        pitem = pitem.GetNext();
                    }
                }
                else if (op == OpEnum.PrevPrev)
                {
                    pitem = pitem.GetPrevious();
                    if (pitem != null)
                    {
                        pitem = pitem.GetPrevious();
                    }
                }
                else if (op == OpEnum.Parent)
                {
                    pitem = pitem.GetParent();
                }
                else if (op == OpEnum.Daughter)
                {
                    pitem = pitem.Daughter;
                }
                else if (op == OpEnum.LastDaughter)
                {
                    pitem = pitem.GetLastDaughter();
                }
                else if (op == OpEnum.Relation)
                {
                    string relationName = (string)_compiledPath[i++];
                    pitem =
                            pitem.SharedContents
                                    .GetItemRelation(relationName);
                }
                else
                {
                    this.LogInfo("findItem: bad feature " + op + " in "
                            + _path);
                }
            }
            return pitem;
        }

        public virtual object FindFeature(Item item)
        {
            if (InterpretPaths)
            {
                return item.FindFeature(_path);
            }

            Item pitem = FindItem(item);
            object results = null;
            if (pitem != null)
            {
                //if (LOGGER.isLoggable(Level.FINER))
                //{
                //    LOGGER.finer("findFeature: Item [" + pitem + "], feature '"
                //            + feature + "'");
                //}
                this.LogInfo("findFeature: Item [" + pitem + "], feature '" + _feature + "'");
                results = pitem.Features.GetObject(_feature);
            }
            results = (results == null) ? "0" : results;
            //if (LOGGER.isLoggable(Level.FINER))
            //{
            //    LOGGER.finer("findFeature: ...results = '" + results + "'");
            //}
            this.LogInfo("findFeature: ...results = '" + results + "'");
            return results;
        }

        private object[] Compile(string obj0)
        {
            if (_path == null)
            {
                return new object[0];
            }

            var list = new List<object>();
            var tok = new StringTokenizer(_path, ":.");

            while (tok.hasMoreTokens())
            {
                string token = tok.nextToken();
                OpEnum op = OpEnum.GetInstance(token);
                if (op == null)
                {
                    throw new Error("Bad path compiled " + _path);
                }

                list.Add(op);

                if (op == OpEnum.Relation)
                {
                    list.Add(tok.nextToken());
                }
            }
            return list.ToArray();
        }

        public override string ToString()
        {
            return _pathAndFeature;
        }

    }
}
