﻿/*-------------------------------------------------------------
 *   auth: bouyei
 *   date: 2017/2/10 9:33:53
 *contact: 453840293@qq.com
 *profile: www.openthinking.cn
 *    Ltd: 
 *   guid: fd219fea-b1b9-48b2-b864-2f26d24f678e
---------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Bouyei.DbProviderFactory.DbAdoProvider.Plugins
{
    using IBM.Data.DB2;

    internal class Db2Bulk : IDisposable
    {
        DB2BulkCopy bulkCopy = null;
        bool disposed = false;

        public BulkCopiedArgs BulkCopiedHandler { get; set; }

        public BulkCopyOptions Option { get; private set; }

        public string ConnectionString { get; private set; }

        public Db2Bulk(string ConnectionString, int timeout = 1800,
            BulkCopyOptions option = BulkCopyOptions.KeepIdentity)
        {
            this.Option = option;
            this.ConnectionString = ConnectionString;
            bulkCopy = CreatedBulkCopy(option);
            bulkCopy.BulkCopyTimeout = timeout;
        }

        public Db2Bulk(IDbConnection dbConnection, int timeout = 1800, 
            BulkCopyOptions option = BulkCopyOptions.KeepIdentity)
        {
            this.Option = option;
            this.ConnectionString = ConnectionString;
            DB2Connection oracleConnection = (DB2Connection)dbConnection;
            bulkCopy = new DB2BulkCopy(oracleConnection, (DB2BulkCopyOptions)option);
            bulkCopy.BulkCopyTimeout = timeout;
        }
        private DB2BulkCopy CreatedBulkCopy(BulkCopyOptions option)
        {
            if (option == BulkCopyOptions.Default ||
                option == BulkCopyOptions.KeepIdentity)
            {
                return new DB2BulkCopy(ConnectionString, (DB2BulkCopyOptions)option);
            }
            else if (option == BulkCopyOptions.TableLock)
            {
                return new DB2BulkCopy(ConnectionString, DB2BulkCopyOptions.TableLock);
            }
            else return new DB2BulkCopy(ConnectionString);
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

        private void InitBulkCopy(DataTable dt,int batchSize)
        {
            if (bulkCopy.ColumnMappings.Count > 0) bulkCopy.ColumnMappings.Clear();

            bulkCopy.ColumnMappings.Capacity = dt.Columns.Count;
            bulkCopy.DestinationTableName = dt.TableName;

            for (int i = 0; i < dt.Columns.Count; ++i)
            {
                bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName,
                    dt.Columns[i].ColumnName);
            }
            if (BulkCopiedHandler != null)
            {
                bulkCopy.NotifyAfter = batchSize; 
                bulkCopy.DB2RowsCopied += BulkCopy_DB2RowsCopied;
            }
        }

        private void InitBulkCopy(string tableName, string[] columnNames, int batchSize)
        {
            if (bulkCopy.ColumnMappings.Count > 0) bulkCopy.ColumnMappings.Clear();

            bulkCopy.DestinationTableName = tableName;
            for (int i = 0; i < columnNames.Length; ++i)
            {
                bulkCopy.ColumnMappings.Add(columnNames[i],
                    columnNames[i]);
            }

            if (BulkCopiedHandler != null)
            {
                bulkCopy.NotifyAfter = batchSize;
                bulkCopy.DB2RowsCopied += BulkCopy_DB2RowsCopied;
            }
        }

        private void InitBulkCopy(string tableName, int batchSize)
        {
            if (bulkCopy.ColumnMappings.Count > 0) bulkCopy.ColumnMappings.Clear();

            bulkCopy.DestinationTableName = tableName;

            if (BulkCopiedHandler != null)
            {
                bulkCopy.NotifyAfter = batchSize;
                bulkCopy.DB2RowsCopied += BulkCopy_DB2RowsCopied;
            }
        }

        void BulkCopy_DB2RowsCopied(object sender, DB2RowsCopiedEventArgs e)
        {
            if (BulkCopiedHandler != null)
            {
                BulkCopiedHandler(e.RowsCopied);
            }
        }

        public void WriteToServer(DataTable dt,int batchSize=10240)
        {
            InitBulkCopy(dt, batchSize);
            bulkCopy.WriteToServer(dt);

            if (bulkCopy.Errors.Count > 0)
            {
                throw new Exception(string.Format("入库失败条数:{0}信息;{1}", bulkCopy.Errors.Count, bulkCopy.Errors[0].Message));
            }
        }

        public void WriteToServer(string tableName, IDataReader iDataReader, int batchSize = 10240)
        {
            string[] columnNames = new string[iDataReader.FieldCount];
            for (int i = 0; i < columnNames.Length; ++i)
            {
                columnNames[i] = iDataReader.GetName(i);
            }
            InitBulkCopy(tableName, columnNames, batchSize);
            bulkCopy.WriteToServer(iDataReader);
        }

        public void WriteToServer(string tableName, DataRow[] rows, int batchSize = 10240)
        {
            InitBulkCopy(tableName, batchSize);

            bulkCopy.WriteToServer(rows);
        }

        public void WriteToServer(DataTable dt, DataRowState rowState, int batchSize = 10240)
        {
            InitBulkCopy(dt, batchSize);
            bulkCopy.WriteToServer(dt, rowState);

            if (bulkCopy.Errors.Count > 0)
            {
                throw new Exception(string.Format("入库失败条数:{0}信息;{1}", bulkCopy.Errors.Count, bulkCopy.Errors[0].Message));
            }
        }

        public void WriteToServer(DataRow[] rows,int batchSize=10240)
        {
            InitBulkCopy(rows[0].Table, batchSize);
            bulkCopy.WriteToServer(rows);

            if (bulkCopy.Errors.Count > 0)
            {
                throw new Exception(string.Format("入库失败条数:{0}信息;{1}", bulkCopy.Errors.Count, bulkCopy.Errors[0].Message));
            }
        }

    }
}
