using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("TransactionHistoryArchive", Schema = "Production")]
    public partial class TransactionHistoryArchive : INotifyPropertyChanged
    {
        #region Members
        private int _transactionId;
        [Key]
        public int TransactionId 
        { 
            get { return _transactionId; } 
            set { SetField(ref _transactionId, value, nameof(TransactionId)); } 
        }
        private decimal _actualCost;
        public decimal ActualCost 
        { 
            get { return _actualCost; } 
            set { SetField(ref _actualCost, value, nameof(ActualCost)); } 
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
        private int _quantity;
        public int Quantity 
        { 
            get { return _quantity; } 
            set { SetField(ref _quantity, value, nameof(Quantity)); } 
        }
        private int _referenceOrderId;
        public int ReferenceOrderId 
        { 
            get { return _referenceOrderId; } 
            set { SetField(ref _referenceOrderId, value, nameof(ReferenceOrderId)); } 
        }
        private int _referenceOrderLineId;
        public int ReferenceOrderLineId 
        { 
            get { return _referenceOrderLineId; } 
            set { SetField(ref _referenceOrderLineId, value, nameof(ReferenceOrderLineId)); } 
        }
        private DateTime _transactionDate;
        public DateTime TransactionDate 
        { 
            get { return _transactionDate; } 
            set { SetField(ref _transactionDate, value, nameof(TransactionDate)); } 
        }
        private string _transactionType;
        public string TransactionType 
        { 
            get { return _transactionType; } 
            set { SetField(ref _transactionType, value, nameof(TransactionType)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (TransactionId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[TransactionHistoryArchive]
                (
                    [ActualCost],
                    [ModifiedDate],
                    [ProductID],
                    [Quantity],
                    [ReferenceOrderID],
                    [ReferenceOrderLineID],
                    [TransactionDate],
                    [TransactionID],
                    [TransactionType]
                )
                VALUES
                (
                    @ActualCost,
                    @ModifiedDate,
                    @ProductId,
                    @Quantity,
                    @ReferenceOrderId,
                    @ReferenceOrderLineId,
                    @TransactionDate,
                    @TransactionId,
                    @TransactionType
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[TransactionHistoryArchive] SET
                    [ActualCost] = @ActualCost,
                    [ModifiedDate] = @ModifiedDate,
                    [ProductID] = @ProductId,
                    [Quantity] = @Quantity,
                    [ReferenceOrderID] = @ReferenceOrderId,
                    [ReferenceOrderLineID] = @ReferenceOrderLineId,
                    [TransactionDate] = @TransactionDate,
                    [TransactionID] = @TransactionId,
                    [TransactionType] = @TransactionType
                WHERE
                    [TransactionID] = @TransactionId";
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
            TransactionHistoryArchive other = obj as TransactionHistoryArchive;
            if (other == null) return false;

            if (ActualCost != other.ActualCost)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (Quantity != other.Quantity)
                return false;
            if (ReferenceOrderId != other.ReferenceOrderId)
                return false;
            if (ReferenceOrderLineId != other.ReferenceOrderLineId)
                return false;
            if (TransactionDate != other.TransactionDate)
                return false;
            if (TransactionId != other.TransactionId)
                return false;
            if (TransactionType != other.TransactionType)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ActualCost == default(decimal) ? 0 : ActualCost.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (Quantity == default(int) ? 0 : Quantity.GetHashCode());
                hash = hash * 23 + (ReferenceOrderId == default(int) ? 0 : ReferenceOrderId.GetHashCode());
                hash = hash * 23 + (ReferenceOrderLineId == default(int) ? 0 : ReferenceOrderLineId.GetHashCode());
                hash = hash * 23 + (TransactionDate == default(DateTime) ? 0 : TransactionDate.GetHashCode());
                hash = hash * 23 + (TransactionId == default(int) ? 0 : TransactionId.GetHashCode());
                hash = hash * 23 + (TransactionType == null ? 0 : TransactionType.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(TransactionHistoryArchive left, TransactionHistoryArchive right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TransactionHistoryArchive left, TransactionHistoryArchive right)
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
