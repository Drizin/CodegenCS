using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductModelIllustration", Schema = "Production")]
    public partial class ProductModelIllustration
    {
        #region Members
        [Key]
        public int ProductModelId { get; set; }
        [Key]
        public int IllustrationId { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductModelId == default(int) && IllustrationId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ProductModelIllustration]
                (
                    [IllustrationID],
                    [ModifiedDate],
                    [ProductModelID]
                )
                VALUES
                (
                    @IllustrationId,
                    @ModifiedDate,
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
                UPDATE [Production].[ProductModelIllustration] SET
                    [IllustrationID] = @IllustrationId,
                    [ModifiedDate] = @ModifiedDate,
                    [ProductModelID] = @ProductModelId
                WHERE
                    [ProductModelID] = @ProductModelId AND 
                    [IllustrationID] = @IllustrationId";
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
            ProductModelIllustration other = obj as ProductModelIllustration;
            if (other == null) return false;

            if (IllustrationId != other.IllustrationId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
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
                hash = hash * 23 + (IllustrationId == default(int) ? 0 : IllustrationId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductModelId == default(int) ? 0 : ProductModelId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductModelIllustration left, ProductModelIllustration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductModelIllustration left, ProductModelIllustration right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
