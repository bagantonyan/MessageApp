using AutoMapper;
using MessageApp.API.Models.Messages;
using MessageApp.DAL.Entities;

namespace MessageApp.API.Mappings
{
    public class APIMappingProfile : Profile
    {
        public APIMappingProfile()
        {
            CreateMap<Message, MessageResponseModel>();
        }
    }
}