using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesTerritoryHistory", Schema = "Sales")]
    public partial class SalesTerritoryHistory
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public int TerritoryId { get; set; }
        [Key]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && TerritoryId == default(int) && StartDate == default(DateTime))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesTerritoryHistory]
                (
                    [BusinessEntityID],
                    [EndDate],
                    [ModifiedDate],
                    [StartDate],
                    [TerritoryID]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @EndDate,
                    @ModifiedDate,
                    @StartDate,
                    @TerritoryId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesTerritoryHistory] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [EndDate] = @EndDate,
                    [ModifiedDate] = @ModifiedDate,
                    [StartDate] = @StartDate,
                    [TerritoryID] = @TerritoryId
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [TerritoryID] = @TerritoryId AND 
                    [StartDate] = @StartDate";
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
            SalesTerritoryHistory other = obj as SalesTerritoryHistory;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (EndDate != other.EndDate)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StartDate != other.StartDate)
                return false;
            if (TerritoryId != other.TerritoryId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (EndDate == null ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                hash = hash * 23 + (TerritoryId == default(int) ? 0 : TerritoryId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesTerritoryHistory left, SalesTerritoryHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesTerritoryHistory left, SalesTerritoryHistory right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
