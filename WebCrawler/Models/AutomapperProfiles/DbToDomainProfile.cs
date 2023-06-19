using AutoMapper;
using WebCrawler.Models.Database;

namespace WebCrawler.Models.AutomapperProfiles;

public class DbToDomainProfile : Profile
{
    public DbToDomainProfile()
    {
        CreateMapsFromDb();
        CreateMapsToDb();
    }

    private void CreateMapsFromDb()
    {
        CreateMap<WebsiteExecutionData, WebsiteExecution>()
            .ForMember(dest => dest.WebsiteGraph, 
                opt => 
                    opt.MapFrom(src => 
                        WebsiteGraphSnapshot.JsonConverter.Deserialize(src.WebsiteGraphSnapshotJson)));
        
        CreateMap<WebsiteRecordData, WebsiteRecord>();
        CreateMap<CrawlInfoData, CrawlInfo>();
    }
    
    private void CreateMapsToDb()
    {
        CreateMap<WebsiteExecution, WebsiteExecutionData>()
            .ForMember(dest => dest.WebsiteGraphSnapshotJson, 
                opt => 
                    opt.MapFrom(src => 
                        WebsiteGraphSnapshot.JsonConverter.Serialize(src.WebsiteGraph!.GetSnapshot())));

        CreateMap<WebsiteRecord, WebsiteRecordData>();
        CreateMap<CrawlInfo, CrawlInfoData>();
    }
}