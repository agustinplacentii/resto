namespace Restaurant.Api.Dtos;

public record LoginRequest(string Username, string Password);

public record LoginResponse(int Id, string Username);
