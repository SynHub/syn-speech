using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
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
        private readonly FeatureSet _features;
        private readonly FeatureSet _relations;

        /// <summary>
        /// Creates an utterance with the given set of tokenized text.
        /// </summary>
        /// <param name="tokenizer">The list of tokens for this utterance.</param>
        public Utterance(CharTokenizer tokenizer)
        {
            _features = new FeatureSet();
            _relations = new FeatureSet();
            SetTokenList(tokenizer);
        }

        /// <summary>
        /// Creates a new relation with the given name and adds it to this utterance.
        /// </summary>
        /// <param name="name">The name of the new relation.</param>
        /// <returns>the newly created relation</returns>
        public virtual Relation CreateRelation(string name)
        {
            Relation relation = new Relation(name, this);
            _relations.SetObject(name, relation);
            return relation;
        }

        /// <summary>
        /// Retrieves a relation from this utterance.
        /// </summary>
        /// <param name="name">The name of the Relation.</param>
        /// <returns>The relation or null if the relation is not found</returns>
        public virtual Relation GetRelation(string name)
        {
            return (Relation)_relations.GetObject(name);
        }

        /// <summary>
        ///Determines if this utterance contains a relation with the given name.
        /// </summary>
        /// <param name="name">The name of the relation of interest.</param>
        /// <returns></returns>
        public virtual bool HasRelation(string name)
        {
            return _relations.IsPresent(name);
        }

        /// <summary>
        /// Removes the named feature from this set of features.
        /// </summary>
        /// <param name="name">The name of the feature of interest.</param>
        public virtual void Remove(string name)
        {
            _features.Remove(name);
        }

        /// <summary>
        /// Convenience method that sets the named feature as an int.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void SetInt(string name, int value)
        {
            _features.SetInt(name, value);
        }

        /// <summary>
        /// Convenience method that sets the named feature as a float.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void SetFloat(string name, float value)
        {
            _features.SetFloat(name, value);
        }

        /// <summary>
        /// Convenience method that sets the named feature as a String.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void SetString(string name, string value)
        {
            _features.SetString(name, value);
        }

        /// <summary>
        /// Sets the named feature.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="value">The value of the feature.</param>
        public virtual void SetObject(string name, object value)
        {
            _features.SetObject(name, value);
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
        public virtual Item GetItem(string relation, float time)
        {
            Relation segmentRelation = null;
            string pathName;

            if (relation.Equals(Relation.Word))
            {
                pathName = "R:SylStructure.parent.parent.R:Word";
            }
            else if (relation.Equals(Relation.Token))
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
            Item segmentItem = GetItem(segmentRelation, time);

            if (segmentItem != null)
            {
                return path.FindItem(segmentItem);
            }
            return null;
        }

        private static Item GetItem(Relation segmentRelation, float time)
        {
            Item lastSegment = segmentRelation.Tail;
            // If given time is closer to the front than the end, search from
            // the front; otherwise, start search from end
            // this might not be the best strategy though.
            float lastSegmentEndTime = GetSegmentEnd(lastSegment);
            if (time < 0 || lastSegmentEndTime < time)
            {
                return null;
            }
            if (lastSegmentEndTime - time > time)
            {
                return FindFromFront(segmentRelation, time);
            }
            return FindFromEnd(segmentRelation, time);
        }

        private static Item FindFromEnd(Relation segmentRelation, float time)
        {
            Item item = segmentRelation.Tail;
            while (item != null && GetSegmentEnd(item) > time)
            {
                item = item.GetPrevious();
            }

            if (item != segmentRelation.Tail)
            {
                item = item.GetNext();
            }

            return item;
        }

        private static Item FindFromFront([In] Relation segmentRelation, [In] float time)
        {
            Item item = segmentRelation.Head;
            while (item != null && time > GetSegmentEnd(item))
            {
                item = item.GetNext();
            }
            return item;
        }

        private static float GetSegmentEnd(Item segment)
        {
            FeatureSet segmentFeatureSet = segment.Features;
            return segmentFeatureSet.GetFloat("end");
        }

        /// <summary>
        /// Sets the token list for this utterance. Note that this could be
        /// optimized by turning the token list directly into the token relation.
        /// </summary>
        /// <param name="tokenizer">The tokenList.</param>
        private void SetTokenList(IEnumerator<Token> tokenizer)
        {
            Relation relation = CreateRelation(Relation.Token);
            while (tokenizer.MoveNext())
            {
                Token token = tokenizer.Current;
                string tokenWord = token.Word;

                if (!string.IsNullOrEmpty(tokenWord))
                {
                    Item item = relation.AppendItem();

                    FeatureSet featureSet = item.Features;
                    featureSet.SetString("name", tokenWord);
                    featureSet.SetString("whitespace", token.Whitespace);
                    featureSet.SetString("prepunctuation",token.PrePunctuation);
                    featureSet.SetString("punc", token.PostPunctuation);
                    featureSet.SetString("file_pos", token.Position.ToString(CultureInfo.InvariantCulture));
                    featureSet.SetString("line_number", token.LineNumber.ToString(CultureInfo.InvariantCulture));

                }
            }
        }
    }
}
