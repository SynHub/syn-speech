using Microsoft.VisualStudio.TestTools.UnitTesting;
//Incomplete because this same test fails in the Java verion.
namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class DynamicTrigramModelTest
    {

    //    private IDictionary dictionary;

    //    public DynamicTrigramModelTest()
    //    {
    //        var dictUrl = new URL("cmudict-en-us.dict");
    //        var noiseDictUrl = new URL("noisedict");

    //        dictionary = new TextDictionary(dictUrl, noiseDictUrl, null, null, new UnitManager());
    //        dictionary.Allocate();
    //    }

    //    [TestMethod]
    //    public void DynamicTrigramModel_UnigramModel()
    //    {
    //        var model = new DynamicTrigramModel(dictionary);
    //        model.SetText(Arrays.AsList("one"));
    //        model.Allocate();
    //        Assert.IsTrue(model.Vocabulary.Contains("one"));
    //        var expected = model.GetProbability(new WordSequence(dictionary.GetWord("one")));
    //        var result = LogMath.GetLogMath().LinearToLog(1.0 / 3);
    //        Assert.AreEqual(expected, result);
    //    }
    //
    }
}
