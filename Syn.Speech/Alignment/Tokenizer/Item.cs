using System.Runtime.InteropServices;
using Syn.Logging;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public sealed class Item
    {
        private Item _parent;
        private Item _next;
        private Item _prev;

        public Item(Relation relation, ItemContents sharedContents)
        {
            OwnerRelation = relation;
            if (sharedContents != null)
            {
                SharedContents = sharedContents;
            }
            else
            {
                SharedContents = new ItemContents();
            }
            _parent = null;
            Daughter = null;
            _next = null;
            _prev = null;

            SharedContents.AddItemRelation(relation.Name, this);
        }

        public Item GetItemAs(string relationName)
        {
            return SharedContents.GetItemRelation(relationName);
        }

        public Relation OwnerRelation { get; private set; }

        public ItemContents SharedContents { get; private set; }

        public bool HasDaughters
        {
            get { return Daughter != null; }
        }

        public Item Daughter { get; private set; }

        public Item GetNthDaughter(int which)
        {
            var nthDaughter = Daughter;
            var count = 0;
            while (count++ != which && nthDaughter != null)
            {
                nthDaughter = nthDaughter._next;
            }
            return nthDaughter;
        }

        public Item GetLastDaughter()
        {
            var lastDaughter = Daughter;
            if (lastDaughter == null)
            {
                return null;
            }
            while (lastDaughter._next != null)
            {
                lastDaughter = lastDaughter._next;
            }
            return lastDaughter;
        }

        public Item AddDaughter(Item item)
        {
            Item newItem;

            var p = GetLastDaughter();

            if (p != null)
            {
                newItem = p.AppendItem(item);
            }
            else
            {
                ItemContents itemContents;
                if (item == null)
                {
                    itemContents = new ItemContents();
                }
                else
                {
                    itemContents = item.SharedContents;
                }
                newItem = new Item(OwnerRelation, itemContents) { _parent = this };
                Daughter = newItem;
            }
            return newItem;
        }

        public Item CreateDaughter()
        {
            return AddDaughter(null);
        }

        public Item GetParent()
        {
            Item item;
            for (item = this; item._prev != null; item = item._prev) { }
            return item._parent;
        }


        public Utterance Utterance
        {
            get { return OwnerRelation.Utterance; }
        }


        public FeatureSet Features
        {
            get { return SharedContents.Features; }
        }


        public object FindFeature(string pathAndFeature)
        {
            string feature;
            string path;
            object results = null;

            var lastDot = pathAndFeature.LastIndexOf(".", System.StringComparison.Ordinal);
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

            var item = FindItem(path);
            if (item != null)
            {
                results = item.Features.GetObject(feature);
            }
            results = results ?? "0";

            // System.out.println("FI " + pathAndFeature + " are " + results);

            return results;
        }

        public Item FindItem(string path)
        {
            var pitem = this;

            if (path == null)
            {
                return this;
            }

            var tok = new StringTokenizer(path, ":.");

            while (pitem != null && tok.hasMoreTokens())
            {
                var token = tok.nextToken();
                if (token.Equals("n"))
                {
                    pitem = pitem.GetNext();
                }
                else if (token.Equals("p"))
                {
                    pitem = pitem.GetPrevious();
                }
                else if (token.Equals("nn"))
                {
                    pitem = pitem.GetNext();
                    if (pitem != null)
                    {
                        pitem = pitem.GetNext();
                    }
                }
                else if (token.Equals("pp"))
                {
                    pitem = pitem.GetPrevious();
                    if (pitem != null)
                    {
                        pitem = pitem.GetPrevious();
                    }
                }
                else if (token.Equals("parent"))
                {
                    pitem = pitem.GetParent();
                }
                else if (token.Equals("daughter") || token.Equals("daughter1"))
                {
                    pitem = pitem.Daughter;
                }
                else if (token.Equals("daughtern"))
                {
                    pitem = pitem.GetLastDaughter();
                }
                else if (token.Equals("R"))
                {
                    var relationName = tok.nextToken();
                    pitem =
                            pitem.SharedContents
                                    .GetItemRelation(relationName);
                }
                else
                {
                    this.LogInfo("findItem: bad feature " + token + " in " + path);
                }
            }
            return pitem;
        }

        public Item GetNext()
        {
            return _next;
        }

        public Item GetPrevious()
        {
            return _prev;
        }

        public Item AppendItem(Item originalItem)
        {
            ItemContents contents;

            if (originalItem == null)
            {
                contents = null;
            }
            else
            {
                contents = originalItem.SharedContents;
            }

            var newItem = new Item(OwnerRelation, contents) {_next = _next};
            if (_next != null)
            {
                _next._prev = newItem;
            }

            Attach(newItem);

            if (OwnerRelation.Tail == this)
            {
                OwnerRelation.Tail=newItem;
            }
            return newItem;
        }



        internal void Attach([In] Item obj0)
        {
            _next = obj0;
            obj0._prev = this;
        }

        public Item PrependItem(Item originalItem)
        {
            ItemContents contents;

            if (originalItem == null)
            {
                contents = null;
            }
            else
            {
                contents = originalItem.SharedContents;
            }

            var newItem = new Item(OwnerRelation, contents) {_prev = _prev};
            if (_prev != null)
            {
                _prev._next = newItem;
            }
            newItem._next = this;
            _prev = newItem;
            if (_parent != null)
            {
                _parent.Daughter = newItem;
                newItem._parent = _parent;
                _parent = null;
            }
            if (OwnerRelation.Head == this)
            {
                OwnerRelation.Head=newItem;
            }
            return newItem;
        }

        public override string ToString()
        {
            var name = Features.GetString("name");
            if (name == null)
            {
                name = "";
            }
            return name;
        }

        public bool EqualsShared(Item otherItem)
        {
            if (otherItem == null)
            {
                return false;
            }
            return SharedContents.Equals(otherItem.SharedContents);
        }
    }
}
