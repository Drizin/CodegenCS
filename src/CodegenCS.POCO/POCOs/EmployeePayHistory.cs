using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("EmployeePayHistory", Schema = "HumanResources")]
    public partial class EmployeePayHistory
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public DateTime RateChangeDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public byte PayFrequency { get; set; }
        public decimal Rate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && RateChangeDate == default(DateTime))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [HumanResources].[EmployeePayHistory]
                (
                    [BusinessEntityID],
                    [ModifiedDate],
                    [PayFrequency],
                    [Rate],
                    [RateChangeDate]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @ModifiedDate,
                    @PayFrequency,
                    @Rate,
                    @RateChangeDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [HumanResources].[EmployeePayHistory] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [ModifiedDate] = @ModifiedDate,
                    [PayFrequency] = @PayFrequency,
                    [Rate] = @Rate,
                    [RateChangeDate] = @RateChangeDate
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [RateChangeDate] = @RateChangeDate";
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
            EmployeePayHistory other = obj as EmployeePayHistory;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PayFrequency != other.PayFrequency)
                return false;
            if (Rate != other.Rate)
                return false;
            if (RateChangeDate != other.RateChangeDate)
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
                hash = hash * 23 + (PayFrequency == default(byte) ? 0 : PayFrequency.GetHashCode());
                hash = hash * 23 + (Rate == default(decimal) ? 0 : Rate.GetHashCode());
                hash = hash * 23 + (RateChangeDate == default(DateTime) ? 0 : RateChangeDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(EmployeePayHistory left, EmployeePayHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmployeePayHistory left, EmployeePayHistory right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
