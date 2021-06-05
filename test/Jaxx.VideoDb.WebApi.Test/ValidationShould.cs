using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.Data.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class ValidationShould
    {
        [Fact]
        public void VerifyValidDiskId()
        {
            var diskid = "R02F3D05";
            var actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.True(actual, diskid);
        }

        [Fact]
        public void VerifyInvalidDiskId()
        {
            var diskid = "Q02F3D05";
            var actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);

            diskid = "";
            actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);

            diskid = "R02F3";
            actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);

            diskid = "R1F3D02";
            actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);

            diskid = "R1F3D2";
            actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);
        }

        [Fact]
        public void VerifyFach1toFach8()
        {
            // Just F1 to F8 are allowed
            var diskid = "R01F0D02";
            var actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);

            diskid = "R01F9D02";
            actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.False(actual, diskid);

            diskid = "R01F8D02";
            actual = DiskIdValidation.IsValidDiskID(diskid);
            Assert.True(actual, diskid);
        }
    }
}
