﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("PurchaseOrderHeader", Schema = "Purchasing")]
    public partial class PurchaseOrderHeader : INotifyPropertyChanged
    {
        #region Members
        private int _purchaseOrderId;
        [Key]
        public int PurchaseOrderId 
        { 
            get { return _purchaseOrderId; } 
            set { SetField(ref _purchaseOrderId, value, nameof(PurchaseOrderId)); } 
        }
        private int _employeeId;
        public int EmployeeId 
        { 
            get { return _employeeId; } 
            set { SetField(ref _employeeId, value, nameof(EmployeeId)); } 
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
        private DateTime _orderDate;
        public DateTime OrderDate 
        { 
            get { return _orderDate; } 
            set { SetField(ref _orderDate, value, nameof(OrderDate)); } 
        }
        private byte _revisionNumber;
        public byte RevisionNumber 
        { 
            get { return _revisionNumber; } 
            set { SetField(ref _revisionNumber, value, nameof(RevisionNumber)); } 
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
        private decimal _totalDue;
        public decimal TotalDue 
        { 
            get { return _totalDue; } 
            set { SetField(ref _totalDue, value, nameof(TotalDue)); } 
        }
        private int _vendorId;
        public int VendorId 
        { 
            get { return _vendorId; } 
            set { SetField(ref _vendorId, value, nameof(VendorId)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (PurchaseOrderId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Purchasing].[PurchaseOrderHeader]
                (
                    [EmployeeID],
                    [Freight],
                    [ModifiedDate],
                    [OrderDate],
                    [RevisionNumber],
                    [ShipDate],
                    [ShipMethodID],
                    [Status],
                    [SubTotal],
                    [TaxAmt],
                    [VendorID]
                )
                VALUES
                (
                    @EmployeeId,
                    @Freight,
                    @ModifiedDate,
                    @OrderDate,
                    @RevisionNumber,
                    @ShipDate,
                    @ShipMethodId,
                    @Status,
                    @SubTotal,
                    @TaxAmt,
                    @VendorId
                )";

                this.PurchaseOrderId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Purchasing].[PurchaseOrderHeader] SET
                    [EmployeeID] = @EmployeeId,
                    [Freight] = @Freight,
                    [ModifiedDate] = @ModifiedDate,
                    [OrderDate] = @OrderDate,
                    [RevisionNumber] = @RevisionNumber,
                    [ShipDate] = @ShipDate,
                    [ShipMethodID] = @ShipMethodId,
                    [Status] = @Status,
                    [SubTotal] = @SubTotal,
                    [TaxAmt] = @TaxAmt,
                    [VendorID] = @VendorId
                WHERE
                    [PurchaseOrderID] = @PurchaseOrderId";
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
            PurchaseOrderHeader other = obj as PurchaseOrderHeader;
            if (other == null) return false;

            if (EmployeeId != other.EmployeeId)
                return false;
            if (Freight != other.Freight)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OrderDate != other.OrderDate)
                return false;
            if (RevisionNumber != other.RevisionNumber)
                return false;
            if (ShipDate != other.ShipDate)
                return false;
            if (ShipMethodId != other.ShipMethodId)
                return false;
            if (Status != other.Status)
                return false;
            if (SubTotal != other.SubTotal)
                return false;
            if (TaxAmt != other.TaxAmt)
                return false;
            if (TotalDue != other.TotalDue)
                return false;
            if (VendorId != other.VendorId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (EmployeeId == default(int) ? 0 : EmployeeId.GetHashCode());
                hash = hash * 23 + (Freight == default(decimal) ? 0 : Freight.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OrderDate == default(DateTime) ? 0 : OrderDate.GetHashCode());
                hash = hash * 23 + (RevisionNumber == default(byte) ? 0 : RevisionNumber.GetHashCode());
                hash = hash * 23 + (ShipDate == null ? 0 : ShipDate.GetHashCode());
                hash = hash * 23 + (ShipMethodId == default(int) ? 0 : ShipMethodId.GetHashCode());
                hash = hash * 23 + (Status == default(byte) ? 0 : Status.GetHashCode());
                hash = hash * 23 + (SubTotal == default(decimal) ? 0 : SubTotal.GetHashCode());
                hash = hash * 23 + (TaxAmt == default(decimal) ? 0 : TaxAmt.GetHashCode());
                hash = hash * 23 + (TotalDue == default(decimal) ? 0 : TotalDue.GetHashCode());
                hash = hash * 23 + (VendorId == default(int) ? 0 : VendorId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(PurchaseOrderHeader left, PurchaseOrderHeader right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PurchaseOrderHeader left, PurchaseOrderHeader right)
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
