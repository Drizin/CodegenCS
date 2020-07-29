using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductVendor", Schema = "Purchasing")]
    public partial class ProductVendor : INotifyPropertyChanged
    {
        #region Members
        private int _productId;
        [Key]
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private int _averageLeadTime;
        public int AverageLeadTime 
        { 
            get { return _averageLeadTime; } 
            set { SetField(ref _averageLeadTime, value, nameof(AverageLeadTime)); } 
        }
        private decimal? _lastReceiptCost;
        public decimal? LastReceiptCost 
        { 
            get { return _lastReceiptCost; } 
            set { SetField(ref _lastReceiptCost, value, nameof(LastReceiptCost)); } 
        }
        private DateTime? _lastReceiptDate;
        public DateTime? LastReceiptDate 
        { 
            get { return _lastReceiptDate; } 
            set { SetField(ref _lastReceiptDate, value, nameof(LastReceiptDate)); } 
        }
        private int _maxOrderQty;
        public int MaxOrderQty 
        { 
            get { return _maxOrderQty; } 
            set { SetField(ref _maxOrderQty, value, nameof(MaxOrderQty)); } 
        }
        private int _minOrderQty;
        public int MinOrderQty 
        { 
            get { return _minOrderQty; } 
            set { SetField(ref _minOrderQty, value, nameof(MinOrderQty)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private int? _onOrderQty;
        public int? OnOrderQty 
        { 
            get { return _onOrderQty; } 
            set { SetField(ref _onOrderQty, value, nameof(OnOrderQty)); } 
        }
        private decimal _standardPrice;
        public decimal StandardPrice 
        { 
            get { return _standardPrice; } 
            set { SetField(ref _standardPrice, value, nameof(StandardPrice)); } 
        }
        private string _unitMeasureCode;
        public string UnitMeasureCode 
        { 
            get { return _unitMeasureCode; } 
            set { SetField(ref _unitMeasureCode, value, nameof(UnitMeasureCode)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductId == default(int) && BusinessEntityId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Purchasing].[ProductVendor]
                (
                    [AverageLeadTime],
                    [BusinessEntityID],
                    [LastReceiptCost],
                    [LastReceiptDate],
                    [MaxOrderQty],
                    [MinOrderQty],
                    [ModifiedDate],
                    [OnOrderQty],
                    [ProductID],
                    [StandardPrice],
                    [UnitMeasureCode]
                )
                VALUES
                (
                    @AverageLeadTime,
                    @BusinessEntityId,
                    @LastReceiptCost,
                    @LastReceiptDate,
                    @MaxOrderQty,
                    @MinOrderQty,
                    @ModifiedDate,
                    @OnOrderQty,
                    @ProductId,
                    @StandardPrice,
                    @UnitMeasureCode
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Purchasing].[ProductVendor] SET
                    [AverageLeadTime] = @AverageLeadTime,
                    [BusinessEntityID] = @BusinessEntityId,
                    [LastReceiptCost] = @LastReceiptCost,
                    [LastReceiptDate] = @LastReceiptDate,
                    [MaxOrderQty] = @MaxOrderQty,
                    [MinOrderQty] = @MinOrderQty,
                    [ModifiedDate] = @ModifiedDate,
                    [OnOrderQty] = @OnOrderQty,
                    [ProductID] = @ProductId,
                    [StandardPrice] = @StandardPrice,
                    [UnitMeasureCode] = @UnitMeasureCode
                WHERE
                    [ProductID] = @ProductId AND 
                    [BusinessEntityID] = @BusinessEntityId";
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
            ProductVendor other = obj as ProductVendor;
            if (other == null) return false;

            if (AverageLeadTime != other.AverageLeadTime)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (LastReceiptCost != other.LastReceiptCost)
                return false;
            if (LastReceiptDate != other.LastReceiptDate)
                return false;
            if (MaxOrderQty != other.MaxOrderQty)
                return false;
            if (MinOrderQty != other.MinOrderQty)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OnOrderQty != other.OnOrderQty)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (StandardPrice != other.StandardPrice)
                return false;
            if (UnitMeasureCode != other.UnitMeasureCode)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AverageLeadTime == default(int) ? 0 : AverageLeadTime.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (LastReceiptCost == null ? 0 : LastReceiptCost.GetHashCode());
                hash = hash * 23 + (LastReceiptDate == null ? 0 : LastReceiptDate.GetHashCode());
                hash = hash * 23 + (MaxOrderQty == default(int) ? 0 : MaxOrderQty.GetHashCode());
                hash = hash * 23 + (MinOrderQty == default(int) ? 0 : MinOrderQty.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OnOrderQty == null ? 0 : OnOrderQty.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (StandardPrice == default(decimal) ? 0 : StandardPrice.GetHashCode());
                hash = hash * 23 + (UnitMeasureCode == null ? 0 : UnitMeasureCode.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductVendor left, ProductVendor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductVendor left, ProductVendor right)
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
