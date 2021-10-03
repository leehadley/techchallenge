using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FooBar.Libraries.TextGeneration.Constants;
using FooBar.Libraries.TextGeneration.Models;

namespace FooBar.Libraries.TextGeneration
{
    public class Textualiser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string TextualiseDecimalAsString (string input)
        {
            //Guard against null and empty string
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentException("Value must be provided.");
            }

            //Get parsed decimal value
            //Decided to use maths rather than make assumptions about the string
            decimal valueToTextualise = ValidateAndParse(input);

            //Textualise the decimal value
            string returnValue = TextualiseDecimal(valueToTextualise);

            return returnValue;
        }

        private string TextualiseDecimal(decimal value)
        {
            StringBuilder outputBuilder = new StringBuilder();

            //Break down our problem and create a temporary working model:
            //For each order we can store the digits for that order and the
            //text representation as we proceed
            var workingModel = InitaliseWorkingModel(value);

            //Populate the number parts for each order i.e. millions, thousands etc.
            PopulateNumberParts(value, workingModel);

            //Translate the number parts into strings
            PopulateTextParts(workingModel);

            //Build the complete string and return
            foreach (NumberPart numberPart in workingModel.OrderByDescending(n => (int)n.Order))
            {
                if (numberPart.Order == OrderEnum.pence && outputBuilder.Length > 0)
                {
                    outputBuilder.Append("pounds");
                }

                //Don't output pence if there aren't any
                if (numberPart.Number != 0)
                {
                    outputBuilder.Append($"{numberPart.NumberAsText} ");
                }
            }

            //Cheating here to tidy up shoddy spacing
            return outputBuilder.ToString().Replace("  ", " ").Trim();
        }

        private List<NumberPart> InitaliseWorkingModel(decimal value)
        {
            List<NumberPart> workingModel = new List<NumberPart>();

            //We only want to add the relevant elements
            if (value >= (int)OrderEnum.million) {
                workingModel.Add(new NumberPart() { Order = OrderEnum.million });
            }

            if (value >= (int)OrderEnum.thousand) {
                workingModel.Add(new NumberPart() { Order = OrderEnum.thousand });
            }

            if (value >= (int)OrderEnum.hundred) {
                workingModel.Add(new NumberPart() { Order = OrderEnum.hundred });
            }

            if (value >= (int)OrderEnum.tens) {
                workingModel.Add(new NumberPart() { Order = OrderEnum.tens });
            }

            if (value >= (int)OrderEnum.units) {
                workingModel.Add(new NumberPart() { Order = OrderEnum.units });
            }

            workingModel.Add(new NumberPart() { Order = OrderEnum.pence });

            return workingModel;
        }

        private void PopulateNumberParts(decimal value, List<NumberPart> workingModel)
        {
            decimal remainder = value;

            //Work down from the millions; adding parts from the remainder
            foreach (NumberPart numberPart in workingModel.OrderByDescending(n => (int)n.Order)) {
                if (numberPart.Order != OrderEnum.pence)
                {
                    numberPart.Number = (int)Math.Floor(remainder / (int)numberPart.Order);
                    remainder = (int)(value % (decimal)numberPart.Order);
                    if (numberPart.Order == OrderEnum.tens)
                    {
                        numberPart.Number *= 10;
                    }
                } else
                {
                    numberPart.Number = (int)((value - Math.Truncate(value)) * 100);
                }
            }

        }

        private void PopulateTextParts(List<NumberPart> workingModel)
        {
            foreach (NumberPart numberPart in workingModel.OrderByDescending(n => (int)n.Order))
            {
                StringBuilder numberAsWordsBuilder = new StringBuilder();
                numberAsWordsBuilder.Append(NumberToWords(numberPart.Number));
                if (numberPart.Order != OrderEnum.tens && numberPart.Order != OrderEnum.units)
                {
                    numberAsWordsBuilder.Append($" {numberPart.Order.ToString()}");
                }
                numberPart.NumberAsText = numberAsWordsBuilder.ToString();
            }
        }

        private string NumberToWords (int number)
        {
            if (number > 999) {
                throw new ArgumentException("Value cannot be greater than 999");
            }

            bool hasPrecedent = false;

            StringBuilder numberInWordsBuilder = new StringBuilder();
            string paddedStringToParse = number.ToString().PadLeft(3, 'X');

            //Hundreds
            if (paddedStringToParse.Substring(0, 1) != "X") { 
                numberInWordsBuilder.Append((UnitsEnum)Convert.ToInt32(paddedStringToParse.Substring(0, 1)));
                numberInWordsBuilder.AppendFormat(" {0}", OrderEnum.hundred.ToString());
                hasPrecedent = true;
            }

            //Tens
            if (paddedStringToParse.Substring(1, 1) != "X") {
                if (!numberInWordsBuilder.ToString().EndsWith(" ")) {
                    numberInWordsBuilder.Append(" ");
                }
                numberInWordsBuilder.Append("and ");
                numberInWordsBuilder.Append((TensEnum)Convert.ToInt32(paddedStringToParse.Substring(1, 1) + "0"));
                hasPrecedent = true;
            }

            //Units
            if (paddedStringToParse.Substring(2, 1) != "0") {
                if (hasPrecedent)
                {
                    numberInWordsBuilder.Append(" ");
                }
                numberInWordsBuilder.AppendFormat("{0}", (UnitsEnum)Convert.ToInt32(paddedStringToParse.Substring(2, 1)));
            }

            return numberInWordsBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private decimal ValidateAndParse(string input)
        {
            string stringToParse = input;

            //Only GBP jas been mentioned, we would check for other currencies if required.
            var hasCurrency = stringToParse.StartsWith("£");

            //See if we can do a straight parse first. Once we have a decimal value we know
            //we are good to go.
            decimal parsedValue = new decimal();
            var canParse = decimal.TryParse(stringToParse, out parsedValue);

            if (!canParse) //We have some more work to do
            {
                //First check for multiple decimal points. If we have more than 1 then we are stuffed
                //as we cannot make assumptions about which one was intentional - even though logically
                //if one were 3 characters from the end of the string, we could potentially take the risk.
                //But I am not going to do this here.

                CheckForMultipleDecimalPoints(stringToParse);

                //Remove any commas
                stringToParse = RemoveCommas(stringToParse);

                //Replace all non-numeric characters with zero as per acceptance criteria, we have
                //already checked for currency and preceding zeros will not hurt.
                var numericStringToParse = ReplaceAlphaCharactersWithZero(stringToParse);

                //Try to parse the string one final time
                if (!decimal.TryParse(numericStringToParse, out parsedValue))
                {
                    throw new ArgumentException($"Unknown parsing error, parsing: [{numericStringToParse}]");
                }
            }

            if (parsedValue > 1000000000) {
                throw new ArgumentException($"Invalid input, value must be less than 1000000000, parsed value: [{parsedValue}]");
            }

            return parsedValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        private void CheckForMultipleDecimalPoints (string input)
        {
            int decimalPointCount = input.Count(c => c == '.');
            if (decimalPointCount > 1)
            {
                throw new ArgumentException($"Value may only contain one decimal point, parsing: [{input}]");
            }

        }

        private string RemoveCommas(string input) {
            return input.Replace(",", string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ReplaceAlphaCharactersWithZero (string input)
        {
            return Regex.Replace(input, "[^0-9.]", "0");
        }
    }
}
