using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class Customer
    {
        #region Members
        [Key]
        public int CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? PersonId { get; set; }
        public Guid Rowguid { get; set; }
        public int? StoreId { get; set; }
        public int? TerritoryId { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CustomerId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Customer]
                (
                    [ModifiedDate],
                    [PersonID],
                    [StoreID],
                    [TerritoryID]
                )
                VALUES
                (
                    @ModifiedDate,
                    @PersonId,
                    @StoreId,
                    @TerritoryId
                )";

                this.CustomerId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Customer] SET
                    [ModifiedDate] = @ModifiedDate,
                    [PersonID] = @PersonId,
                    [StoreID] = @StoreId,
                    [TerritoryID] = @TerritoryId
                WHERE
                    [CustomerID] = @CustomerId";
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
            Customer other = obj as Customer;
            if (other == null) return false;

            if (AccountNumber != other.AccountNumber)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PersonId != other.PersonId)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StoreId != other.StoreId)
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
                hash = hash * 23 + (AccountNumber == null ? 0 : AccountNumber.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PersonId == null ? 0 : PersonId.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StoreId == null ? 0 : StoreId.GetHashCode());
                hash = hash * 23 + (TerritoryId == null ? 0 : TerritoryId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Customer left, Customer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Customer left, Customer right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
