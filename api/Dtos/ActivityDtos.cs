namespace Restaurant.Api.Dtos;

public record ActivityLogDto(int Id, string Type, string Description, DateTime CreatedAt);
