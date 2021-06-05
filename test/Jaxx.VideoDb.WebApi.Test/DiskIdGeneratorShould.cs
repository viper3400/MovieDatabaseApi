using Jaxx.VideoDb.Data.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class DiskIdGeneratorShould
    {
        private ILogger<DiskIdGenerator> _diskIdGeneratorLogger = new LoggerFactory().CreateLogger<DiskIdGenerator>();
        [Fact]
        public void GetMissingIntegersInAList()
        {
            var list = new List<int> { 2, 3, 6, 8 };
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var missingList = generator.GetMissingIntegerValues(list, 10);

            Assert.Contains(1, missingList);
            Assert.Contains(4, missingList);
            Assert.Contains(5, missingList);
            Assert.Contains(7, missingList);
            Assert.Contains(9, missingList);
            Assert.Contains(10, missingList);
            Assert.Equal(6, missingList.Count);
        }

        [Fact]
        public void GetMissingIntegersInAListWithAutoMaxValue()
        {
            var list = new List<int> { 2, 3, 6, 8 };
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var missingList = generator.GetMissingIntegerValues(list);

            Assert.Contains(1, missingList);
            Assert.Contains(4, missingList);
            Assert.Contains(5, missingList);
            Assert.Contains(7, missingList);
            Assert.Equal(4, missingList.Count);
        }
        
        [Fact]
        public void GetMissingIntegersInAListWithEmtpyList()
        {
            var list = new List<int>();
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var missingList = generator.GetMissingIntegerValues(list);

            Assert.Empty(missingList);
        }


        [Fact]
        public void GetMissingIntegersInAListWithoutMissing()
        {
            var list = new List<int> {1, 2, 3, 4, 5, 6, 7, 8 };
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var missingList = generator.GetMissingIntegerValues(list);

            Assert.Empty(missingList);
        }

        [Fact]
        public void GetNextValueFromMissingList()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);

            var list = new List<int> { 2, 3, 6, 8 };            
            var next = generator.GetNextInteger(list);
            Assert.Equal(1, next);

            list = new List<int> { 1, 2, 3, 6, 8 };
            next = generator.GetNextInteger(list);
            Assert.Equal(4, next);

            list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            next = generator.GetNextInteger(list);
            Assert.Equal(9, next);
        }

        [Fact]
        public void GetNextValueFromEmptyList()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);

            var list = new List<int> ();
            var next = generator.GetNextInteger(list);
            Assert.Equal(1, next);         
        }

        [Fact]
        public void SplitDiskidList()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var diskids = new List<string> { "R12F3D02", "R12F3D03", "R12F3D06", "R12F3D08" };
            var actual = generator.GetDiskIdPositionParts(diskids);
            Assert.Contains(2, actual);
            Assert.Contains(3, actual);
            Assert.Contains(6, actual);
            Assert.Contains(8, actual);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void EvaluateSameShelterAndCompartment()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var diskids = new List<string> { "R12F3D02", "R12F3D03", "R12F3D06", "R12F3D08" };
            Assert.True(generator.IsSameShelterAndCompartment("R12F3", diskids));
        }

        [Fact]
        public void EvaluateNotSameShelterAndCompartment()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var diskids = new List<string> { "R12F3D02", "R13F3D03", "R12F3D06", "R12F3D08" };
            Assert.False(generator.IsSameShelterAndCompartment("R12F3", diskids));
            Assert.False(generator.IsSameShelterAndCompartment("R15F8", diskids));
        }

        [Fact]
        public void EvaluateSameShelterAndCompartmentFromEmptyList()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var diskids = new List<string>();
            Assert.True(generator.IsSameShelterAndCompartment("R12F1", diskids));
        }

        [Fact]
        public void ReturnNextDiskId()
        {
            var generator = new DiskIdGenerator(_diskIdGeneratorLogger);
            var diskids = new List<string> { "R12F3D02", "R12F3D03", "R12F3D06", "R12F3D08" };
            var actual = generator.GetNextDiskId("R12F3", diskids);
            Assert.Equal("R12F3D01", actual);

            diskids = new List<string> { "R12F3D01", "R12F3D02", "R12F3D03", "R12F3D06", "R12F3D08" };
            actual = generator.GetNextDiskId("R12F3", diskids);
            Assert.Equal("R12F3D04", actual);

            diskids = new List<string> { "R12F3D01", "R12F3D02", "R12F3D03", "R12F3D04"};
            actual = generator.GetNextDiskId("R12F3", diskids);
            Assert.Equal("R12F3D05", actual);
        }
    }
}
