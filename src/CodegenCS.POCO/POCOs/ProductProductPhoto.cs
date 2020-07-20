using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductProductPhoto", Schema = "Production")]
    public partial class ProductProductPhoto
    {
        #region Members
        [Key]
        public int ProductId { get; set; }
        [Key]
        public int ProductPhotoId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Primary { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductId == default(int) && ProductPhotoId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ProductProductPhoto]
                (
                    [ModifiedDate],
                    [Primary],
                    [ProductID],
                    [ProductPhotoID]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Primary,
                    @ProductId,
                    @ProductPhotoId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[ProductProductPhoto] SET
                    [ModifiedDate] = @ModifiedDate,
                    [Primary] = @Primary,
                    [ProductID] = @ProductId,
                    [ProductPhotoID] = @ProductPhotoId
                WHERE
                    [ProductID] = @ProductId AND 
                    [ProductPhotoID] = @ProductPhotoId";
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
            ProductProductPhoto other = obj as ProductProductPhoto;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Primary != other.Primary)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (ProductPhotoId != other.ProductPhotoId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Primary == default(bool) ? 0 : Primary.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (ProductPhotoId == default(int) ? 0 : ProductPhotoId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductProductPhoto left, ProductProductPhoto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductProductPhoto left, ProductProductPhoto right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
