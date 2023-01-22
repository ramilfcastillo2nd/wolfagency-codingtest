using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Entities
{
    [Table("UserRefreshTokens")]
    public class AppUserToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public AppUser User { get; set; }
        public string RefreshToken { get; set; }
        public string TimeZone { get; set; }
        public string TimeZoneId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ExpiredOn { get; set; }
        public bool Taken { get; set; }
        public bool Current { get; set; }
    }
}
