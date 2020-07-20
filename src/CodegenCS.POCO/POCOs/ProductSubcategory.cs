using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ProductSubcategory
    {
        #region Members
        [Key]
        public int ProductSubcategoryId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public int ProductCategoryId { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductSubcategoryId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ProductSubcategory]
                (
                    [ModifiedDate],
                    [Name],
                    [ProductCategoryID]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Name,
                    @ProductCategoryId
                )";

                this.ProductSubcategoryId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ProductSubcategory]
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [Name] = @Name,
                    SET [ProductCategoryID] = @ProductCategoryId
                WHERE
                    [ProductSubcategoryID] = @ProductSubcategoryId";
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
            ProductSubcategory other = obj as ProductSubcategory;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (ProductCategoryId != other.ProductCategoryId)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (ProductCategoryId == default(int) ? 0 : ProductCategoryId.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductSubcategory left, ProductSubcategory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductSubcategory left, ProductSubcategory right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
