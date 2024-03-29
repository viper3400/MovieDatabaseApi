﻿using AutoMapper;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test
{
    //[Collection("AutoMapperCollection")]
    public class DefaultMovieDataServiceShould
    {
        private readonly IMovieDataService _movieDataService;
        private DefaultMovieDataService _movieDataServiceByPassInterface;
        private readonly string _userName;
        private readonly string _viewGroup;
        private readonly MovieDataServiceOptions _movieDataServiceOptions;

        public DefaultMovieDataServiceShould()
        {

            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            _movieDataService = host.Services.GetService(typeof(IMovieDataService)) as IMovieDataService;

            _userName = TestMovieDataServiceHost.UserName;
            _viewGroup = TestMovieDataServiceHost.ViewGroup;
            _movieDataServiceOptions = TestMovieDataServiceHost.MovieDataServiceOptions;
            _movieDataServiceByPassInterface = _movieDataService as DefaultMovieDataService;
            //_movieDataService = _movieDataServiceByPassInterface;

        }

        [Fact]
        [Trait("Category", "Online")]
        public void ReturnMovieDataById()
        {
            var id = 2472;
            var expectedMovie = "Kirschblüten und rote Bohnen";

            var actual = _movieDataService.GetMovieDataAsync(id, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Result.title);
            Assert.Equal("HDD", actual.Result.MediaTypeName);
            Assert.NotNull(actual.Result.LastSeenInformation);
        }

        [Theory]
        [Trait("Category", "Online")]
        [InlineData(new int[] { 62, 2875, 627, 2482 }, 25, 0, 4, 4, "Titanic")]
        [InlineData(new int[] { 62, 2875, 627, 2482 }, 25, 1, 3, 4, "Highlander - Es kann nur einen geben")]
        [InlineData(new int[] { 62, 2875, 627, 2482 }, 25, 2, 2, 4, "Tatsächlich Liebe")]
        [InlineData(new int[] { 62, 2875, 627, 2482 }, 25, 3, 1, 4, "Eddie the Eagle - Alles ist möglich")]
        public async void ReturnPagedMovieDataOrderedBySeenDateGivenASetofIds(int[] idSet, int limit, int offset, int expectedItemCount, int expectedTotalSize, string expectedfirstTitle)
        {
            //var ids = new List<int> { 7, 56, 62, 74, 76, 79, 81, 89, 91, 93, 96, 109, 122, 177, 182, 229, 286, 303, 309, 460, 482, 523, 557, 564, 565, 566, 573, 579, 581, 608, 620, 627, 692, 722, 725, 764, 800, 808, 817, 839, 860, 869, 870, 904, 920, 923, 932, 937, 938, 951, 961, 1042, 1044, 1093, 1107, 1123, 1149, 1172, 1196, 1264, 1290, 1328, 1366, 1539, 1542, 1543, 1572, 1573, 1660, 1668, 1669, 1723, 1728, 1760, 1773, 1801, 1827, 1859, 1880, 1884, 1914, 1985, 1997, 2032, 2064, 2089, 2094, 2142, 2173, 2212, 2274, 2289, 2295, 2296, 2341, 2343, 2344, 2371, 2422, 2440, 2482, 2556, 2600, 2609, 2717, 2756, 2875, 2881 };

            var pagingOptions = new PagingOptions { Limit = limit, Offset = offset };
            var actual = await _movieDataService.GetMovieDataAsync(idSet.ToList(), pagingOptions, new MovieDataOptions { UseInlineCoverImage = true, SortOrder = MovieDataSortOrder.ByLastSeenDateAsc }, new System.Threading.CancellationToken());

            Assert.Equal(expectedTotalSize, actual.TotalSize);
            Assert.Equal(expectedItemCount, actual.Items.Count());
            Assert.Equal(expectedfirstTitle, actual.Items.FirstOrDefault().title);

        }


        [Theory]
        [InlineData(500, 0, 108, 108, "Was nützt die Liebe in Gedanken")]
        [InlineData(50, 0, 50, 108, "Was nützt die Liebe in Gedanken")]
        [InlineData(25, 50, 25, 108, "Robot & Frank")]
        [InlineData(50, 100, 8, 108, "The Color of Magic")]
        [InlineData(10, 10, 10, 108, "The Sixth Sense")]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataOrderedBySeenDateGivenByIds(int limit, int offset, int expectedItemCount, int expectedTotalSize, string firstTitle)
        {
            /*
                select v.title, max(s.viewdate) from videodb_videodata v 
                    left join homewebbridge_userseen s on s.vdb_videoid = v.id
                    where v.id in (  ...)
                    group by v.title
                order by max(s.viewdate) asc
                limit 50 offset 100
             */
            var ids = new List<int> { 7, 56, 62, 74, 76, 79, 81, 89, 91, 93, 96, 109, 122, 177, 182, 229, 286, 303, 309, 460, 482, 523, 557, 564, 565, 566, 573, 579, 581, 608, 620, 627, 692, 722, 725, 764, 800, 808, 817, 839, 860, 869, 870, 904, 920, 923, 932, 937, 938, 951, 961, 1042, 1044, 1093, 1107, 1123, 1149, 1172, 1196, 1264, 1290, 1328, 1366, 1539, 1542, 1543, 1572, 1573, 1660, 1668, 1669, 1723, 1728, 1760, 1773, 1801, 1827, 1859, 1880, 1884, 1914, 1985, 1997, 2032, 2064, 2089, 2094, 2142, 2173, 2212, 2274, 2289, 2295, 2296, 2341, 2343, 2344, 2371, 2422, 2440, 2482, 2556, 2600, 2609, 2717, 2756, 2875, 2881 };

            var pagingOptions = new PagingOptions { Limit = limit, Offset = offset };
            var actual = await _movieDataService.GetMovieDataAsync(ids, pagingOptions, new MovieDataOptions { UseInlineCoverImage = true, SortOrder = MovieDataSortOrder.ByLastSeenDateAsc }, new System.Threading.CancellationToken());

            Assert.Equal(expectedTotalSize, actual.TotalSize);
            Assert.Equal(expectedItemCount, actual.Items.Count());
            Assert.Equal(firstTitle, actual.Items.FirstOrDefault().title);

        }

        [Fact]
        [Trait("Category", "Offline")]
        public void SortQueryByLastSeenDateWhenAllSeenDataIsSet()
        {
            var mockedVideoData = new List<Data.DatabaseModels.videodb_videodata>
            {
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 1,
                    title = "Mocked Movie One",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>() {
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 1, vdb_videoid = 1, viewdate = new DateTime(2021, 12, 1) }
                    },
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 2,
                    title = "Mocked Movie Two",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>() {
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 2, vdb_videoid = 2, viewdate = new DateTime(2020, 12, 2) }
                    },
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 3,
                    title = "Mocked Movie Three",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>() {
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 3, vdb_videoid = 3, viewdate = new DateTime(2021, 12, 2) }
                    },
                }
            };

            var query = mockedVideoData.AsQueryable();
            var actual = _movieDataServiceByPassInterface.QuerySortOrder(MovieDataSortOrder.ByLastSeenDateAsc, query);
            Assert.Equal(2, actual.ToArray()[0].id);
            Assert.Equal(1, actual.ToArray()[1].id);
            Assert.Equal(3, actual.ToArray()[2].id);
        }

        [Fact]
        [Trait("Category", "Offline")]
        public void SortQueryByLastSeenDateWhenNotAllSeenDataIsSet()
        {
            var mockedVideoData = new List<Data.DatabaseModels.videodb_videodata>
            {
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 1,
                    title = "Mocked Movie One",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>() {
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 1, vdb_videoid = 1, viewdate = new DateTime(2021, 12, 1) }
                    },
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 2,
                    title = "Mocked Movie Two",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>(),
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 3,
                    title = "Mocked Movie Three",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>() {
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 2, vdb_videoid = 3, viewdate = new DateTime(2020, 12, 2) }
                    },
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 4,
                    title = "Mocked Movie Four",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>() {
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 3, vdb_videoid = 4, viewdate = new DateTime(2019, 12, 2) },
                        new Data.DatabaseModels.homewebbridge_userseen() { asp_username = "test_user", asp_viewgroup = "test_group", id = 3, vdb_videoid = 4, viewdate = new DateTime(2021, 12, 2) }
                    },
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 5,
                    title = "Mocked Movie Five",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>()
                },
                new Data.DatabaseModels.videodb_videodata
                {
                    id = 6,
                    title = "Mocked Movie Six",
                    SeenInformation = new List<Data.DatabaseModels.homewebbridge_userseen>()
                }
            };

            var query = mockedVideoData.AsQueryable();
            var actual = _movieDataServiceByPassInterface.QuerySortOrder(MovieDataSortOrder.ByLastSeenDateAsc, query);
            Assert.Equal(6, actual.Count());
            Assert.Equal(2, actual.ToArray()[0].id);
            Assert.Equal(5, actual.ToArray()[1].id);
            Assert.Equal(6, actual.ToArray()[2].id);
            Assert.Equal(3, actual.ToArray()[3].id);
            Assert.Equal(1, actual.ToArray()[4].id);
            Assert.Equal(4, actual.ToArray()[5].id);
        }


        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnMovieDataByTitle()
        {
            var expectedId = 1865;
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Title = "Taffe Mädels" };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(1, actual.TotalSize);
            Assert.Equal(expectedId, actual.Items.FirstOrDefault().id);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnMovieDataByTitleCaseInsensitive()
        {
            var expectedId = 1865;
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Title = "taffe Mädels" };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(1, actual.TotalSize);
            Assert.Equal(expectedId, actual.Items.FirstOrDefault().id);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnMovieDataByExcactTitleSearch()
        {
            var expectedId = 2094;
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Title = "HER", ExactMatch = true };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(1, actual.TotalSize);
            Assert.Equal(expectedId, actual.Items.FirstOrDefault().id);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnMovieDataByContainingTitleSearch()
        {
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Title = "HER" };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(98, actual.TotalSize);
        }


        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieByRack()
        {
            var expectedSize = 12;
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Search = "R05F2" };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedSize, actual.TotalSize);
            Assert.Equal(expectedSize, actual.Items.Count());
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataById()
        {
            var id = new List<int> { 2472 };
            var expectedMovie = "Kirschblüten und rote Bohnen";
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions();

            var actual = await _movieDataService.GetMovieDataAsync(id, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Items.FirstOrDefault().title);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataIncludingSeenData()
        {
            var id = new List<int> { 783 };
            var expectedMovie = "Reine Geschmacksache";
            var expectedSeenCount = 6;
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions();

            var actual = await _movieDataService.GetMovieDataAsync(id, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Items.FirstOrDefault().title);
            Assert.True(actual.Items.FirstOrDefault().LastSeenInformation.SeenCount == expectedSeenCount, $"Found {actual.Items.FirstOrDefault().LastSeenInformation.SeenCount}.");
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataIncludingSeenDataWhileMovieIsNotSeen()
        {
            var id = new List<int> { 129 };
            var expectedMovie = "Natural Born Killers";
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions();

            var actual = await _movieDataService.GetMovieDataAsync(id, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Items.FirstOrDefault().title);
            Assert.Null(actual.Items.FirstOrDefault().LastSeenInformation);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieSeenData()
        {
            var pagingOptions = new PagingOptions { Limit = 3000, Offset = 0 };

            var actual = await _movieDataService.GetSeenMovies(pagingOptions, new DateRangeFilterOptions(), new System.Threading.CancellationToken());

            Assert.Equal(2023, actual.TotalSize);
            Assert.Equal("Der Ganz normale Wahnsinn", actual.Items.FirstOrDefault(s => s.SeenDate == new DateTime(2019, 05, 25)).Movie.title);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieSeenDataWithDateRangeFilter()
        {
            var pagingOptions = new PagingOptions { Limit = 3000, Offset = 0 };
            var dateRangeFilterOptions = new DateRangeFilterOptions { FromDate = new DateTime(2020, 1, 11), ToDate = new DateTime(2020, 1, 13) };

            var actual = await _movieDataService.GetSeenMovies(pagingOptions, dateRangeFilterOptions, new System.Threading.CancellationToken());

            Assert.Equal(3, actual.TotalSize);
            Assert.Equal("Willkommen im Wunder Park", actual.Items.FirstOrDefault().Movie.title);
        }

        [Theory]
        [InlineData(200, 0, 113, "Warm Bodies", "")]
        [Trait("Category", "Online")]
        public async void ReturnGetWatchAgainMoviesAsync(int limit, int offset, int expectedCount, string expectedTitle, string notExpectedTitle)
        {
            var pagingOptions = new PagingOptions { Limit = limit, Offset = offset };
            var actual = await _movieDataService.GetWatchAgainMoviesAsync(_userName, pagingOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedCount, actual.Items.Count());
            Assert.Equal(expectedCount, actual.TotalSize);
            Assert.Contains(expectedTitle, actual.Items.Select(item => item.title));
            Assert.DoesNotContain(notExpectedTitle, actual.Items.Select(item => item.title));
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ShouldSetAMovieSeen()
        {
            var id = 2135;
            var date = new DateTime(2018, 08, 18);
            var expectedResultMessage = $"Set movie with id 2135 seen for hans.schuster in viewgroup VG_Default for date 18.08.2018.";
            var actual = await _movieDataService.MovieSeenSetAsync(id, "hans.schuster", "VG_Default", date);

            Assert.Equal(expectedResultMessage, actual.Item2);
            Assert.IsType<int>(actual.Item1);

            // seen date could only be set once per day per viewgroup, this is user independent
            var expectedAlreadyExistsMessage = $"Entry for movie with id {id} for john.doe in viewgroup VG_Default for date 18.08.2018 allready exists.";
            var actualExists = await _movieDataService.MovieSeenSetAsync(id, "john.doe", "VG_Default", date);

            Assert.Equal(expectedAlreadyExistsMessage, actualExists.Item2);
            Assert.Equal(-1, actualExists.Item1);

            var expectedDeleteMessage = $"Removed movie seen date for id {id} in viewgroup VG_Default for date 18.08.2018.";
            var actualDeleted = await _movieDataService.MovieSeenDeleteAsync(id, "VG_Default", date);

            Assert.Equal(expectedDeleteMessage, actualDeleted.Item2);
            Assert.Equal(1, actualDeleted.Item1);

            var expected2ndDeleteMessage = $"No seen entry found for movie with id {id} in viewgroup VG_Default for date 18.08.2018.";
            var actual2ndDeleted = await _movieDataService.MovieSeenDeleteAsync(id, "VG_Default", date);

            Assert.Equal(expected2ndDeleteMessage, actual2ndDeleted.Item2);
            Assert.Equal(-1, actual2ndDeleted.Item1);
        }

        /// <summary>
        /// The set seen / unseen functions should ignore the time part of a date time        
        /// </summary>
        [Fact]
        [Trait("Category", "Online")]
        public async void ShouldSetAMovieSeenWhileRemovingTimePart()
        {
            var id = 2135;
            var date = new DateTime(2018, 08, 19, 13, 44, 22);
            var expectedResultMessage = $"Set movie with id 2135 seen for hans.schuster in viewgroup VG_Default for date 19.08.2018.";
            var options = new MovieSeenOptions { Id = id, Date = date.ToShortDateString() };
            var actual = await _movieDataService.MovieSeenSetAsync(options.Id, "hans.schuster", "VG_Default", date);

            Assert.Equal(expectedResultMessage, actual.Item2);
            Assert.IsType<int>(actual.Item1);

            var expectedAlreadyExistsMessage = $"Entry for movie with id {id} for hans.schuster in viewgroup VG_Default for date 19.08.2018 allready exists.";
            // we add an hour to the date time (movie should not be able to set seen twice a day)
            var changedDateTime = date + new TimeSpan(1, 0, 0);
            var actualExists = await _movieDataService.MovieSeenSetAsync(options.Id, "hans.schuster", "VG_Default", changedDateTime);

            Assert.Equal(expectedAlreadyExistsMessage, actualExists.Item2);
            Assert.Equal(-1, actualExists.Item1);

            var expectedDeleteMessage = $"Removed movie seen date for id {id} in viewgroup VG_Default for date 19.08.2018.";
            var actualDeleted = await _movieDataService.MovieSeenDeleteAsync(options.Id, "VG_Default", changedDateTime);

            Assert.Equal(expectedDeleteMessage, actualDeleted.Item2);
            Assert.Equal(1, actualDeleted.Item1);

            var expected2ndDeleteMessage = $"No seen entry found for movie with id {id} in viewgroup VG_Default for date 19.08.2018.";
            var actual2ndDeleted = await _movieDataService.MovieSeenDeleteAsync(options.Id, "VG_Default", changedDateTime);

            Assert.Equal(expected2ndDeleteMessage, actual2ndDeleted.Item2);
            Assert.Equal(-1, actual2ndDeleted.Item1);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataIncludingFavorite()
        {
            var id = new List<int> { 783 };
            var expectedMovie = "Reine Geschmacksache";
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions();

            var actual = await _movieDataService.GetMovieDataAsync(id, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Items.FirstOrDefault().title);
            Assert.True(actual.Items.FirstOrDefault().IsFavorite, $"Favorite was expected true for movie with id {id.FirstOrDefault()}");

            id = new List<int> { 2368 };
            actual = await _movieDataService.GetMovieDataAsync(id, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());
            Assert.True(actual.Items.FirstOrDefault().IsFavorite, $"Favorite was expected true for movie with id {id.FirstOrDefault()}");
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataIncludingFlagged()
        {
            var id = new List<int> { 134 };
            var expectedMovie = "The Illusionist";
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var movieDataOptions = new MovieDataOptions();

            var actual = await _movieDataService.GetMovieDataAsync(id, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Items.FirstOrDefault().title);
            Assert.True(actual.Items.FirstOrDefault().IsFlagged, $"Flagged was expected true for movie with id {id.FirstOrDefault()}");
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void UpdateMovieData()
        {

            var idToUpdate = 5;
            var titleToUpdate = "Big Fish (updated)";
            var imgUrlToUpdate = "http://ecx.images-amazon.com/images/I/51FRBDZ71YL._SL500_AA240_.jpg";

            var movieRessourceToUpdate = await _movieDataService.GetMovieDataAsync(idToUpdate, new System.Threading.CancellationToken());
            movieRessourceToUpdate.title = titleToUpdate;
            movieRessourceToUpdate.imgurl = imgUrlToUpdate;

            await _movieDataService.UpdateMovieDataAsync(movieRessourceToUpdate.id, movieRessourceToUpdate, new System.Threading.CancellationToken());

            var updatedMovie = await _movieDataService.GetMovieDataAsync(movieRessourceToUpdate.id, new System.Threading.CancellationToken());
            Assert.Equal(titleToUpdate, updatedMovie.title);
            Assert.Equal(_movieDataServiceOptions.HttpCoverImagePath + "5.jpg", updatedMovie.imgurl);
            Assert.Equal(imgUrlToUpdate, updatedMovie.custom3);

            titleToUpdate = "Big Fish";
            movieRessourceToUpdate.title = titleToUpdate;
            await _movieDataService.UpdateMovieDataAsync(movieRessourceToUpdate.id, movieRessourceToUpdate, new System.Threading.CancellationToken());
            updatedMovie = await _movieDataService.GetMovieDataAsync(movieRessourceToUpdate.id, new System.Threading.CancellationToken());
            Assert.Equal(titleToUpdate, updatedMovie.title);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void CreateMovieDataWithGenres()
        {

            var movie = new MovieDataResource
            {
                title = "MyTestMovie",
                subtitle = "MySubtitle",
                diskid = "R29F3D01",
                owner_id = 3,
                mediatype = 16,
                Genres = new List<MovieDataGenreResource> { new MovieDataGenreResource { Id = 4 } }
            };

            var result = await _movieDataService.CreateMovieDataAsync(movie, new System.Threading.CancellationToken());
            await _movieDataService.DeleteMovieDataAsync(result.id, new System.Threading.CancellationToken());

            Assert.Equal(movie.title, result.title);
            Assert.Equal("Comedy", result.Genres.FirstOrDefault().Name);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void CreateMovieData()
        {

            var movie = new MovieDataResource
            {
                title = "MyTestMovie",
                subtitle = "MySubtitle",
                diskid = "R29F3D01",
                owner_id = 3,
                mediatype = 16,
                imgurl = "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
            };

            var result = await _movieDataService.CreateMovieDataAsync(movie, new System.Threading.CancellationToken());
            await _movieDataService.DeleteMovieDataAsync(result.id, new System.Threading.CancellationToken());

            Assert.Equal(movie.title, result.title);
            Assert.Equal(movie.imgurl, result.custom3);
            Assert.Equal(_movieDataServiceOptions.HttpCoverImagePath + result.id + ".jpg", result.imgurl);
            Assert.True(result.Genres.Count == 0);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void HandleUserMovieSettings()
        {
            var movidId = 1646;
            // tear down previous test
            _movieDataServiceByPassInterface.DeleteCompleteUserMovieSetting(movidId, _userName);

            // check preconditions
            var movie = await _movieDataService.GetMovieDataAsync(movidId, new System.Threading.CancellationToken());
            Assert.Equal("Match Point", movie.title);
            Assert.False(movie.IsFlagged);
            Assert.False(movie.IsFavorite);

            await _movieDataService.SetUnsetMovieUserFavorite(movidId, _userName, 1, new System.Threading.CancellationToken());

            var movieFavorite = await _movieDataService.GetMovieDataAsync(movidId, new System.Threading.CancellationToken());
            Assert.False(movieFavorite.IsFlagged);
            Assert.True(movieFavorite.IsFavorite);

            await _movieDataService.SetUnsetMovieUserFlagged(movidId, _userName, 1, new System.Threading.CancellationToken());

            var movieFlagged = await _movieDataService.GetMovieDataAsync(movidId, new System.Threading.CancellationToken());
            Assert.True(movieFlagged.IsFlagged);
            Assert.True(movieFlagged.IsFavorite);

            await _movieDataService.SetUnsetMovieUserFavorite(movidId, _userName, 0, new System.Threading.CancellationToken());
            await _movieDataService.SetUnsetMovieUserFlagged(movidId, _userName, 0, new System.Threading.CancellationToken());

            var resetMovie = await _movieDataService.GetMovieDataAsync(movidId, new System.Threading.CancellationToken());
            Assert.False(resetMovie.IsFlagged);
            Assert.False(resetMovie.IsFavorite);

            // Prepare deleteion
            await _movieDataService.SetUnsetMovieUserFavorite(movidId, _userName, 1, new System.Threading.CancellationToken());
            await _movieDataService.SetUnsetMovieUserFlagged(movidId, _userName, 1, new System.Threading.CancellationToken());

            // tear down test
            _movieDataServiceByPassInterface.DeleteCompleteUserMovieSetting(movidId, _userName);
            var movieDeleted = await _movieDataService.GetMovieDataAsync(movidId, new System.Threading.CancellationToken());
            Assert.False(movieDeleted.IsFlagged);
            Assert.False(movieDeleted.IsFavorite);

        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnPagedMovieDataByGenres()
        {
            var pagingOptions = new PagingOptions { Limit = 1000, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Genres = "Gay,Comedy" };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Contains("Cowboys & Angels", actual.Items.Select(s => s.title));
            Assert.Equal(16, actual.TotalSize);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnEmptyPagedMovieDataWhenGenresNotFound()
        {
            var pagingOptions = new PagingOptions { Limit = 1000, Offset = 0 };
            var movieDataOptions = new MovieDataOptions { Genres = "Thrill" };

            var actual = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, new System.Threading.CancellationToken());

            Assert.Equal(0, actual.TotalSize);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnAllMediaTypes()
        {
            var actual = await _movieDataService.GetAllMediaTypes(new System.Threading.CancellationToken());
            Assert.Equal(7, actual.Count());
            Assert.Equal("Blu-ray", actual.FirstOrDefault(i => i.Id == 16).Name);
            Assert.Equal("HDD", actual.FirstOrDefault(i => i.Id == 14).Name);
            Assert.Equal("4K", actual.FirstOrDefault(i => i.Id == 3).Name);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnAllGenres()
        {
            var actual = await _movieDataService.GetAllGenres(new System.Threading.CancellationToken());
            Assert.Equal(26, actual.Count());
            Assert.Equal("Action", actual.FirstOrDefault(i => i.Id == 1).Name);
            Assert.Equal("Romance", actual.FirstOrDefault(i => i.Id == 14).Name);
        }

        [Fact(Skip = "Long Running")]
        [Trait("Category", "Online")]
        public async void DownloadMissingImages()
        {
            await _movieDataService.DonwloadMissingImages(new System.Threading.CancellationToken());
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void GetNextFreeDiskId()
        {
            var actual = await _movieDataService.GetNextFreeDiskId("R12F5");
            Assert.Equal("R12F5D04", actual);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void GetNextFreeDiskIdInEmtpyShelter()
        {
            var actual = await _movieDataService.GetNextFreeDiskId("R80F5");
            Assert.Equal("R80F5D01", actual);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void GetRacks()
        {
            var actual = await _movieDataService.GetRacks(new System.Threading.CancellationToken());
            Assert.Equal("R01F1", actual.First());
        }
    }
}
