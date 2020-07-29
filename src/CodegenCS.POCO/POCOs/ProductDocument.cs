using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductDocument", Schema = "Production")]
    public partial class ProductDocument : INotifyPropertyChanged
    {
        #region Members
        private int _productId;
        [Key]
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
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
                INSERT INTO [Production].[ProductDocument]
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
                UPDATE [Production].[ProductDocument] SET
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
