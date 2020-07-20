using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class DatabaseLog
    {
        #region Members
        [Key]
        public int DatabaseLogId { get; set; }
        public string DatabaseUser { get; set; }
        public string Event { get; set; }
        public string Object { get; set; }
        public DateTime PostTime { get; set; }
        public string Schema { get; set; }
        public string Tsql { get; set; }
        public string XmlEvent { get; set; }
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
                UPDATE [DatabaseLog]
                    SET [DatabaseUser] = @DatabaseUser,
                    SET [Event] = @Event,
                    SET [Object] = @Object,
                    SET [PostTime] = @PostTime,
                    SET [Schema] = @Schema,
                    SET [TSQL] = @Tsql,
                    SET [XmlEvent] = @XmlEvent
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
    }
}
