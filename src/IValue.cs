namespace SqliteNative
{
    public interface IValue
    {
        ColumnType Type { get; }
        byte[] AsBlob();
        int AsInt32();
        long AsInt64();
        double AsDouble();
        string AsText();
    }
}
