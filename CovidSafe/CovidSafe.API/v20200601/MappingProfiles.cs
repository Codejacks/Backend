using System.Collections.Generic;
using System.Linq;

using AutoMapper;
using CovidSafe.API.v20200601.Protos;
using CovidSafe.Entities.Geospatial;
using CovidSafe.Entities.Reports;

namespace CovidSafe.API.v20200601
{
    /// <summary>
    /// Maps proto types to their internal database representations
    /// </summary>
    public class MappingProfiles : Profile
    {
        /// <summary>
        /// Creates a new <see cref="MappingProfiles"/> instance
        /// </summary>
        public MappingProfiles()
        {
            // Location -> Coordinates
            CreateMap<Location, Coordinates>()
                // Properties have the same name+type
                .ReverseMap();

            // Region -> Region
            CreateMap<Protos.Region, Entities.Geospatial.Region>()
                // Properties have the same name+type
                .ReverseMap();

            // MessageInfo -> InfectionReportMetadata
            CreateMap<MessageInfo, InfectionReportMetadata>()
                .ForMember(
                    im => im.Id,
                    op => op.MapFrom(mi => mi.MessageId)
                )
                .ForMember(
                    im => im.Timestamp,
                    op => op.MapFrom(mi => mi.MessageTimestamp)
                )
                .ReverseMap();

            // IEnumerable<InfectionReportMetadata> -> MessageListResponse
            // This is a one-way response so no ReverseMap is necessary
            CreateMap<IEnumerable<InfectionReportMetadata>, MessageListResponse>()
                .ForMember(
                    mr => mr.MessageInfoes,
                    op => op.MapFrom(im => im)
                )
                .ForMember(
                    mr => mr.MaxResponseTimestamp,
                    op => op.MapFrom(im => im.Count() > 0 ? im.Max(o => o.Timestamp) : 0)
                );

            // Area -> InfectionArea
            CreateMap<Area, InfectionArea>()
                .ForMember(
                    ia => ia.Location,
                    op => op.MapFrom(a => a.Location)
                )
                .ForMember(
                    ia => ia.RadiusMeters,
                    op => op.MapFrom(a => a.RadiusMeters)
                )
                .ForMember(
                    // Not supported in >= v20200601
                    ir => ir.BeginTimestamp,
                    op => op.Ignore()
                )
                .ForMember(
                    // Not supported in >= v20200601
                    ir => ir.EndTimestamp,
                    op => op.Ignore()
                )
                .ReverseMap();

            // AreaMatch -> AreaReport
            CreateMap<AreaMatch, AreaReport>()
                .ForMember(
                    ar => ar.Areas,
                    op => op.MapFrom(am => am.Areas)
                )
                .ForMember(
                    ar => ar.BeginTimestamp,
                    op => op.MapFrom(am => am.BeginTime)
                )
                .ForMember(
                    ar => ar.EndTimestamp,
                    op => op.MapFrom(am => am.EndTime)
                )
                .ForMember(
                    ar => ar.UserMessage,
                    op => op.MapFrom(am => am.UserMessage)
                )
                .ReverseMap();

            // MatchMessage -> InfectionReport
            CreateMap<MatchMessage, InfectionReport>()
                .ForMember(
                    ir => ir.AreaReports,
                    op => op.MapFrom(mm => mm.AreaMatches)
                )
                .ForMember(
                    ir => ir.BooleanExpression,
                    op => op.MapFrom(mm => mm.BoolExpression)
                )
                .ForMember(
                    // Not supported in >= v20200415
                    ir => ir.BluetoothMatchMessage,
                    op => op.Ignore()
                )
                .ForMember(
                    // Not supported in >= v20200601
                    ir => ir.BluetoothSeeds,
                    op => op.Ignore()
                )
                // Other properties have the same name+type
                .ReverseMap();
        }
    }
}
