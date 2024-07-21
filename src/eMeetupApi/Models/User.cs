using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace eMeetupApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string ImageUrl { get; set; }

        [NotMapped]
        [JsonIgnore]
        public IFormFile? Image { get; set; } 
    }
}
