using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesOrderDetail", Schema = "Sales")]
    public partial class SalesOrderDetail : INotifyPropertyChanged
    {
        #region Members
        private int _salesOrderId;
        [Key]
        public int SalesOrderId 
        { 
            get { return _salesOrderId; } 
            set { SetField(ref _salesOrderId, value, nameof(SalesOrderId)); } 
        }
        private int _salesOrderDetailId;
        [Key]
        public int SalesOrderDetailId 
        { 
            get { return _salesOrderDetailId; } 
            set { SetField(ref _salesOrderDetailId, value, nameof(SalesOrderDetailId)); } 
        }
        private string _carrierTrackingNumber;
        public string CarrierTrackingNumber 
        { 
            get { return _carrierTrackingNumber; } 
            set { SetField(ref _carrierTrackingNumber, value, nameof(CarrierTrackingNumber)); } 
        }
        private decimal _lineTotal;
        public decimal LineTotal 
        { 
            get { return _lineTotal; } 
            set { SetField(ref _lineTotal, value, nameof(LineTotal)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private short _orderQty;
        public short OrderQty 
        { 
            get { return _orderQty; } 
            set { SetField(ref _orderQty, value, nameof(OrderQty)); } 
        }
        private int _productId;
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private int _specialOfferId;
        public int SpecialOfferId 
        { 
            get { return _specialOfferId; } 
            set { SetField(ref _specialOfferId, value, nameof(SpecialOfferId)); } 
        }
        private decimal _unitPrice;
        public decimal UnitPrice 
        { 
            get { return _unitPrice; } 
            set { SetField(ref _unitPrice, value, nameof(UnitPrice)); } 
        }
        private decimal _unitPriceDiscount;
        public decimal UnitPriceDiscount 
        { 
            get { return _unitPriceDiscount; } 
            set { SetField(ref _unitPriceDiscount, value, nameof(UnitPriceDiscount)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SalesOrderId == default(int) && SalesOrderDetailId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesOrderDetail]
                (
                    [CarrierTrackingNumber],
                    [ModifiedDate],
                    [OrderQty],
                    [ProductID],
                    [SalesOrderID],
                    [SpecialOfferID],
                    [UnitPrice],
                    [UnitPriceDiscount]
                )
                VALUES
                (
                    @CarrierTrackingNumber,
                    @ModifiedDate,
                    @OrderQty,
                    @ProductId,
                    @SalesOrderId,
                    @SpecialOfferId,
                    @UnitPrice,
                    @UnitPriceDiscount
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesOrderDetail] SET
                    [CarrierTrackingNumber] = @CarrierTrackingNumber,
                    [ModifiedDate] = @ModifiedDate,
                    [OrderQty] = @OrderQty,
                    [ProductID] = @ProductId,
                    [SalesOrderID] = @SalesOrderId,
                    [SpecialOfferID] = @SpecialOfferId,
                    [UnitPrice] = @UnitPrice,
                    [UnitPriceDiscount] = @UnitPriceDiscount
                WHERE
                    [SalesOrderID] = @SalesOrderId AND 
                    [SalesOrderDetailID] = @SalesOrderDetailId";
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
            SalesOrderDetail other = obj as SalesOrderDetail;
            if (other == null) return false;

            if (CarrierTrackingNumber != other.CarrierTrackingNumber)
                return false;
            if (LineTotal != other.LineTotal)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OrderQty != other.OrderQty)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalesOrderId != other.SalesOrderId)
                return false;
            if (SpecialOfferId != other.SpecialOfferId)
                return false;
            if (UnitPrice != other.UnitPrice)
                return false;
            if (UnitPriceDiscount != other.UnitPriceDiscount)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CarrierTrackingNumber == null ? 0 : CarrierTrackingNumber.GetHashCode());
                hash = hash * 23 + (LineTotal == default(decimal) ? 0 : LineTotal.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OrderQty == default(short) ? 0 : OrderQty.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalesOrderId == default(int) ? 0 : SalesOrderId.GetHashCode());
                hash = hash * 23 + (SpecialOfferId == default(int) ? 0 : SpecialOfferId.GetHashCode());
                hash = hash * 23 + (UnitPrice == default(decimal) ? 0 : UnitPrice.GetHashCode());
                hash = hash * 23 + (UnitPriceDiscount == default(decimal) ? 0 : UnitPriceDiscount.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesOrderDetail left, SalesOrderDetail right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesOrderDetail left, SalesOrderDetail right)
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
