namespace SolidSpace.UI.Factory
{
    public interface IUIFactory
    {
        IToolWindow CreateToolWindow();
        IToolButton CreateToolButton();
        ITagLabel CreateTagLabel();
        ILayoutGrid CreateLayoutGrid();
        IGeneralButton CreateGeneralButton();
        IStringField CreateStringField();
        IVerticalFixedItemList CreateVerticalList();
    }
}