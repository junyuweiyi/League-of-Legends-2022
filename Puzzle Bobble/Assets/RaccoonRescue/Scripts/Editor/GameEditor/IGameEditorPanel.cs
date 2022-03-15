using System.Collections.Generic;

public interface IGameEditorPanel
{
    void Draw(LevelEditor levelEditor, LevelEditorBase levelEditorBase);
}

public class GameEditorPanel: IGameEditorPanel
{
    readonly Queue<IGameEditorPanel> _panels = new Queue<IGameEditorPanel>();

    LevelEditor _levelEditor;
    protected LevelEditorBase _levelEditorBase;

    public void RegisterPanel(IGameEditorPanel panel)
    {
        _panels.Enqueue(panel);
    }

    public virtual void Draw(LevelEditor levelEditor, LevelEditorBase levelEditorBase)
    {
        _levelEditor = levelEditor;
        _levelEditorBase = levelEditorBase;
        OnDraw();
        foreach (var panel in _panels)
        {
            panel.Draw(levelEditor, levelEditorBase);
        }
    }

    protected virtual void OnDraw() { }

    protected void Save()
    {
        _levelEditor?.SaveItem();
    }
}