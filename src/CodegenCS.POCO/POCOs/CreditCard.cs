using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("CreditCard", Schema = "Sales")]
    public partial class CreditCard : INotifyPropertyChanged
    {
        #region Members
        private int _creditCardId;
        [Key]
        public int CreditCardId 
        { 
            get { return _creditCardId; } 
            set { SetField(ref _creditCardId, value, nameof(CreditCardId)); } 
        }
        private string _cardNumber;
        public string CardNumber 
        { 
            get { return _cardNumber; } 
            set { SetField(ref _cardNumber, value, nameof(CardNumber)); } 
        }
        private string _cardType;
        public string CardType 
        { 
            get { return _cardType; } 
            set { SetField(ref _cardType, value, nameof(CardType)); } 
        }
        private byte _expMonth;
        public byte ExpMonth 
        { 
            get { return _expMonth; } 
            set { SetField(ref _expMonth, value, nameof(ExpMonth)); } 
        }
        private short _expYear;
        public short ExpYear 
        { 
            get { return _expYear; } 
            set { SetField(ref _expYear, value, nameof(ExpYear)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
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
                INSERT INTO [Sales].[CreditCard]
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
                UPDATE [Sales].[CreditCard] SET
                    [CardNumber] = @CardNumber,
                    [CardType] = @CardType,
                    [ExpMonth] = @ExpMonth,
                    [ExpYear] = @ExpYear,
                    [ModifiedDate] = @ModifiedDate
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
