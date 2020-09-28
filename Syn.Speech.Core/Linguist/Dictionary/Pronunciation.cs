using System;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.Dictionary
{
    /// <summary>
    /// Provides pronunciation information for a word.
    /// </summary>
    public class Pronunciation 
    {
        public static Pronunciation Unknown = new Pronunciation(Unit.EmptyArray, null, null, 1.0f);


        /// <summary>
        /// Creates a pronunciation
        /// </summary>
        /// <param name="units">represents the pronunciation</param>
        /// <param name="tag">a grammar specific tag</param>
        /// <param name="wordClassification">the classification for this word</param>
        /// <param name="probability">the probability of this pronunciation occurring</param> 
        public Pronunciation(Unit[] units,
                        string tag,
                        WordClassification wordClassification,
                        float probability) 
        {
            WordClassification = wordClassification;
            Units = units;
            Tag = tag;
            Probability = probability;
        }

        /// <summary>
        /// Creates a pronunciation
        /// </summary>
        /// <param name="units">represents the pronunciation</param>
        /// <param name="tag">a grammar specific tag</param>
        /// <param name="wordClassification">the classification for this word</param>
        /// <param name="probability">the probability of this pronunciation occurring</param> 
        public Pronunciation(List<Unit> units,
                      string tag,
                      WordClassification wordClassification,
                      float probability) 
        {
            WordClassification = wordClassification;
            Units = units.ToArray();
            Tag = tag;
            Probability = probability;
        }

        /// <summary>
        /// Creates a pronunciation with defaults
        /// </summary>
        /// <param name="units">represents the pronunciation</param>
        public Pronunciation(List<Unit> units) 
            :this(units, null, null, 1.0f)
        {
        }
        /// <summary>
        /// Sets the word this pronunciation represents.
        /// </summary>
        /// <param name="word">the Word this Pronunciation represents</param>
        public void SetWord(Word word) 
        {
            if (Word == null) 
            {
                Word = word;
            } 
            else 
            {
                throw new Exception("Word of Pronunciation cannot be set twice.");
            }
        }

        /// <summary>
        /// Retrieves the word that this Pronunciation object represents.
        /// </summary>
        /// <value>the word</value>
        public Word Word { get; private set; }

        /// <summary>
        /// Retrieves the word classification for this pronunciation
        /// </summary>
        /// <value></value>
        public WordClassification WordClassification { get; private set; }

        /// <summary>
        /// Retrieves the units for this pronunciation
        /// </summary>
        /// <value></value>
        public Unit[] Units { get; private set; }

        public string Tag { get; private set; }

        /// <summary>
        /// Retrieves the probability for the pronunciation. A word may have multiple pronunciations that are not all equally
        /// probable. All probabilities for particular word sum to 1.0.
        /// </summary>
        /// <value>the probability of this pronunciation as a value between 0 and 1.0.</value>
        public float Probability { get; private set; }

        /// <summary>
        /// Dumps a pronunciation
        /// </summary>
        public void Dump() 
        {
            Console.Out.WriteLine(ToString());
        }

        /// <summary>
        /// Returns a string representation of this Pronunication. 
        /// </summary>
        /// <returns></returns>
        public override string ToString() 
        {
            StringBuilder result = new StringBuilder().Append(Word).Append('(');
            foreach (Unit unit in Units) 
            {
                result.Append(unit).Append(' ');
            }
            result.Append(')');
            return result.ToString();
        }

        /// <summary>
        /// Returns a detailed string representation of this Pronunication.
        /// </summary>
        /// <returns></returns>
        public string ToDetailedString() 
        {
            StringBuilder result = new StringBuilder().Append(Word).Append(' ');
            foreach (Unit unit in Units) 
            {
                result.Append(unit).Append(' ');
            }
            result.Append("\n   class: ").Append(WordClassification)
                .Append(" tag: ").Append(Tag).Append(" prob: ").Append(Probability);

            return result.ToString();
        }
    }
}
