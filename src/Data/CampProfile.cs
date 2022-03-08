using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            CreateMap<Talk, TalkModel>()
                .ReverseMap()
                .ForMember(t => t.Camp, opt => opt.Ignore())
                .ForMember(t => t.Speaker, opt => opt.Ignore());

            CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();

            CreateMap<Camp, CampModel>()
                .ForMember(m => m.Venue, o => o.MapFrom(m => m.Location.VenueName))
                .ReverseMap();
        }
    }
}
