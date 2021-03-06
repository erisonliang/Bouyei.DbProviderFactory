﻿/*-------------------------------------------------------------
 *   auth: bouyei
 *   date: 2017/2/10 9:34:04
 *contact: 453840293@qq.com
 *profile: www.openthinking.cn
 *    Ltd: 
 *   guid: 44ea6ba7-acc1-452e-9c70-58b7a7083a3f
---------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Bouyei.DbProviderFactory.DbAdoProvider.Plugins
{
    using Oracle.DataAccess.Client;

    internal class OracleBulk : IDisposable
    {
        OracleBulkCopy bulkCopy = null;
        bool disposed = false;

        public BulkCopiedArgs BulkCopiedHandler { get; set; }

        public BulkCopyOptions Option { get; private set; }

        public string ConnectionString { get; private set; }

        public OracleBulk(string ConnectionString, int timeout = 1800, 
            BulkCopyOptions option = BulkCopyOptions.KeepIdentity)
        {
            this.Option = option;
            this.ConnectionString = ConnectionString;
            bulkCopy = CreatedBulkCopy(option);
            bulkCopy.BulkCopyTimeout = timeout;
        }

        public OracleBulk(IDbConnection dbConnection, int timeout = 1800,
            BulkCopyOptions option = BulkCopyOptions.KeepIdentity)
        {
            this.Option = option;
            this.ConnectionString = ConnectionString;
            OracleConnection oracleConnection = (OracleConnection)dbConnection;
            bulkCopy = new OracleBulkCopy(oracleConnection, (OracleBulkCopyOptions)option);
            bulkCopy.BulkCopyTimeout = timeout;
        }
        private OracleBulkCopy CreatedBulkCopy(BulkCopyOptions option)
        {
            if (option == BulkCopyOptions.Default)
            {
                return new OracleBulkCopy(ConnectionString, OracleBulkCopyOptions.Default);
            }
            else if (option == BulkCopyOptions.UseInternalTransaction)
            {
                return new OracleBulkCopy(ConnectionString, OracleBulkCopyOptions.UseInternalTransaction);
            }
            else return new OracleBulkCopy(ConnectionString);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (bulkCopy != null)
                {
                    bulkCopy.Close();
                    bulkCopy = null;
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            if (bulkCopy != null)
                bulkCopy.Close();
        }

        private void InitBulkCopy(DataTable dt, int batchSize = 10240)
        {
            if (bulkCopy.ColumnMappings.Count > 0) bulkCopy.ColumnMappings.Clear();

            bulkCopy.ColumnMappings.Capacity = dt.Columns.Count;
            bulkCopy.DestinationTableName = dt.TableName;
            bulkCopy.BatchSize = batchSize;

            for (int i = 0; i < dt.Columns.Count; ++i)
            {
                bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName,
                    dt.Columns[i].ColumnName);
            }
            if (BulkCopiedHandler != null)
            {
                bulkCopy.NotifyAfter = batchSize;
                bulkCopy.OracleRowsCopied += BulkCopy_OracleRowsCopied;
            }
        }

        private void InitBulkCopy(string tableName, string[] columnNames, int batchSize)
        {
            if (bulkCopy.ColumnMappings.Count > 0) bulkCopy.ColumnMappings.Clear();
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.BatchSize = batchSize;

            for (int i = 0; i < columnNames.Length; ++i)
            {
                bulkCopy.ColumnMappings.Add(columnNames[i],
                    columnNames[i]);
            }

            if (BulkCopiedHandler != null)
            {
                bulkCopy.NotifyAfter = batchSize;
                bulkCopy.OracleRowsCopied += BulkCopy_OracleRowsCopied;
            }
        }

        private void InitBulkCopy(string tableName, int batchSize)
        {
            if (bulkCopy.ColumnMappings.Count > 0) bulkCopy.ColumnMappings.Clear();

            bulkCopy.DestinationTableName = tableName;
            bulkCopy.BatchSize = batchSize;

            if (BulkCopiedHandler != null)
            {
                bulkCopy.NotifyAfter = batchSize;
                bulkCopy.OracleRowsCopied += BulkCopy_OracleRowsCopied;
            }
        }

        public void WriteToServer(DataTable dt, int batchSize = 102400)
        {
            InitBulkCopy(dt, batchSize);
            bulkCopy.WriteToServer(dt);
        }

        public void WriteToServer(string tableName, IDataReader iDataReader, int batchSize = 102400)
        {
            string[] columnNames = new string[iDataReader.FieldCount];
            for (int i = 0; i < columnNames.Length; ++i)
            {
                columnNames[i] = iDataReader.GetName(i);
            }
            InitBulkCopy(tableName, columnNames, batchSize);
            bulkCopy.WriteToServer(iDataReader);
        }

        public void WriteToServer(string tableName, DataRow[] rows, int batchSize = 102400)
        {
            InitBulkCopy(tableName, batchSize);
            bulkCopy.WriteToServer(rows);
        }

        void BulkCopy_OracleRowsCopied(object sender, OracleRowsCopiedEventArgs eventArgs)
        {
            if (BulkCopiedHandler != null)
            {
                BulkCopiedHandler(eventArgs.RowsCopied);
            }
        }

        public void WriteToServer(DataTable dt, DataRowState rowState, int batchSize = 102400)
        {
            InitBulkCopy(dt, batchSize);
            bulkCopy.WriteToServer(dt, rowState);
        }

        public void WriteToServer(DataRow[] rows, int batchSize = 102400)
        {
            InitBulkCopy(rows[0].Table, batchSize);
            bulkCopy.WriteToServer(rows);
        }
    }
}
