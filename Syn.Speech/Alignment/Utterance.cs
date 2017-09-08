using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

//PATROLLED
namespace Syn.Speech.Alignment
{

    /// <summary>
    /// Holds all the data for an utterance to be spoken. It is incrementally
    /// modified by various UtteranceProcessor implementations. An utterance
    /// contains a set of Features (essential a set of properties) and a set of
    /// Relations. A Relation is an ordered set of Item graphs. The utterance
    /// contains a set of features and implements FeatureSet so that applications
    /// can set/get features directly from the utterance. If a feature query is not
    /// found in the utterance feature set, the query is forwarded to the FeatureSet
    /// of the voice associated with the utterance.
    /// </summary>
    public class Utterance
    {
        private readonly FeatureSet features;
        private readonly FeatureSet relations;

        /// <summary>
        /// Creates an utterance with the given set of tokenized text.
        /// </summary>
        /// <param name="tokenizer">The list of tokens for this utterance.</param>
        public Utterance(CharTokenizer tokenizer)
        {
            features = new FeatureSet();
            relations = new FeatureSet();
            setTokenList(tokenizer);
        }

        /// <summary>
        /// Creates a new relation with the given name and adds it to this utterance.
        /// </summary>
        /// <param name="name">The name of the new relation.</param>
        /// <returns>the newly created relation</returns>
        public virtual Relation createRelation(string name)
        {
            Relation relation = new Relation(name, this);
            relations.setObject(name, relation);
            return relation;
        }

        /// <summary>
        /// Retrieves a relation from this utterance.
        /// </summary>
        /// <param name="name">The name of the Relation.</param>
        /// <returns>The relation or null if the relation is not found</returns>
        public virtual Relation getRelation(string name)
        {
            return (Relation)relations.getObject(name);
        }

        /// <summary>
        ///Determines if this utterance contains a relation with the given name.
        /// </summary>
        /// <param name="name">The name of the relation of interest.</param>
        /// <returns></returns>
        public virtual bool hasRelation(string name)
        {
            return relations.isPresent(name);
        }

        /// <summary>
        /// Removes the named feature from this set of features.
        /// </summary>
        /// <param name="name">The name of the feature of interest.</param>
        public virtual void remove(string name)
        {
            features.remove(name);
        }

        /// <summary>
        /// Convenience method that sets the named feature as an int.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void setInt(string name, int value)
        {
            features.setInt(name, value);
        }

        /// <summary>
        /// Convenience method that sets the named feature as a float.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void setFloat(string name, float value)
        {
            features.setFloat(name, value);
        }

        /// <summary>
        /// Convenience method that sets the named feature as a String.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void setString(string name, string value)
        {
            features.setString(name, value);
        }

        /// <summary>
        /// Sets the named feature.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void setObject(string name, object value)
        {
            features.setObject(name, value);
        }

        /// <summary>
        /// Returns the Item in the given Relation associated with the given time.
        /// </summary>
        /// <param name="relation">The name of the relation.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        /// <exception>if the Segment durations have not been
        /// calculated in the Utterance or if the given relation is not
        /// present in the Utterance</exception>
        public virtual Item getItem(string relation, float time)
        {
            Relation segmentRelation = null;
            string pathName = null;

            if (relation.Equals(Relation.WORD))
            {
                pathName = "R:SylStructure.parent.parent.R:Word";
            }
            else if (relation.Equals(Relation.TOKEN))
            {
                pathName = "R:SylStructure.parent.parent.R:Token.parent";
            }
            else
            {
                throw new ArgumentException(
                        "Utterance.getItem(): relation cannot be " + relation);
            }

            PathExtractor path = new PathExtractor(pathName, false);

            // get the Item in the Segment Relation with the given time
            Item segmentItem = getItem(segmentRelation, time);

            if (segmentItem != null)
            {
                return path.findItem(segmentItem);
            }
            else
            {
                return null;
            }
        }

        private static Item getItem(Relation segmentRelation, float time)
        {
            Item lastSegment = segmentRelation.getTail();
            // If given time is closer to the front than the end, search from
            // the front; otherwise, start search from end
            // this might not be the best strategy though.
            float lastSegmentEndTime = getSegmentEnd(lastSegment);
            if (time < 0 || lastSegmentEndTime < time)
            {
                return null;
            }
            else if (lastSegmentEndTime - time > time)
            {
                return findFromFront(segmentRelation, time);
            }
            else
            {
                return findFromEnd(segmentRelation, time);
            }
        }

        private static Item findFromEnd(Relation segmentRelation, float time)
        {
            Item item = segmentRelation.getTail();
            while (item != null && getSegmentEnd(item) > time)
            {
                item = item.getPrevious();
            }

            if (item != segmentRelation.getTail())
            {
                item = item.getNext();
            }

            return item;
        }

        private static Item findFromFront([In] Relation segmentRelation, [In] float time)
        {
            Item item = segmentRelation.getHead();
            while (item != null && time > getSegmentEnd(item))
            {
                item = item.getNext();
            }
            return item;
        }

        private static float getSegmentEnd(Item segment)
        {
            FeatureSet segmentFeatureSet = segment.getFeatures();
            return segmentFeatureSet.getFloat("end");
        }

        /// <summary>
        /// Sets the token list for this utterance. Note that this could be
        /// optimized by turning the token list directly into the token relation.
        /// </summary>
        /// <param name="tokenizer">The tokenList.</param>
        private void setTokenList(IEnumerator<Token> tokenizer)
        {
            Relation relation = createRelation(Relation.TOKEN);
            while (tokenizer.MoveNext())
            {
                Token token = tokenizer.Current;
                string tokenWord = token.getWord();

                if (!string.IsNullOrEmpty(tokenWord))
                {
                    Item item = relation.appendItem();

                    FeatureSet featureSet = item.getFeatures();
                    featureSet.setString("name", tokenWord);
                    featureSet.setString("whitespace", token.getWhitespace());
                    featureSet.setString("prepunctuation",
                            token.getPrepunctuation());
                    featureSet.setString("punc", token.getPostpunctuation());
                    featureSet.setString("file_pos", token.getPosition().ToString(CultureInfo.InvariantCulture));
                    featureSet.setString("line_number", token.getLineNumber().ToString(CultureInfo.InvariantCulture));

                }
            }
        }
    }
}
