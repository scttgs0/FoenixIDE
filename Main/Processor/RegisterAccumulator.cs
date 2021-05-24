namespace FoenixIDE.Processor
{
    public class RegisterAccumulator : Register
    {
        public int Value16
        {
            get { return this._value; }
            set { this._value = value; }
        }
    }
}
