
namespace FrameWork.Model.DTO
{
    public class ListDto<T>
    {
        public ListDto(IEnumerable<T> data, int totalCount, int ListSize, int ListNumber)
        {
            this.Data = data;
            this.TotalCount = totalCount;
            this.ListSize = ListSize;
            this.ListNumber = ListNumber;
        }
        public IEnumerable<T>? Data { get; set; }
        public int TotalCount { get; set; }
        public int ListSize { get; set; }
        public int ListNumber { get; set; }
    }
}
