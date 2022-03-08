using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            CreateMap<Talk, TalkModel>();
            CreateMap<Speaker, SpeakerModel>();
            CreateMap<Camp, CampModel>()
                .ForMember(m => m.Venue, o => o.MapFrom(m => m.Location.VenueName))
                .ReverseMap();
        }
    }
}
