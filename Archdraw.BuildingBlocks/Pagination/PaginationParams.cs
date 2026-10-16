namespace Archdraw.BuildingBlocks.Pagination;

public class PaginationParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int MaxSize { get; set; } = 200;

    public int FinalSize => Math.Clamp(PageSize, 1, MaxSize);
    public int FinalPage => Math.Max(PageNumber, 1);
    public int Skip => (FinalPage - 1) * FinalSize;
}
