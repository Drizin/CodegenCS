using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class SalesOrderHeader
    {
        #region Members
        [Key]
        public int SalesOrderId { get; set; }
        public string AccountNumber { get; set; }
        public int BillToAddressId { get; set; }
        public string Comment { get; set; }
        public string CreditCardApprovalCode { get; set; }
        public int? CreditCardId { get; set; }
        public int? CurrencyRateId { get; set; }
        public int CustomerId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Freight { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool OnlineOrderFlag { get; set; }
        public DateTime OrderDate { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public byte RevisionNumber { get; set; }
        public Guid Rowguid { get; set; }
        public string SalesOrderNumber { get; set; }
        public int? SalesPersonId { get; set; }
        public DateTime? ShipDate { get; set; }
        public int ShipMethodId { get; set; }
        public int ShipToAddressId { get; set; }
        public byte Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public int? TerritoryId { get; set; }
        public decimal TotalDue { get; set; }
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
                INSERT INTO [SalesOrderHeader]
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
                UPDATE [SalesOrderHeader]
                    SET [AccountNumber] = @AccountNumber,
                    SET [BillToAddressID] = @BillToAddressId,
                    SET [Comment] = @Comment,
                    SET [CreditCardApprovalCode] = @CreditCardApprovalCode,
                    SET [CreditCardID] = @CreditCardId,
                    SET [CurrencyRateID] = @CurrencyRateId,
                    SET [CustomerID] = @CustomerId,
                    SET [DueDate] = @DueDate,
                    SET [Freight] = @Freight,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [OnlineOrderFlag] = @OnlineOrderFlag,
                    SET [OrderDate] = @OrderDate,
                    SET [PurchaseOrderNumber] = @PurchaseOrderNumber,
                    SET [RevisionNumber] = @RevisionNumber,
                    SET [SalesPersonID] = @SalesPersonId,
                    SET [ShipDate] = @ShipDate,
                    SET [ShipMethodID] = @ShipMethodId,
                    SET [ShipToAddressID] = @ShipToAddressId,
                    SET [Status] = @Status,
                    SET [SubTotal] = @SubTotal,
                    SET [TaxAmt] = @TaxAmt,
                    SET [TerritoryID] = @TerritoryId
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
    }
}
