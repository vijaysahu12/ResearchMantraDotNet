using AutoMapper;
using RM.Database.ResearchMantraContext;
using RM.Model.RequestModel;

namespace RM.API.Helpers
{
    public class MappingProfileHelper : Profile
    {
        public MappingProfileHelper()
        {
            _ = CreateMap<CallPerformanceRequestModel, CallPerformance>();
        }
    }
}
