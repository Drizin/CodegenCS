using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SpecialOffer", Schema = "Sales")]
    public partial class SpecialOffer : INotifyPropertyChanged
    {
        #region Members
        private int _specialOfferId;
        [Key]
        public int SpecialOfferId 
        { 
            get { return _specialOfferId; } 
            set { SetField(ref _specialOfferId, value, nameof(SpecialOfferId)); } 
        }
        private string _category;
        public string Category 
        { 
            get { return _category; } 
            set { SetField(ref _category, value, nameof(Category)); } 
        }
        private string _description;
        public string Description 
        { 
            get { return _description; } 
            set { SetField(ref _description, value, nameof(Description)); } 
        }
        private decimal _discountPct;
        public decimal DiscountPct 
        { 
            get { return _discountPct; } 
            set { SetField(ref _discountPct, value, nameof(DiscountPct)); } 
        }
        private DateTime _endDate;
        public DateTime EndDate 
        { 
            get { return _endDate; } 
            set { SetField(ref _endDate, value, nameof(EndDate)); } 
        }
        private int? _maxQty;
        public int? MaxQty 
        { 
            get { return _maxQty; } 
            set { SetField(ref _maxQty, value, nameof(MaxQty)); } 
        }
        private int _minQty;
        public int MinQty 
        { 
            get { return _minQty; } 
            set { SetField(ref _minQty, value, nameof(MinQty)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private DateTime _startDate;
        public DateTime StartDate 
        { 
            get { return _startDate; } 
            set { SetField(ref _startDate, value, nameof(StartDate)); } 
        }
        private string _type;
        public string Type 
        { 
            get { return _type; } 
            set { SetField(ref _type, value, nameof(Type)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SpecialOfferId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SpecialOffer]
                (
                    [Category],
                    [Description],
                    [DiscountPct],
                    [EndDate],
                    [MaxQty],
                    [MinQty],
                    [ModifiedDate],
                    [StartDate],
                    [Type]
                )
                VALUES
                (
                    @Category,
                    @Description,
                    @DiscountPct,
                    @EndDate,
                    @MaxQty,
                    @MinQty,
                    @ModifiedDate,
                    @StartDate,
                    @Type
                )";

                this.SpecialOfferId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SpecialOffer] SET
                    [Category] = @Category,
                    [Description] = @Description,
                    [DiscountPct] = @DiscountPct,
                    [EndDate] = @EndDate,
                    [MaxQty] = @MaxQty,
                    [MinQty] = @MinQty,
                    [ModifiedDate] = @ModifiedDate,
                    [StartDate] = @StartDate,
                    [Type] = @Type
                WHERE
                    [SpecialOfferID] = @SpecialOfferId";
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
            SpecialOffer other = obj as SpecialOffer;
            if (other == null) return false;

            if (Category != other.Category)
                return false;
            if (Description != other.Description)
                return false;
            if (DiscountPct != other.DiscountPct)
                return false;
            if (EndDate != other.EndDate)
                return false;
            if (MaxQty != other.MaxQty)
                return false;
            if (MinQty != other.MinQty)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StartDate != other.StartDate)
                return false;
            if (Type != other.Type)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Category == null ? 0 : Category.GetHashCode());
                hash = hash * 23 + (Description == null ? 0 : Description.GetHashCode());
                hash = hash * 23 + (DiscountPct == default(decimal) ? 0 : DiscountPct.GetHashCode());
                hash = hash * 23 + (EndDate == default(DateTime) ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (MaxQty == null ? 0 : MaxQty.GetHashCode());
                hash = hash * 23 + (MinQty == default(int) ? 0 : MinQty.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                hash = hash * 23 + (Type == null ? 0 : Type.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SpecialOffer left, SpecialOffer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SpecialOffer left, SpecialOffer right)
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
