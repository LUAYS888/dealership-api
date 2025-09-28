namespace Dealership.Api.Domain;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");  //To create a unique identifier
    public string Email { get; set; } = default!;      //Start Email as null, but don’t warn me about it. I’ll assign a real value before using it.
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = Domain.Role.Customer;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
