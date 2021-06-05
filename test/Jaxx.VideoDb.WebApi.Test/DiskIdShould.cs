using Jaxx.VideoDb.Data.BusinessModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class DiskIdShould
    {
        [Fact]
        public void ReturnShelterAndCompartment()
        {
            var diskid = new DiskId("R12F3D07");
            Assert.Equal("R12F3", diskid.ShelterAndCompartment);
        }

        [Fact]
        public void ReturnCompartmentPosition()
        {
            var diskid = new DiskId("R12F3D22");
            Assert.Equal("D22", diskid.CompartmentPosition);
        }

        [Fact]
        public void ReturnCompartmentPositionAsIntValue()
        {
            var diskid = new DiskId("R12F3D04");
            Assert.Equal(4, diskid.CompartmentPositionIntValue);
        }
    }
}
