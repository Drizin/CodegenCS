using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class CreditCard
    {
        #region Members
        [Key]
        public int CreditCardId { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public byte ExpMonth { get; set; }
        public short ExpYear { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CreditCardId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [CreditCard]
                (
                    [CardNumber],
                    [CardType],
                    [ExpMonth],
                    [ExpYear],
                    [ModifiedDate]
                )
                VALUES
                (
                    @CardNumber,
                    @CardType,
                    @ExpMonth,
                    @ExpYear,
                    @ModifiedDate
                )";

                this.CreditCardId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [CreditCard]
                    SET [CardNumber] = @CardNumber,
                    SET [CardType] = @CardType,
                    SET [ExpMonth] = @ExpMonth,
                    SET [ExpYear] = @ExpYear,
                    SET [ModifiedDate] = @ModifiedDate
                WHERE
                    [CreditCardID] = @CreditCardId";
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
            CreditCard other = obj as CreditCard;
            if (other == null) return false;

            if (CardNumber != other.CardNumber)
                return false;
            if (CardType != other.CardType)
                return false;
            if (ExpMonth != other.ExpMonth)
                return false;
            if (ExpYear != other.ExpYear)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CardNumber == null ? 0 : CardNumber.GetHashCode());
                hash = hash * 23 + (CardType == null ? 0 : CardType.GetHashCode());
                hash = hash * 23 + (ExpMonth == default(byte) ? 0 : ExpMonth.GetHashCode());
                hash = hash * 23 + (ExpYear == default(short) ? 0 : ExpYear.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(CreditCard left, CreditCard right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CreditCard left, CreditCard right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
