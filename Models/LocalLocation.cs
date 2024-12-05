public class LocalLocation
{
    public int Id { get; set; }
    public required string LocationNickname { get; set; }
    public string CreatorId { get; set; } = "";
    public string Description { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedOn { get; set; }
}
