using Jaxx.VideoDb.Data.BusinessLogic;
using Jaxx.VideoDb.Data.BusinessModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class LastSeenSentenceProviderShould
    {
        private readonly ITestOutputHelper output;

        public LastSeenSentenceProviderShould(ITestOutputHelper output)
        {
            this.output = output;
            // Set default culture for all threads in app domain
            var culture = new System.Globalization.CultureInfo("de-CH");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

        }

        public static IEnumerable<object[]> TestData
           => new object[][]{
                new object[] { new DateTime(2020, 3, 13), new DateTime(2021, 3, 12), "am Freitag, 13. März 2020. Das war vor 364 Tagen." },
                new object[] { new DateTime(2021, 3, 13), new DateTime(2021, 3, 13), "heute." },
                new object[] { new DateTime(2021, 3, 12), new DateTime(2021, 3, 13), "gestern." },
                new object[] { new DateTime(2020, 3, 13), new DateTime(2021, 3, 13), "am Freitag, 13. März 2020. Das war vor 1 Jahr(en) und 0 Tag(en)." },
                new object[] { new DateTime(2020, 3, 13), new DateTime(2021, 3, 13), "am Freitag, 13. März 2020. Das war vor 1 Jahr(en) und 0 Tag(en)." },
                new object[] { new DateTime(2019, 3, 13), new DateTime(2020, 3, 13), "am Mittwoch, 13. März 2019. Das war vor 1 Jahr(en) und 0 Tag(en)." },
                new object[] { new DateTime(2019, 3, 13), new DateTime(2021, 3, 13), "am Mittwoch, 13. März 2019. Das war vor 2 Jahr(en) und 0 Tag(en)." },
                new object[] { new DateTime(2019, 3, 1), new DateTime(2020, 2, 29), "am Freitag, 1. März 2019. Das war vor 365 Tagen." },
                new object[] { new DateTime(2019, 3, 1), new DateTime(2020, 2, 28), "am Freitag, 1. März 2019. Das war vor 364 Tagen." },
                new object[] { new DateTime(2019, 3, 1), new DateTime(2020, 3, 1), "am Freitag, 1. März 2019. Das war vor 1 Jahr(en) und 0 Tag(en)." },
                new object[] { new DateTime(2018, 3, 1), new DateTime(2020, 2, 29), "am Donnerstag, 1. März 2018. Das war vor 1 Jahr(en) und 365 Tag(en)." },
           };



        [Theory]
        [MemberData(nameof(TestData))]
        public void ProvideCorrectSeenSentence(DateTime seenDate, DateTime referenceDate, string expected )
        {
            ILastSeenSentenceProvider provider = new NodaTimeLastSeenSentenceProvider();
            LastSeenInformation info = new LastSeenInformation();
            info.SeenCount = 2;
            info.LastSeenDate = seenDate;
            info.DaysSinceLastView = (referenceDate - info.LastSeenDate).Days;
            //Assert.Equal(365, info.DaysSinceLastView);

            var actual = provider.GetLastSeenSentence(info, referenceDate);
            output.WriteLine(actual.LastSeenSentence);
            Assert.Equal($"Du hast diesen Film bereits {info.SeenCount} Mal gesehen, zuletzt {expected}", actual.LastSeenSentence);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(35)]
        [InlineData(90)]
        public void ReadableTimeSinceLastViewHtmlLowerThan91days(int days)
        {
            ILastSeenSentenceProvider provider = new NodaTimeLastSeenSentenceProvider();
            LastSeenInformation info = new LastSeenInformation();
            info.SeenCount = 2;
            info.LastSeenDate = DateTime.Now - new TimeSpan(days, 0, 0, 0, 0); ;
            info.DaysSinceLastView = (DateTime.Now - info.LastSeenDate).Days;
            Assert.Equal(days, info.DaysSinceLastView);

            var actual = provider.GetReadableTimeSinceLastViewHtml(info.LastSeenDate);
            Assert.Equal($"{days}d", actual);
        }

        [Fact]        
        public void ReadableTimeSinceLastViewHtml4Months()
        {
            ILastSeenSentenceProvider provider = new NodaTimeLastSeenSentenceProvider();
            LastSeenInformation info = new LastSeenInformation();
            info.SeenCount = 2;
            info.LastSeenDate = DateTime.Now - new TimeSpan(150, 0, 0, 0, 0); ;
            info.DaysSinceLastView = (DateTime.Now - info.LastSeenDate).Days;
            Assert.Equal(150, info.DaysSinceLastView);

            var actual = provider.GetReadableTimeSinceLastViewHtml(info.LastSeenDate);
            Assert.Equal($"4M", actual);
        }

        [Fact]
        public void ReadableTimeSinceLastViewHtmlYear()
        {
            ILastSeenSentenceProvider provider = new NodaTimeLastSeenSentenceProvider();
            LastSeenInformation info = new LastSeenInformation();
            info.SeenCount = 2;
            info.LastSeenDate = DateTime.Now - new TimeSpan(367, 0, 0, 0, 0); ;
            info.DaysSinceLastView = (DateTime.Now - info.LastSeenDate).Days;
            Assert.Equal(367, info.DaysSinceLastView);

            var actual = provider.GetReadableTimeSinceLastViewHtml(info.LastSeenDate);
            Assert.Equal($"1Y", actual);
        }

        [Fact]
        public void ReadableTimeSinceLastViewHtmlYearFract14()
        {
            ILastSeenSentenceProvider provider = new NodaTimeLastSeenSentenceProvider();
            LastSeenInformation info = new LastSeenInformation();
            info.SeenCount = 2;
            info.LastSeenDate = DateTime.Now - new TimeSpan(465, 0, 0, 0, 0); ;
            info.DaysSinceLastView = (DateTime.Now - info.LastSeenDate).Days;
            Assert.Equal(465, info.DaysSinceLastView);

            var actual = provider.GetReadableTimeSinceLastViewHtml(info.LastSeenDate);
            Assert.Equal($"1&frac14;Y", actual);
        }
    }
}
