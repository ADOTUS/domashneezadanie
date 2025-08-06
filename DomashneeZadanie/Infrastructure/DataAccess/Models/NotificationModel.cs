using LinqToDB.Mapping;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using AssociationAttribute = LinqToDB.Mapping.AssociationAttribute;

namespace DomashneeZadanie.Infrastructure.DataAccess.Models
{
    [Table("Notification")]
    public class NotificationModel
    {
        [PrimaryKey, NotNull]
        public Guid Id { get; set; }

        [Column, NotNull]
        public Guid UserId { get; set; }

        [Column, NotNull]
        public string Type { get; set; } = null!;

        [Column, NotNull]
        public string Text { get; set; } = null!;

        [Column, NotNull]
        public DateTime ScheduledAt { get; set; }

        [Column, NotNull]
        public bool IsNotified { get; set; }

        [Column]
        public DateTime? NotifiedAt { get; set; }
    }
}
