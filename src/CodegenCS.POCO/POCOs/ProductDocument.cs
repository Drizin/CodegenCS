using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ProductDocument
    {
        #region Members
        [Key]
        public int ProductId { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ProductDocument]
                (
                    [ModifiedDate],
                    [ProductID]
                )
                VALUES
                (
                    @ModifiedDate,
                    @ProductId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ProductDocument] SET
                    [ModifiedDate] = @ModifiedDate,
                    [ProductID] = @ProductId
                WHERE
                    [ProductID] = @ProductId";
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
            ProductDocument other = obj as ProductDocument;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductId != other.ProductId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductDocument left, ProductDocument right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductDocument left, ProductDocument right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
