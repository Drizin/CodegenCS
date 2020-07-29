using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductInventory", Schema = "Production")]
    public partial class ProductInventory : INotifyPropertyChanged
    {
        #region Members
        private int _productId;
        [Key]
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private short _locationId;
        [Key]
        public short LocationId 
        { 
            get { return _locationId; } 
            set { SetField(ref _locationId, value, nameof(LocationId)); } 
        }
        private byte _bin;
        public byte Bin 
        { 
            get { return _bin; } 
            set { SetField(ref _bin, value, nameof(Bin)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private short _quantity;
        public short Quantity 
        { 
            get { return _quantity; } 
            set { SetField(ref _quantity, value, nameof(Quantity)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private string _shelf;
        public string Shelf 
        { 
            get { return _shelf; } 
            set { SetField(ref _shelf, value, nameof(Shelf)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductId == default(int) && LocationId == default(short))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ProductInventory]
                (
                    [Bin],
                    [LocationID],
                    [ModifiedDate],
                    [ProductID],
                    [Quantity],
                    [Shelf]
                )
                VALUES
                (
                    @Bin,
                    @LocationId,
                    @ModifiedDate,
                    @ProductId,
                    @Quantity,
                    @Shelf
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[ProductInventory] SET
                    [Bin] = @Bin,
                    [LocationID] = @LocationId,
                    [ModifiedDate] = @ModifiedDate,
                    [ProductID] = @ProductId,
                    [Quantity] = @Quantity,
                    [Shelf] = @Shelf
                WHERE
                    [ProductID] = @ProductId AND 
                    [LocationID] = @LocationId";
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
            ProductInventory other = obj as ProductInventory;
            if (other == null) return false;

            if (Bin != other.Bin)
                return false;
            if (LocationId != other.LocationId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (Quantity != other.Quantity)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (Shelf != other.Shelf)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Bin == default(byte) ? 0 : Bin.GetHashCode());
                hash = hash * 23 + (LocationId == default(short) ? 0 : LocationId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (Quantity == default(short) ? 0 : Quantity.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (Shelf == null ? 0 : Shelf.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductInventory left, ProductInventory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductInventory left, ProductInventory right)
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
