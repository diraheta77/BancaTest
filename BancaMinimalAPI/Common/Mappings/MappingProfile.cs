using AutoMapper;
using BancaMinimalAPI.Models;
using BancaMinimalAPI.Features.CreditCards.DTOs;
using BancaMinimalAPI.Features.Transactions.DTOs;
using BancaMinimalAPI.Features.Configuration.DTOs;

namespace BancaMinimalAPI.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Credit Card mappings
            CreateMap<CreditCard, CreditCardDTO>();
            CreateMap<CreditCardStatementDTO, CreditCardDTO>();
            CreateMap<CreditCard, CreditCardStatementDTO>();

            // Transaction mappings
            CreateMap<Transaction, TransactionDTO>();
            CreateMap<TransactionDTO, Transaction>();

            // ...existing code...
            CreateMap<Models.Configuration, ConfigurationDTO>().ReverseMap();

        }
    }
}