namespace Crystal.Core.Endpoints.Account;

public class AccountInfoResponse
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Logins { get; set; } = new();
    public bool HasPassword { get; set; }
}