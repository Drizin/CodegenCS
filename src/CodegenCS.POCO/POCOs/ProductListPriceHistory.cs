using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ProductListPriceHistory
    {
        #region Members
        [Key]
        public int ProductId { get; set; }
        [Key]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal ListPrice { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductId == default(int) && StartDate == default(DateTime))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ProductListPriceHistory]
                (
                    [EndDate],
                    [ListPrice],
                    [ModifiedDate],
                    [ProductID],
                    [StartDate]
                )
                VALUES
                (
                    @EndDate,
                    @ListPrice,
                    @ModifiedDate,
                    @ProductId,
                    @StartDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ProductListPriceHistory]
                    SET [EndDate] = @EndDate,
                    SET [ListPrice] = @ListPrice,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [ProductID] = @ProductId,
                    SET [StartDate] = @StartDate
                WHERE
                    [ProductID] = @ProductId AND 
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
            ProductListPriceHistory other = obj as ProductListPriceHistory;
            if (other == null) return false;

            if (EndDate != other.EndDate)
                return false;
            if (ListPrice != other.ListPrice)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (StartDate != other.StartDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (EndDate == null ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (ListPrice == default(decimal) ? 0 : ListPrice.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductListPriceHistory left, ProductListPriceHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductListPriceHistory left, ProductListPriceHistory right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
