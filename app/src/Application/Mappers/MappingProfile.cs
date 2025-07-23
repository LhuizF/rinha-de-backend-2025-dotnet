using AutoMapper;
using Rinha.Application.DTOs;
using Rinha.Domain.Entities;

namespace Rinha.Application.Mappers
{
  public class MappingProfile : Profile
  {
    public MappingProfile()
    {
      CreateMap<PaymentsSummary, PaymentsSummaryDTO>().ReverseMap();
      CreateMap<SummaryDetails, SummaryDetailsDTO>().ReverseMap();
    }
  }
}
