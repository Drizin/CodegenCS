using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("PurchaseOrderDetail", Schema = "Purchasing")]
    public partial class PurchaseOrderDetail : INotifyPropertyChanged
    {
        #region Members
        private int _purchaseOrderId;
        [Key]
        public int PurchaseOrderId 
        { 
            get { return _purchaseOrderId; } 
            set { SetField(ref _purchaseOrderId, value, nameof(PurchaseOrderId)); } 
        }
        private int _purchaseOrderDetailId;
        [Key]
        public int PurchaseOrderDetailId 
        { 
            get { return _purchaseOrderDetailId; } 
            set { SetField(ref _purchaseOrderDetailId, value, nameof(PurchaseOrderDetailId)); } 
        }
        private DateTime _dueDate;
        public DateTime DueDate 
        { 
            get { return _dueDate; } 
            set { SetField(ref _dueDate, value, nameof(DueDate)); } 
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
        private decimal _receivedQty;
        public decimal ReceivedQty 
        { 
            get { return _receivedQty; } 
            set { SetField(ref _receivedQty, value, nameof(ReceivedQty)); } 
        }
        private decimal _rejectedQty;
        public decimal RejectedQty 
        { 
            get { return _rejectedQty; } 
            set { SetField(ref _rejectedQty, value, nameof(RejectedQty)); } 
        }
        private decimal _stockedQty;
        public decimal StockedQty 
        { 
            get { return _stockedQty; } 
            set { SetField(ref _stockedQty, value, nameof(StockedQty)); } 
        }
        private decimal _unitPrice;
        public decimal UnitPrice 
        { 
            get { return _unitPrice; } 
            set { SetField(ref _unitPrice, value, nameof(UnitPrice)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (PurchaseOrderId == default(int) && PurchaseOrderDetailId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Purchasing].[PurchaseOrderDetail]
                (
                    [DueDate],
                    [ModifiedDate],
                    [OrderQty],
                    [ProductID],
                    [PurchaseOrderID],
                    [ReceivedQty],
                    [RejectedQty],
                    [UnitPrice]
                )
                VALUES
                (
                    @DueDate,
                    @ModifiedDate,
                    @OrderQty,
                    @ProductId,
                    @PurchaseOrderId,
                    @ReceivedQty,
                    @RejectedQty,
                    @UnitPrice
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Purchasing].[PurchaseOrderDetail] SET
                    [DueDate] = @DueDate,
                    [ModifiedDate] = @ModifiedDate,
                    [OrderQty] = @OrderQty,
                    [ProductID] = @ProductId,
                    [PurchaseOrderID] = @PurchaseOrderId,
                    [ReceivedQty] = @ReceivedQty,
                    [RejectedQty] = @RejectedQty,
                    [UnitPrice] = @UnitPrice
                WHERE
                    [PurchaseOrderID] = @PurchaseOrderId AND 
                    [PurchaseOrderDetailID] = @PurchaseOrderDetailId";
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
            PurchaseOrderDetail other = obj as PurchaseOrderDetail;
            if (other == null) return false;

            if (DueDate != other.DueDate)
                return false;
            if (LineTotal != other.LineTotal)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OrderQty != other.OrderQty)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (PurchaseOrderId != other.PurchaseOrderId)
                return false;
            if (ReceivedQty != other.ReceivedQty)
                return false;
            if (RejectedQty != other.RejectedQty)
                return false;
            if (StockedQty != other.StockedQty)
                return false;
            if (UnitPrice != other.UnitPrice)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (DueDate == default(DateTime) ? 0 : DueDate.GetHashCode());
                hash = hash * 23 + (LineTotal == default(decimal) ? 0 : LineTotal.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OrderQty == default(short) ? 0 : OrderQty.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (PurchaseOrderId == default(int) ? 0 : PurchaseOrderId.GetHashCode());
                hash = hash * 23 + (ReceivedQty == default(decimal) ? 0 : ReceivedQty.GetHashCode());
                hash = hash * 23 + (RejectedQty == default(decimal) ? 0 : RejectedQty.GetHashCode());
                hash = hash * 23 + (StockedQty == default(decimal) ? 0 : StockedQty.GetHashCode());
                hash = hash * 23 + (UnitPrice == default(decimal) ? 0 : UnitPrice.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(PurchaseOrderDetail left, PurchaseOrderDetail right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PurchaseOrderDetail left, PurchaseOrderDetail right)
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
