namespace AccMotec.Logic
{
    public class MotecParseException : System.Exception
    {
        public MotecParseException() {}
        public MotecParseException(string message) : base(message) {}
        public MotecParseException(string message, System.Exception inner) : base(message, inner) {}
        public MotecParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {}
    }
}