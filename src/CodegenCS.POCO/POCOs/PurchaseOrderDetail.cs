using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class PurchaseOrderDetail
    {
        #region Members
        [Key]
        public int PurchaseOrderId { get; set; }
        [Key]
        public int PurchaseOrderDetailId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime ModifiedDate { get; set; }
        public short OrderQty { get; set; }
        public int ProductId { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal RejectedQty { get; set; }
        public decimal StockedQty { get; set; }
        public decimal UnitPrice { get; set; }
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
                INSERT INTO [PurchaseOrderDetail]
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
                UPDATE [PurchaseOrderDetail]
                    SET [DueDate] = @DueDate,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [OrderQty] = @OrderQty,
                    SET [ProductID] = @ProductId,
                    SET [PurchaseOrderID] = @PurchaseOrderId,
                    SET [ReceivedQty] = @ReceivedQty,
                    SET [RejectedQty] = @RejectedQty,
                    SET [UnitPrice] = @UnitPrice
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
    }
}
