using System;

//I would not ordinary tightly couple this project/library. Ideally it would be broken out into a nuget package
//and injected using IOC, or expose methods through a micro-service, depending on the implementation.
using FooBar.Libraries.TextGeneration;

namespace FooBar.Applications.Textualiser.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Libraries.TextGeneration.Textualiser textualiser = new Libraries.TextGeneration.Textualiser();
            string userInput = string.Empty;
            while (userInput.ToLower() != "x")
            {
                userInput = RequestInput();
                if (!string.IsNullOrEmpty(userInput) && userInput.ToLower() != "x")
                {
                    System.Console.WriteLine(textualiser.TextualiseDecimalAsString(userInput));
                    System.Console.WriteLine();
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string RequestInput()
        {
            string userInput = string.Empty;
            try
            {
                System.Console.WriteLine("Conduent Tech Challenge");
                System.Console.WriteLine("-----------------------");
                System.Console.WriteLine();
                System.Console.WriteLine("Please enter a currency value in GBP to convert it to words or enter x to quit:");
                userInput = System.Console.ReadLine();
            }
            catch (Exception exception)
            {
                userInput = null;
                System.Console.WriteLine("We are sorry, something went wrong and your value could not be converted into text. Please enter another value or enter x to quit.");
                //I would usually log the exception somewhere e.g., event log, file, db, external api etc.
                //and include the input values and stack trace (with any inner exceptions) to aid debugging.
            }
            
            return userInput;

        }
    }
}
