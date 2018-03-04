namespace SqliteNative
{
    public interface IColumns
    {
        int Count { get; }
        int DataCount { get; }
        string NameOf(int index);

        byte[] GetBlob(int index);
        string GetText(int index);
        long GetInt64(int index);
        int GetInt32(int index);
        double GetDouble(int index);
        IValue GetValue(int index);
        ColumnType GetType(int index);
    }
}
