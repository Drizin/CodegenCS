using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class DatabaseLog : INotifyPropertyChanged
    {
        #region Members
        private int _databaseLogId;
        [Key]
        public int DatabaseLogId 
        { 
            get { return _databaseLogId; } 
            set { SetField(ref _databaseLogId, value, nameof(DatabaseLogId)); } 
        }
        private string _databaseUser;
        public string DatabaseUser 
        { 
            get { return _databaseUser; } 
            set { SetField(ref _databaseUser, value, nameof(DatabaseUser)); } 
        }
        private string _event;
        public string Event 
        { 
            get { return _event; } 
            set { SetField(ref _event, value, nameof(Event)); } 
        }
        private string _object;
        public string Object 
        { 
            get { return _object; } 
            set { SetField(ref _object, value, nameof(Object)); } 
        }
        private DateTime _postTime;
        public DateTime PostTime 
        { 
            get { return _postTime; } 
            set { SetField(ref _postTime, value, nameof(PostTime)); } 
        }
        private string _schema;
        public string Schema 
        { 
            get { return _schema; } 
            set { SetField(ref _schema, value, nameof(Schema)); } 
        }
        private string _tsql;
        public string Tsql 
        { 
            get { return _tsql; } 
            set { SetField(ref _tsql, value, nameof(Tsql)); } 
        }
        private string _xmlEvent;
        public string XmlEvent 
        { 
            get { return _xmlEvent; } 
            set { SetField(ref _xmlEvent, value, nameof(XmlEvent)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (DatabaseLogId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [DatabaseLog]
                (
                    [DatabaseUser],
                    [Event],
                    [Object],
                    [PostTime],
                    [Schema],
                    [TSQL],
                    [XmlEvent]
                )
                VALUES
                (
                    @DatabaseUser,
                    @Event,
                    @Object,
                    @PostTime,
                    @Schema,
                    @Tsql,
                    @XmlEvent
                )";

                this.DatabaseLogId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [DatabaseLog] SET
                    [DatabaseUser] = @DatabaseUser,
                    [Event] = @Event,
                    [Object] = @Object,
                    [PostTime] = @PostTime,
                    [Schema] = @Schema,
                    [TSQL] = @Tsql,
                    [XmlEvent] = @XmlEvent
                WHERE
                    [DatabaseLogID] = @DatabaseLogId";
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
            DatabaseLog other = obj as DatabaseLog;
            if (other == null) return false;

            if (DatabaseUser != other.DatabaseUser)
                return false;
            if (Event != other.Event)
                return false;
            if (Object != other.Object)
                return false;
            if (PostTime != other.PostTime)
                return false;
            if (Schema != other.Schema)
                return false;
            if (Tsql != other.Tsql)
                return false;
            if (XmlEvent != other.XmlEvent)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (DatabaseUser == null ? 0 : DatabaseUser.GetHashCode());
                hash = hash * 23 + (Event == null ? 0 : Event.GetHashCode());
                hash = hash * 23 + (Object == null ? 0 : Object.GetHashCode());
                hash = hash * 23 + (PostTime == default(DateTime) ? 0 : PostTime.GetHashCode());
                hash = hash * 23 + (Schema == null ? 0 : Schema.GetHashCode());
                hash = hash * 23 + (Tsql == null ? 0 : Tsql.GetHashCode());
                hash = hash * 23 + (XmlEvent == null ? 0 : XmlEvent.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(DatabaseLog left, DatabaseLog right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DatabaseLog left, DatabaseLog right)
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
