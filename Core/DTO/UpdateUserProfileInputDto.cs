namespace Core.DTO
{
    public class UpdateUserProfileInputDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LinkedInUrl { get; set; }
        public string LinkedInName { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? DateHired { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public int? SdrTypeId { get; set; }
    }
}
