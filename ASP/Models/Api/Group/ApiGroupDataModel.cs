using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.Api.Group
{
    public class ApiGroupDataModel
    {
        public String Name { get; set; } = null!;
        public String ImageUrl { get; set; } = null!;
        public String Description { get; set; } = null!;
        public String Slug { get; set; } = null!;
        public String? ParentId { get; set; }
    }
}
