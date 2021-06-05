using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Services
{
    public class DefaultMovieImageDownloadService : IMovieImageDownloadService
    {
        private readonly ILogger<DefaultMovieImageDownloadService> _logger;
        private readonly MovieDataServiceOptions _options;
        private readonly MovieMetaEngineOptions _defaultMovieMetaEngineOptions;
        private readonly VideoDbContext _context;
        private readonly IMovieMetaService _metaService;

        public DefaultMovieImageDownloadService(
            VideoDbContext context,
            MovieDataServiceOptions serviceOptions,
            IOptions<MovieMetaEngineOptions> defaultMovieMetaEngineAccessor,
            IMovieMetaService metaService,
        ILogger<DefaultMovieImageDownloadService> logger)
        {
            _logger = logger;            
            _options = serviceOptions;
            _context = context;
            _defaultMovieMetaEngineOptions = defaultMovieMetaEngineAccessor.Value;
            _metaService = metaService;
            _logger.LogDebug("New instance created.");
        }

        /// <summary>
        /// Downloads an Image from the given Url to the given Filename
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Filename"></param>
        /// <returns>Tuple<bool, string>, boolean indicates wether download was successfull, string provides further information.</bool></returns>
        internal async Task<Tuple<bool, string>> DownloadImageAsync(string Url, string Filename)
        {
            var response = new Tuple<bool, string>(false, $"Download from {Url} to {Filename} not started.");
            _logger.LogDebug($"Try download from {Url} to {Filename}.");

            WebClient webClient = new WebClient();

            var webResponse = webClient.OpenRead(Url);
            var contentType = webClient.ResponseHeaders["Content-Type"];
            webResponse.Close();

            if (contentType.StartsWith("image/"))
            {
                _logger.LogDebug($"Download from {Url} to {Filename} started.");
                await webClient.DownloadFileTaskAsync(Url, Filename);
                _logger.LogDebug($"Download from {Url} to {Filename} finished.");

                if (File.Exists(Filename))
                {
                    _logger.LogDebug($"Download from {Url} to {Filename} sucessfull.");
                    response = new Tuple<bool, string>(true, $"Download from {Url} to {Filename} sucessfull.");
                }
                else
                {
                    _logger.LogDebug($"Download from {Url} to {Filename} failed.");
                    response = new Tuple<bool, string>(false, $"Download from {Url} to {Filename} failed.");
                }
            }
            else
            {
                _logger.LogDebug($"Download from {Url} to {Filename} failed because of a non image content type.");
                response = new Tuple<bool, string>(false, $"Download from {Url} to {Filename} failed because of a non image content type.");
            }

            return response;
        }

        /// <summary>
        /// Downloads cover image for the given movieEntity,
        /// </summary>
        /// <param name="movieEntity"></param>
        /// <returns></returns>
        public bool DownloadCoverImageAsync(videodb_videodata movieEntity)
        {
            var result = false;
            var isImgUrlLocal = movieEntity.imgurl == _options.HttpCoverImagePath + movieEntity.id.ToString() + ".jpg";

            if (string.IsNullOrWhiteSpace(movieEntity.imgurl))
            {
                _logger.LogTrace($"Field ImgUrl is empty for record with id {movieEntity.id}. Skip record.");
            }
            else
            {
                if (isImgUrlLocal)
                {
                    _logger.LogTrace($"imgUrl is local. Skip {movieEntity.id}.");
                }
                else
                {
                    var success = DownloadCoverImageLocal(
                        movieEntity.id.ToString(),
                        movieEntity.imgurl.ToString(),
                        _options.LocalCoverImagePath,
                        _options.HttpCoverImagePath);

                    if (success.Item1)
                    {
                        movieEntity.custom3 = movieEntity.imgurl;
                        movieEntity.imgurl = success.Item2;
                        _context.VideoData.Update(movieEntity);
                        _context.SaveChanges();
                    }
                }
            }

            return result;
        }

        internal Tuple<bool, string> DownloadCoverImageLocal(string id, string imgurl, string localPath, string HTTPCoverPath)
        {
            // Den neuen Pfad zusammensetzen            
            var localImgPath = System.IO.Path.Combine(localPath, id + ".jpg");
            string localHTTPPath = HTTPCoverPath + id + ".jpg";
            bool ExistsLocalImg = (System.IO.File.Exists(localImgPath));
            string remoteUri = imgurl;
            string localFileName = localImgPath;

            if (ExistsLocalImg)
            {
                // Das lokale Image existiert schon
                // --> Umbenennen
                //renamedFileName = libjfunx.operating.FileOperation.GetCountedUpExtension(localFileName);
                //System.IO.File.Move(localFileName, renamedFileName);
                _logger.LogInformation($"Skip existing local file: {localFileName}.");
            }

            // Jetzt wird die Datei auch erstmalig oder neu heruntergeladen
            var success = DownloadImageAsync(remoteUri, localFileName).Result;

            if (success.Item1)
            {
                success = new Tuple<bool, string>(true, localHTTPPath);
            }

            return success;
        }

        /// <summary>
        /// Downloads background image for the given movieEntity
        /// </summary>
        /// <param name="movieEntity"></param>
        /// <returns></returns>
        public async Task DownloadBackgroundImageAsync(videodb_videodata movieEntity, int sleepTime = 0)
        {
            _logger.LogTrace("Start img scan for {0} with {1}", movieEntity.title, _metaService.GetType().ToString());
            var localImgPath = System.IO.Path.Combine(_options.LocalBackgroundImagePath, $"{movieEntity.id}.jpg");
            var localImgOrphanedPath = System.IO.Path.Combine(_options.LocalBackgroundImagePath, $"{movieEntity.id}.orphaned");
            if (!File.Exists(localImgPath) && !System.IO.File.Exists(localImgOrphanedPath))
            {
                _logger.LogTrace($"Try to download image for {movieEntity.title} with id {movieEntity.id}.");
                var title = movieEntity.title;

                _metaService.ChangeEngineType(_defaultMovieMetaEngineOptions.BackgroundImageEngine);
                var metaData = await _metaService.SearchMovieByTitleAsync(title, new PagingOptions { Limit = 1, Offset = 0 }, new CancellationToken());
                if (metaData.TotalSize == 0)
                {
                    _logger.LogInformation("No metadata found for {0}", movieEntity.title);
                    CreateOrphanFile(localImgOrphanedPath);
                    return;
                }

                try
                {
                    var imgUrl = metaData.Items.FirstOrDefault().BackgroundImgUrl;
                    var uri = new Uri(imgUrl);
                    var webClient = new System.Net.WebClient();

                    webClient.DownloadFile(uri, localImgPath);
                    // To prevent our request beeing trapped as "attack" we sleep some ms
                    _logger.LogTrace("Sleep for {0} ms.", sleepTime);
                    Thread.Sleep(sleepTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    CreateOrphanFile(localImgOrphanedPath);
                }
            }
            else _logger.LogTrace($"Image for {movieEntity.title} with id {movieEntity.id} already exists.");

        }

        private void CreateOrphanFile(string filePath)
        {
            System.IO.File.Create(filePath);
        }
    }
}

