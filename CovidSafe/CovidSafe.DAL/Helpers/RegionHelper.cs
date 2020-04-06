﻿using System;
using System.Collections.Generic;
using System.Linq;
using CovidSafe.Entities.Geospatial;
using CovidSafe.Entities.Protos;

namespace CovidSafe.DAL.Helpers
{
    /// <summary>
    /// Helper functions for working with <see cref="Region"/> objects
    /// </summary>
    public static class RegionHelper
    {
        /// <summary>
        /// Returns a unique identifier for a provided <see cref="Region"/>
        /// </summary>
        /// <param name="region">Source <see cref="Region"/></param>
        /// <returns><see cref="Region"/> identifier</returns>
        /// <remarks>
        /// ID format strips decimal places.
        /// </remarks>
        public static string GetRegionIdentifier(Region region)
        {
            if(region != null)
            {
                return String.Format("{0},{1},{2}", region.LatitudePrefix, region.LongitudePrefix, region.Precision);
            }
            else
            {
                throw new ArgumentNullException(nameof(region));
            }
        }

        /// <summary>
        /// Creates a new <see cref="RegionBoundary"/> from a provided <see cref="Region"/>
        /// </summary>
        /// <param name="region">Source <see cref="Region"/></param>
        public static RegionBoundary GetRegionBoundary(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            Tuple<double, double> latRange = PrecisionHelper.GetRange(region.LatitudePrefix, region.Precision);
            Tuple<double, double> lonRange = PrecisionHelper.GetRange(region.LongitudePrefix, region.Precision);

            return new RegionBoundary
            {
                Min = new Location { Latitude = latRange.Item1, Longitude = lonRange.Item1 },
                Max = new Location { Latitude = latRange.Item2, Longitude = lonRange.Item2 }
            };
        }

        /// <summary>
        /// Enumerates all regions of given precisions range connected with given <see cref="Region"/>/>
        /// Being connected means either intersects with the region extended by overlap amount of precision-aligned grids
        /// </summary>
        /// <param name="region">Source <see cref="Region"/></param>
        /// <param name="extension">Size of region extension (in precision-aligned steps)</param>
        /// <param name="precisionStart"> Start precision parameter. Any integer value.</param>
        /// <param name="precisionCount"> Count of precision parameters to include. Any non-negative integer value.</param>
        /// <returns>IEnumerable<<see cref="Region"/>> - all connected regions</returns>
        public static IEnumerable<Region> GetConnectedRegions(Region region, int extension, int precisionStart, int precisionCount = 1)
        {
            RegionBoundary rb = GetRegionBoundary(region);
            for (int precision = precisionStart; precision < precisionStart + precisionCount; ++precision)
            {
                double step = PrecisionHelper.GetStep(precision);
                for (double lat = rb.Min.Latitude - extension * step; lat < rb.Max.Latitude + extension * step; lat += step)
                {
                    for (double lon = rb.Min.Longitude - extension * step; lon < rb.Max.Longitude + extension * step; lon += step)
                    {
                        yield return new Region
                        {
                            LatitudePrefix = lat,
                            LongitudePrefix = lon,
                            Precision = precision
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Adjusts region coordinate prefixes to be aligned with precision<see cref="Region"/>/>
        /// </summary>
        /// <param name="region">Input <see cref="Region"/></param>
        /// <returns><see cref="Region"/> - region with adjusted coordinate prefixes</returns>
        public static Region AdjustToPrecision(Region region)
        {
            return new Region
            {
                LatitudePrefix = PrecisionHelper.Round(region.LatitudePrefix, region.Precision),
                LongitudePrefix = PrecisionHelper.Round(region.LongitudePrefix, region.Precision),
                Precision = region.Precision
            };
        }
    }
}