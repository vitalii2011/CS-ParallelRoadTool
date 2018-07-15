namespace ParallelRoadTool
{
    public class NetTypeItem
    {
        public string FilterText;
        public float HorizontalOffset;
        public bool IsReversed;
        public NetInfo NetInfo;
        public float VerticalOffset;

        public NetTypeItem(NetInfo netInfo, string filterText, float horizontalOffset, float verticalOffset, bool isReversed)
        {
            NetInfo = netInfo;
            FilterText = filterText;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }
    }
}