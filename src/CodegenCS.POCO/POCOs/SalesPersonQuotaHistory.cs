using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesPersonQuotaHistory", Schema = "Sales")]
    public partial class SalesPersonQuotaHistory
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public DateTime QuotaDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        public decimal SalesQuota { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && QuotaDate == default(DateTime))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesPersonQuotaHistory]
                (
                    [BusinessEntityID],
                    [ModifiedDate],
                    [QuotaDate],
                    [SalesQuota]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @ModifiedDate,
                    @QuotaDate,
                    @SalesQuota
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesPersonQuotaHistory] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [ModifiedDate] = @ModifiedDate,
                    [QuotaDate] = @QuotaDate,
                    [SalesQuota] = @SalesQuota
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [QuotaDate] = @QuotaDate";
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
            SalesPersonQuotaHistory other = obj as SalesPersonQuotaHistory;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (QuotaDate != other.QuotaDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalesQuota != other.SalesQuota)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (QuotaDate == default(DateTime) ? 0 : QuotaDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalesQuota == default(decimal) ? 0 : SalesQuota.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesPersonQuotaHistory left, SalesPersonQuotaHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesPersonQuotaHistory left, SalesPersonQuotaHistory right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
