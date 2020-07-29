using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesOrderHeader", Schema = "Sales")]
    public partial class SalesOrderHeader : INotifyPropertyChanged
    {
        #region Members
        private int _salesOrderId;
        [Key]
        public int SalesOrderId 
        { 
            get { return _salesOrderId; } 
            set { SetField(ref _salesOrderId, value, nameof(SalesOrderId)); } 
        }
        private string _accountNumber;
        public string AccountNumber 
        { 
            get { return _accountNumber; } 
            set { SetField(ref _accountNumber, value, nameof(AccountNumber)); } 
        }
        private int _billToAddressId;
        public int BillToAddressId 
        { 
            get { return _billToAddressId; } 
            set { SetField(ref _billToAddressId, value, nameof(BillToAddressId)); } 
        }
        private string _comment;
        public string Comment 
        { 
            get { return _comment; } 
            set { SetField(ref _comment, value, nameof(Comment)); } 
        }
        private string _creditCardApprovalCode;
        public string CreditCardApprovalCode 
        { 
            get { return _creditCardApprovalCode; } 
            set { SetField(ref _creditCardApprovalCode, value, nameof(CreditCardApprovalCode)); } 
        }
        private int? _creditCardId;
        public int? CreditCardId 
        { 
            get { return _creditCardId; } 
            set { SetField(ref _creditCardId, value, nameof(CreditCardId)); } 
        }
        private int? _currencyRateId;
        public int? CurrencyRateId 
        { 
            get { return _currencyRateId; } 
            set { SetField(ref _currencyRateId, value, nameof(CurrencyRateId)); } 
        }
        private int _customerId;
        public int CustomerId 
        { 
            get { return _customerId; } 
            set { SetField(ref _customerId, value, nameof(CustomerId)); } 
        }
        private DateTime _dueDate;
        public DateTime DueDate 
        { 
            get { return _dueDate; } 
            set { SetField(ref _dueDate, value, nameof(DueDate)); } 
        }
        private decimal _freight;
        public decimal Freight 
        { 
            get { return _freight; } 
            set { SetField(ref _freight, value, nameof(Freight)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private bool _onlineOrderFlag;
        public bool OnlineOrderFlag 
        { 
            get { return _onlineOrderFlag; } 
            set { SetField(ref _onlineOrderFlag, value, nameof(OnlineOrderFlag)); } 
        }
        private DateTime _orderDate;
        public DateTime OrderDate 
        { 
            get { return _orderDate; } 
            set { SetField(ref _orderDate, value, nameof(OrderDate)); } 
        }
        private string _purchaseOrderNumber;
        public string PurchaseOrderNumber 
        { 
            get { return _purchaseOrderNumber; } 
            set { SetField(ref _purchaseOrderNumber, value, nameof(PurchaseOrderNumber)); } 
        }
        private byte _revisionNumber;
        public byte RevisionNumber 
        { 
            get { return _revisionNumber; } 
            set { SetField(ref _revisionNumber, value, nameof(RevisionNumber)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private string _salesOrderNumber;
        public string SalesOrderNumber 
        { 
            get { return _salesOrderNumber; } 
            set { SetField(ref _salesOrderNumber, value, nameof(SalesOrderNumber)); } 
        }
        private int? _salesPersonId;
        public int? SalesPersonId 
        { 
            get { return _salesPersonId; } 
            set { SetField(ref _salesPersonId, value, nameof(SalesPersonId)); } 
        }
        private DateTime? _shipDate;
        public DateTime? ShipDate 
        { 
            get { return _shipDate; } 
            set { SetField(ref _shipDate, value, nameof(ShipDate)); } 
        }
        private int _shipMethodId;
        public int ShipMethodId 
        { 
            get { return _shipMethodId; } 
            set { SetField(ref _shipMethodId, value, nameof(ShipMethodId)); } 
        }
        private int _shipToAddressId;
        public int ShipToAddressId 
        { 
            get { return _shipToAddressId; } 
            set { SetField(ref _shipToAddressId, value, nameof(ShipToAddressId)); } 
        }
        private byte _status;
        public byte Status 
        { 
            get { return _status; } 
            set { SetField(ref _status, value, nameof(Status)); } 
        }
        private decimal _subTotal;
        public decimal SubTotal 
        { 
            get { return _subTotal; } 
            set { SetField(ref _subTotal, value, nameof(SubTotal)); } 
        }
        private decimal _taxAmt;
        public decimal TaxAmt 
        { 
            get { return _taxAmt; } 
            set { SetField(ref _taxAmt, value, nameof(TaxAmt)); } 
        }
        private int? _territoryId;
        public int? TerritoryId 
        { 
            get { return _territoryId; } 
            set { SetField(ref _territoryId, value, nameof(TerritoryId)); } 
        }
        private decimal _totalDue;
        public decimal TotalDue 
        { 
            get { return _totalDue; } 
            set { SetField(ref _totalDue, value, nameof(TotalDue)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SalesOrderId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesOrderHeader]
                (
                    [AccountNumber],
                    [BillToAddressID],
                    [Comment],
                    [CreditCardApprovalCode],
                    [CreditCardID],
                    [CurrencyRateID],
                    [CustomerID],
                    [DueDate],
                    [Freight],
                    [ModifiedDate],
                    [OnlineOrderFlag],
                    [OrderDate],
                    [PurchaseOrderNumber],
                    [RevisionNumber],
                    [SalesPersonID],
                    [ShipDate],
                    [ShipMethodID],
                    [ShipToAddressID],
                    [Status],
                    [SubTotal],
                    [TaxAmt],
                    [TerritoryID]
                )
                VALUES
                (
                    @AccountNumber,
                    @BillToAddressId,
                    @Comment,
                    @CreditCardApprovalCode,
                    @CreditCardId,
                    @CurrencyRateId,
                    @CustomerId,
                    @DueDate,
                    @Freight,
                    @ModifiedDate,
                    @OnlineOrderFlag,
                    @OrderDate,
                    @PurchaseOrderNumber,
                    @RevisionNumber,
                    @SalesPersonId,
                    @ShipDate,
                    @ShipMethodId,
                    @ShipToAddressId,
                    @Status,
                    @SubTotal,
                    @TaxAmt,
                    @TerritoryId
                )";

                this.SalesOrderId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesOrderHeader] SET
                    [AccountNumber] = @AccountNumber,
                    [BillToAddressID] = @BillToAddressId,
                    [Comment] = @Comment,
                    [CreditCardApprovalCode] = @CreditCardApprovalCode,
                    [CreditCardID] = @CreditCardId,
                    [CurrencyRateID] = @CurrencyRateId,
                    [CustomerID] = @CustomerId,
                    [DueDate] = @DueDate,
                    [Freight] = @Freight,
                    [ModifiedDate] = @ModifiedDate,
                    [OnlineOrderFlag] = @OnlineOrderFlag,
                    [OrderDate] = @OrderDate,
                    [PurchaseOrderNumber] = @PurchaseOrderNumber,
                    [RevisionNumber] = @RevisionNumber,
                    [SalesPersonID] = @SalesPersonId,
                    [ShipDate] = @ShipDate,
                    [ShipMethodID] = @ShipMethodId,
                    [ShipToAddressID] = @ShipToAddressId,
                    [Status] = @Status,
                    [SubTotal] = @SubTotal,
                    [TaxAmt] = @TaxAmt,
                    [TerritoryID] = @TerritoryId
                WHERE
                    [SalesOrderID] = @SalesOrderId";
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
            SalesOrderHeader other = obj as SalesOrderHeader;
            if (other == null) return false;

            if (AccountNumber != other.AccountNumber)
                return false;
            if (BillToAddressId != other.BillToAddressId)
                return false;
            if (Comment != other.Comment)
                return false;
            if (CreditCardApprovalCode != other.CreditCardApprovalCode)
                return false;
            if (CreditCardId != other.CreditCardId)
                return false;
            if (CurrencyRateId != other.CurrencyRateId)
                return false;
            if (CustomerId != other.CustomerId)
                return false;
            if (DueDate != other.DueDate)
                return false;
            if (Freight != other.Freight)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OnlineOrderFlag != other.OnlineOrderFlag)
                return false;
            if (OrderDate != other.OrderDate)
                return false;
            if (PurchaseOrderNumber != other.PurchaseOrderNumber)
                return false;
            if (RevisionNumber != other.RevisionNumber)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalesOrderNumber != other.SalesOrderNumber)
                return false;
            if (SalesPersonId != other.SalesPersonId)
                return false;
            if (ShipDate != other.ShipDate)
                return false;
            if (ShipMethodId != other.ShipMethodId)
                return false;
            if (ShipToAddressId != other.ShipToAddressId)
                return false;
            if (Status != other.Status)
                return false;
            if (SubTotal != other.SubTotal)
                return false;
            if (TaxAmt != other.TaxAmt)
                return false;
            if (TerritoryId != other.TerritoryId)
                return false;
            if (TotalDue != other.TotalDue)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AccountNumber == null ? 0 : AccountNumber.GetHashCode());
                hash = hash * 23 + (BillToAddressId == default(int) ? 0 : BillToAddressId.GetHashCode());
                hash = hash * 23 + (Comment == null ? 0 : Comment.GetHashCode());
                hash = hash * 23 + (CreditCardApprovalCode == null ? 0 : CreditCardApprovalCode.GetHashCode());
                hash = hash * 23 + (CreditCardId == null ? 0 : CreditCardId.GetHashCode());
                hash = hash * 23 + (CurrencyRateId == null ? 0 : CurrencyRateId.GetHashCode());
                hash = hash * 23 + (CustomerId == default(int) ? 0 : CustomerId.GetHashCode());
                hash = hash * 23 + (DueDate == default(DateTime) ? 0 : DueDate.GetHashCode());
                hash = hash * 23 + (Freight == default(decimal) ? 0 : Freight.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OnlineOrderFlag == default(bool) ? 0 : OnlineOrderFlag.GetHashCode());
                hash = hash * 23 + (OrderDate == default(DateTime) ? 0 : OrderDate.GetHashCode());
                hash = hash * 23 + (PurchaseOrderNumber == null ? 0 : PurchaseOrderNumber.GetHashCode());
                hash = hash * 23 + (RevisionNumber == default(byte) ? 0 : RevisionNumber.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalesOrderNumber == null ? 0 : SalesOrderNumber.GetHashCode());
                hash = hash * 23 + (SalesPersonId == null ? 0 : SalesPersonId.GetHashCode());
                hash = hash * 23 + (ShipDate == null ? 0 : ShipDate.GetHashCode());
                hash = hash * 23 + (ShipMethodId == default(int) ? 0 : ShipMethodId.GetHashCode());
                hash = hash * 23 + (ShipToAddressId == default(int) ? 0 : ShipToAddressId.GetHashCode());
                hash = hash * 23 + (Status == default(byte) ? 0 : Status.GetHashCode());
                hash = hash * 23 + (SubTotal == default(decimal) ? 0 : SubTotal.GetHashCode());
                hash = hash * 23 + (TaxAmt == default(decimal) ? 0 : TaxAmt.GetHashCode());
                hash = hash * 23 + (TerritoryId == null ? 0 : TerritoryId.GetHashCode());
                hash = hash * 23 + (TotalDue == default(decimal) ? 0 : TotalDue.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesOrderHeader left, SalesOrderHeader right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesOrderHeader left, SalesOrderHeader right)
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
