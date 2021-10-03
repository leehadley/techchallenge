using Microsoft.VisualStudio.TestTools.UnitTesting;
using FooBar.Libraries.TextGeneration;

namespace FooBar.Libraries.TextGeneration.Tests
{
    [TestClass]
    public class TextualiserTest
    {
        [TestMethod]
        public void CanOutputMillions()
        {
            //Arrange
            Textualiser textualiser = new Textualiser();
            string inputValue = "£123,456,789.10";
            string expectResult = "one hundred and twenty three million four hundred and fifty six thousand seven hundred and eighty nine pounds and ten pence";
            //Act
            string result = textualiser.TextualiseDecimalAsString(inputValue);

            //Assert
            Assert.AreEqual(expectResult, result);
        }

        [TestMethod]
        public void CanOutputThousands()
        {
            //Arrange
            Textualiser textualiser = new Textualiser();
            string inputValue = "£456,789.10";
            string expectResult = "four hundred and fifty six thousand seven hundred and eighty nine pounds and ten pence";
            //Act
            string result = textualiser.TextualiseDecimalAsString(inputValue);

            //Assert
            Assert.AreEqual(expectResult, result);
        }

        [TestMethod]
        public void CanOutputHundreds()
        {
            //Arrange
            Textualiser textualiser = new Textualiser();
            string inputValue = "£789.10";
            string expectResult = "seven hundred and eighty nine pounds and ten pence";
            //Act
            string result = textualiser.TextualiseDecimalAsString(inputValue);

            //Assert
            Assert.AreEqual(expectResult, result);
        }

        [TestMethod]
        public void CanOutputUnits()
        {
            //Arrange
            Textualiser textualiser = new Textualiser();
            string inputValue = "£9.10";
            string expectResult = "nine pounds and ten pence";
            //Act
            string result = textualiser.TextualiseDecimalAsString(inputValue);

            //Assert
            Assert.AreEqual(expectResult, result);
        }

        [TestMethod]
        public void CanOutputPence()
        {
            //Arrange
            Textualiser textualiser = new Textualiser();
            string inputValue = ".20";
            string expectResult = "";
            //Act
            string result = textualiser.TextualiseDecimalAsString(inputValue);

            //Assert
            Assert.AreEqual(expectResult, result);
        }

    }
}
