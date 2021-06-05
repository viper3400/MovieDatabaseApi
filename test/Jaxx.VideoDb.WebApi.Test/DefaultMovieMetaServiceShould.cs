using AutoMapper;
using Jaxx.VideoDb.Data.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.BusinessModels;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared.Models;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Infrastructure;

namespace Jaxx.VideoDb.WebApi.Test
{
    [Collection("AutoMapperCollection")]
    public class DefaultMovieMetaServiceShould
    {
        private IMovieMetaService _movieMetaServcie;

        public DefaultMovieMetaServiceShould(AutoMapperFixture fixture)
        {
            // ServiceCollectionExtensions.UseStaticRegistration = false; // <-- HERE                  
            var genreMappingXmlConfiguration = "ofdbtovideodbgenremapping.xml";

            var config = new ConfigurationBuilder()
                .AddJsonFile("ClientSecrets.json")
                .Build();

            if (!File.Exists(genreMappingXmlConfiguration)) throw new FileNotFoundException($"{genreMappingXmlConfiguration} missing");
            var connectionString = config["TESTDB_CONNECTIONSTRING"];
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddTransient<XmlMapper.IObjectMapper>(o => new XmlMapper.ObjectMapper(genreMappingXmlConfiguration))
                .AddAutoMapper(typeof(DefaultAutomapperProfile))
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<DefaultMovieMetaService>();

            _movieMetaServcie = new DefaultMovieMetaService(logger, serviceProvider.GetService<IMapper>(), new List<MovieMetaEngine.IMovieMetaSearch> { new OfdbParser.OfdbMovieMetaSearch() }, new DefaultMovieMetaEngineRepository());
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async void ReturnMovieMetaByTitle()
        {
            var expectedMovie = "Kirschblüten und rote Bohnen";
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var actual = await _movieMetaServcie.SearchMovieByTitleAsync(expectedMovie, pagingOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, actual.Items.FirstOrDefault().Title);
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async void ConvertMetaMovieResourceIntoMovieDataResource()
        {
            var expectedMovie = "Am Ende eines viel zu kurzen Tages";
            var barcode = "4009750396872";
            var pagingOptions = new PagingOptions { Limit = 100, Offset = 0 };
            var SUT = await _movieMetaServcie.SearchMovieByBarcodeAsync(barcode, pagingOptions, new System.Threading.CancellationToken());

            Assert.Equal(expectedMovie, SUT.Items.FirstOrDefault().Title);
            Assert.IsType<MovieMetaResource>(SUT.Items.FirstOrDefault());
            // As long as auch meta search wont return a barcode we have to set it here
            SUT.Items.FirstOrDefault().Barcode = barcode;

            var actual = await _movieMetaServcie.ConvertMovieMetaAsync(SUT.Items.FirstOrDefault(), new System.Threading.CancellationToken());
            Assert.Equal(expectedMovie, actual.title);
            Assert.Equal(barcode, actual.custom2);
            Assert.IsType<MovieDataResource>(actual);


        }
    }
}
