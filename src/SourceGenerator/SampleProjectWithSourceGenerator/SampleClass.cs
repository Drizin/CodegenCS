namespace SampleProjectWithSourceGenerator
{
    public partial class SampleClass
    {
        public SampleClass()
        {
            Initialize(); // Guess what? This method will be generated on the fly!
        }
    }
}
