using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductReview", Schema = "Production")]
    public partial class ProductReview
    {
        #region Members
        [Key]
        public int ProductReviewId { get; set; }
        public string Comments { get; set; }
        public string EmailAddress { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ReviewerName { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductReviewId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ProductReview]
                (
                    [Comments],
                    [EmailAddress],
                    [ModifiedDate],
                    [ProductID],
                    [Rating],
                    [ReviewDate],
                    [ReviewerName]
                )
                VALUES
                (
                    @Comments,
                    @EmailAddress,
                    @ModifiedDate,
                    @ProductId,
                    @Rating,
                    @ReviewDate,
                    @ReviewerName
                )";

                this.ProductReviewId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[ProductReview] SET
                    [Comments] = @Comments,
                    [EmailAddress] = @EmailAddress,
                    [ModifiedDate] = @ModifiedDate,
                    [ProductID] = @ProductId,
                    [Rating] = @Rating,
                    [ReviewDate] = @ReviewDate,
                    [ReviewerName] = @ReviewerName
                WHERE
                    [ProductReviewID] = @ProductReviewId";
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
            ProductReview other = obj as ProductReview;
            if (other == null) return false;

            if (Comments != other.Comments)
                return false;
            if (EmailAddress != other.EmailAddress)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (Rating != other.Rating)
                return false;
            if (ReviewDate != other.ReviewDate)
                return false;
            if (ReviewerName != other.ReviewerName)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Comments == null ? 0 : Comments.GetHashCode());
                hash = hash * 23 + (EmailAddress == null ? 0 : EmailAddress.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (Rating == default(int) ? 0 : Rating.GetHashCode());
                hash = hash * 23 + (ReviewDate == default(DateTime) ? 0 : ReviewDate.GetHashCode());
                hash = hash * 23 + (ReviewerName == null ? 0 : ReviewerName.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductReview left, ProductReview right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductReview left, ProductReview right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
