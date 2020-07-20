using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductModelProductDescriptionCulture", Schema = "Production")]
    public partial class ProductModelProductDescriptionCulture
    {
        #region Members
        [Key]
        public int ProductModelId { get; set; }
        [Key]
        public int ProductDescriptionId { get; set; }
        [Key]
        public string CultureId { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductModelId == default(int) && ProductDescriptionId == default(int) && CultureId == null)
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ProductModelProductDescriptionCulture]
                (
                    [CultureID],
                    [ModifiedDate],
                    [ProductDescriptionID],
                    [ProductModelID]
                )
                VALUES
                (
                    @CultureId,
                    @ModifiedDate,
                    @ProductDescriptionId,
                    @ProductModelId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[ProductModelProductDescriptionCulture] SET
                    [CultureID] = @CultureId,
                    [ModifiedDate] = @ModifiedDate,
                    [ProductDescriptionID] = @ProductDescriptionId,
                    [ProductModelID] = @ProductModelId
                WHERE
                    [ProductModelID] = @ProductModelId AND 
                    [ProductDescriptionID] = @ProductDescriptionId AND 
                    [CultureID] = @CultureId";
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
            ProductModelProductDescriptionCulture other = obj as ProductModelProductDescriptionCulture;
            if (other == null) return false;

            if (CultureId != other.CultureId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductDescriptionId != other.ProductDescriptionId)
                return false;
            if (ProductModelId != other.ProductModelId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CultureId == null ? 0 : CultureId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductDescriptionId == default(int) ? 0 : ProductDescriptionId.GetHashCode());
                hash = hash * 23 + (ProductModelId == default(int) ? 0 : ProductModelId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductModelProductDescriptionCulture left, ProductModelProductDescriptionCulture right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductModelProductDescriptionCulture left, ProductModelProductDescriptionCulture right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
