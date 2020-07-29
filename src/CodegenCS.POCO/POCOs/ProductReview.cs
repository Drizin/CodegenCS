using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductReview", Schema = "Production")]
    public partial class ProductReview : INotifyPropertyChanged
    {
        #region Members
        private int _productReviewId;
        [Key]
        public int ProductReviewId 
        { 
            get { return _productReviewId; } 
            set { SetField(ref _productReviewId, value, nameof(ProductReviewId)); } 
        }
        private string _comments;
        public string Comments 
        { 
            get { return _comments; } 
            set { SetField(ref _comments, value, nameof(Comments)); } 
        }
        private string _emailAddress;
        public string EmailAddress 
        { 
            get { return _emailAddress; } 
            set { SetField(ref _emailAddress, value, nameof(EmailAddress)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private int _productId;
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private int _rating;
        public int Rating 
        { 
            get { return _rating; } 
            set { SetField(ref _rating, value, nameof(Rating)); } 
        }
        private DateTime _reviewDate;
        public DateTime ReviewDate 
        { 
            get { return _reviewDate; } 
            set { SetField(ref _reviewDate, value, nameof(ReviewDate)); } 
        }
        private string _reviewerName;
        public string ReviewerName 
        { 
            get { return _reviewerName; } 
            set { SetField(ref _reviewerName, value, nameof(ReviewerName)); } 
        }
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

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}
