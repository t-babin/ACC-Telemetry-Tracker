namespace AccTelemetryTracker.Logic;

public class MotecFileExistsException : Exception
{
    public MotecFileExistsException() { }
    public MotecFileExistsException(string message) : base(message) { }
    public MotecFileExistsException(string message, System.Exception inner) : base(message, inner) { }
    public MotecFileExistsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
