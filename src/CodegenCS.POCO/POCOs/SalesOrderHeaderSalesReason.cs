using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesOrderHeaderSalesReason", Schema = "Sales")]
    public partial class SalesOrderHeaderSalesReason
    {
        #region Members
        [Key]
        public int SalesOrderId { get; set; }
        [Key]
        public int SalesReasonId { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SalesOrderId == default(int) && SalesReasonId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesOrderHeaderSalesReason]
                (
                    [ModifiedDate],
                    [SalesOrderID],
                    [SalesReasonID]
                )
                VALUES
                (
                    @ModifiedDate,
                    @SalesOrderId,
                    @SalesReasonId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesOrderHeaderSalesReason] SET
                    [ModifiedDate] = @ModifiedDate,
                    [SalesOrderID] = @SalesOrderId,
                    [SalesReasonID] = @SalesReasonId
                WHERE
                    [SalesOrderID] = @SalesOrderId AND 
                    [SalesReasonID] = @SalesReasonId";
                conn.Execute(cmd, this);
            }
        }
        #endregion ActiveRecord

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            SalesOrderHeaderSalesReason other = obj as SalesOrderHeaderSalesReason;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (SalesOrderId != other.SalesOrderId)
                return false;
            if (SalesReasonId != other.SalesReasonId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (SalesOrderId == default(int) ? 0 : SalesOrderId.GetHashCode());
                hash = hash * 23 + (SalesReasonId == default(int) ? 0 : SalesReasonId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesOrderHeaderSalesReason left, SalesOrderHeaderSalesReason right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesOrderHeaderSalesReason left, SalesOrderHeaderSalesReason right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
