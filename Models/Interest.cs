using System.ComponentModel.DataAnnotations;

namespace PersonInterestsApi.Models
{
    public class Interest
    {
        [Key]
        public int InterestId { get; set; }
        [StringLength(30)]
        public string InterestTitle { get; set;}
        [StringLength(200)]
        public string InterestDescription { get; set;}
    }
}
