namespace Jaxx.WebApi.Shared.Infrastructure
{
    public interface IRewriteConfiguration
    {
        string Protcol { get; set; }
        string RewriteUrl { get; set; }
    }
}