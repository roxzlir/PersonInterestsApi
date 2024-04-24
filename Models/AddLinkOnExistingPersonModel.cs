namespace PersonInterestsApi.Models
{
    public class AddLinkOnExistingPersonModel
    {
        public int PersonId { get; set; }
        public int InterestId { get; set; }
        public string LinkAddress { get; set; }
    }
}
