using FooBar.Libraries.TextGeneration.Constants;

namespace FooBar.Libraries.TextGeneration.Models
{
    public class NumberPart
    {
        public OrderEnum Order { get; set; }
        public int Number { get; set; }
        public string NumberAsText { get; set; }
    }
}
