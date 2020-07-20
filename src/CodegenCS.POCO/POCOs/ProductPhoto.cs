using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductPhoto", Schema = "Production")]
    public partial class ProductPhoto
    {
        #region Members
        [Key]
        public int ProductPhotoId { get; set; }
        public Byte[] LargePhoto { get; set; }
        public string LargePhotoFileName { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Byte[] ThumbNailPhoto { get; set; }
        public string ThumbnailPhotoFileName { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductPhotoId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ProductPhoto]
                (
                    [LargePhoto],
                    [LargePhotoFileName],
                    [ModifiedDate],
                    [ThumbNailPhoto],
                    [ThumbnailPhotoFileName]
                )
                VALUES
                (
                    @LargePhoto,
                    @LargePhotoFileName,
                    @ModifiedDate,
                    @ThumbNailPhoto,
                    @ThumbnailPhotoFileName
                )";

                this.ProductPhotoId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[ProductPhoto] SET
                    [LargePhoto] = @LargePhoto,
                    [LargePhotoFileName] = @LargePhotoFileName,
                    [ModifiedDate] = @ModifiedDate,
                    [ThumbNailPhoto] = @ThumbNailPhoto,
                    [ThumbnailPhotoFileName] = @ThumbnailPhotoFileName
                WHERE
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
            ProductPhoto other = obj as ProductPhoto;
            if (other == null) return false;

            if (LargePhoto != other.LargePhoto)
                return false;
            if (LargePhotoFileName != other.LargePhotoFileName)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ThumbNailPhoto != other.ThumbNailPhoto)
                return false;
            if (ThumbnailPhotoFileName != other.ThumbnailPhotoFileName)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (LargePhoto == null ? 0 : LargePhoto.GetHashCode());
                hash = hash * 23 + (LargePhotoFileName == null ? 0 : LargePhotoFileName.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ThumbNailPhoto == null ? 0 : ThumbNailPhoto.GetHashCode());
                hash = hash * 23 + (ThumbnailPhotoFileName == null ? 0 : ThumbnailPhotoFileName.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductPhoto left, ProductPhoto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductPhoto left, ProductPhoto right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
