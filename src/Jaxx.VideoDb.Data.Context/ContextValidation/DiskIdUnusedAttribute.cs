using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using Jaxx.VideoDb.Data.Validation;

namespace Jaxx.VideoDb.Data.Context.ContextValidation
{
    public class DiskIdUnusedAttribute : ValidationAttribute
    {
        private string IdPropertyName { get; set; }
        private string DesiredMediaTypeName { get; set; }

        public DiskIdUnusedAttribute(string idPropertyName, string desiredMediaTypeName)
        {
            IdPropertyName = idPropertyName;
            DesiredMediaTypeName = desiredMediaTypeName;
        }

        protected override ValidationResult IsValid(object diskIdCandidate, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;
            Type type = instance.GetType();
            object idpropertyvalue = type.GetProperty(IdPropertyName).GetValue(instance, null);
            string mediatypevalue = type.GetProperty(DesiredMediaTypeName).GetValue(instance, null).ToString();
            int currentVideoId = (int)type.GetProperty("id").GetValue(instance, null);

            var config = (ConfigurationRoot)validationContext.GetService(typeof(IConfiguration));

            // check if mediatype of the current video is an excluded mediatype
            // if so, an EMPTY diskid is considered as valid. (if a Diskid is passed, it HAS to be valid!)
            var mediaTypesWithoutDiskIdValidation = config.GetSection("MovieDataServiceOptions:MediaTypesWithoutDiskIdValidation").GetChildren();
            if (mediaTypesWithoutDiskIdValidation.Any(item => item.Value == mediatypevalue) && (diskIdCandidate == null || string.IsNullOrWhiteSpace(diskIdCandidate.ToString())))
                return ValidationResult.Success;

            // if mediatype is not excluded, and Diskid is null or invaldid
            // return an unsuccessfull validation result
            if (diskIdCandidate == null || !DiskIdValidation.IsValidDiskID(diskIdCandidate.ToString()))
                return new ValidationResult($"<{diskIdCandidate}> is no valid disk id.");

            // if Diskid is valid, it should be unique and should not be already in use
            // BUT: In case of update diskid of the current video should be able to stay the same
            var dbContext = (VideoDbContext)validationContext.GetService(typeof(VideoDbContext));
            bool isCurrentDiskidNewDiskid = false;
            if (diskIdCandidate != null && currentVideoId != 0)
            {
                var curentDiskid = dbContext.VideoData.FirstOrDefault(v => v.id == currentVideoId).diskid;
                isCurrentDiskidNewDiskid = curentDiskid == diskIdCandidate.ToString();
            }

            if (!isCurrentDiskidNewDiskid)
            {
                var contextValidator = new DiskIdContextValidation(dbContext);
                var result = contextValidator.IsDiskIdUsed(diskIdCandidate.ToString());
                if (result) return new ValidationResult($"Diskid already in use");
                else return ValidationResult.Success;
            }
            else return ValidationResult.Success;

        }
    }
}
