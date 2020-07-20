using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ShoppingCartItem
    {
        #region Members
        [Key]
        public int ShoppingCartItemId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ShoppingCartId { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ShoppingCartItemId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ShoppingCartItem]
                (
                    [DateCreated],
                    [ModifiedDate],
                    [ProductID],
                    [Quantity],
                    [ShoppingCartID]
                )
                VALUES
                (
                    @DateCreated,
                    @ModifiedDate,
                    @ProductId,
                    @Quantity,
                    @ShoppingCartId
                )";

                this.ShoppingCartItemId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ShoppingCartItem] SET
                    [DateCreated] = @DateCreated,
                    [ModifiedDate] = @ModifiedDate,
                    [ProductID] = @ProductId,
                    [Quantity] = @Quantity,
                    [ShoppingCartID] = @ShoppingCartId
                WHERE
                    [ShoppingCartItemID] = @ShoppingCartItemId";
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
            ShoppingCartItem other = obj as ShoppingCartItem;
            if (other == null) return false;

            if (DateCreated != other.DateCreated)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (Quantity != other.Quantity)
                return false;
            if (ShoppingCartId != other.ShoppingCartId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (DateCreated == default(DateTime) ? 0 : DateCreated.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (Quantity == default(int) ? 0 : Quantity.GetHashCode());
                hash = hash * 23 + (ShoppingCartId == null ? 0 : ShoppingCartId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ShoppingCartItem left, ShoppingCartItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ShoppingCartItem left, ShoppingCartItem right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
