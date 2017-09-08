using System.Runtime.InteropServices;
//PATROLLED
using Syn.Speech.Helper;

namespace Syn.Speech.Alignment
{
    public class Item
    {
        private readonly Relation ownerRelation;
        private readonly ItemContents contents;
        private Item parent;
        private Item daughter;
        private Item next;
        private Item prev;

        public Item(Relation relation, ItemContents sharedContents)
        {
            ownerRelation = relation;
            if (sharedContents != null)
            {
                contents = sharedContents;
            }
            else
            {
                contents = new ItemContents();
            }
            parent = null;
            daughter = null;
            next = null;
            prev = null;

            getSharedContents().addItemRelation(relation.getName(), this);
        }

        public virtual Item getItemAs(string relationName)
        {
            return getSharedContents().getItemRelation(relationName);
        }

        public virtual Relation getOwnerRelation()
        {
            return ownerRelation;
        }

        public virtual ItemContents getSharedContents()
        {
            return contents;
        }

        public virtual bool hasDaughters()
        {
            return daughter != null;
        }

        public virtual Item getDaughter()
        {
            return daughter;
        }

        public virtual Item getNthDaughter(int which)
        {
            Item nthDaughter = daughter;
            int count = 0;
            while (count++ != which && nthDaughter != null)
            {
                nthDaughter = nthDaughter.next;
            }
            return nthDaughter;
        }

        public virtual Item getLastDaughter()
        {
            Item lastDaughter = daughter;
            if (lastDaughter == null)
            {
                return null;
            }
            while (lastDaughter.next != null)
            {
                lastDaughter = lastDaughter.next;
            }
            return lastDaughter;
        }

        public virtual Item addDaughter(Item item)
        {
            Item newItem;
            ItemContents itemContents;

            Item p = getLastDaughter();

            if (p != null)
            {
                newItem = p.appendItem(item);
            }
            else
            {
                if (item == null)
                {
                    itemContents = new ItemContents();
                }
                else
                {
                    itemContents = item.getSharedContents();
                }
                newItem = new Item(getOwnerRelation(), itemContents);
                newItem.parent = this;
                daughter = newItem;
            }
            return newItem;
        }

        public virtual Item createDaughter()
        {
            return addDaughter(null);
        }

        public virtual Item getParent()
        {
            Item item;
            for (item = this; item.prev != null; item = item.prev){}
            return item.parent;
        }


        public virtual Utterance getUtterance()
        {
            return getOwnerRelation().getUtterance();
        }


        public virtual FeatureSet getFeatures()
        {
            return getSharedContents().getFeatures();
        }


        public virtual object findFeature(string pathAndFeature)
        {
            int lastDot;
            string feature;
            string path;
            Item item;
            object results = null;

            lastDot = pathAndFeature.LastIndexOf(".");
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

            item = findItem(path);
            if (item != null)
            {
                results = item.getFeatures().getObject(feature);
            }
            results = (results == null) ? "0" : results;

            // System.out.println("FI " + pathAndFeature + " are " + results);

            return results;
        }

        public virtual Item findItem(string path)
        {
                   Item pitem = this;
        StringTokenizer tok;

        if (path == null) {
            return this;
        }

        tok = new StringTokenizer(path, ":.");

        while (pitem != null && tok.hasMoreTokens()) {
            string token = tok.nextToken();
            if (token.Equals("n")) {
                pitem = pitem.getNext();
            } else if (token.Equals("p")) {
                pitem = pitem.getPrevious();
            } else if (token.Equals("nn")) {
                pitem = pitem.getNext();
                if (pitem != null) {
                    pitem = pitem.getNext();
                }
            } else if (token.Equals("pp")) {
                pitem = pitem.getPrevious();
                if (pitem != null) {
                    pitem = pitem.getPrevious();
                }
            } else if (token.Equals("parent")) {
                pitem = pitem.getParent();
            } else if (token.Equals("daughter") || token.Equals("daughter1")) {
                pitem = pitem.getDaughter();
            } else if (token.Equals("daughtern")) {
                pitem = pitem.getLastDaughter();
            } else if (token.Equals("R")) {
                string relationName = tok.nextToken();
                pitem =
                        pitem.getSharedContents()
                                .getItemRelation(relationName);
            } else {
                this.LoggerInfo("findItem: bad feature " + token + " in " + path);
            }
        }
        return pitem;
        }

        public virtual Item getNext()
        {
            return next;
        }

        public virtual Item getPrevious()
        {
            return prev;
        }

        public virtual Item appendItem(Item originalItem)
        {
            ItemContents contents;
            Item newItem;

            if (originalItem == null)
            {
                contents = null;
            }
            else
            {
                contents = originalItem.getSharedContents();
            }

            newItem = new Item(getOwnerRelation(), contents);
            newItem.next = next;
            if (next != null)
            {
                next.prev = newItem;
            }

            attach(newItem);

            if (ownerRelation.getTail() == this)
            {
                ownerRelation.setTail(newItem);
            }
            return newItem;
        }

   

        internal virtual void attach([In] Item obj0)
        {
            next = obj0;
            obj0.prev = this;
        }

        public virtual Item prependItem(Item originalItem)
        {
            ItemContents contents;
            Item newItem;

            if (originalItem == null)
            {
                contents = null;
            }
            else
            {
                contents = originalItem.getSharedContents();
            }

            newItem = new Item(getOwnerRelation(), contents);
            newItem.prev = prev;
            if (prev != null)
            {
                prev.next = newItem;
            }
            newItem.next = this;
            prev = newItem;
            if (parent != null)
            {
                parent.daughter = newItem;
                newItem.parent = parent;
                parent = null;
            }
            if (ownerRelation.getHead() == this)
            {
                ownerRelation.setHead(newItem);
            }
            return newItem;
        }

        public override string ToString()
        {
            string name = getFeatures().getString("name");
            if (name == null)
            {
                name = "";
            }
            return name;
        }

        public virtual bool equalsShared(Item otherItem)
        {
            if (otherItem == null)
            {
                return false;
            }
            else
            {
                return getSharedContents().Equals(otherItem.getSharedContents());
            }
        }
    }
}
