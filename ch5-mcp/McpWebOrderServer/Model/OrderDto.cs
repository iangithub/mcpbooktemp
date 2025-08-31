public record OrderDto(
    int Id,
    string Customer,
    DateTime OrderDate,
    int Total,
    string Status);