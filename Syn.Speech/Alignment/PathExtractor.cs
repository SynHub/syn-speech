using System;
using System.Collections.Generic;
using Syn.Speech.Helper;

//PATROLLED
namespace Syn.Speech.Alignment
{
    public class PathExtractor
    {

        //private static bool INTERPRET_PATHS = String.instancehelper_equals(java.lang.System.getProperty("com.sun.speech.freetts.interpretCartPaths", "false"), (object) "true");
        //private static bool LAZY_COMPILE = String.instancehelper_equals(java.lang.System.getProperty("com.sun.speech.freetts.lazyCartCompile", "true"), (object)"true");

        private static readonly bool INTERPRET_PATHS;
        private static readonly bool LAZY_COMPILE;
        public const string INTERPRET_PATHS_PROPERTY = "com.sun.speech.freetts.interpretCartPaths";
        public const string LAZY_COMPILE_PROPERTY = "com.sun.speech.freetts.lazyCartCompile";
        private readonly string pathAndFeature;
        private readonly string path;
        private readonly string feature;
        private object[] compiledPath;

        static PathExtractor()
        {
            var interpretPaths = Environment.GetEnvironmentVariable("com.sun.speech.freetts.interpretCartPaths");
            INTERPRET_PATHS = !string.IsNullOrEmpty(interpretPaths) && bool.Parse(interpretPaths);

            var lazyCompile = Environment.GetEnvironmentVariable("com.sun.speech.freetts.lazyCartCompile");
            LAZY_COMPILE = string.IsNullOrEmpty(lazyCompile) || bool.Parse(lazyCompile);
        }

        public PathExtractor(string pathAndFeature, bool wantFeature)
        {
            this.pathAndFeature = pathAndFeature;
            if (INTERPRET_PATHS)
            {
                path = pathAndFeature;
                return;
            }

            if (wantFeature)
            {
                int lastDot = pathAndFeature.LastIndexOf(".");
                // string can be of the form "p.feature" or just "feature"

                if (lastDot == -1)
                {
                    feature = pathAndFeature;
                    path = null;
                }
                else
                {
                    feature = pathAndFeature.Substring(lastDot + 1);
                    path = pathAndFeature.Substring(0, lastDot);
                }
            }
            else
            {
                path = pathAndFeature;
            }

            if (!LAZY_COMPILE)
            {
                compiledPath = compile(path);
            }
        }

        public virtual Item findItem(Item item)
        {

            if (INTERPRET_PATHS)
            {
                return item.findItem(path);
            }

            if (compiledPath == null)
            {
                compiledPath = compile(path);
            }

            Item pitem = item;

            for (int i = 0; pitem != null && i < compiledPath.Length; )
            {
                OpEnum op = (OpEnum)compiledPath[i++];
                if (op == OpEnum.NEXT)
                {
                    pitem = pitem.getNext();
                }
                else if (op == OpEnum.PREV)
                {
                    pitem = pitem.getPrevious();
                }
                else if (op == OpEnum.NEXT_NEXT)
                {
                    pitem = pitem.getNext();
                    if (pitem != null)
                    {
                        pitem = pitem.getNext();
                    }
                }
                else if (op == OpEnum.PREV_PREV)
                {
                    pitem = pitem.getPrevious();
                    if (pitem != null)
                    {
                        pitem = pitem.getPrevious();
                    }
                }
                else if (op == OpEnum.PARENT)
                {
                    pitem = pitem.getParent();
                }
                else if (op == OpEnum.DAUGHTER)
                {
                    pitem = pitem.getDaughter();
                }
                else if (op == OpEnum.LAST_DAUGHTER)
                {
                    pitem = pitem.getLastDaughter();
                }
                else if (op == OpEnum.RELATION)
                {
                    string relationName = (string)compiledPath[i++];
                    pitem =
                            pitem.getSharedContents()
                                    .getItemRelation(relationName);
                }
                else
                {
                    this.LoggerInfo("findItem: bad feature " + op + " in "
                            + path);
                }
            }
            return pitem;
        }

        public virtual object findFeature(Item item)
        {
            if (INTERPRET_PATHS)
            {
                return item.findFeature(path);
            }

            Item pitem = findItem(item);
            object results = null;
            if (pitem != null)
            {
                //if (LOGGER.isLoggable(Level.FINER))
                //{
                //    LOGGER.finer("findFeature: Item [" + pitem + "], feature '"
                //            + feature + "'");
                //}
                this.LoggerInfo("findFeature: Item [" + pitem + "], feature '" + feature + "'");
                results = pitem.getFeatures().getObject(feature);
            }
            results = (results == null) ? "0" : results;
            //if (LOGGER.isLoggable(Level.FINER))
            //{
            //    LOGGER.finer("findFeature: ...results = '" + results + "'");
            //}
            this.LoggerInfo("findFeature: ...results = '" + results + "'");
            return results;
        }

        private object[] compile(string obj0)
        {
            if (path == null)
            {
                return new object[0];
            }

            var list = new List<object>();
            var tok = new StringTokenizer(path, ":.");

            while (tok.hasMoreTokens())
            {
                string token = tok.nextToken();
                OpEnum op = OpEnum.getInstance(token);
                if (op == null)
                {
                    throw new Error("Bad path compiled " + path);
                }

                list.Add(op);

                if (op == OpEnum.RELATION)
                {
                    list.Add(tok.nextToken());
                }
            }
            return list.ToArray();
        }

        public override string ToString()
        {
            return pathAndFeature;
        }

    }
}
