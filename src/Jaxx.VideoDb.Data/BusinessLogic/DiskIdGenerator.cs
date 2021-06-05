using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jaxx.VideoDb.Data.BusinessLogic
{
    /// <summary>
    /// Class for generating new diskids
    /// </summary>
    public class DiskIdGenerator
    {
        private readonly ILogger _logger;
        public DiskIdGenerator(ILogger<DiskIdGenerator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the next free Diskid for a given list of DiskIds.
        /// </summary>
        /// <param name="DiskIds"></param>
        /// <returns></returns>
        public string GetNextDiskId(string shelterAndCompartment, List<string> DiskIds)
        {
            if (!IsSameShelterAndCompartment(shelterAndCompartment, DiskIds)) throw new ArgumentException("Given Diskids don't belong to same shelter and department.");

            var nextPositionValue = DiskIds.Count > 0 ? GetNextInteger(GetDiskIdPositionParts(DiskIds)) : 1;
            var nextDiskId = shelterAndCompartment + "D" + nextPositionValue.ToString("D2");

            return nextDiskId;
        }

        /// <summary>
        /// Get all missing integer values in the given list starting with 1 to maxValue (included).
        /// </summary>
        /// <param name="usedIntegerList"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        internal List<int> GetMissingIntegerValues(List<int> usedIntegerList, int maxValue)
        {
            return Enumerable.Range(1, maxValue).Except(usedIntegerList).ToList();
        }

        /// <summary>
        /// Get all missing integer values in the given list starting with 1 to maxValue in the given list.
        /// </summary>
        /// <param name="usedIntegerList"></param>
        /// <returns></returns>
        internal List<int> GetMissingIntegerValues(List<int> usedIntegerList)
        {
            var maxValue = usedIntegerList.Count > 0 ? usedIntegerList.Max() : 0;
            return GetMissingIntegerValues(usedIntegerList, maxValue);
        }

        /// <summary>
        /// Returns the next free int from a list of integers. If one is missing in between, this one will be returned.
        /// If list is complete, max value + 1 will be returned
        /// </summary>
        /// <param name="usedIntegerList"></param>
        /// <returns></returns>
        internal int GetNextInteger(List<int> usedIntegerList)
        {
            var missing = GetMissingIntegerValues(usedIntegerList);
            var next = missing.Count > 0 ? missing.Min() : usedIntegerList.Count > 0 ? usedIntegerList.Max() + 1 : 1;
            return next;
        }

        /// <summary>
        /// Returns the position parts for all given diskids as a list of int: R20F3D05 -> 5
        /// </summary>
        /// <param name="DiskIds"></param>
        /// <returns></returns>
        internal List<int> GetDiskIdPositionParts(List<string> DiskIds)
        {
            var idList = new List<int>();
            foreach (var diskid in DiskIds)
            {
                try
                {

                    idList.Add(new DiskId(diskid).CompartmentPositionIntValue);
                }
                catch (FormatException e)
                {
                    _logger.LogError("GetDiskIdPositionParts: {0}", e.Message);
                }
            }

            return idList;
        }

        /// <summary>
        /// Returns true, if all ids in the given list belong to same shelter and compartment
        /// </summary>
        /// <param name="DiskIds"></param>
        /// <returns></returns>
        internal bool IsSameShelterAndCompartment(string shelterAndCompartment, List<string> DiskIds)
        {
            var shelterAndCompartmentList = new List<string>();

            foreach (var item in DiskIds)
            {
                shelterAndCompartmentList.Add(new DiskId(item).ShelterAndCompartment);
            }

            var result = shelterAndCompartmentList.All(item => item == shelterAndCompartment) ? true : false;
            return result;
        }

    }
}
