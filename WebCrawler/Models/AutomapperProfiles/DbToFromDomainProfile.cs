using AutoMapper;
using WebCrawler.Models.Database;

namespace WebCrawler.Models.AutomapperProfiles;

public class DbToFromDomainProfile : Profile
{
    public DbToFromDomainProfile()
    {
        CreateMapsFromDb();
        CreateMapsToDb();
    }

    private void CreateMapsFromDb()
    {
        CreateMap<WebsiteExecutionData, WebsiteExecution>()
            .ForMember(dest => dest.WebsiteGraph, 
                opt => 
                    opt.MapFrom<WebsiteGraph?>(src => null));
        CreateMap<WebsiteRecordData, WebsiteRecord>()
            .ForMember(dest => dest.CrawlInfo, opt => opt.MapFrom(src => src.CrawlInfoData))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        CreateMap<CrawlInfoData, CrawlInfo>()
            .ForMember(dest => dest.LastExecution, opt => opt.MapFrom(src => src.LastExecutionData))
            .ForMember(dest => dest.WebsiteRecordId, opt => opt.MapFrom(src => src.WebsiteRecordDataId));
    }
    
    private void CreateMapsToDb()
    {
        CreateMap<WebsiteExecution, WebsiteExecutionData>()
            .ForMember(dest => dest.WebsiteGraphSnapshotJson, 
                opt => 
                    opt.MapFrom(src => 
                        WebsiteGraphSnapshot.JsonConverter.Serialize(src.WebsiteGraph!.GetSnapshot())));

        CreateMap<WebsiteRecord, WebsiteRecordData>()
            .ForMember(dest => dest.CrawlInfoData, opt => opt.MapFrom(src => src.CrawlInfo))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        CreateMap<CrawlInfo, CrawlInfoData>()
            .ForMember(dest => dest.LastExecutionData, opt => opt.MapFrom(src => src.LastExecution))
            .ForMember(dest => dest.WebsiteRecordDataId, opt => opt.MapFrom(src => src.WebsiteRecordId));
    }
}