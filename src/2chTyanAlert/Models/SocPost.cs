namespace _2chTyanAlert.Models;

public record SocPost(
    int Num,
    string Comment,
    long Timestamp,
    List<string>? imageUrls,
    int? Score
);