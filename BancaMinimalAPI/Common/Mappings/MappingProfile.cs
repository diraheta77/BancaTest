using AutoMapper;
using BancaMinimalAPI.Models;
using BancaMinimalAPI.Features.CreditCards.DTOs;
using BancaMinimalAPI.Features.Transactions.DTOs;

namespace BancaMinimalAPI.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreditCard, CreditCardDTO>();
            CreateMap<Transaction, TransactionDTO>()
                .ForMember(dest => dest.Type, 
                    opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}