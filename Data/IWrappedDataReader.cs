using System;
using System.Data;

namespace SQL.Data
{
    public interface IWrappedDataReader : IDataReader, IDisposable, IDataRecord
    {
        IDataReader Reader { get; }
        IDbCommand Command { get; }
    }
}