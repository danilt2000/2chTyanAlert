using _2chTyanAlert.Models;

namespace _2chTyanAlert.Mapper
{
    public static class SocPostMapper
    {
        public static List<SocPost> MapWithTopTyan(List<SocPost> posts)
        {
            return posts.Select(post => post with { IsTopTyan = post.Score.HasValue && post.Score.Value >= 80 }).ToList();
        }
    }
}
