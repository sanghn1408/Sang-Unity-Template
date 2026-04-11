
public interface IUIManager
{
    void Show<T>(System.Action callBack = null) where T : PanelBase;
    void Hide<T>(System.Action callBack = null) where T : PanelBase;
}
