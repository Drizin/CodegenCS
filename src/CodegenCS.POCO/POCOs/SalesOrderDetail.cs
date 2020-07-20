using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class SalesOrderDetail
    {
        #region Members
        [Key]
        public int SalesOrderId { get; set; }
        [Key]
        public int SalesOrderDetailId { get; set; }
        public string CarrierTrackingNumber { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime ModifiedDate { get; set; }
        public short OrderQty { get; set; }
        public int ProductId { get; set; }
        public Guid Rowguid { get; set; }
        public int SpecialOfferId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
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
                INSERT INTO [SalesOrderDetail]
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
                UPDATE [SalesOrderDetail] SET
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
    }
}
