//PATROLLED

namespace Syn.Speech.Alignment
{
    public class ItemContents 
    {
        private readonly FeatureSet features;
        private readonly FeatureSet relations;

        public ItemContents()
        {
            features = new FeatureSet();
            relations = new FeatureSet();
        }

        public virtual void addItemRelation(string relationName, Item item)
        {
            relations.setObject(relationName, item);
        }

        public virtual void removeItemRelation(string relationName)
        {
            relations.remove(relationName);
        }

        public virtual Item getItemRelation(string relationName)
        {
            return (Item)relations.getObject(relationName);
        }

        public virtual FeatureSet getFeatures()
        {
            return features;
        }
    }
}
