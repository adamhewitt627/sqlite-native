namespace SqliteNative
{
    public interface IError
    {
        string Message { get; }
        string String { get; }
        int Code { get; }
        int ExtendedCode { get; }
    }
}
