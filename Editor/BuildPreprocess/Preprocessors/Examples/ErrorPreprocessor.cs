namespace Ugly.BuildPreprocess
{
    public class ErrorPreprocessor : AbstractPreprocessor
    {
        public ErrorPreprocessor() : base(typeof(DefaultConfig), false) { }

        protected override bool Process(out string message)
        {
            message = "bla bla smth wrong!";
            return false;
        }
    }
}
