using System;
using System.Threading.Tasks;
using Jaxx.VideoDb.Data.DatabaseModels;

namespace Jaxx.VideoDb.WebCore.Services
{
    public interface IMovieImageDownloadService
    {
        bool DownloadCoverImageAsync(videodb_videodata movieEntity);
        Task DownloadBackgroundImageAsync(videodb_videodata movieEntity, int sleepTime = 0);
    }
}